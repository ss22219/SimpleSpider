using Newtonsoft.Json;
using SimpleSpider.Publish;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.UI
{

    public class UserConfig
    {
        public static Dictionary<string, Type> PublisherTypes = new Dictionary<string, Type>();

        public static Publisher CreatePublisher(string name)
        {
            return ((Publisher)typeof(Publisher).Assembly.CreateInstance(PublisherTypes[name].FullName));
        }

        static UserConfig()
        {
            typeof(Publisher).Assembly.GetTypes().Where(t => t.BaseType == typeof(Publisher)).ToList().ForEach(t =>
            {
                PublisherTypes[((Publisher)typeof(Publisher).Assembly.CreateInstance(t.FullName)).PublisherName] = t;
            });

            Publishers = new List<Publisher>();
            if (File.Exists("userConfig.json"))
            {
                var configList = JsonConvert.DeserializeObject<List<PublisherConfig>>(File.ReadAllText("userConfig.json"));
                foreach (var config in configList)
                {
                    var publisher = CreatePublisher(config.PublisherName);
                    publisher.Name = config.Name;
                    foreach (var item in config.Option)
                    {
                        publisher.Options.FirstOrDefault(o => o.Name == item.Key).Value = item.Value;
                    }
                    Publishers.Add(publisher);
                }
            }
        }

        public static void Save()
        {
            var configList = new List<PublisherConfig>();
            foreach (var item in Publishers)
            {
                var config = new PublisherConfig()
                {
                    Name = item.Name,
                    PublisherName = item.PublisherName,
                    Option = new Dictionary<string, string>()
                };
                foreach (var option in item.Options)
                {
                    config.Option[option.Name] = option.Value;
                }
                configList.Add(config);
            }

            File.WriteAllText("userConfig.json", JsonConvert.SerializeObject(configList));
        }
        public static List<Publisher> Publishers { get; set; }
    }

    public class PublisherConfig
    {
        public string PublisherName { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Option { get; set; }
    }
}
