using CefSharp;
using CefSharp.WinForms;
using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace SimpleSpider.UI
{
    public partial class FrmLogin : Form
    {
        private ProxyServer proxyServer;
        private ChromiumWebBrowser webBrowser;
        IBrowser browser;
        public Dictionary<string, string> Cookies { get; set; }
        Uri uri;

        void initWebBrowser(string url)
        {
            webBrowser = new ChromiumWebBrowser(url); //初始页面
            webBrowser.Dock = DockStyle.Fill;
            this.webBrowser.FrameLoadStart += new EventHandler<CefSharp.FrameLoadStartEventArgs>(async (s, e) =>
           {
               browser = webBrowser.GetBrowser();
               uri = new Uri(browser.MainFrame.Url);
               var obj = (await browser.MainFrame.EvaluateScriptAsync("document.cookie")).Result;
               Cookie = obj == null ? Cookie : obj.ToString();
               obj = (await browser.MainFrame.EvaluateScriptAsync("document.inputEncoding")).Result;
               Encoding = obj == null ? Encoding : obj.ToString().ToLower();
           });
            this.Controls.Add(webBrowser);
        }

        private Task ProxyServer_BeforeRequest(object sender, SessionEventArgs e)
        {

            return Task.Factory.StartNew(() =>
            {
                var str = "";
                var cookies = e.WebSession.Request.Headers.GetHeaders("Cookie");
                if (cookies != null)
                    foreach (var item in cookies)
                    {
                        str += item.Value + ";";
                    }
                str = str.TrimEnd(';');
                Cookies[e.WebSession.Request.RequestUri.Host] = str;
            });
        }

        public FrmLogin(string url)
        {
            Cookies = new Dictionary<string, string>();
            proxyServer = new ProxyServer();
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.BeforeRequest += ProxyServer_BeforeRequest;
            proxyServer.Start();
            proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);

            this.url = url;
            InitializeComponent();
        }

        string url { get; set; }
        public string Cookie { get; set; }
        public string Encoding { get; set; }
        public int ClientConnectionCount { get; private set; }
        public int ServerConnectionCount { get; private set; }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            initWebBrowser(url);
        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Cookies.ContainsKey(uri.Host))
                Cookie = this.Cookies[uri.Host];
            proxyServer.DisableAllSystemProxies();
            proxyServer.Dispose();
        }
    }
}
