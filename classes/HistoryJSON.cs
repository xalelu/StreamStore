using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace streamStore.classes
{
    class HistoryJSON
    {
        public static Dictionary<string, HTask> HistoryTask = new Dictionary<string, HTask>();
        public static string historyPath = System.Environment.CurrentDirectory + "\\history.txt";
        public static void Init()
        {
            //產生紀錄History的紀錄檔
            //程式啟動, 即物件化"歷史紀錄"json檔
            if(!File.Exists(historyPath))
            {//若沒有該檔案, 則自動產生
                FileStream fs = File.Create(historyPath);
                fs.Close();
            }
        }
        
        //封裝(刪除)的任務
        public static void wrapTask(JTask _task)
        {
            StringBuilder HTask = new StringBuilder();
            HTask.Append("任務ID: " + _task.ID + "\n");
            HTask.Append("IP/PORT: " + _task.source_ip + ": " + _task.source_port.ToString() + "\n");
            HTask.Append("建立時間: " + _task.AddTime.ToString("yyyy/MM/dd HH:mm:ss") + "\n");
            HTask.Append("結束時間: " + _task.endTime.ToString("yyyy/MM/dd HH:mm:ss") + "\n");
            HTask.Append("完成輸出: -------------------------------------------------\n");
            foreach(logTask i in _task.htaskList)
            {
                HTask.Append("  任務開始時間: " + i.startTime + "\n");
                HTask.Append("  任務結束時間: " + i.endTime + "\n");
                HTask.Append("  間 隔: " + i.interval + "\n");
            }
            HTask.Append("----------------------------------------------------------------- \n");
        }
    }

    class HTask
    {
        public string ID;
        public DateTime addTime;
        public string ip;
        public string port;
        public List<logTask> tasklog;
        public DateTime endTime;
        public HState state;
        public HTask(string ID, DateTime addTime, string ip, string port)
        {
            this.ID = ID;
            this.addTime = addTime;
            this.ip = ip;
            this.port = port;
            this.tasklog = new List<logTask>();
        }
        //加入log紀錄(完成下載才加入logTask)
        public void AddlogTask(logTask logT)
        {

        }
        //封裝
        public void wrap()
        {
            this.state = HState.wrap;
            this.endTime = DateTime.Now;
        }
    }

    class logTask
    {
        public string startTime;
        public string endTime;
        public string interval;
        public logTask(string startTime, string endTime, string interval)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.interval = interval;
        }
    }

    enum HState
    {
        wrap = 0,
        alive = 1
    }
}
