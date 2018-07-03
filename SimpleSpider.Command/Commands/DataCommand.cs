using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class DataCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "data";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            if (args[0].IndexOf('=') == -1)
            {
                if(!data.ContainsKey(args[0]))
                    return new CommandResult() { PipelineOutput = $"data[{args[0]}] 不存在！", Success = false };
                pipelineInput = data[args[0]];
            }
            else
                foreach (var item in args)
                {
                    if (item.IndexOf('=') == -1)
                        continue;
                    var set = item.Split('=');
                    var setData = string.IsNullOrEmpty(set[1]) || set[1] == "*" ? pipelineInput.ToString() : set[1];
                    data[set[0]] = setData;
                }
            return new CommandResult() { PipelineOutput = pipelineInput, Data = data, Success = true };
        }
    }
}
