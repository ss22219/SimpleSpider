using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Publish.DedeCMS
{
    public class DedeCMSPublisher : Publisher
    {
        List<Option> _options;

        public override List<Option> Options
        {
            get
            {
                if (_options == null)
                    initializationOptions();
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        public override string PublisherName
        {
            get
            {
                return "DedeCMS";
            }
        }

        private void initializationOptions()
        {
            _options = new List<Option>() {
                new Option() { Name = "SiteUrl",DisplayName="后台地址", OptionInputType= OptionInputType.Text},
                new Option() { Name = "Encoding", DisplayName = "编码", Value="utf-8", OptionInputType= OptionInputType.Select, SelectValues = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("utf-8","utf-8"),
                    new KeyValuePair<string, string>("gb2312","gb2312")
                } },
                new  Option() {Name= "Cookie", DisplayName = "Cookie" , OptionInputType = OptionInputType.Cookie, SelectValues = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("Url", "{SiteUrl}")
                } },
                new  Option() {Name= "TypeId", DisplayName = "栏目ID", OptionInputType= OptionInputType.RemoteMatch, SelectValues =new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("{SiteUrl}/makehtml_list.php", @"<option value='(?<Value>\d+)'>(?<Name>[^<>]+)</option>"),
                }}
            };
        }

        HttpContent GetPostData(Encoding encoding, List<KeyValuePair<string, string>> list)
        {
            var postData = new MultipartFormDataContent();
            foreach (var item in list)
            {
                postData.Add(new ByteArrayContent(encoding.GetBytes(item.Value)), item.Key);
            }
            return postData;
        }

        public override async Task<PublishResult> Publish(Dictionary<string, string> data)
        {
            var encoding = Encoding.GetEncoding(GetOptionValue("Encoding"));
            var list = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("title", data["title"]),
                new KeyValuePair<string, string>("typeid", GetOptionValue("TypeId")),
                new KeyValuePair<string, string>("body", data["content"]),
                new KeyValuePair<string, string>("channelid","1"),
                new KeyValuePair<string, string>("dopost","save"),
                new KeyValuePair<string, string>("pubdate", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")),
                new KeyValuePair<string, string>("imageField.x", "10"),
                new KeyValuePair<string, string>("imageField.y", "12"),
                new KeyValuePair<string, string>("remote", "1"),
                new KeyValuePair<string, string>("dellink", "1"),
                new KeyValuePair<string, string>("autolitpic", "1"),
                new KeyValuePair<string, string>("needwatermark", "1"),
                new KeyValuePair<string, string>("sptype", "hand"),
                new KeyValuePair<string, string>("notpost", "0"),
                new KeyValuePair<string, string>("click", new Random().Next(100,999).ToString()),
            };
            var postData = GetPostData(encoding, list);

            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = false
            };
            var client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.AcceptCharset.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(GetOptionValue("Encoding")));
            client.DefaultRequestHeaders.Add("Cookie", GetOptionValue("Cookie"));
            try
            {
                var result = await client.PostAsync(GetOptionValue("SiteUrl") + "/article_add.php", postData);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return Result(false, "网络提交失败");
                }
                var bytes = await result.Content.ReadAsByteArrayAsync();
                var content = encoding.GetString(bytes);
                if (content.IndexOf("登录") != -1)
                {
                    return Result(false, "登陆已经过期");
                }
                else if (content.IndexOf("成功发布文章") == -1)
                {
                    return Result(false, content);
                }

                while (content.IndexOf("task_do.php") != -1)
                {
                    var url = GetOptionValue("SiteUrl") + "/" + new Regex(@"task_do\.php[^<>']+").Match(content).Groups[0].Value;
                    result = await client.GetAsync(url);
                    if (result.StatusCode != HttpStatusCode.OK)
                        content = null;
                    bytes = await result.Content.ReadAsByteArrayAsync();
                    content = encoding.GetString(bytes);
                }
            }
            catch (Exception)
            {
                return Result(false, "网络提交失败");
            }
            return Result(true);
        }
    }
}
