using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamStore.classes
{
    class TaskManager
    {
        private static Dictionary<string, JTask> TaskSet;

        public static void init(){
            TaskSet = new Dictionary<string, JTask>();
        }

        //查詢該ID是否存在
        public static string checkTaskIDExists(string IP)
        {
            string result = "";
            try
            {
                foreach(JTask i in TaskSet.Values)
                {
                    if(i.source_ip == IP)
                    {
                        result = i.ID;
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return result;
            }
        }

        public static string getDownloadPath(string IP)
        {
            foreach (JTask i in TaskSet.Values)
            {
                if (i.source_ip == IP)
                {
                    return i.getDownloadPath();
                }
            }
            return "";
        }

        public static Boolean checkTaskIDExists2(string ID)
        {
            string result = "";
            try
            {
                return TaskSet.ContainsKey(ID) ? true : false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public static Dictionary<string, JTask> getTaskSet()                                                                                                                                                            
        {
            return TaskManager.TaskSet;
        }

        //查詢特定任務是否已設定其條件.
        public static Boolean checkTaskCondition(string IP)
        {
            string THEID = checkTaskIDExists(IP);
            return (THEID != "") ? TaskSet[THEID].CheckCondition(): false;
        }

        //取得線程狀況及數量
        public static string getProcessStatusAndCount()
        {
            int alive = 0;
            int stop = 0;
            int disconnect = 0;
            foreach (KeyValuePair<string, JTask> i in TaskSet)
            {
                if (i.Value.getState() == status.running) { 
                    alive += 1; 
                }
                else if(i.Value.getState() == status.disconnect || i.Value.getState() == status.start)
                {
                    disconnect += 1;
                }
                else
                {
                    stop += 1;
                }
            }
            return "閒置: " + stop.ToString() + " - 運行: " + alive.ToString() + " - 連線: " + disconnect.ToString();
        }


        public static List<string> getAcviateID()
        {
            List<string> ve = new List<string>();
            foreach(KeyValuePair<string, JTask> i in TaskSet)
            {
                ve.Add(i.Value.source_ip + " : " + i.Key); //用IP識別度比較高
            }
            return ve;
        }

        //加入任務
        public static string addTask(JTask _task, string ID = "")
        {
            try
            {
                if (ID == "")
                {
                    ID = Common_util.generalID(new int[] { 3, 8 }, '-');
                    while (checkTaskIDExists2(ID))
                    {
                        ID = Common_util.generalID(new int[] { 3, 8 }, '-');
                    }
                }
                _task.ID = ID;
                TaskSet.Add(ID, _task);
                return ID;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return "";
            }
        }

        //啟動特定設備執行任務
        public static void startTask(string IP)
        {
            string THEID = checkTaskIDExists(IP);
            if(THEID != "")
            {
                TaskSet[THEID].startTask();
            }
            else
            {
                Console.WriteLine("the host is not exists.");
            }
        }

        //啟動執行任務
        public static void startTask()
        {
            foreach (KeyValuePair<string, JTask> i in TaskSet)
            {
                i.Value.connect(); //與sensor連線
                i.Value.startTask();//開始任務
            }
        }

        //中斷特定設備執行任務
        public static void stopTask(string IP)
        {
            string THEID = checkTaskIDExists(IP);
            if (THEID != "")
            {
                TaskSet[THEID].stopTask();
            }
        }

        //中斷執行任務
        public static void stopTask()
        {
            foreach (KeyValuePair<string, JTask> i in TaskSet)
            {
                i.Value.connect(); //與sensor連線
                i.Value.stopTask();//開始任務
            }
        }

        //釋放任務
        public static void releaseTask(string IP)
        {
            foreach (JTask i in TaskSet.Values)
            {
                if (i.source_ip == IP)
                {
                    TaskSet[i.ID].Dispose();
                    TaskSet[i.ID] = null;
                    TaskSet.Remove(i.ID);
                    XmlHandler.RestoreXML(datatype.Task);
                }
            }
        }

        public static void Clear()
        {
            if (TaskSet.Count < 1) return;
            foreach (KeyValuePair<string, JTask> i in TaskSet)
            {
                if (i.Value.getState() == status.running)
                {
                    releaseTask(i.Key);
                }
            }
            TaskSet.Clear();
        }

        public static JTask getTask(string IP)
        {
            foreach (JTask i in TaskSet.Values)
            {
                if (i.source_ip == IP)
                {
                    return i;
                }
            }
            return null;
        }
    }
}
