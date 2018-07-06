using SimpleSpider.Publish.DedeCMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleSpider.UI
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool ret;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out ret);
            if (ret)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.Run(new FrmMain());
                mutex.ReleaseMutex();
            }
            else
            {
                var hwnd = Win32API.FindWindow(null, "发布中心");
                Win32API.ShowWindow(hwnd, 1);
                Win32API.SetForegroundWindow(hwnd);
                Application.Exit();//退出程序   
            }
        }
    }
}
