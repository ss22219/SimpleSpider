﻿using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Unreal.QCloud.Api;
using static SimpleSpider.UI.FrmDataManage;

namespace SimpleSpider.UI
{
    public partial class FrmPublish : Form
    {
        public ChromiumWebBrowser webBrowser { get; set; }
        public List<ArticleInfo> Data { get; set; }
        public List<ArticleInfo> PublishedData = new List<ArticleInfo>();
        IBrowser browser;
        public FrmPublish(List<ArticleInfo> data)
        {
            this.Data = data;

            InitializeComponent();
            initWebBrowser();
        }

        void Log(string msg)
        {
            if (msg == null)
                throw new Exception();
            try
            {
                BeginInvoke(new Action(() =>
                    listBox1.Items.Add(msg)
                ));
            }
            catch (Exception)
            {
            }
        }

        void initWebBrowser()
        {
            webBrowser = new ChromiumWebBrowser(new Uri(Directory.GetCurrentDirectory() + "/umeditor/index.html").ToString()); //初始页面

            webBrowser.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
            webBrowser.Location = new System.Drawing.Point(1, 53);
            webBrowser.Size = new System.Drawing.Size(groupBox2.Size.Width, 167);
            this.webBrowser.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>((s,e)=> {
                browser = this.webBrowser.GetBrowser();
                setAppendContent(_setContent);
            });

            this.groupBox2.Controls.Add(webBrowser);
        }

        string getAppendContent()
        {
            if (browser == null)
                return _setContent;
            var task = browser.MainFrame.EvaluateScriptAsync("getContent()");
            task.Wait();
            if (task.Result == null || !task.Result.Success)
                return _setContent;
            return task.Result.Result.ToString();
        }

        string _setContent;
        void setAppendContent(string content, bool append = false)
        {
            if (content == null)
                content = "";
            if (_setContent == null)
                _setContent = content;
            if (browser != null)
            {
                var script = $"var content = ({JsonConvert.SerializeObject(new { content = content })}).content;setContent(content)";
                browser.MainFrame.ExecuteJavaScriptAsync(script);
            }
        }

        void Save()
        {
            var publisher = (Publisher)cbPublisher.SelectedItem;
            if (publisher == null)
                return;
            var appendContent = getAppendContent();
            if (appendContent == null)
                return;
            btnPublish.Enabled = false;

            if (!UserConfig.UIOptions.ContainsKey(publisher))
                UserConfig.UIOptions[publisher] = new Dictionary<string, string>();
            UserConfig.UIOptions[publisher]["imgAlt"] = txtImgAlt.Text;
            UserConfig.UIOptions[publisher]["append"] = appendContent;
            UserConfig.Save();
        }

        private async void btnPublish_Click(object sender, EventArgs e)
        {
            var publisher = (Publisher)cbPublisher.SelectedItem;
            if (publisher == null)
                return;

            btnPublish.Enabled = false;
            Save();

            if (publisher.ArticalOptions.Any(o => o.Required && string.IsNullOrEmpty(o.Value)))
            {
                MessageBox.Show("发布选项 " + publisher.ArticalOptions.FirstOrDefault(o => o.Required && string.IsNullOrEmpty(o.Value)).DisplayName + " 不能为空");
                return;
            }
            int i = 0;
            foreach (var item in Data)
            {
                int count = 0;
                Start:
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
                var appendContent = getAppendContent();
                content = content + appendContent;
                content = SetImgAlt(content);
                var result = await publisher.Publish(new Dictionary<string, string>() {
                    {"title",title },
                    {"content",content}
                });
                if (!result.Success)
                {
                    Log("发布错误 \r\n title:" + title + "\r\ncontent:" + content + "\r\n" + result.Message);
                    if (result.Message.IndexOf("标题不能为空") != -1 && !string.IsNullOrWhiteSpace(title) && count < 3)
                    {
                        Log("尝试重新发布");
                        count++;
                        goto Start;
                    }
                }
                else
                {
                    PublishedData.Add(item);
                    Log(" 发布成功：" + item.title);
                }
                i++;
                SetProcess(i);
                if (numTimeLimit.Value > 0)
                {
                    Log(" 等待" + numTimeLimit.Value + "分钟");
                    await Task.Delay(TimeSpan.FromMinutes((double)numTimeLimit.Value));
                }
            }
            this.btnPublish.Enabled = true;
            Log("发布结束");
        }

