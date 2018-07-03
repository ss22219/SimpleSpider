using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ReplaceCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "replace";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            var names = args[0].Split(',');
            var rule = args[1];
            foreach (var item in names)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    var regex = new Regex(args[i]);
                    data[item] = regex.Replace(data[item], args.Length < i + 1 ? args[i + 1] : "");
                    i++;
                }
            }
            return new CommandResult() { Success = true, Data = data, PipelineOutput = pipelineInput };
        }
    }
}
