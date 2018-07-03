using Newtonsoft.Json;
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
    public partial class FrmSiteList : Form
    {
        public FrmSiteList()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Edit((Publisher)dataGridView1.Rows[e.RowIndex].DataBoundItem);
        }

        void Edit(Publisher publisher)
        {
            var frm = new FrmPublisherSetting(publisher);
            frm.Show();
            frm.FormClosing += FrmPublish_FormClosing;
        }

        private void FrmPublish_FormClosing(object sender, FormClosingEventArgs e)
        {
            Bind();
            UserConfig.Save();
        }

        void Bind()
        {
            this.dataGridView1.DataSource = UserConfig.Publishers.ToArray();
        }

        private void 添加RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new FrmCreateSiteSetting();
            frm.Show();
            frm.FormClosing += FrmCreateSite_FormClosing;
        }

        private void FrmCreateSite_FormClosing(object sender, FormClosingEventArgs e)
        {
            var frm = (FrmCreateSiteSetting)sender;
            if (frm.Publisher != null)
            {
                UserConfig.Publishers.Add(frm.Publisher);
                Bind();
                UserConfig.Save();
            }
        }

        private void 修改TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Edit((Publisher)dataGridView1.SelectedRows[0].DataBoundItem);
        }

        private void 删除CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserConfig.Publishers.Remove((Publisher)dataGridView1.SelectedRows[0].DataBoundItem);
            Bind();
            UserConfig.Save();
        }

        private void FrmSiteList_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;

            //禁止用户改变DataGridView1所有行的行高
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            Bind();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var publisher = (Publisher)dataGridView1.SelectedRows[0].DataBoundItem;
                var newPublisher = UserConfig.CreatePublisher(publisher.PublisherName);
                newPublisher.Name = publisher.Name + " 复制";
                newPublisher.Options = JsonConvert.DeserializeObject<List<Option>>(JsonConvert.SerializeObject(publisher.Options));
                UserConfig.Publishers.Add(newPublisher);
                Bind();
                UserConfig.Save();
            }
        }
    }
}