        private string SetImgAlt(string content)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(content);
            foreach (var img in htmlDoc.DocumentNode.Descendants("img"))
            {
                img.SetAttributeValue("alt", txtImgAlt.Text);
            }
            return htmlDoc.DocumentNode.OuterHtml;
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
            cbPublisher.DisplayMember = "Name";
            cbPublisher.DataSource = UserConfig.Publishers.ToArray();
            if (lastPublisher != null && UserConfig.Publishers.Any(l => l == lastPublisher))
                cbPublisher.SelectedIndex = UserConfig.Publishers.IndexOf(lastPublisher);
            lastPublisher = null;
            FrmPublish_Resize(sender, e);
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
                    left += label.Width;
                    var textbox = new TextBox() { Text = option.Value, Name = option.Name, Top = top + 1, Left = left, Width = 100 };
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
                else if (option.OptionInputType == OptionInputType.RemoteMatch)
                {
                    var label = new Label() { Text = option.DisplayName + ": ", Name = "lb" + option.Name, Top = top + 5, Left = left, Width = 100 };
                    var size = label.CreateGraphics().MeasureString(label.Text, label.Font);
                    label.Width = (int)size.Width + 5;

                    left += label.Width;
                    var combox = new ComboBox() { Text = option.Value, Name = option.Name, Top = top, Left = left, Width = 100 };
                    combox.DisplayMember = "Key";
                    combox.ValueMember = "Value";
                    combox.Click += new EventHandler((sender, ev) =>
                    {
                        var val = combox.Text;
                        if (combox.DataSource == null)
                        {
                            var option1 = publisher.ArticalOptions.Where(o => o.Name == combox.Name).FirstOrDefault();
                            var url = ReplaceParam(option1.SelectValues[0].Key);
                            var regexRule = option1.SelectValues[0].Value;
                            if (!url.StartsWith("http"))
                            {
                                MessageBox.Show("地址不正确");
                                return;
                            }
                            var client = new WebClient();
                            client.Headers["Cookie"] = GetOption("Cookie").Value;
                            try
                            {
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
                                combox.DataSource = selectValues;
                                if (!string.IsNullOrEmpty(val))
                                    combox.SelectedValue = val;
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("获取栏目失败，可能Cookie已经失效");
                                return;
                            }
                        }
                    });

                    combox.SelectedIndexChanged += new EventHandler((sender, ev) =>
                    {
                        var option1 = GetOption(combox.Name);
                        option.Value = combox.SelectedValue.ToString();
                    });
                    combox.Tag = label;
                    control = combox;
                    this.groupBox1.Controls.Add(label);
                    this.groupBox1.Controls.Add(combox);
                }

                if (control.Right > maxWidth)
                {
                    line++;
                    left = 20;

                    control.Top += 30;
                    control.Left = left;
                    if (control.Tag != null)
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

        private Option GetOption(string name)
        {
            var publisher = (Publisher)cbPublisher.SelectedValue;
            var option = publisher.Options.Where(p => p.Name == name).FirstOrDefault();
            if (option == null)
                option = publisher.ArticalOptions.Where(p => p.Name == name).FirstOrDefault();
            return option;
        }

        private string ReplaceParam(string express)
        {
            var publisher = (Publisher)cbPublisher.SelectedValue;
            if (express == null)
                return string.Empty;
            foreach (var item in publisher.Options)
            {
                if (express.IndexOf("{" + item.Name + "}") != -1)
                    express = express.Replace("{" + item.Name + "}", item.Value);
            }
            return express;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new FrmSiteList();
            frm.Show();
            frm.FormClosing += FrmSiteSetting_FormClosing;
        }

        private void FrmSiteSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            cbPublisher.DataSource = UserConfig.Publishers.ToArray();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            var publisher = (Publisher)((ComboBox)sender).SelectedValue;
            if (UserConfig.UIOptions.ContainsKey(publisher))
            {
                if (UserConfig.UIOptions[publisher].ContainsKey("imgAlt"))
                    txtImgAlt.Text = UserConfig.UIOptions[publisher]["imgAlt"];
                if (UserConfig.UIOptions[publisher].ContainsKey("imgAlt"))
                    setAppendContent(UserConfig.UIOptions[publisher]["append"]);
            }
            LoadArticalOptions(publisher);
        }

        private void FrmPublish_Resize(object sender, EventArgs e)
        {
            if (this.Height < 100)
                return;
            if (cbPublisher.SelectedValue != null)
            {
                var publisher = (Publisher)cbPublisher.SelectedValue;
                LoadArticalOptions(publisher);
            }
            this.groupBox1.Top = this.label2.Top - this.groupBox1.Height - 10;
            this.groupBox2.Height = this.groupBox1.Top - this.groupBox2.Top - 10;
            this.webBrowser.Height = this.groupBox2.Height - 60;
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
                setAppendContent("<img data-append src=\"" + url + "\" />", true);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
                new FrmShowLog(listBox1.SelectedItems[0].ToString()).Show();
        }

        public static Publisher lastPublisher;
        private void FrmPublish_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cbPublisher.SelectedValue != null)
                lastPublisher = (Publisher)cbPublisher.SelectedValue;
            Save();
            if (FrmDataManage.Instance != null)
            {
                if (FrmDataManage.Instance.WindowState == FormWindowState.Minimized)
                    FrmDataManage.Instance.WindowState = FormWindowState.Normal;
                FrmDataManage.Instance.Activate();
            }
        }
    }
}
