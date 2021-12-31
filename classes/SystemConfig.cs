using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamStore.classes
{
    class SystemConfig
    {
        public int handleUnit; //分段的處理
        public string downloadPath; //下載的位置
        public Boolean deleteTaskAuto; //自動刪除任務
        public Boolean recordTask; //紀錄任務
        public Boolean startToRun; //啟動即運行(Task未完成的任務)

        public SystemConfig(int handleUnit = 12, string downloadPath = "", Boolean deleteTaskAuto = false, Boolean recordTask = false, Boolean startToRun = false)
        {
            this.handleUnit = handleUnit;
            this.downloadPath = (downloadPath == "") ? System.Environment.CurrentDirectory : downloadPath;
            this.deleteTaskAuto = deleteTaskAuto; 
            this.recordTask = recordTask;
            this.startToRun = startToRun;
        }
    }
}
