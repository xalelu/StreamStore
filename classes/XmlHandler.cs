using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace streamStore.classes
{
    class XmlHandler
    {
        //參考: https://dotblogs.com.tw/kevinya/2019/04/24/135606
        public static string TaskPath = @"" + System.Environment.CurrentDirectory + "\\Task.xml"; //任務資料
        public static string SystemPath = @"" + System.Environment.CurrentDirectory + "\\System.xml"; //系統資料
        public static void Init(datatype type) //對應JTask狀態
        {
            try
            {
                if (type == datatype.Task)
                {
                    //查詢task.xml檔案是否存在.
                    if (!File.Exists(TaskPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", null, null);
                        declaration.Encoding = "UTF-8";
                        doc.AppendChild(declaration);
                        //建立根節點
                        XmlElement TaskList = doc.CreateElement("TaskList");
                        doc.AppendChild(TaskList);
                        //建立子節點
                        TaskManager.init();
                        doc.Save(TaskPath);//存檔成xml
                    }
                    else
                    {
                        //參考: https://www.itread01.com/content/1548951145.html
                        XmlDocument x = new XmlDocument();
                        x.Load(TaskPath); //載入xml文件
                        XmlNode xmlNode = x.GetElementsByTagName("TaskList")[0];

                        TaskManager.init();
                        XmlNodeList nodeList = xmlNode.ChildNodes;//逐一處理Task節點

                        if (nodeList.Count > 0)
                        {
                            foreach (XmlElement task in nodeList)
                            {
                                if (task.HasAttributes)
                                {
                                    for (int i = 0; i < task.Attributes.Count; i++)
                                    {
                                        string ModuleName = task.Attributes["ModuleName"].Value;
                                        string ID = task.Attributes["ID"].Value;
                                        string IP = task.Attributes["IP"].Value;
                                        int PORT = Convert.ToInt16(task.Attributes["PORT"].Value);
                                        string ADDTIME = task.Attributes["ADDTIME"].Value;
                                        JTask _Jtask = new JTask(ModuleName, IP, PORT, ADDTIME);
                                        if (task.HasAttribute("start")) _Jtask.startTime = DateTime.Parse(task.GetAttribute("start"));
                                        if (task.HasAttribute("end")) _Jtask.endTime = DateTime.Parse(task.GetAttribute("end"));
                                        if (task.HasAttribute("tmp")) _Jtask.tempTime = DateTime.Parse(task.GetAttribute("tmp"));
                                        if (task.HasAttribute("interval")) _Jtask.interval = Convert.ToInt32(task.GetAttribute("interval"));
                                        TaskManager.addTask(_Jtask, ID);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //處理系統設定 的部分
                    if (!File.Exists(SystemPath))
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", null, null);
                        declaration.Encoding = "UTF-8";
                        doc.AppendChild(declaration);
                        //建立根節點
                        XmlElement SystemNode = doc.CreateElement("System");
                        doc.AppendChild(SystemNode);
                        XmlElement handleUnit = doc.CreateElement("handleUnit");
                        handleUnit.InnerText = "2"; //分段下載基本小時 (預設:12小時)
                        XmlElement downloadPath = doc.CreateElement("downloadPath");
                        downloadPath.InnerText = System.Environment.CurrentDirectory; //下載位置: (預設: 執行檔所在目錄)
                        XmlElement enable = doc.CreateElement("enable");
                        enable.SetAttribute("deleteTaskAuto", "0"); //自動刪除任務(預設: 不啟動)
                        enable.SetAttribute("recordTask", "0"); //紀錄歷史任務(預設: 不啟動)
                        enable.SetAttribute("startToRun", "0"); //啟動即運行中斷的任務(預設: 不啟動)

                        SystemNode.AppendChild(handleUnit);
                        SystemNode.AppendChild(downloadPath);
                        SystemNode.AppendChild(enable);

                        //建立子節點
                        doc.Save(SystemPath);//存檔成xml
                        XmlHandler.Init(datatype.System);
                    }
                    else
                    {
                        //重新讀取相關的部分
                        XmlDocument x = new XmlDocument();
                        x.Load(SystemPath); //載入xml文件(系統相關)
                        XmlNode _handleUnit = x.GetElementsByTagName("handleUnit")[0];
                        int _handleunit = Convert.ToInt16(_handleUnit.InnerText);
                        XmlNode downloadPath = x.GetElementsByTagName("downloadPath")[0];
                        string _downloadpath = downloadPath.InnerText;
                        XmlNode enable = x.GetElementsByTagName("enable")[0];
                        Boolean _deletetaskauto = (enable.Attributes["deleteTaskAuto"].Value == "1") ? true : false;
                        Boolean _recordtask = (enable.Attributes["recordTask"].Value == "1") ? true : false;
                        Boolean _starttorun = (enable.Attributes["startToRun"].Value == "1") ? true : false;

                        //取得底下的所有設定檔
                        Program.SC = new SystemConfig(_handleunit, _downloadpath, _deletetaskauto, _recordtask, _starttorun);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //更新xml
        //儲存任務群 的資料
        public static void RestoreXML(datatype type)
        {
            try
            {
                switch(type)
                {
                    case datatype.System:
                        XmlDocument x = new XmlDocument();
                        x.Load(SystemPath); //載入xml文件(系統相關)
                        XmlNode _handleUnit = x.GetElementsByTagName("handleUnit")[0];
                        _handleUnit.InnerText = Program.SC.handleUnit.ToString();
                        XmlNode downloadPath = x.GetElementsByTagName("downloadPath")[0];
                        downloadPath.InnerText = Program.SC.downloadPath;
                        XmlNode enable = x.GetElementsByTagName("enable")[0];
                        enable.Attributes["deleteTaskAuto"].Value = (Program.SC.deleteTaskAuto) ? "1" : "0";
                        enable.Attributes["recordTask"].Value = (Program.SC.recordTask) ? "1" : "0";
                        enable.Attributes["startToRun"].Value = (Program.SC.startToRun) ? "1" : "0";
                        x.Save(SystemPath);
                        break;

                    case datatype.Task:
                        XmlDocument y = new XmlDocument();
                        y.Load(TaskPath); //載入xml文件
                        XmlNode xmlNode = y.GetElementsByTagName("TaskList")[0];

                        //(1) 刪除原先的紀錄
                        XmlNodeList nodeList = xmlNode.ChildNodes;//逐一處理Task節點
                        xmlNode.RemoveAll();

                        //(2) 重建紀錄
                        foreach(KeyValuePair<String, JTask> i in TaskManager.getTaskSet())
                        {
                            XmlElement TaskNode = y.CreateElement("Task");
                            TaskNode.SetAttribute("ModuleName", i.Value.ModuleName);
                            TaskNode.SetAttribute("ID", i.Value.ID); //(1)
                            TaskNode.SetAttribute("IP", i.Value.source_ip);//(2)
                            TaskNode.SetAttribute("PORT", i.Value.source_port.ToString());//(3)
                            TaskNode.SetAttribute("ADDTIME", i.Value.AddTime.ToString("yyyy/MM/dd HH:mm:ss"));//(4)
                            TaskNode.SetAttribute("start", i.Value.startTime.ToString("yyyy/MM/dd HH:mm:ss"));//(5)
                            TaskNode.SetAttribute("end", i.Value.endTime.ToString("yyyy/MM/dd HH:mm:ss"));//(6)
                            TaskNode.SetAttribute("tmp", i.Value.tempTime.ToString("yyyy/MM/dd HH:mm:ss"));//(7)
                            TaskNode.SetAttribute("interval", i.Value.interval.ToString());//(8)
                            xmlNode.AppendChild(TaskNode);
                        }
                        y.Save(TaskPath);
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //加入新的任務
        public void AddTask(string ID, string ip, string port, DateTime addtime)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement TaskList = doc.CreateElement("TaskList");
                doc.AppendChild(TaskList);
                //建立子節點
                XmlElement Task = doc.CreateElement("Task");
                Task.SetAttribute("ModuleName", "");//設定未知的ModuleName
                Task.SetAttribute("ID", ID);//設定屬性
                Task.SetAttribute("ip", ip);//設定屬性
                Task.SetAttribute("port", port);//設定屬性
                Task.SetAttribute("add", addtime.ToString("yyyy/mm/dd HH:mm:ss")); //任務加入時間

                //Task.SetAttribute("start", );//條件的開始時間 <之後加入的部分>
                //Task.SetAttribute("end", );//條件的結束時間 <之後加入的部分>
                //Task.SetAttribute("tmp", );//運行的最後時間 <之後加入的部分>
                //Task.SetAttribute("interval", );//間隔 <之後加入的部分>
                //加入至TaskList節點底下
                TaskList.AppendChild(Task);
                RestoreXML(datatype.Task);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //設定任務條件
        public void updateTask(string ID, DateTime start, DateTime end, DateTime tmp, int interval)
        {

        }
        
        //移除任務
        public void RemoveTask(string id)
        {
            try
            {
                RestoreXML(datatype.Task);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //紀錄結束的任務
        public void FinishTask(string id)
        {
            try
            {
                //紀錄過去運行過的任務
                string finalbox = System.Environment.CurrentDirectory + "\\history.txt";
                if(!File.Exists(finalbox))
                {
                    FileStream fs = File.Create(finalbox);
                    fs.Dispose();
                }

                using (StreamWriter recorder = new StreamWriter(finalbox))
                {
                    JTask target = TaskManager.getTask(id);
                    recorder.Write(target.wrap());
                }
                RestoreXML(datatype.Task);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public enum datatype
    {
        Task = 0, //任務相關
        System = 1 //系統相關
    }
}
