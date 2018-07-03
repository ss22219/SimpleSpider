using SimpleSpider.Publish;
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
    public partial class FrmCreateSiteSetting : Form
    {
        public Publisher Publisher;
        public FrmCreateSiteSetting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!UserConfig.PublisherTypes.ContainsKey(comboBox1.Text))
            {
                MessageBox.Show("采集模块不存在");
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("站点名称不能为空");
                return;
            }
            var publisher = UserConfig.CreatePublisher(comboBox1.Text);
            publisher.Name = textBox1.Text;
            this.Publisher = publisher;
            Close();
        }

        private void FrmCreateSiteSetting_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = UserConfig.PublisherTypes.Select(p => p.Key).ToList();
        }

        private void FrmCreateSiteSetting_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                button1_Click(null, null);
        }
    }
}
