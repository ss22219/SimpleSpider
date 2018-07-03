using Newtonsoft.Json;
using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleSpider.UI
{
    public partial class FrmPublisherSetting : Form
    {
        Publisher publisher;
        Publisher publisherBack;
        public Publisher Publisher;

        public FrmPublisherSetting(Publisher publisher)
        {
            InitializeComponent();
            this.Text = publisher.PublisherName + " - " + publisher.Name + " 发布配置";
            int row = 0;

            var newPublisher = UserConfig.CreatePublisher(publisher.PublisherName);
            newPublisher.Name = publisher.Name;
            newPublisher.Options = JsonConvert.DeserializeObject<List<Option>>(JsonConvert.SerializeObject(publisher.Options));
            this.publisher = newPublisher;
            this.publisherBack = publisher;

            foreach (var option in this.publisher.Options)
            {
                var top = 35 + (row * 30);
                var label = new Label() { Text = option.DisplayName, Top = top, Left = 0 };
                label.TextAlign = ContentAlignment.MiddleRight;
                if (option.OptionInputType == OptionInputType.Text)
                {
                    var textbox = new TextBox() { Text = option.Value, Name = option.Name, Top = top, Left = 120, Width = 200 };
                    textbox.TextChanged += Textbox_TextChanged;
                    this.Controls.Add(textbox);
                }
                else if (option.OptionInputType == OptionInputType.Cookie)
                {
                    var textbox = new TextBox() { Text = option.Value, Name = option.Name, Top = top, Left = 120, Width = 150 };
                    textbox.TextChanged += Textbox_TextChanged;
                    this.Controls.Add(textbox);
                    var button = new Button() { Text = "获取", Name = "btn" + option.Name, Top = top - 1, Left = 270, Width = 50 };
                    button.Click += LoginButton_Click;
                    this.Controls.Add(button);
                }
                else if (option.OptionInputType == OptionInputType.Select)
                {
                    var combox = new ComboBox() { Text = option.Value, Name = option.Name, Top = top, Left = 120, Width = 200 };
                    combox.DisplayMember = "Key";
                    combox.ValueMember = "Value";
                    combox.DataSource = option.SelectValues;
                    combox.SelectedIndexChanged += Combox_SelectedIndexChanged;
                    this.Controls.Add(combox);
                }
                else if (option.OptionInputType == OptionInputType.RemoteMatch)
                {
                    var combox = new ComboBox() { Text = option.Value, Name = option.Name, Top = top, Left = 120, Width = 200 };
                    combox.DisplayMember = "Key";
                    combox.ValueMember = "Value";
                    combox.Click += Combox_Click;
                    combox.SelectedIndexChanged += Combox_SelectedIndexChanged;
                    this.Controls.Add(combox);
                }
                this.Controls.Add(label);
                row++;
            }
            this.Height = 110 + (row * 30);
            this.Width = 400;
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            var name = ((Button)sender).Name.Replace("btn", "");
            var frm = new FrmLogin(ReplaceParam(GetOption(name).SelectValues[0].Value)) { Cookie = this.Controls[name].Text, Text = name };
            frm.Show();
            frm.FormClosing += FrmLogin_FormClosing;
        }

        private void FrmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            var frm = (FrmLogin)sender;
            var name = frm.Text;
            if (!string.IsNullOrEmpty(frm.Cookie))
            {
                this.Controls[name].Text = frm.Cookie;
            }
            if (!string.IsNullOrEmpty(frm.Encoding))
            {
                if (this.Controls.ContainsKey("Encoding"))
                    this.Controls["Encoding"].Text = frm.Encoding;
            }
        }

        private void Textbox_TextChanged(object sender, EventArgs e)
        {
            var box = ((TextBox)sender);
            var option = GetOption(box.Name);
            option.Value = box.Text;
        }

        private void Combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            var option = GetOption(box.Name);
            option.Value = box.SelectedValue.ToString();
        }

        private string ReplaceParam(string express)
        {
            if (express == null)
                return string.Empty;
            foreach (var item in publisher.Options)
            {
                if (express.IndexOf("{" + item.Name + "}") != -1)
                    express = express.Replace("{" + item.Name + "}", item.Value);
            }
            return express;
        }

        private Option GetOption(string name)
        {
            return publisher.Options.Where(p => p.Name == name).FirstOrDefault();
        }

        private void Combox_Click(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            var val = box.Text;
            if (box.DataSource == null)
            {
                var option = publisher.Options.Where(o => o.Name == box.Name).FirstOrDefault();
                var url = ReplaceParam(option.SelectValues[0].Key);
                var regexRule = option.SelectValues[0].Value;
                if (!url.StartsWith("http"))
                {
                    MessageBox.Show("地址不正确");
                    return;
                }
                var client = new WebClient();
                client.Headers["Cookie"] = GetOption("Cookie").Value;
                var data = client.DownloadData(ReplaceParam(url));
                var encodingOption = GetOption("Encoding");
                var encoding = Encoding.GetEncoding(encodingOption == null ? "utf-8" : encodingOption.Value);
                var content = encoding.GetString(data);
                List<KeyValuePair<string, string>> selectValues = new List<KeyValuePair<string, string>>();
                if (!new Regex(regexRule).IsMatch(content))
                {
                    MessageBox.Show("获取栏目失败，可能Cookie已经失效");
                    return;
                }
                foreach (Match item in new Regex(regexRule).Matches(content))
                {
                    selectValues.Add(new KeyValuePair<string, string>(item.Groups["Name"].Value, item.Groups["Value"].Value));
                }
                box.DataSource = selectValues;
                if (!string.IsNullOrEmpty(val))
                    box.SelectedValue = val;
            }
        }

        private void FrmPublisherSetting_Load(object sender, EventArgs e)
        {
            txtName.Text = publisher.Name;
            foreach (var option in publisher.Options)
            {
                this.Controls[option.Name].Text = option.Value;
            }
            btnSave.Left = this.Width / 2 - btnSave.Width / 2;
            btnSave.Top = this.Height - 70;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (var option in publisher.Options)
            {
                if (string.IsNullOrEmpty(option.Value))
                {
                    MessageBox.Show(option.DisplayName + " 不能为空！");
                    return;
                }
            }

            publisherBack.Options = publisher.Options;
            publisherBack.Name = publisher.Name;
            Publisher = publisherBack;
            Close();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            publisher.Name = txtName.Text;
        }

        private void FrmPublisherSetting_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btnSave_Click(null, null);
            else if (e.KeyChar == '\u001b')
                Close();
        }
    }
}
