using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Publish.Blogs
{
    public class SinaPublisher : Publisher
    {
        List<Option> _options;
        List<Option> _articalOptions;

        public override List<Option> ArticalOptions
        {
            get
            {
                if (_articalOptions == null)
                    initializationArticalOptions();
                return _articalOptions;
            }

            set
            {
                _articalOptions = value;
            }
        }

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
                return "新浪博客";
            }
        }
        private void initializationOptions()
        {
            _options = new List<Option>() {
                new  Option() {Name= "Cookie", DisplayName = "Cookie" , OptionInputType = OptionInputType.Cookie, SelectValues = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("Url", "http://control.blog.sina.com.cn/admin/article/article_add.php")
                } }
            };
        }


        private void initializationArticalOptions()
        {
            _articalOptions = new List<Option>() {
                new  Option() {Name= "blog_class", DisplayName = "分类ID", OptionInputType= OptionInputType.RemoteMatch, Required = true, SelectValues = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("http://control.blog.sina.com.cn/admin/article/article_add.php", @"<option value=""(?<Value>\d+)"" >(?<Name>[^<>]+)</option>"),
                }}};
        }

        HttpContent GetPostData(List<KeyValuePair<string, string>> list)
        {
            var postData = new FormUrlEncodedContent(list);
            return postData;
        }

        public override async Task<PublishResult> Publish(Dictionary<string, string> data)
        {
            var url = "http://control.blog.sina.com.cn/admin/article/article_add.php";
            var content = data["content"];
            var title = data["title"];
            var encoding = Encoding.UTF8;
            var webClient = new HttpWebClient() { Encoding = encoding, Cookie = GetOptionValue("Cookie") };
            var html = await webClient.Get(url);

            if (html == null)
                return Result(false, "获取html失败");
            if (html.IndexOf("vtoken") == -1)
                return Result(false, "获取token失败");
            var tokenReg = new Regex(@"name=""vtoken"" value=""(.+?)""");
            if (!tokenReg.IsMatch(html))
                return Result(false, "获取token失败");
            var token = tokenReg.Match(html).Groups[0].Value;

            var list = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("sina_sort_id", "117"),
                new KeyValuePair<string, string>("conlen", content.Length.ToString()),
                new KeyValuePair<string, string>("blog_title", title),
                new KeyValuePair<string, string>("blog_body", content),
                new KeyValuePair<string, string>("blog_class", ArticalOptions.First(o=>o.Name == "blog_class").Value),
                new KeyValuePair<string, string>("tag", ""),
                new KeyValuePair<string, string>("x_cms_flag", "0"),
                
            };

            var hiddenRegex = new Regex(@"type=""hidden"" name=""([^""]+)""[^<>]+?value=""([^""]*)""");
            foreach (Match item in hiddenRegex.Matches(html))
            {
                var name = item.Groups[1].Value;
                var val = item.Groups[2].Value;
                if (name == "timestamp" || name == "conlen")
                    continue;
                list.Add(new KeyValuePair<string, string>(name, val));
                if (name == "date_pub")
                    list.Add(new KeyValuePair<string, string>("time", DateTime.Now.ToString("hh:mm:ss")));
            }
            var postData = GetPostData(list);
            webClient.Referer = url;
            var result = await webClient.Post("http://control.blog.sina.com.cn/admin/article/article_post.php", postData);
            if (!result.Success)
                return result;
            if (result.Message.IndexOf("B06001") == -1)
                return Result(false, "发布失败！");
            return Result(true);
        }
    }
}
