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
            UIOptions = new Dictionary<Publisher, Dictionary<string, string>>();
            if (File.Exists("userConfig.json"))
            {
                var configList = JsonConvert.DeserializeObject<List<PublisherConfig>>(File.ReadAllText("userConfig.json"));
                foreach (var config in configList)
                {
                    var publisher = CreatePublisher(config.PublisherName);
                    publisher.Name = config.Name;
                    if (config.Option != null)
                        foreach (var item in config.Option)
                        {
                            var option = publisher.Options.FirstOrDefault(o => o.Name == item.Key);
                            if (option != null)
                                option.Value = item.Value;
                        }

                    if (config.ArticalOptions != null)
                        foreach (var item in config.ArticalOptions)
                        {
                            var option = publisher.ArticalOptions.FirstOrDefault(o => o.Name == item.Key);
                            if (option != null)
                                option.Value = item.Value;
                        }
                    Publishers.Add(publisher);
                    config.UIOptions = config.UIOptions == null ? new Dictionary<string, string>() : config.UIOptions;
                    UIOptions[publisher] = config.UIOptions;
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
                    Option = new Dictionary<string, string>(),
                    ArticalOptions = new Dictionary<string, string>()
                };
                if (item.Options != null)
                    foreach (var option in item.Options)
                    {
                        config.Option[option.Name] = option.Value;
                    }

                if (item.ArticalOptions != null)
                    foreach (var option in item.ArticalOptions)
                    {
                        config.ArticalOptions[option.Name] = option.Value;
                    }
                if (UIOptions.ContainsKey(item))
                    config.UIOptions = UIOptions[item];
                configList.Add(config);
            }

            File.WriteAllText("userConfig.json", JsonConvert.SerializeObject(configList));
        }

        public static Dictionary<Publisher, Dictionary<string, string>> UIOptions { get; set; }
        public static List<Publisher> Publishers { get; set; }
    }

    public class PublisherConfig
    {
        public string PublisherName { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Option { get; set; }
        public Dictionary<string, string> ArticalOptions { get; set; }
        public Dictionary<string, string> UIOptions { get; set; }
    }
}
