using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace streamStore.classes
{
    class RecordHistory
    {
        //封裝紀錄任務
        public static void wrapRecord(JTask task)
        {

        }

        //讀取已被紀錄的歷史任務
        public static HistoryTask getHistory()
        {
            return null;
        }
    }

    //ID, IP, PORT, AddTime, Task<strint, end> 
    //[{"id":"任務ID", "ip":"來源ip", "port":"來源port", "addTime":"加入時間", "endTime":"刪除時間", "actions":[{"start":"開始時間", "end":"結束時間", "interval":"間隔"},..]
    //存成JSON檔
    class HistoryTask
    {

    }
}
