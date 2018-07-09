using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Publish
{
    public class PublishResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public abstract class Publisher
    {
        public string GetOptionValue(string name)
        {
            return Options.FirstOrDefault(o => o.Name == name).Value;
        }

        public string GetArticelOptionValue(string name)
        {
            return ArticalOptions.FirstOrDefault(o => o.Name == name).Value;
        }
        public abstract string PublisherName { get; }
        public string Name { get; set; }

        public Dictionary<string,string> Cache { get; set; }

        public abstract List<Option> Options { get; set; }
        public abstract List<Option> ArticalOptions { get; set; }

        public abstract Task<PublishResult> Publish(Dictionary<string, string> data);

        public PublishResult Result(bool success = true, string message = null)
        {
            return new PublishResult() { Success = success, Message = message };
        }


        protected List<string> GetImages(string content)
        {
            var list = new List<string>();
            foreach (Match item in new Regex(@"<img\s+[^<>]*?src=['""]?([^'""<>]+)['""]?[^<>]*?>", RegexOptions.IgnoreCase).Matches(content))
            {
                if (item.Groups[0].Value.IndexOf("data-append") == -1)
                    list.Add(item.Groups[1].Value);
            }
            return list;
        }
    }
}
