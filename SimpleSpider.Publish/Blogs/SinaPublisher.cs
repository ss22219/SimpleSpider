using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Publish.Blogs
{
    public class SinaPublisher : Publisher
    {
        List<Option> _options;
        List<Option> _articalOptions;
        public override List<Option> ArticalOptions
        {
            get;

            set;
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
                    new KeyValuePair<string, string>("Url", "http://i.blog.sina.com.cn/")
                } }
            };
        }

        public override Task<PublishResult> Publish(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }
    }
}
