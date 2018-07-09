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
                initCef();

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

        static void initCef() {

            ///设置
            var setting = new CefSharp.CefSettings();
            setting.Locale = "zh-CN";
            setting.CachePath = "CHBrowser/BrowserCache";//缓存路径
            setting.AcceptLanguageList = "zh-CN,zh;q=0.8";//浏览器引擎的语言
            setting.LocalesDirPath = "CHBrowser/localeDir";//日志
            setting.LogFile = "CHBrowser/LogData";//日志文件
            setting.PersistSessionCookies = true;//
            setting.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";//浏览器内核
            setting.UserDataPath = "CHBrowser/userData";//个人数据
            ///初始化
            CefSharp.Cef.Initialize(setting);
        }
    }
}
