using System;
using System.Windows.Forms;
using streamStore.classes;

namespace streamStore
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        public static string Version = "V1.01";
        public static SystemConfig SC;
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles(); //�Ұʹw�]�����˦�
            Application.SetCompatibleTextRenderingDefault(false);

            TaskManager.init();
            Application.Run(new Form1());
        }
    }
}
