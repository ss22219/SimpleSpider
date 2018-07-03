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
using static SimpleSpider.UI.FrmDataManage;

namespace SimpleSpider.UI
{
    public partial class FrmPublish : Form
    {
        public List<ArticleInfo> Data { get; set; }
        public List<ArticleInfo> PublishedData = new List<ArticleInfo>();
        public FrmPublish(List<ArticleInfo> data)
        {
            this.Data = data;
            InitializeComponent();
        }

        void Log(string msg)
        {
            BeginInvoke(new Action(() =>
                listView1.Items.Add(msg)
            ));
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            var publisher = (Publisher)comboBox1.SelectedItem;
            int i = 0;
            foreach (var item in Data)
            {
                var result = await publisher.Publish(new Dictionary<string, string>() {
                    {"title",item.title },
                    {"content",item.content }
                });
                Log(result.Message);
                if (!result.Success)
                {
                    Log("发布错误");
                    return;
                }
                else
                {
                    PublishedData.Add(item);
                    Log(" 发布成功：" + item.title);
                    i++;
                    SetProcess(i);
                }
            }
            Log("发布结束");
            Close();
        }

        private void SetProcess(int count)
        {
            BeginInvoke(new Action(() => label2.Text = count + "/" + Data.Count));
        }

        private void FrmPublish_Load(object sender, EventArgs e)
        {
            SetProcess(0);
            comboBox1.DisplayMember = "Name";
            comboBox1.DataSource = UserConfig.Publishers.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new FrmSiteList();
            frm.Show();
            frm.FormClosing += Frm_FormClosing;
        }

        private void Frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            comboBox1.DataSource = UserConfig.Publishers.ToArray();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                new FrmShowLog(listView1.SelectedItems[0].Text).Show();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
        }
    }
}
