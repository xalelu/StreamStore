using streamStore.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace streamStore
{
    public partial class IOForm : System.Windows.Forms.Form
    {
        public int commandIndex = 0; //指令順序號 20201228 sam add
        public bool recorddemine = false; //20211116 sam add(設定是否紀錄request/response console)
        public List<string> taskids;
        public IOForm()
        {
            InitializeComponent();
            taskids = new List<string>();
        }
        
        public void Show()
        {
            checkTarget.Items.Clear();
            taskids = TaskManager.getAcviateID();
            checkTarget.Items.AddRange(taskids.ToArray());
        }

        //修改讀取紀錄
        private void checkTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            request_record.Clear();
            response_record.Clear();
            commandIndex = 0;
        }

        //視窗關閉
        public void close(Object sensor, EventArgs e)
        {
            this.request_record.Clear();
            this.response_record.Clear();
            this.commandIndex = 0; //將指令順序置歸零
            this.recorddemine = false;
        }

        //按鍵處理
        private void IO_btn_Click(object sender, EventArgs e)
        {
            Button target = (Button)sender;
            switch (target.Name)
            {
                case "stop_btn":
                    if (target.Text == "暫停紀錄")
                    {
                        target.Text = "啟動紀錄";
                        recorddemine = false;
                    }
                    else
                    {
                        target.Text = "暫停紀錄";
                        recorddemine = true;
                    }
                    break;

                case "clear_btn":
                    request_record.Clear();
                    response_record.Clear();
                    commandIndex = 0; //將指令順序置歸零
                    stop_btn.Text = "暫停紀錄";
                    recorddemine = true;
                    break;
            }
        }
    }
}
