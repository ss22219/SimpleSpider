using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Unreal.QCloud.Api;
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
            try
            {
                BeginInvoke(new Action(() =>
                    listView1.Items.Add(msg)
                ));
            }
            catch (Exception)
            {
            }
        }
        

        private async void btnPublish_Click(object sender, EventArgs e)
        {

            button2.Enabled = false;
            var publisher = (Publisher)comboBox1.SelectedItem;
            int i = 0;
            foreach (var item in Data)
            {
                var content = item.content;
                var title = item.title;
                if (cbReplaceWord.Checked)
                {
                    var wordserverUrl = ConfigurationManager.AppSettings["wordserver_url"];
                    try
                    {
                        var data = new System.Collections.Specialized.NameValueCollection();
                        data["content"] = content;
                        content = Encoding.UTF8.GetString(new WebClient().UploadValues(wordserverUrl, data));

                        data = new System.Collections.Specialized.NameValueCollection();
                        data["content"] = title;
                        title = Encoding.UTF8.GetString(new WebClient().UploadValues(wordserverUrl, data));
                    }
                    catch (Exception)
                    {
                        Log("连接伪原创服务器失败！");
                        return;
                    }
                }

                var result = await publisher.Publish(new Dictionary<string, string>() {
                    {"title",title },
                    {"content",content + txtAppend.Text }
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
            try
            {
                BeginInvoke(new Action(() => label2.Text = count + "/" + Data.Count));
            }
            catch (Exception)
            {
            }
        }

        private void FrmPublish_Load(object sender, EventArgs e)
        {
            if (ConfigurationManager.AppSettings["wordserver_url"] != null)
            {
                cbReplaceWord.Visible = true;
                cbReplaceWord.Checked = true;
            }
            SetProcess(0);
            comboBox1.DisplayMember = "Name";
            comboBox1.DataSource = UserConfig.Publishers.ToArray();

        }

        private void LoadArticalOptions(Publisher publisher)
        {
            var maxWidth = this.groupBox1.Width;
            this.groupBox1.Controls.Clear();
            int line = 0;
            var left = 20;
            foreach (var option in publisher.ArticalOptions)
            {
                var top = 20 + line * 30;
                Control control = null;
                if (option.OptionInputType == OptionInputType.Text)
                {
                    var label = new Label() { Text = option.DisplayName + ": ", Name = "lb" + option.Name, Top = top + 5, Left = left, Width = 100 };
                    var size = label.CreateGraphics().MeasureString(label.Text, label.Font);
                    label.Width = (int)size.Width + 5;

                    var textbox = new TextBox() { Text = option.Value, Name = option.Name, Top = top + 1, Left = label.Width + left, Width = 100 };
                    size = textbox.CreateGraphics().MeasureString(textbox.Text, textbox.Font);

                    textbox.Width = (int)size.Width + 10;
                    textbox.TextChanged += new EventHandler((sender, ev) =>
                    {
                        option.Value = textbox.Text;
                    });
                    control = textbox;
                    control.Tag = label;
                    this.groupBox1.Controls.Add(label);
                    this.groupBox1.Controls.Add(textbox);
                }
                if (option.OptionInputType == OptionInputType.File)
                {
                    var label = new Label() { Text = option.DisplayName + ": ", Name = "lb" + option.Name, Top = top + 5, Left = left, Width = 100 };
                    var size = label.CreateGraphics().MeasureString(label.Text, label.Font);
                    label.Width = (int)size.Width + 5;

                    var btn = new Button() { Text = "选择文件", Name = option.Name, Top = top + 1, Left = label.Width + left, Width = 100 };
                    size = btn.CreateGraphics().MeasureString(btn.Text, btn.Font);

                    btn.Width = (int)size.Width + 10;
                    btn.Click += new EventHandler(async (sender, ev) =>
                   {
                       option.Value = await UploadImage();
                   });
                    control = btn;
                    control.Tag = label;
                    this.groupBox1.Controls.Add(label);
                    this.groupBox1.Controls.Add(btn);
                }
                else if (option.OptionInputType == OptionInputType.CheckBox)
                {
                    var checkbox = new CheckBox() { Text = option.DisplayName, Checked = option.Value == "True", Name = option.Name, Top = top, Left = left, Width = 100 };
                    var size = checkbox.CreateGraphics().MeasureString(checkbox.Text, checkbox.Font);
                    checkbox.Width = (int)size.Width + 20;
                    checkbox.Click += new EventHandler((sender, ev) =>
                    {
                        option.Value = ((CheckBox)sender).Checked ? "True" : null;
                    });
                    control = checkbox;
                    this.groupBox1.Controls.Add(checkbox);
                }
                else if (option.OptionInputType == OptionInputType.Select)
                {
                    var combox = new ComboBox() { Text = option.Value, Name = option.Name, Top = top, Left = left, Width = 100 };
                    combox.DisplayMember = "Key";
                    combox.ValueMember = "Value";
                    combox.DataSource = option.SelectValues;
                    combox.SelectedIndexChanged += new EventHandler((sender, ev) =>
                    {
                        option.Value = ((ComboBox)sender).Text;
                    });
                    control = combox;
                    this.groupBox1.Controls.Add(combox);
                }
                if (control.Right > maxWidth)
                {
                    line++;
                    left = 20;

                    control.Top += 30;
                    control.Left = left;
                    if (control is TextBox)
                    {
                        var label = (Label)control.Tag;
                        label.Top += 30;
                        label.Left = left;
                        control.Left = label.Right;
                    }
                }
                left += control.Width + 20;
            }
            var old = this.groupBox1.Height;
            this.groupBox1.Height = 60 + line * 30;
            this.Height += this.groupBox1.Height - old;
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

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadArticalOptions((Publisher)((ComboBox)sender).SelectedValue);
        }

        private void FrmPublish_Resize(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
                comboBox1_SelectedValueChanged(comboBox1, null);
            this.listView1.Top = this.groupBox1.Bottom + 40;
            this.listView1.Height = this.Height - this.groupBox1.Bottom - 40;
            this.label2.Top = this.listView1.Top - 23;
        }

        async Task<string> UploadImage()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = "simplespider/" + Path.GetFileName(openFileDialog1.FileName);
                var appid = ConfigurationManager.AppSettings["appId"];
                var secretId = ConfigurationManager.AppSettings["secretId"];
                var secretKey = ConfigurationManager.AppSettings["secretKey"];
                var region = ConfigurationManager.AppSettings["region"];
                var bucketName = ConfigurationManager.AppSettings["bucketName"];
                var url = $"http://{bucketName}-{appid}.pic{region}.myqcloud.com/{path}";

                var api = new CosCloud(int.Parse(appid), secretId, secretKey, region);
                var stat = await api.GetFileStat(bucketName, path);
                if (stat.code != -197)
                {
                    if (MessageBox.Show("文件已经存在，是否直接插入？", "文件已经存在", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        return url;
                    }
                    else
                        return null;
                }
                using (var fs = File.Open(openFileDialog1.FileName, FileMode.Open))
                {
                    var result = await api.UploadFile(bucketName, path, fs);
                    if (result.code != 0)
                    {
                        return null;
                    }
                }
                return url;
            }
            return null;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var url = await UploadImage();
            if (!string.IsNullOrEmpty(url))
                txtAppend.Text += "<img data-append src=\"" + url + "\" />";
        }
    }
}
