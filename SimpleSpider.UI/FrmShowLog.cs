using System;

namespace SimpleSpider.UI
{
    public partial class FrmShowLog : FormBase
    {
        public FrmShowLog(string msg)
        {
            InitializeComponent();
            this.textBox1.Text = msg;
        }

        private void FrmShowLog_Load(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().StartsWith("<"))
            {
                this.webBrowser1.DocumentText = textBox1.Text;
                this.webBrowser1.Visible = true;
            }
        }

        private void 源码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.webBrowser1.Visible = false;
        }
    }
}
