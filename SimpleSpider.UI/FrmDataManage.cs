﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
namespace SimpleSpider.UI
{
    public partial class FrmDataManage : FormBase
    {
        private string tableName;
        private string connectionString;
        public static Form Instance;
        public class ArticleInfo
        {
            public int id { get; set; }
            public string title { get; set; }
            public string content { get; set; }
            public bool publish { get; set; }
            public string source { get; set; }
            public string tag { get; set; }
        }

        public FrmDataManage()
        {
            if (Instance != null)
                Instance.Close();
            Instance = this;
            InitializeComponent();
            //禁止用户改变DataGridView1所有行的行高
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
        }

        public void Bind()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var filter = string.IsNullOrEmpty(comboBox1.Text) ? null : " and tag='" + comboBox1.Text + "'";
                dataGridView1.DataSource = connection.Query<ArticleInfo>($"select top {numericUpDown1.Value} * from [{tableName}] where publish = 0 {filter} order by id desc");
                if (comboBox1.Items.Count == 0)
                {
                    var tags = connection.Query<string>($"select top {numericUpDown1.Value} tag from [{tableName}] where publish = 0 group by tag").ToList();
                    tags.Insert(0, "");
                    comboBox1.DataSource = tags;
                }
            }
        }

        private void FrmDataManage_Load(object sender, EventArgs e)
        {
            tableName = System.Configuration.ConfigurationManager.AppSettings["tableName"];
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            Bind();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bind();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Bind();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<ArticleInfo> data = new List<ArticleInfo>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                var article = (ArticleInfo)row.DataBoundItem;
                data.Add(article);
            }
            var frm = new FrmPublish(data) { Width = Width };
            frm.Show();
            frm.FormClosing += FrmPublish_FormClosing;
        }

        private void FrmPublish_FormClosing(object sender, FormClosingEventArgs e)
        {
            var frm = (FrmPublish)sender;
            if (frm.PublishedData.Count > 0)
                using (var connection = new SqlConnection(connectionString))
                {
                    var ids = "";
                    foreach (var item in frm.PublishedData)
                    {
                        ids += item.id + ",";
                    }
                    connection.Execute($"update {tableName} set publish=1 where id in ({ids.TrimEnd(',')})");
                }
            Bind();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                var article = (ArticleInfo)row.DataBoundItem;
                new FrmShowLog(article.content).Show();
                break;
            }
        }

        private void 发布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private void FrmDataManage_FormClosing(object sender, FormClosingEventArgs e)
        {
            Instance = null;
            FrmMain.Instance.WindowState = FormWindowState.Normal;
            FrmMain.Instance.Activate();
        }
    }
}
