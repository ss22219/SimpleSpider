using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public abstract string PublisherName { get; }
        public string Name { get; set; }
        public abstract List<Option> Options { get; set; }
        public abstract Task<PublishResult> Publish(Dictionary<string, string> data);

        public PublishResult Result(bool success = true, string message = null)
        {
            return new PublishResult() { Success = success, Message = message };
        }
    }
}
