using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using System.Text.RegularExpressions;

namespace streamStore.classes
{
    delegate void StateChangeHandler(object sender, MyEventArgs e);//狀態變更通知
    delegate void TimeChangeHandler(object sender, updateTimeevgArgs e);//下載時間變更
    delegate void RecordLog(string deviceID, List<byte> args, ModbusType type, Color color);//紀錄log(modbus)
    delegate void RecordLogHTML(string deviceID, string args, ModbusType type, Color color);//紀錄log(html)

    delegate void ExceptionHandler(Exception e);
    delegate void changeStateHandler(status e); //操作人員點擊"中斷"按鍵, 即觸發該事件
    delegate void changeModuleNameHandler(object sender, string modulename); //JMask取得http標頭的ModuleName屬性
    delegate void processGo(object sender, string processStr);
    
    class JTask : BaseJTask
    {

        public string ModuleName = "";//設備種類
        public string ID;
        public DateTime AddTime;//該任務加入時間
        public TcpClient client;
        public string source_ip;
        public int source_port;
        private int tryconnectlimit=3;
        protected status state;
        private Thread t;
        private CancellationTokenSource taskController;
        private CancellationToken token;
        private Task t2;

        private System.Timers.Timer _time; 
        private int thesecond = 0;
        private string grade = "......"; //▁▂▃▄▅▆▇█

        private byte[] buf;
        public int interval = 60;//資料間隔預設1秒
        //public int setInterval = 8;//每一次抓8個小時(預設)
        public DateTime startTime; //開始時間
        public DateTime endTime; //結束時間

        public DateTime startTempTime;//分時時間(運行開始時間點)
        public DateTime tempTime; //分段時間(運行時間點)

        public RecordLog RecordLogHandler; //紀錄事件(MODBUS)
        public RecordLogHTML RecordLogHandlerHtml; //紀錄事件(HTML)
        public StateChangeHandler stateChangeHandler; //開始運行事件
        public TimeChangeHandler timeChangeHandler; //更新下載完成時間
        protected ExceptionHandler exceptionHandler; //異常中斷情況
        public changeStateHandler changeStateHandler; //傳遞介面操作者, 點擊"中斷"的處理
        public changeModuleNameHandler changeModuleNameHandler; //取得ModuleName後的相關處理
        public processGo processgo;

        public List<logTask> htaskList; //紀錄過去完成的歷史任務

        public DateTime receiveTime;//計時處理(逾時)
        private int ProcessScope = 0;//進度單位量
        private int ProcessAll = 0;//進度總量
        //寫法 1
        /*
        public DateTime startTime { get; set; }
        //寫法 2
        public DateTime endTime
        {
            get { return endTime; }
            set { endTime = value; }
        }
        //要測試的
        private int thevalue
        {
            get { return (thevalue > 5) ? thevalue : 0; }
            set { thevalue = (value > 5) ? value : value * 8; }
        }
        */

        public JTask(string ModuleName, string ip, int port, string addtime = "")
        {
            this.ModuleName = ModuleName;
            this.source_ip = ip;
            this.source_port = port;
            this.state = status.idle;
            this.client = new TcpClient();
            this.buf = new byte[1024];
            this.startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.endTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.tempTime = new DateTime(1970, 1, 1, 0, 0, 0);
            this.AddTime = (addtime != "") ? DateTime.Parse(addtime) : DateTime.Now;
            this.taskController = new CancellationTokenSource();
            this.htaskList = new List<logTask>();
            this._time = new System.Timers.Timer();

            this.exceptionHandler += this.ReHandler;
            this.changeStateHandler += this.setState;
            this._time.Elapsed += this.go;
            this._time.Interval = 1000; //每秒處理1次.

        }

        //啟動 動態進度
        private void start_Time()
        {
            this._time.Start();
        }
        //關閉 動態進度
        private void stop_Time()
        {
            this._time.Stop();
        }
        //更動 進度表文字
        public void go(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender != null)
            {
                this.thesecond += 1;
                this.processgo(this, this.grade.Substring(0, this.thesecond % this.grade.Length));
            }
        }

