using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace streamStore.classes
{
    class CSV_handler
    {
        public JTask source;//建置csv的來源

        public CSV_handler(ref JTask source)//自CSV來切入
        {
            this.source = source; //指定觸發來源.
        }

        //table資料寫入csv
        public void SaveCSV(System.Data.DataTable dt, string fullPath)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            System.IO.FileStream fs = new System.IO.FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            string data = "";
  
            for (int i = 0; i<dt.Columns.Count; i++)//寫入列名
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i<dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            for (int i = 0; i < dt.Rows.Count; i++) //寫入各行資料
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");//替換英文冒號 英文冒號需要換成兩個冒號
                    if (str.Contains(',') || str.Contains('"') || str.Contains('\r') || str.Contains('\n')) //含逗號 冒號 換行符的需要放到引號中
                    {
                        str = string.Format("\"{0}\"", str);
                    }

                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }    


        //寫入資料
        //參考:https://weichiencoding.medium.com/c-vs2017-microsoft-office-interop-excel-%E7%94%A8%E6%B3%95-7a788e3d5b37
        //參考(快速寫入Excel): https://www.itread01.com/content/1548609326.html
    }
}
