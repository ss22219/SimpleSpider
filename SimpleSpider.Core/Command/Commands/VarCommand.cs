using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleSpider.Command.Commands
{
    public class VarCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "var";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            if (args[0].IndexOf('=') == -1)
            {
                if(!data.ContainsKey(args[0]))
                    return new CommandResult() { PipelineOutput = $"var[{args[0]}] 不存在！", Success = false };
                pipelineInput = data[args[0]];
            }
            else
                foreach (var item in args)
                {
                    if (item.IndexOf('=') == -1)
                        continue;
                    var match = new Regex(@"^(\w+)=(.*)").Match(item);
                    var name = match.Groups[1].Value;
                    var val = match.Groups[2].Value;
                    var setData = string.IsNullOrEmpty(val) || val == "${pipeline}" ? pipelineInput.ToString() : val;
                    data[name] = setData;
                }
            return new CommandResult() { PipelineOutput = pipelineInput, Data = data, Success = true };
        }
    }
}
