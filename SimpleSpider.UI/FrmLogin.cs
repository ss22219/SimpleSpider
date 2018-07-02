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
    public partial class FrmLogin : Form
    {
        public FrmLogin(string url)
        {
            InitializeComponent();
            this.webBrowser1.Url = new Uri(url);
        }

        public string Cookie { get; set; }

        private void FrmLogin_Load(object sender, EventArgs e)
        {

        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cookie = webBrowser1.Document.Cookie;
        }
    }
}
