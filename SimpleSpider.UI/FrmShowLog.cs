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
    public partial class FrmShowLog : FormBase
    {
        public FrmShowLog(string msg)
        {
            InitializeComponent();
            this.textBox1.Text = msg;
        }

        private void FrmShowLog_Load(object sender, EventArgs e)
        {

        }
    }
}
