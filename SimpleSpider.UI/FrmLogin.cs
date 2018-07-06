using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleSpider.UI
{
    public partial class FrmLogin : FormBase
    {
        public FrmLogin(string url)
        {
            this.url = url;
            InitializeComponent();
        }

        string url { get; set; }
        public string Cookie { get; set; }
        public string Encoding { get; set; }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            this.webBrowser1.Url = new Uri(url);
        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cookie = webBrowser1.Document.Cookie;
            Encoding = webBrowser1.Document.Encoding;
        }
    }
}
