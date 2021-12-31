using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using streamStore.classes;
using System.Resources; //yymrow
using System.Drawing;
using System.Drawing.Drawing2D;

namespace streamStore
{
    delegate void UpdateProcessStatus();
    delegate void DeletedataGridViewRow(ref DataGridViewRow target);//刪除某資料行

    public partial class Form1 : Form
    {
        private string condition = "";
        public string contextID = "";
        public string _deviceID;
        private UpdateProcessStatus UPS;
        private DeletedataGridViewRow DGV;
        private UserInfo xale = new UserInfo();

        public Form1()
        {
            InitializeComponent();

            //設定選取日期/日曆的色調
            this.monthCalendar1.TitleBackColor = Color.DarkSlateGray;
            this.monthCalendar1.TitleForeColor = Color.Cornsilk;

            //(1)設置資料列表的mouse event顏色
            dataGridView1.CellClick += dataGridView_CellContentClick;
            dataGridView1.CellMouseEnter += dataGridView_CellContentMouseEnter;
            dataGridView1.CellMouseLeave += dataGridView_CellContentMouseLeave;

            //(2)設置menu列表的mouse event顏色
            //this.menuStrip1.Renderer = new MyRenderer();
            任務ToolStripMenuItem.MouseEnter += TaskToolStripMenuItem_MouseEnter;
            任務ToolStripMenuItem.MouseLeave += TaskToolStripMenuItem_MouseLeave;
            //新建任務ToolStripMenuItem.MouseEnter += TaskToolStripMenuItem_MouseEnter;
            //新建任務ToolStripMenuItem.MouseLeave += TaskToolStripMenuItem_MouseLeave;
            //清空歷史任務ToolStripMenuItem.MouseEnter += TaskToolStripMenuItem_MouseEnter;
            //清空歷史任務ToolStripMenuItem.MouseLeave += TaskToolStripMenuItem_MouseLeave;
            
            UPS += this.updateProcessStatus; 
            DGV += this.deletedataGridViewRow;
        }
        
