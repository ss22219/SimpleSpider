using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ResultCommand : ICommand
    {
        public static List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();

        public string Name
        {
            get
            {
                return "result";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            var dic = new Dictionary<string, string>();
            foreach (var item in args)
            {
                if (data.ContainsKey(item))
                    dic[item] = data[item];
            }
            Rows.Add(dic);
            return new CommandResult() { Success = true };
        }
    }
}