        public void setState(status ste)
        {
            this.state = ste;
            this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
        }

        public status getState()
        {
            return this.state;
        }

        public void connect()
        {
            try
            {
                if (this.client == null)
                {
                    this.client = new TcpClient();
                }
                if (this.source_ip != "" && this.source_port != 0)
                {
                    this.client.SendBufferSize = 1024;//1024 K快取
                    //this.client.SendTimeout = 1000;//1 秒發佈逾時
                    //this.client.ReceiveTimeout = 1000;//1 秒接收逾時
                    this.client.Connect(IPAddress.Parse(this.source_ip), this.source_port);
                    this.state = status.start;
                    this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                }
            }
            catch (Exception ex)
            {
                //紀錄連線是否正確
                Debug.WriteLine(ex.Message);
                this.state = status.disconnect;
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                throw;
            }
        }

        //取得總
        private int processGap(DateTime _startTime, int _calcurateValue)
        {
            TimeSpan ts = this.endTime.Subtract(_startTime); //要下載的時間range總秒數
            return (Convert.ToInt32(ts.TotalSeconds) > _calcurateValue) ? 100 / (Convert.ToInt32(ts.TotalSeconds) / _calcurateValue) : 100;
        }

        //取得其下載位址
        public string getDownloadPath()
        {
            string time_tag = startTime.ToString("yyyyMMddHHmmss") + "_" + endTime.ToString("yyyyMMddHHmmss");
            return Program.SC.downloadPath + "\\" + this.getipfilename() + "_" + time_tag + ".csv";
        }