        private void TaskToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            ToolStripMenuItem _target = (ToolStripMenuItem)sender;
            _target.BackColor = Color.DarkSlateGray;
            _target.ForeColor = Color.DarkSlateGray; //LightYellow;
        }
        private void TaskToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem _target = (ToolStripMenuItem)sender;
            _target.BackColor = (_target.Name == "任務ToolStripMenuItem") ? Color.LightCyan : Color.White;
            _target.ForeColor = Color.DarkSlateGray;
        }

        private void 新建任務ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newTaskUI.BringToFront();
            newTaskUI.Visible = true;
            _sourceip.Text = "";
            _sourceport.Text = "80";
        }

        //查詢是否存在相同IP
        private Boolean TaskIPExists(string IP)
        {
            Dictionary<string, JTask> alltask = TaskManager.getTaskSet();
            if (alltask.Count > 0)
            {
                foreach (KeyValuePair<string, JTask> i in alltask)
                {
                    if (i.Value.source_ip == IP)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Task_Click(object sender, EventArgs e)
        {
            Button _target = (Button)sender;
            switch (_target.Name)
            {
                case "sure":
                    if (_sourceip.Text == "")
                    {
                        showMsg("ip是空值!!");
                        _sourceip.Text = "";
                        return;
                    }
                    if(!Regex.IsMatch(_sourceip.Text, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
                    {
                        showMsg("ip格式異常");
                        _sourceip.Text = "";
                        return;
                    }

                    if (_sourceport.Text == "")
                    {
                        showMsg("port是空值!!");
                        _sourceport.Text = "";
                        return;
                    }
                    if(!Regex.IsMatch(_sourceport.Text, @"^[0-9]+$"))
                    {
                        showMsg("port格式異常!!");
                        _sourceport.Text = "";
                        return;
                    }

                    Boolean checkIPexists = TaskIPExists(_sourceip.Text);
                    if(checkIPexists)
                    {
                        showMsg("已有相同設備ip存在.");
                        return;
                    }

                    JTask MTASK = new JTask("", _sourceip.Text, Convert.ToInt32(_sourceport.Text));
                    MTASK.RecordLogHandler += this.RecordLog;
                    MTASK.RecordLogHandlerHtml += this.RecordLogHtml;
                    MTASK.stateChangeHandler += this.stateChangeHandler;
                    MTASK.timeChangeHandler += this.timeChangeHandler;
                    MTASK.changeModuleNameHandler += this.changeModuleNameHandler;

                    string theID = TaskManager.addTask(MTASK);
                    XmlHandler.RestoreXML(datatype.Task);

                    MessageBox.Show((theID != "") ? "完成新增任務!" : "發生錯誤");
                    if (theID != "")
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.DefaultCellStyle.BackColor = (this.dataGridView1.Rows.Count % 2 == 0) ? Color.Cornsilk : Color.BlanchedAlmond;
                        //Image
                        DataGridViewImageCell IC = new DataGridViewImageCell();
                        IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.Default, 80, 80);
                        IC.ImageLayout = DataGridViewImageCellLayout.Stretch;
                        //Image img = System.Drawing.Image.FromFile("");
                        row.Cells.Add(IC);

                        //設備種類
                        DataGridViewTextBoxCell cell_1 = new DataGridViewTextBoxCell();
                        cell_1.Value = "不明";
                        row.Cells.Add(cell_1);

                        //ID, 取消ID
                        /*
                        DataGridViewTextBoxCell cell_2 = new DataGridViewTextBoxCell();
                        cell_2.Value = theID;
                        row.Cells.Add(cell_2);
                        */

                        //ip
                        DataGridViewTextBoxCell cell_3 = new DataGridViewTextBoxCell();
                        cell_3.Value = MTASK.source_ip;
                        row.Cells.Add(cell_3);

                        //port
                        DataGridViewTextBoxCell cell_4 = new DataGridViewTextBoxCell();
                        cell_4.Value = MTASK.source_port;
                        row.Cells.Add(cell_4);

                        //運行狀態
                        DataGridViewTextBoxCell cell_5 = new DataGridViewTextBoxCell();
                        cell_5.Value = "閒置";
                        row.Cells.Add(cell_5);

                        //運行狀態
                        DataGridViewTextBoxCell cell_6 = new DataGridViewTextBoxCell();
                        cell_6.Style.BackColor = Color.BurlyWood;
                        cell_6.Value = "--";
                        row.Cells.Add(cell_6);

                        //最後下載時間
                        DataGridViewTextBoxCell cell_7 = new DataGridViewTextBoxCell();
                        cell_7.Value = "--";
                        row.Cells.Add(cell_7);

                        //操作
                        DataGridViewButtonCell cell_8 = new DataGridViewButtonCell();
                        cell_8.FlatStyle = FlatStyle.Popup;
                        cell_8.Style.BackColor = Color.DarkSeaGreen;//Khaki
                        cell_8.Style.ForeColor = Color.Black;
                        cell_8.Value = "啟動"; //啟動, 關閉
                        row.Cells.Add(cell_8);

                        //設定下載條件 
                        DataGridViewButtonCell cell_9 = new DataGridViewButtonCell();
                        cell_9.FlatStyle = FlatStyle.Popup;
                        cell_9.Style.BackColor = Color.DarkSeaGreen;//Khaki
                        cell_9.Style.ForeColor = Color.Black;
                        cell_9.Value = "設定";
                        row.Cells.Add(cell_9);

                        //刪除任務
                        DataGridViewButtonCell cell_10 = new DataGridViewButtonCell();
                        cell_10.FlatStyle = FlatStyle.Popup;
                        cell_10.Style.BackColor = Color.DarkSeaGreen;//Khaki
                        cell_10.Style.ForeColor = Color.Black;

                        cell_10.Value = "刪除";
                        row.Cells.Add(cell_10);
                        row.Height = 70;//設定Row的高度

                        //將新增的連線 加入到DataGridValue
                        dataGridView1.Rows.Add(row);

                        newTaskUI.Visible = false;
                        newTaskUI.SendToBack();
                        _sourceip.Text = "";
                        _sourceport.Text = "";
                        this.updateProcessStatus();
                    }
                    break;

                case "cancel":
                    _sourceip.Text = "";
                    _sourceport.Text = "";
                    newTaskUI.Visible = false;
                    newTaskUI.SendToBack();
                    break;
            }
        }

        //mouse移入時 (按鍵)
        private void MouseEnter(object sender, EventArgs e)
        {
            Button _target = (Button)sender;
            _target.BackColor = Color.DarkSlateGray;
            _target.ForeColor = Color.LightYellow;
        }

        //mouse移出時 (按鍵)
        private void MouseLeave(object sender, EventArgs e)
        {
            Button _target = (Button)sender;
            _target.BackColor = Color.DarkSeaGreen;// LightGray;
            _target.ForeColor = Color.Black;
        }

        //mouse移入時 (ToolStripMenuItem)
        private void MenuMouseHover(object sender, EventArgs e)
        {
            ToolStripMenuItem _target = (ToolStripMenuItem)sender;
            //_target.BackColor = Color.DarkSlateGray;
            _target.ForeColor = Color.LightYellow;
        }

        //mouse移出時 (ToolStripMenuItem)
        private void MenuMouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem _target = (ToolStripMenuItem)sender;
            //_target.BackColor = Color.White;
            _target.ForeColor = Color.DarkSlateGray;
        }

        //更新線程狀態
        private void updateProcessStatus()
        {
            process_status.Text = TaskManager.getProcessStatusAndCount();
        }

        private void deletedataGridViewRow(ref DataGridViewRow row)
        {
            dataGridView1.Rows.Remove(row);
        }

        private void dataGridView_CellContentMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView _target = (DataGridView)sender;
            int RowIndex = e.RowIndex;
            if (RowIndex < 0 || RowIndex == _target.Rows.Count) return;
            int ColumnIndex = e.ColumnIndex;

            if(ColumnIndex >= 7 && ColumnIndex <= 9)
            {
                _target.Rows[RowIndex].Cells[ColumnIndex].Style.BackColor = Color.DarkSlateGray;
                _target.Rows[RowIndex].Cells[ColumnIndex].Style.ForeColor = Color.LightYellow;
            }
        }

        private void dataGridView_CellContentMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView _target = (DataGridView)sender;
            int RowIndex = e.RowIndex;
            if (RowIndex < 0 || RowIndex == _target.Rows.Count) return;
            int ColumnIndex = e.ColumnIndex;

            if (ColumnIndex >= 7 && ColumnIndex <= 9)
            {
                _target.Rows[RowIndex].Cells[ColumnIndex].Style.BackColor = Color.DarkSeaGreen;
                _target.Rows[RowIndex].Cells[ColumnIndex].Style.ForeColor = Color.Black;
            }
        }
        
        /*
        private void dataGridView_CellContentMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView _target = (DataGridView)sender;
            int RowIndex = e.RowIndex;
            if (RowIndex < 0 || RowIndex == _target.Rows.Count - 1) return;
            int ColumnIndex = e.ColumnIndex;

            switch (ColumnIndex)
            {
                case 7:
                    _target.Rows[RowIndex].Cells[7].Style.BackColor = Color.DarkSeaGreen;
                    _target.Rows[RowIndex].Cells[7].Style.ForeColor = Color.Black;
                    break;

                case 8:
                    _target.Rows[RowIndex].Cells[8].Style.BackColor = Color.DarkSeaGreen;
                    _target.Rows[RowIndex].Cells[8].Style.ForeColor = Color.Black;
                    break;

                case 9:
                    _target.Rows[RowIndex].Cells[9].Style.BackColor = Color.DarkSeaGreen;
                    _target.Rows[RowIndex].Cells[9].Style.ForeColor = Color.Black;
                    break;
            }
        }
        */

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView _target = (DataGridView)sender;
            int RowIndex = e.RowIndex;
            if (RowIndex < 0 || RowIndex == _target.Rows.Count) return;
            int ColumnIndex = e.ColumnIndex;
            
            switch (ColumnIndex)
            {
                case 7:
                    string statusName = _target.Rows[RowIndex].Cells[ColumnIndex].Value.ToString();
                    //showMsg("點擊了狀態!" + statusName);
                    DialogResult dr;
                    switch (statusName)
                    {
                        case "運行":
                            //showMsg("確定要中斷下載任務?");
                            this._deviceID = _target.Rows[RowIndex].Cells[2].Value.ToString();
                            dr = MessageBox.Show("確定要運行下載任務?", "提 示", MessageBoxButtons.YesNo);
                            if (dr == DialogResult.Yes)
                            {
                                if (TaskManager.checkTaskCondition(this._deviceID))
                                {
                                    showMsg("立即運行下載任務!!");
                                    _target.Rows[RowIndex].Cells[7].Value = "--";
                                    _target.Rows[RowIndex].Cells[ColumnIndex].Value = "運行";
                                    _target.Rows[RowIndex].Cells[4].Value = "建立連線"; //無關JTask, 僅用於秀文字
                                    TaskManager.startTask(this._deviceID);
                                }
                                else
                                {
                                    showMsg("該任務尚未設定條件式");
                                    return;
                                }
                            }
                            break;

                        case "啟動":
                            //showMsg("確定要運行下載任務?");
                            this._deviceID = _target.Rows[RowIndex].Cells[2].Value.ToString();
                            dr = MessageBox.Show("確定要運行下載任務?", "提 示", MessageBoxButtons.YesNo);
                            if (dr == DialogResult.Yes)
                            {
                                if (TaskManager.checkTaskCondition(this._deviceID))
                                {
                                    showMsg("立即運行下載任務!!");
                                    _target.Rows[RowIndex].Cells[ColumnIndex].Value = "運行";
                                    _target.Rows[RowIndex].Cells[4].Value = "建立連線"; //無關JTask, 僅用於秀文字
                                    TaskManager.startTask(this._deviceID);
                                }
                                else
                                {
                                    showMsg("該任務尚未設定條件式");
                                    return;
                                }
                            }
                            break;

                        case "中斷": 
                            this._deviceID = _target.Rows[RowIndex].Cells[2].Value.ToString();
                            dr = MessageBox.Show("確定要中斷下載任務?", "提 示", MessageBoxButtons.YesNo);
                            if (dr == DialogResult.Yes)
                            {
                                JTask _ta = TaskManager.getTask(this._deviceID);
                                if(_ta != null)
                                {
                                    //(1)連線無法建立, 嘗試建立連線
                                    Console.WriteLine(_ta.getState().ToString());
                                    if (_ta.getState() == classes.status.disconnect)
                                    {
                                        _ta.changeStateHandler(streamStore.classes.status.reset);
                                    }
                                    //(2)連線已建立連線, 運行中
                                    else if (_ta.getState() == classes.status.running)
                                    {
                                        _ta.changeStateHandler(streamStore.classes.status.stop);
                                    }
                                }
                            }
                            break;
                    }
                    this.updateProcessStatus();
                    break;

                case 8://設定條件時間
                    this._deviceID = _target.Rows[RowIndex].Cells[2].Value.ToString();
                    string status = _target.Rows[RowIndex].Cells[4].Value.ToString();
                    if(status == "運行中")
                    {
                        showMsg("任務運行中, 無法重新設定時間條件" + status);
                        return;
                    }
                    condition_panel.Visible = true;
                    condition_panel.BringToFront();
                    
                    JTask _ve = TaskManager.getTask(this._deviceID);
                    if (_ve == null)
                    {
                        showMsg("發生未知異常, 找不到相關設備任務..");
                        return;
                    }
                    else
                    {
                        DateTime _now = DateTime.Now;
                        if (_ve.startTime.ToString("yyyy/MM/dd") != "1970/01/01")
                        {
                            start_date.Text = _ve.startTime.ToString("yyyy/MM/dd");
                            startTime_hour.Value = _ve.startTime.Hour;
                            startTime_minute.Value = _ve.startTime.Minute;
                        }
                        else
                        {
                            //若沒設定過的話, 就顯示當下時間
                            start_date.Text = _now.ToString("yyyy/MM/dd");
                            startTime_hour.Value = _now.Hour;
                            startTime_minute.Value = _now.Minute;
                        }

                        if (_ve.endTime.ToString("yyyy/MM/dd") != "1970/01/01")
                        {
                            end_date.Text = _ve.endTime.ToString("yyyy/MM/dd");
                            endTime_hour.Value = _ve.endTime.Hour;
                            endTime_minute.Value = _ve.endTime.Minute;
                        }
                        else
                        {
                            //若沒設定過的話, 就顯示當下時間
                            end_date.Text = _now.ToString("yyyy/MM/dd");
                            endTime_hour.Value = _now.Hour;
                            endTime_minute.Value = _now.Minute;
                        }
                        theinterval.Text = _ve.interval.ToString();
                    }
                    break;

                case 9: //刪除任務
                    this._deviceID = _target.Rows[RowIndex].Cells[2].Value.ToString();
                    DialogResult res = MessageBox.Show("確定要刪除" + _deviceID + "任務嗎?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.OK)
                    {
                        TaskManager.releaseTask(_deviceID);
                        _target.Rows.RemoveAt(RowIndex); //刪除列表中的deviceID
                    }
                    XmlHandler.RestoreXML(datatype.Task); //重新更新xml檔
                    break;
            }
        }

        //開啟檔案所在位置.
        private void OpenFolderAndSelectFile(String fileFullName)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + fileFullName;
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            XmlHandler.Init(datatype.System);//讀取xml內容, 以建置Sytem
            XmlHandler.Init(datatype.Task);//讀取xml內容, 以建置TaskManager.Tasks容器
            this.version.Text = Program.Version;

            //this.menuStrip1.Renderer = new ToolStripProfessionalRenderer(new TestColorTable());
            Dictionary<string, JTask> alltask = TaskManager.getTaskSet();
            if (alltask.Count > 0)
            {
                foreach(KeyValuePair<string, JTask> i in alltask)
                {
                    //進行相關委派事件的綁定
                    i.Value.RecordLogHandler += this.RecordLog;
                    i.Value.RecordLogHandlerHtml += this.RecordLogHtml;
                    i.Value.stateChangeHandler += this.stateChangeHandler;
                    i.Value.timeChangeHandler += this.timeChangeHandler;
                    i.Value.changeModuleNameHandler += this.changeModuleNameHandler;
                    i.Value.processgo += this.processGoHandler;

                    //ID
                    DataGridViewRow row = new DataGridViewRow();
                    row.DefaultCellStyle.BackColor = (this.dataGridView1.Rows.Count % 2 == 0) ? Color.Cornsilk : Color.BlanchedAlmond;
                    //Image
                    DataGridViewImageCell IC = new DataGridViewImageCell();
                    switch (i.Value.ModuleName.ToUpper())
                    {
                        case "CTR100":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.CTR100, 80, 80);
                            break;

                        case "EP6":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.EP6, 80, 80);
                            break;

                        case "CW9":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.CW9, 80, 80);
                            break;

                        default:
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.Default, 80, 80);
                            break;
                    }
                    IC.ImageLayout = DataGridViewImageCellLayout.Stretch;
                    row.Cells.Add(IC);

                    //設備種類
                    DataGridViewTextBoxCell cell_1 = new DataGridViewTextBoxCell();
                    cell_1.Value = i.Value.ModuleName;
                    row.Cells.Add(cell_1);

                    //ID
                    /*
                    DataGridViewTextBoxCell cell_2 = new DataGridViewTextBoxCell();
                    cell_2.Value = i.Value.ID;
                    row.Cells.Add(cell_2);
                    */

                    //ip
                    DataGridViewTextBoxCell cell_3 = new DataGridViewTextBoxCell();
                    cell_3.Value = i.Value.source_ip;
                    row.Cells.Add(cell_3);

                    //port
                    DataGridViewTextBoxCell cell_4 = new DataGridViewTextBoxCell();
                    cell_4.Value = i.Value.source_port;
                    row.Cells.Add(cell_4);

                    //運行狀態
                    DataGridViewTextBoxCell cell_5 = new DataGridViewTextBoxCell();
                    cell_5.Value = "閒置";
                    row.Cells.Add(cell_5);

                    DataGridViewTextBoxCell cell_6 = new DataGridViewTextBoxCell();
                    cell_6.Style.BackColor = Color.BurlyWood;
                    cell_6.Value = "--";
                    row.Cells.Add(cell_6);

                    //最後下載時間
                    DataGridViewTextBoxCell cell_7 = new DataGridViewTextBoxCell();
                    if(i.Value.compareTime(i.Value.tempTime, i.Value.startTime) == 0 && i.Value.tempTime.Year == 1970)
                    {
                        cell_7.Value = "--";
                    }
                    else
                    {
                        cell_7.Value = i.Value.tempTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    row.Cells.Add(cell_7);
                    row.Height = 70;//設定Row的高度

                    //操作
                    DataGridViewButtonCell cell_8 = new DataGridViewButtonCell();
                    cell_8.FlatStyle = FlatStyle.Popup;
                    cell_8.Style.BackColor = Color.DarkSeaGreen;//Khaki;
                    cell_8.Style.ForeColor = Color.Black;
                    
                    cell_8.Value = "啟動"; //啟動, 關閉
                    row.Cells.Add(cell_8);

                    //設定下載條件
                    DataGridViewButtonCell cell_9 = new DataGridViewButtonCell();
                    cell_9.FlatStyle = FlatStyle.Popup;
                    cell_9.Style.BackColor = Color.DarkSeaGreen;//.Khaki;s
                    cell_9.Style.ForeColor = Color.Black;
                    cell_9.Value = "設定";
                    row.Cells.Add(cell_9);

                    //刪除任務
                    DataGridViewButtonCell cell_10 = new DataGridViewButtonCell();
                    cell_10.FlatStyle = FlatStyle.Popup;
                    cell_10.Style.BackColor = Color.DarkSeaGreen;//.Khaki;
                    cell_10.Style.ForeColor = Color.Black;
                    cell_10.Value = "刪除";
                    row.Cells.Add(cell_10);

                    //將新增的連線 加入到DataGridValue
                    dataGridView1.Rows.Add(row);

                    if(Program.SC.startToRun && i.Value.checkAutoStartTask()) //查詢是否啟用"啟動即運行"
                    {
                        TaskManager.startTask(i.Key);
                    }
                }
            }
            
            newTaskUI.Visible = true;
            newTaskUI.BringToFront();
            _sourceip.Text = "";
            _sourceport.Text = "80";
            _sourceport.Enabled = true;
            this.updateProcessStatus();
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            TaskManager.Clear(); //清空所有任務
        }

        private void showMsg(string msg)
        {
            MessageBox.Show(msg);
        }

        private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            system_config.Visible = true;
            system_config.BringToFront();
            handlebasic.Value = Program.SC.handleUnit;
            downloadPath.Text = Program.SC.downloadPath;
            delete_task_auto.Checked = Program.SC.deleteTaskAuto;
            record_task.Checked = Program.SC.recordTask;
            start_to_run.Checked = Program.SC.startToRun;
        }

        //系統設定相關
        private void system_Click(object sender, EventArgs e)
        {
            Button _target = (Button)sender;
            switch(_target.Name)
            {
                case "selectdownloadpath_btn":
                    FolderBrowserDialog FBD = new FolderBrowserDialog();
                    FBD.ShowDialog();
                    downloadPath.Text = FBD.SelectedPath;
                    break;

                case "system_sure"://確定
                    //寫到系統檔
                    try
                    {
                        Program.SC.handleUnit = Convert.ToInt32(handlebasic.Value);
                        Program.SC.downloadPath = (downloadPath.Text != "") ? downloadPath.Text : System.Environment.CurrentDirectory;
                        Program.SC.deleteTaskAuto = (delete_task_auto.Checked) ? true : false;
                        Program.SC.recordTask = (record_task.Checked) ? true : false;
                        Program.SC.startToRun = (start_to_run.Checked) ? true : false;
                        XmlHandler.RestoreXML(datatype.System);
                        showMsg("完成設定系統數值");
                    }
                    catch(Exception ex)
                    {
                        showMsg("Exception: " + ex.Message);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                    finally
                    {
                        system_config.Visible = false;
                        system_config.SendToBack();
                    }
                    break;

                case "system_close"://關閉
                    system_config.Visible = false;
                    system_config.SendToBack();
                    break;

            }
            
        }

        //時間條件/搜尋間隔 設定
        private void condition_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "condition_sure":
                    String _startDate = start_date.Text + " " + startTime_hour.Value.ToString().PadLeft(2, '0') + ":" + startTime_minute.Value.ToString().PadLeft(2, '0') + ":00";
                    String _endDate = end_date.Text + " " + endTime_hour.Value.ToString().PadLeft(2, '0') + ":" + endTime_minute.Value.ToString().PadLeft(2, '0') + ":00";

                    if (start_date.Text == "--" || end_date.Text == "--")
                    {
                        showMsg("時間條件未設定完整..");
                        return;
                    }
                    if(!Regex.IsMatch(theinterval.Text, @"^[0-9]+$"))
                    {
                        showMsg("指定間隔必須為數字");
                        return;
                    }
                    if(theinterval.Text == "0")
                    {
                        showMsg("間隔數須大於0");
                        return;
                    }

                    DateTime start_real = DateTime.Parse(_startDate);
                    DateTime end_real = DateTime.Parse(_endDate);
                    int result = DateTime.Compare(start_real, end_real);
                    if (result >= 1)
                    {
                        showMsg("開始/結束時間 設定異常..");
                        return;
                    }
                    else
                    {
                        JTask _ve = TaskManager.getTask(this._deviceID);
                        if (_ve == null)
                        {
                            showMsg("發生未知異常, 找不到相關設備任務..");
                            return;
                        }
                        else
                        {
                            _ve.startTime = DateTime.Parse(_startDate);
                            _ve.endTime = DateTime.Parse(_endDate);
                            _ve.tempTime = new DateTime(_ve.startTime.Year, _ve.startTime.Month, _ve.startTime.Day, _ve.startTime.Hour, _ve.startTime.Minute, _ve.startTime.Second);
                            _ve.interval = Convert.ToInt16(theinterval.Text);
                            XmlHandler.RestoreXML(datatype.Task); //儲存
                        }
                    }

                    condition_panel.Visible = false;
                    condition_panel.SendToBack();

                    //直接執行
                    TaskManager.startTask(this._deviceID);

                    break;

                case "condition_reset":
                    JTask _ve2 = TaskManager.getTask(this._deviceID);
                    _ve2.startTime = new DateTime(1970, 1, 1, 0, 0, 0);
                    _ve2.endTime = new DateTime(1970, 1, 1, 0, 0, 0);
                    _ve2.tempTime = new DateTime(1970, 1, 1, 0, 0, 0);
                    _ve2.setState(status.idle);
                    _ve2.interval = 1;
                    XmlHandler.RestoreXML(datatype.Task); //儲存

                    start_date.Text = "--";
                    end_date.Text = "--";
                    theinterval.Text = "60";
                    startTime_hour.Value = 0;
                    startTime_minute.Value = 0;
                    endTime_hour.Value = 0;
                    endTime_minute.Value = 0;

                    foreach (DataGridViewRow i in dataGridView1.Rows)
                    {
                        if (i.Cells[2].Value == this._deviceID)
                        {
                            //i.Cells[4].Value = "閒置"; //狀態
                            i.Cells[5].Value = "--"; //進度
                            i.Cells[6].Value = ""; //最後紀錄時間
                            i.Cells[7].Value = "啟動"; //作業
                        }
                    }

                    showMsg("完成重置條件");

                    condition_panel.Visible = false;
                    condition_panel.SendToBack();
                    break;

                case "condition_cancel":
                    condition_panel.Visible = false;
                    condition_panel.SendToBack();
                    break;
            }
        }

        /*
        private void 清空歷史任務ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("確定要刪除所有任務?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (res == DialogResult.OK)
            {
                TaskManager.Clear();
                dataGridView1.Rows.Clear();
                XmlHandler.RestoreXML(datatype.Task);
            }
        }
        */

        private void selectDate_Click(object sender, EventArgs e)
        {
            Button _target = (Button)sender; 
            this.monthCalendar1.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateSelected);
            this.monthCalendar1.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateChanged);

            switch (_target.Name)
            {
                case "selectstartDate":
                    condition = "start";
                    break;
                case "selectendDate":
                    condition = "end";
                    break;
            }

            calendar.Visible = true;
            calendar.BringToFront();
        }

        private void calendar_Click(object sender, EventArgs e)
        {
            Button _target = (Button)sender;
            switch (_target.Name)
            {
                case "calendar_sure":
                    if (condition == "start") start_date.Text = this.textBox1.Text;
                    if (condition == "end") end_date.Text = this.textBox1.Text;
                    break;

                case "calendar_cancel":
                    start_date.Text = (start_date.Text != "") ? start_date.Text : "";
                    end_date.Text = (end_date.Text != "") ? end_date.Text : "";
                    break;
            }

            calendar.Visible = false;
            calendar.SendToBack();
            condition = "";
        }

        private void monthCalendar1_DateSelected(object sender, System.Windows.Forms.DateRangeEventArgs e)
        {
            this.textBox1.Text = e.Start.ToShortDateString();
        }

        private void monthCalendar1_DateChanged(object sender, System.Windows.Forms.DateRangeEventArgs e)
        {
            this.textBox1.Text = e.Start.ToShortDateString();
        }

        public List<IOForm> _io_form = new List<IOForm>(); //通訊紀錄
        public HistoryTask history_form = new HistoryTask(); //歷史任務
        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IOForm io_form = new IOForm();
            io_form.Show();
            io_form.recorddemine = true;
            io_form.commandIndex = 0;
            io_form.Visible = true;
            _io_form.Add(io_form);
        }

        //JTASK 狀態變更.
        private void stateChangeHandler(object sender, MyEventArgs e)
        {
            JTask _target = (JTask)sender; //取得現行任務的狀態
            //更新DateGridView 對應的設備ID 的狀態.
            foreach (DataGridViewRow i in dataGridView1.Rows)
            {
                if (i.Cells[2].Value == e.deviceID)
                {
                    switch (e.st)
                    {
                        case status.idle:
                            i.Cells[4].Value = "閒置";
                            i.Cells[7].Value = "運行";
                            break;

                        case status.running:
                            i.Cells[4].Value = "運行中";
                            i.Cells[7].Value = "中斷";
                            break;

                        case status.start:
                            i.Cells[4].Value = "已連線";
                            i.Cells[7].Value = "運行";
                            break;

                        case status.stop:
                            i.Cells[4].Value = "已中斷";
                            i.Cells[7].Value = "運行";
                            break;

                        case status.reset:
                            i.Cells[4].Value = "已重置";
                            i.Cells[7].Value = "運行";
                            break;

                        case status.disconnect:
                            i.Cells[4].Value = "嘗試連線";
                            i.Cells[5].Value = "--";
                            i.Cells[7].Value = "中斷";
                            break;
                    }
                    if (e.code == 1)
                    {
                        if (Program.SC.deleteTaskAuto)
                        {
                            TaskManager.releaseTask(e.deviceID);
                            this.Invoke(DGV, i);
                        }
                        showMsg(e.deviceID + ", 已完成下載作業!!");
                        OpenFolderAndSelectFile(TaskManager.getDownloadPath(e.deviceID));
                    }
                }
            }
            this.Invoke(UPS); //更新各任務狀況
        }

        //下載完成時間(進度)更新
        private void timeChangeHandler(object sender, updateTimeevgArgs e)
        {
            JTask _target = (JTask)sender; //取得現行任務的狀態.
            //更新DateGridView 對應的設備ID 的狀態.
            foreach (DataGridViewRow i in dataGridView1.Rows)
            {
                if (i.Cells[2].Value == _target.source_ip)
                {
                    i.Cells[6].Value = e.msg;
                    i.Cells[5].Value = e.processStr; //進度表
                }
            }
        }

        private void changeModuleNameHandler(object sender, string ModuleName)
        {
            JTask _target = (JTask)sender; //取得現行任務的狀態
            //更新DateGridView 對應的設備ID 的狀態.
            foreach (DataGridViewRow i in dataGridView1.Rows)
            {
                if (i.Cells[2].Value == _target.source_ip)
                {
                    i.Cells[1].Value = ModuleName;
                    DataGridViewImageCell IC = new DataGridViewImageCell();
                    switch (ModuleName.ToUpper())
                    {
                        case "CTR100":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.CTR100, 80, 80);
                            i.Cells[0] = IC;    
                            break;

                        case "EP6":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.EP6, 80, 80);
                            i.Cells[0] = IC;
                            break;

                        case "CW9":
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.CW9, 80, 80);
                            i.Cells[0] = IC;
                            break;

                        default:
                            IC.Value = new Bitmap(JNC設備歷史資料下載軟體.Properties.Resources.Default, 80, 80);
                            i.Cells[0] = IC;
                            break;
                    }
                    return;
                }
            }
        }

        private void processGoHandler(object sender, string processStr)
        {
            JTask _target = (JTask)sender; //取得現行任務的狀態
            foreach (DataGridViewRow i in dataGridView1.Rows)
            {
                if (i.Cells[2].Value == _target.source_ip && i.Cells[4].Value.ToString().Contains("運行中"))
                {
                    i.Cells[4].Value = "運行中" + processStr;
                }
            }
        }

        public delegate void myRecord(string deviceID, string payload, ModbusType type, Color color); //20201228 sam add(設定是否紀錄request/response console)
        //查詢request/response Log用
        public void RecordLog(string deviceID, List<byte> args, ModbusType type, Color color) //Handles Comport.RecordLog
        {
            try
            {
                if (this._io_form == null) return;
                String result = "";
                foreach (Byte i in args)
                {
                    //result &= i & " " 10進位
                    result += Convert.ToString(i, 16).PadLeft(2, '0').ToUpper() + " "; //16進位
                }
                foreach (IOForm i in _io_form)
                {
                    if (i.recorddemine)
                    {
                        myRecord a = new myRecord(RecordJob);
                        this.Invoke(a, new object[] { deviceID, result + Environment.NewLine, type, color });
                    }
                }
            }
            //End If
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        //查詢request/response Log用
        public void RecordLogHtml(string deviceID, string args, ModbusType type, Color color) //Handles Comport.RecordLog
        {
            try
            {
                if (this._io_form == null) return;
                String result = args;
                foreach (IOForm i in _io_form)
                {
                    if (i.recorddemine)
                    {
                        myRecord a = new myRecord(RecordJob);
                        this.Invoke(a, new object[] { deviceID, result + Environment.NewLine, type, color });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        public void RecordJob(string deviceID, string payload, ModbusType type, Color color)
        {
            if (this._io_form.Count < 1) return;
            try
            {
                string thetime = DateTime.Now.ToString("mm:ss:fff");
                foreach (IOForm i in _io_form)
                {
                    switch (type)
                    {
                        case ModbusType.Request:
                            if (i.checkTarget.SelectedItem == null) continue;
                            if (!i.checkTarget.SelectedItem.ToString().Contains(deviceID)) continue;
                            i.commandIndex += 1;
                            i.request_record.SelectionColor = color;
                            i.request_record.AppendText("█" + i.commandIndex.ToString().PadLeft(3, '0') + "█" + thetime + "█ " + payload);
                            i.request_record.ScrollToCaret();
                            break;

                        case ModbusType.Response:
                            if (i.checkTarget.SelectedItem == null) continue;
                            if (!i.checkTarget.SelectedItem.ToString().Contains(deviceID)) continue;
                            i.response_record.SelectionColor = color;
                            i.response_record.AppendText("█" + i.commandIndex.ToString().PadLeft(3, '0') + "█" + thetime + "█ " + payload);
                            i.response_record.ScrollToCaret();
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void 歷史任務ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.history_form = new HistoryTask();
            history_form.Show();
            history_form.Visible = true;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void start_date_Click(object sender, EventArgs e)
        {

        }

        private void condition_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void system_config_Paint(object sender, PaintEventArgs e)
        {

        }

        private void newTaskUI_Paint(object sender, PaintEventArgs e)
        {

        }

        private void 任務ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }

    public enum ModbusType
    {
        Request = 0,
        Response = 1,
    }

    //修改ToolMenu background color
    //reference: https://dotblogs.com.tw/jshsshwa/2013/05/05/102810
    public class MyRenderer: ToolStripProfessionalRenderer
    {
        //自訂的Renderer 繼承 MyColors
        public MyRenderer() : base(new MyColors()) { }
    }

    //自定義MyColors 繼承 ProfessionalColorTable
    //override 顏色屬性
    public class MyColors : ProfessionalColorTable
    {
        //子項目被點選後的顏色
        public override Color MenuItemSelected
        {
            //回傳深藍色
            get { return Color.DarkSlateGray; }
        }

        //子項目被點選後的漸層起始顏色
        public override Color MenuItemSelectedGradientBegin
        {
            get { return Color.DarkSlateGray; }
        }

        //子項目被點選後的漸層結束顏色
        public override Color MenuItemSelectedGradientEnd
        {
            get { return Color.DarkSlateGray; }
        }

        //子項目按下時的漸層起始顏色
        public override Color MenuItemPressedGradientBegin
        {
            get { return Color.DarkSlateGray; }
        }

        //子項目按下時的漸層結束顏色
        public override Color MenuItemPressedGradientEnd
        {
            get { return Color.DarkSlateGray; }
        }

        //MenuStrip父層漸層起始顏色
        public override Color MenuStripGradientBegin
        {
            get { return Color.White; }
        }

        //MenuStrip父層漸層結束顏色
        public override Color MenuStripGradientEnd
        {
            get { return Color.White; }
        }
    }

    //參考: https://programmerall.com/article/3062627656/
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Addr { get; set; }
        public int Press { get; set; }

        //Progress bar image Properties
        public Image PressImg
        {
            get
            {
                Bitmap bmp = new Bitmap(104, 30); //Here, 104 is given to leave 2 pixels on the left and right, and the remaining 100 is the percentage value
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.White); //Fill in white background
                //g.FillRectangle(Brushes.Red, 2, 2, this.Press, 26);  //Normal effect
                //Fill gradient effect
                g.FillRectangle(new LinearGradientBrush(new Point(30, 2), new Point(30, 30), Color.Black, Color.Gray), 2, 2, this.Press, 26);
                return bmp;
            }
        }
    }
}

