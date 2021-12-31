using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamStore.classes
{
    class MyEventArgs
    {
        public string deviceID;
        public status st;
        public string descript;
        public int code;

        public MyEventArgs(string deviceIP, status st, string descript = "", int code = 0)
        {
            this.deviceID = deviceIP;
            this.st = st;
            this.descript = descript;
            this.code = code;
        }
    }

    //更新下載時間(進度)
    class updateTimeevgArgs
    {
        public string msg;
        public string processStr;
        public updateTimeevgArgs(string msg, string processStr)
        {
            this.msg = msg;
            this.processStr = processStr;
        }
    }

    //用法點擊"中斷"按鍵, 發出的事件
    class InteruptionEvent: Exception
    {
        public string _Message;
        public InteruptionEvent(string myMsg)
        {
            this._Message = myMsg;
        }
    }
}
