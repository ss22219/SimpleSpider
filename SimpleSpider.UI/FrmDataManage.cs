using System;
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
    public partial class FrmDataManage : Form
    {
        private string tableName;
        private string connectionString;

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
            InitializeComponent();
        }

        void Bind()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var filter = string.IsNullOrEmpty(comboBox1.SelectedText) ? null : " and tag='" + comboBox1.SelectedText + "'";
                dataGridView1.DataSource = connection.Query<ArticleInfo>($"select top {numericUpDown1.Value} * from [{tableName}] where publish = 0 {filter}");
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
            var frm = new FrmPublish(data) { Width = Width};
            frm.ShowDialog();
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
    }
}
