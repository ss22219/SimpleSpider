using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Publish
{
    public enum OptionInputType
    {
        Text,
        Select,
        CheckBox,
        File,
        Cookie,
        RemoteMatch
    }

    public class Option
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public OptionInputType OptionInputType { get; set; }
        public List<KeyValuePair<string,string>> SelectValues { get; set; }
        public string Value { get; set; }
    }
}