        //原子處理(連線即發佈, 即中斷)
        public void atom()
        {
            int packageLen = 0;
            int calcurateValue = ((Program.SC.handleUnit * 3600) / this.interval) * this.interval;//處理預設分段處理 與 使用者的interval的最小值, 作為真正的分段處理值 
            this.ProcessAll = 0;
            this.start_Time(); //啟動進度動態作業
            
            try
            {
                if (this.source_ip != "" && this.source_port != 0)
                {
                    //如下: 
                    //http://192.168.5.89/data.csv?&start=2021-11-23-16-48:0&end=2021-11-23-17-3:0&interval=1
                    if(this.compareTime(startTime, tempTime) <= 0 && this.compareTime(tempTime, endTime) < 0)
                    {
                        this.startTempTime = new DateTime(this.tempTime.Year, this.tempTime.Month, this.tempTime.Day, this.tempTime.Hour, this.tempTime.Minute, this.tempTime.Second);
                        this.ProcessScope = this.processGap(this.startTempTime, calcurateValue);
                        //用於顯示而已
                        //(1)舊作法
                        //this.timeChangeHandler(this, new updateTimeevgArgs(this.startTempTime.AddMinutes(startTempTime.Minute * -1).ToString("yyyy/MM/dd HH:mm:ss")));
                        //if (this.compareTime(startTime, startTempTime) < 0) startTempTime.AddMinutes(1);

                        //(2)新作法
                        this.timeChangeHandler(this, new updateTimeevgArgs(this.startTempTime.ToString("yyyy-MM-dd HH:mm:ss"), "0%"));
                        //if (this.compareTime(startTime, startTempTime) < 0) startTempTime.AddSeconds(this.interval);
                    }
                    else if(this.compareTime(tempTime, endTime) == 0)
                    {
                        this.startTempTime = new DateTime(this.endTime.Year, this.endTime.Month, this.endTime.Day, this.endTime.Hour, this.endTime.Minute, this.endTime.Second);
                    }
                    //this.startTempTime = new DateTime(this.startTime.Year, this.startTime.Month, this.startTime.Day, this.startTime.Hour, this.startTime.Minute, 0);
                    if (this.compareTime(this.startTempTime, this.endTime) < 0)
                    {
                        //(1)舊作法
                        //this.tempTime = this.startTempTime.AddHours(Program.SC.handleUnit); //依據設定的單位時間為基準, 分段抓取.

                        //(2)新作法
                        this.tempTime = this.startTempTime.AddSeconds(calcurateValue);
                        //若tempTime分段處理的末端時間 大於 最後時間, 則以最後時間為主
                        if(this.compareTime(this.tempTime, this.endTime) >= 0)
                        {
                            this.tempTime = new DateTime(this.endTime.Year, this.endTime.Month, this.endTime.Day, this.endTime.Hour, this.endTime.Minute, this.endTime.Second);
                        }
                    }
                    else
                    {
                        XmlHandler.RestoreXML(datatype.Task);//更新狀態
                        this.timeChangeHandler(this, new updateTimeevgArgs("success", "--"));
                    }
                    
                    List<byte> packageContent = new List<byte>();//接收位元
                    while (true)
                    {
                        int compareResult_0 = this.compareTime(startTempTime, endTime);
                        if (compareResult_0 >= 0)
                        {
                            this.tempTime = new DateTime(this.startTime.Year, this.startTime.Month, this.startTime.Day, this.startTime.Hour, this.startTime.Minute, this.startTime.Second);
                            XmlHandler.RestoreXML(datatype.Task);//更新狀態
                            this.ProcessScope = 0;
                            this.ProcessAll = 0;
                            this.timeChangeHandler(this, new updateTimeevgArgs("success", "--"));
                            break;
                        }

                        if (this.state == status.stop || this.state == status.reset)
                        {
                            this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, startTempTime.Second);
                            XmlHandler.RestoreXML(datatype.Task);
                            throw new InteruptionEvent("介面發出中斷指令!");
                        }


                        this.client = new TcpClient();
                        this.client.Client.SendBufferSize = 1024;//1024 K快取
                        this.client.Client.Connect(IPAddress.Parse(this.source_ip), this.source_port);
                       

                        if (this.client.Client.Connected)
                        {
                            this.state = status.running;
                            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": 開始運行");
                            this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));

                            byte[] payload = (this.compareTime(startTime, this.startTempTime)==0) ? this.generalRequestPackage(this.startTempTime, this.tempTime) : this.generalRequestPackage(this.startTempTime.AddSeconds(this.interval), this.tempTime);
                            this.client.Client.Send(payload);
                            int packageLength = -1; //封包的大小

                            DateTime veStart = DateTime.Now;
                            int cacheReceiveCount = 0;
                            int trytime = 20;//處理接收封包的次數
                            while (true)
                            {
                                if (this.client.Client.Available > 0)
                                {
                                    veStart = DateTime.Now;
                                    if (this.state == status.stop)
                                    {
                                        this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, startTempTime.Second);
                                        XmlHandler.RestoreXML(datatype.Task);
                                        throw new InteruptionEvent("介面發出中斷指令!");
                                    }
                                    int byteCount = this.client.Client.Receive(this.buf);

                                    string result = System.Text.Encoding.UTF8.GetString(this.buf);
                                    if (packageLen == 0)
                                    {
                                        int keyIndex = result.IndexOf("Content-Length:");
                                        if (keyIndex > 0)
                                        {
                                            string substr = result.Substring(keyIndex + 15);
                                            //int rn = substr.IndexOf("\r\n");
                                            packageLen = Convert.ToInt32(substr.Substring(0, substr.IndexOf("\r\n")));
                                            if (packageLen > 0 && result.IndexOf("\r\n\r\n") > 0)
                                            {
                                                string response_header = result.Substring(0, result.IndexOf("\r\n\r\n"));
                                                this.RecordLogHandlerHtml(this.ID, response_header, ModbusType.Response, Color.LightGoldenrodYellow);
                                                packageContent.AddRange(Encoding.UTF8.GetBytes(result.Substring(result.IndexOf("\r\n\r\n") + 4)));
                                            }
                                        }
                                        else //標頭沒有帶content-type的情況
                                        {
                                            packageLen = -1;
                                            if(result.IndexOf("\r\n\r\n") > 0)
                                            {
                                                string response_header = result.Substring(0, result.IndexOf("\r\n\r\n"));
                                                this.RecordLogHandlerHtml(this.ID, response_header, ModbusType.Response, Color.LightGoldenrodYellow);
                                                byte[] helo = Encoding.UTF8.GetBytes(result.Substring(result.IndexOf("\r\n\r\n") + 4));
                                                packageContent.AddRange(Encoding.UTF8.GetBytes(result.Substring(result.IndexOf("\r\n\r\n") + 4)));
                                            }
                                        }

                                        //抓取http標頭的 ModuleName 屬性
                                        int moduleNameIndex = result.IndexOf("ModuleName:");//抓取http標頭的 moduleName屬性
                                        if(moduleNameIndex > 0)
                                        {
                                            string substr = result.Substring(moduleNameIndex + 11);
                                            this.ModuleName = substr.Substring(0, substr.IndexOf("\r\n")).Trim();
                                            XmlHandler.RestoreXML(datatype.Task);//更新xmlHandler
                                            this.changeModuleNameHandler(this, this.ModuleName);
                                        }
                                    }
                                    else
                                    {
                                        packageContent.AddRange(this.buf);
                                        if (packageLen > 0)
                                        {
                                            //if (packageContent.Count >= packageLen)
                                            //{
                                                if (packageContent.Count > packageLen)
                                                {
                                                    packageContent.RemoveRange(packageLen, packageContent.Count - packageLen);
                                                }
                                                break;//跳出迴圈
                                            //}
                                        }
                                    }
                                    Array.Clear(this.buf, 0, this.buf.Length); //清空陣列
                                }

                                if (this.state == status.stop)
                                {
                                    this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, startTempTime.Second);
                                    XmlHandler.RestoreXML(datatype.Task);
                                    throw new InteruptionEvent("介面發出中斷指令!");
                                }
                                TimeSpan checktimeout = DateTime.Now - veStart;
                                if (packageLen > 0) //(1)HTML標頭有content-Length的情況
                                {
                                    if (checktimeout.TotalMilliseconds > 1200) //若超過1.5秒, 則發出逾時
                                    {
                                        throw new TimeoutException();
                                    }
                                }
                                else if(packageLen < 0)//(2)HTML標頭沒有content-Length的情況<若這次的接收到的位元數與上次相同, 則認定其已接收完畢>
                                {
                                    if (cacheReceiveCount < packageContent.Count)
                                    {
                                        cacheReceiveCount = packageContent.Count; //紀錄上次接收到的位元數.
                                        trytime = 20;
                                        continue;
                                    }
                                    else if(cacheReceiveCount == packageContent.Count)
                                    {
                                        Thread.Sleep(800);//暫停一秒等侯.
                                        trytime -= 1;
                                        if (trytime == 0)
                                        {
                                            packageLen = 0;//下一個request的封包標頭, 要透過該變數重新判斷, 所以須重置為0
                                            break;
                                        }
                                        continue;
                                    }
                                }
                            }
                            int csvlen = packageContent.Count;

                            //預設僅抓取CSV檔
                            string time_tag = startTime.ToString("yyyyMMddHHmmss") + "_" + endTime.ToString("yyyyMMddHHmmss");
                            this.RecordLogHandlerHtml(this.ID, " Finished Receive " + packageContent.Count.ToString() + " bytes..\r\n\r\n", ModbusType.Response, Color.LightGoldenrodYellow);

                            //寫入的動作
                            writeToFile(Program.SC.downloadPath + "\\" + this.getipfilename() + "_" + time_tag + ".csv", packageContent.ToArray());
                            packageContent.Clear();

                            //觸發更新最後時間
                            XmlHandler.RestoreXML(datatype.Task);
                            this.timeChangeHandler(this, new updateTimeevgArgs(this.tempTime.ToString("yyyy-MM-dd HH:mm:ss"), this.ProcessAll.ToString() + "%"));

                            //DateTime cacheTime = this.tempTime.AddSeconds(this.interval);
                            this.startTempTime = new DateTime(this.tempTime.Year, this.tempTime.Month, this.tempTime.Day, this.tempTime.Hour, this.tempTime.Minute, this.tempTime.Second);
                            //this.startTempTime = this.startTempTime.AddSeconds(1);
                            //(1)舊作法
                            //this.startTempTime = this.startTempTime.AddMinutes(1);//.AddSeconds(this.interval);
                            //this.tempTime = this.tempTime.AddHours(Program.SC.handleUnit);

                            //(2)新作法
                            //this.startTempTime = this.startTempTime.AddSeconds(this.interval);//.AddSeconds(this.interval);
                            this.tempTime = this.startTempTime.AddSeconds(calcurateValue);

                            this.ProcessAll += this.ProcessScope;

                            int compareResult = this.compareTime(tempTime, endTime);
                            if (this.compareTime(startTempTime, endTime) < 0 && compareResult >= 0)
                            {
                                this.tempTime = new DateTime(this.endTime.Year, this.endTime.Month, this.endTime.Day, this.endTime.Hour, this.endTime.Minute, this.endTime.Second);
                            }
                        }
                    }

                    if (this.client.Client.Connected)
                    {
                        this.client.Client.Close();
                        this.client.Dispose();
                        this.client = null;
                        this.state = status.idle;
                        this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 1));
                    }
                }
            }
            catch(InteruptionEvent ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                this.stop_Time(); //中止進度動態作業
                //這裡是使用者自行中斷的動作.
                if (this.client != null)
                {
                    if (this.client.Client != null) this.client.Client.Close();
                    this.client.Dispose();
                    this.client = null;
                }
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": 使用者中斷");
                //有可能是(1)正常運行中的中斷 或 (2)嘗試重新連線的中斷
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
            }
            catch(SocketException ex)
            {
                //紀錄連線是否正確
                this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, 0);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                this.stop_Time(); //中止進度動態作業
                if (this.client != null)
                {
                    if(this.client.Client != null) this.client.Client.Close();
                    this.client.Dispose();
                    this.client = null;
                }
                this.state = (this.state != status.reset) ? status.disconnect : status.reset;
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": 發生socket中斷");
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                if (this.state != status.reset)
                {
                    this.exceptionHandler(ex);//觸發下載被中斷事件的處理
                }
            }
            
            catch(TimeoutException ex)
            {
                this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, 0);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                this.stop_Time(); //中止進度動態作業
                if (this.client != null)
                {
                    if (this.client.Client != null) this.client.Client.Close();
                    this.client.Dispose();
                    this.client = null;
                }
                this.state = (this.state != status.reset) ? status.disconnect : status.reset;
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": 逾時中斷");
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                if (this.state != status.reset)
                {
                    this.exceptionHandler(ex);//觸發下載被中斷事件的處理
                }
            }
            
            catch (Exception ex)
            {
                //紀錄連線是否正確
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);

                this.stop_Time(); //中止進度動態作業
                this.tempTime = new DateTime(startTempTime.Year, startTempTime.Month, startTempTime.Day, startTempTime.Hour, startTempTime.Minute, 0);
                if (this.client != null)
                {
                    if (this.client.Client != null) this.client.Client.Close();
                    this.client.Dispose();
                    this.client = null;
                }
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": 發生不明中斷");
                if (this != null)
                {
                    this.state = (this.state != status.reset) ? status.disconnect : status.reset;
                    this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                    if (this.state != status.reset)
                    {
                        this.exceptionHandler(ex);//觸發下載被中斷事件的處理
                    }
                }
            }
            /*
            finally
            {
                //this.client.Client.Disconnect(false);
                if (this.client != null)
                {
                    if (this.client.Client != null) this.client.Client.Close();
                    this.client.Dispose();
                    this.client = null;
                }
            }
            */
        }

        private string getipfilename()
        {
            return this.ModuleName + "_(" + this.source_ip.Replace('.','_') + ")";
        }

        //針對外來異常導到下載中斷
        public void ReHandler(Exception e)
        {
            try
            {
                Debug.WriteLine(e.Message);
                /*
                this.t = new Thread(new ThreadStart(atom));
                this.t.IsBackground = true;
                this.t.Start();
                */
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": 準備運行");
                this.t2 = Task.Factory.StartNew(atom);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        //比對時間(運行中時間/最絡時間):
        public int compareTime(DateTime tmpTime, DateTime endTime)
        {
            return DateTime.Compare(tmpTime, endTime);
        }

        //二進位寫入檔案
        public int writeToFile(string filePath, byte[] payload)
        {
            try
            {
                FileStream file;
                StreamWriter file2;
                int field_finaladdress = 0;
                
                if (!File.Exists(filePath))
                {
                    file = File.Create(filePath);
                    file.Close();
                }
                else
                {
                    //這裡將payload對應的首個\xe5\xba\xa6\x0a[換行]前的的標題拿掉, 只留數值
                    byte the_bit = 0xA;
                    field_finaladdress = Array.IndexOf(payload, the_bit) + 1;
                    //ArrayExtension.AddRange(ref payload, new byte[] { 0xE5, 0xBA, 0xA6, 0x0A });
                }

                file2 = new StreamWriter(filePath, true, System.Text.Encoding.UTF8);//true: 追加, false: 覆蓋/指定要處理的編碼方法

                if (field_finaladdress > 0)
                {
                    byte[] _payload = new byte[payload.Length - field_finaladdress];
                    Array.Copy(payload, field_finaladdress, _payload, 0, _payload.Length);
                    file2.Write(Encoding.UTF8.GetString(_payload));
                }
                else
                {
                    file2.Write(Encoding.UTF8.GetString(payload));
                }

                file2.Flush();
                file2.Close();
                file2.Dispose();
                return 1;
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return -1;
            }
        }

        //二進位寫入檔案.
        public int writeToFile2(string filePath, byte[] payload)
        {
            try
            {
                FileStream file;
                FileStream file2;
                int field_finaladdress = 0;
                if (!File.Exists(filePath))
                {
                    file = File.Create(filePath);
                    file.Close();
                }

                file2 = File.Open(filePath, FileMode.Append, FileAccess.Write);//避免覆寫掉原來內容.
                file2.Write(payload, 0, payload.Length);

                file2.Close();
                file2.Dispose();
                return 1;
            }
            catch (InvalidCastException e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return -1;
            }
        }

        //確認條件是否成立(是否有設開始時間/結束時間/interval)
        public Boolean CheckCondition()
        {
            return (this.startTime.Year == 1970 || this.endTime.Year == 1970 || interval == 0) ? false : true;
        }

        //啟動任務
        public void startTask()
        {
            switch (this.state)
            {
                case status.idle: //閒置狀態, 尚未建立連線
                    this.t2 = Task.Factory.StartNew(atom, this.token);
                    this.t2.ContinueWith(task =>
                    {
                        var status = string.Format("任務完成，完成狀態為：\rIsCanceled={0}\rIsCompleted={1}\rIsFaulted={2}", task.IsCanceled, task.IsCompleted, task.IsFaulted);
                        // MessageBox.Show(status);
                    });
                    break;

                case status.reset: //閒置狀態, 尚未建立連線
                    this.state = status.idle;
                    this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                    this.t2 = Task.Factory.StartNew(atom, this.token);
                    break;

                case status.start:
                    if (this.client.Connected)
                    {
                        if (this.t == null)
                        {
                            this.t2 = Task.Factory.StartNew(atom);
                        }
                        else
                        {
                            if (this.t.IsAlive)
                            {
                                this.t.Abort();
                                this.t = null;
                                Thread.Sleep(1);
                                this.t2 = Task.Factory.StartNew(atom);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("connect error happened.");
                    }
                    break;

                case status.stop:
                    this.state = status.idle;
                    this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                    this.t2 = Task.Factory.StartNew(atom);
                    break;
            }
        }

        //查詢是否符合"可自動運行任務"的條件
        public Boolean checkAutoStartTask()
        {
            if (this.startTime.Year != 1970 && this.endTime.Year != 1970)
            {
                return (this.compareTime(tempTime, startTime) != 0 && this.compareTime(tempTime, endTime) < 0) ? true : false;
            }
            else
            {
                return false;
            }
        }

        private byte[] generalRequestPackage(DateTime startRange, DateTime endRange)
        {
            string httpHeader = "GET /data.csv?&start=" + startRange.ToString("yyyy-MM-dd-HH-mm") + "&end=" + endRange.ToString("yyyy-MM-dd-HH-mm") + "&interval=" + this.interval.ToString() + " HTTP/1.1\r\n";
            //httpHeader += "Content-Length: 0\r\n";
            httpHeader += "Host: " + this.source_ip + ":" + this.source_port.ToString() + "\r\n";
            httpHeader += "Connection: keep-alive\r\n";
            httpHeader += "Content-Type: Text/csv\r\n\r\n";
            this.RecordLogHandlerHtml(this.ID, httpHeader, ModbusType.Request, Color.LightSeaGreen);
            return Encoding.UTF8.GetBytes(httpHeader);
        }

        //----------背景主要處理的部分------------------------------->>****<<----------
        private void DemoHandle()
        {
            bool goreceived = true;
            try
            {   
                NetworkStream ns = this.client.GetStream();
                //payload改http:
                //如下: 
                //http://192.168.5.89/data.csv?&start=2021-11-23-16-48:0&end=2021-11-23-17-3:0&interval=1
                //GET /data.csv?data.csv?&start=2021-11-23-16-48:0&end=2021-11-23-17-3:0&interval=1 HTTP1.1
                //Content - Type: Text / CSV encoding: UTF8;

                //modbus
                //byte[] payload = new byte[] { 0x21, 0x85, 0x0, 0x0, 0x0, 0x6, 0x1, 0x3, 0x0, 0x0, 0x0, 0x3 };
                //http
                byte[] payload = this.generalRequestPackage(this.startTime, this.endTime);
                this.sendBtye(ref ns, payload);

                StringBuilder received = new StringBuilder();
                Dictionary<string, string> package = new Dictionary<string, string>();
                int packageLength = 0; //封包的大小
                List<byte> hello = new List<byte>();//接收位元
                while (true)
                {
                    //(1)modbus協議的解析
                    /*
                    if (ns.CanRead)
                    {
                        ns.Read(this.buf, 0, this.buf.Length);
                        List<byte> cachestream = (received.Count == 0) ? this.allbytesHandler(this.buf, 0, received.Count) : this.allbytesHandler(this.buf, received.Count, (received[5] + 6) - received.Count);
                        if (cachestream == null)
                        {
                            throw new Exception("receive error happened");
                        }
                        else
                        {
                            received.AddRange(cachestream);
                        }
                    }
                    
                    if (received.Count > 6)
                    {
                        if (received.Count == received[5] + 6)
                        {
                            //(B) 紀錄接收到位元
                            this.RecordLogHandler(this.ID, received, ModbusType.Response, Color.LightGoldenrodYellow);
                            received.Clear();

                            this.sendBtye(ref ns, payload);

                            Thread.Sleep(1);
                        }
                    }
                    */

                    //(2)http協議的解析:CSV檔
                    if (ns.CanRead)
                    {
                        ns.Read(this.buf, 0, this.buf.Length);
                        hello.AddRange(this.buf);
                        /*
                        received.Append(Encoding.UTF8.GetString(this.buf));
                        this.handlerByte(ref package, received.ToString());
                        if (package.ContainsKey("Content-Length"))
                        {
                            
                        }
                        */
                    }

                    if (packageLength > 0) //已紀錄這次封包的長度
                    {
                        //檢該
                    }
                }
                //進行封包解析.
                //ModbusParser.in_parse(this.buf);
            }
            catch(Exception ex)
            {
                this.state = status.stop;
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                this.client.Dispose();
                this.client.Close();
                this.client = null;

                this.state = status.idle;
                this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        //處理HTTP封包(RESPONSE/csv)
        private byte[] getResponseHeader( byte[] payload)
        {
            byte[] filter = { 0x13, 0x10, 0x13, 0x10};
            byte[] checkByte = new byte[filter.Length];

            int gapIndex = 0;
            for(int i = 0;i < payload.Length; i+=filter.Length)
            {
                payload.CopyTo(checkByte, i);
                //比對二個位元
                if(filter.GetHashCode() == checkByte.GetHashCode())
                {
                    gapIndex = i;
                    break;
                }
            }

            if(gapIndex > 0)
            {
                byte[] final = new byte[gapIndex];
                payload.CopyTo(final, 0);
                return final;
            }
            else
            {
                return null;
            }
        }

        //處理HTTP封包(REQUEST)
        private void handlerByte(ref Dictionary<string, string> _set, string payload)
        {
            string[] filter1 = payload.Split("\r\n\r\n");
            string[] filter2 = filter1[0].Split("\r\n");
            if (_set.Count == 0)
            {
                for (int i = 0; i < filter2.Length; i++)
                {
                    if (!_set.ContainsKey("method")) //i == 0
                    {
                        string[] method_url_version = filter1[i].Split(" ");
                        if (!_set.ContainsKey("method")) { _set["method"] = method_url_version[0]; }
                        if (!_set.ContainsKey("url")) { _set["url"] = method_url_version[1]; }
                        if (!_set.ContainsKey("version")) { _set["version"] = method_url_version[2]; }
                    }
                    else
                    {
                        string[] otherParam = filter2[i].Split(": ");
                        if (otherParam.Length > 1) { _set[otherParam[0]] = _set[otherParam[1]]; }
                    }
                }
                if (filter1.Length > 1) { _set["body"] += filter1[1]; }
            }
            else
            {
                _set["body"] += filter1[0];
            }
        }

        //發佈位元出去
        private void sendBtye(ref NetworkStream ns,  byte[] payload)
        {
            if (this.state != status.running) return;
            Debug.WriteLine("send bytes!!");
            ns.Write(payload);
            //(A) 紀錄發佈出去的位元
            this.RecordLogHandler(this.ID, payload.ToList(), ModbusType.Request, Color.LightSeaGreen);
        }

        //recordCut: 已接收到的位元數量, lackCut: 差多少位元
        private List<byte> allbytesHandler(byte[] buf, int recordCut, int lackCut)
        {
            try
            {
                List<byte> new_package = new List<byte>();
                int index = 0;
                if (recordCut > 0)
                {
                    //後續分段部分
                    while (index < lackCut)
                    {
                        new_package.Add(buf[index]);
                        index += 1;
                    }
                }
                else
                {
                    //頭部分//modbus的部分
                    int thelen = (buf.Length > 5) ? buf[5] : 0;
                    while (index < thelen + 6)
                    {
                        new_package.Add(buf[index]);
                        index += 1;
                    }
                }
                return new_package;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //----------背景主要處理的部分------------------------------->>***<<----------
        public void stopTask()
        {
            this.state = status.stop;
            this.stateChangeHandler(this, new MyEventArgs(this.source_ip, this.state, "", 0));
        }

        //最終封裝
        public string wrap()
        {
            return "[ " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + "final";
        }

        //釋放連線
        public void Dispose()
        {

        }

    }

    enum status //狀態
    {
        none=-1,
        idle=0, //閒置(釋放tcpclient)
        start=1,//開始運行: 即連線成功
        running=2,//運行中 [連線中]
        stop=3,//停止處理 [連線中]
        disconnect =4,//連線中斷
        reset = 5 //重置
    }

}
