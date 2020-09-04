using System.Collections.Generic;

namespace SimpleSpider.Command.Commands
{
    public class IfCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "if";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            bool flag = true;
            foreach (var item in args)
            {
                if (!data.ContainsKey(item) || string.IsNullOrWhiteSpace(data[item]))
                {
                    flag = false;
                }
                if (!flag)
                    break;
            }
            return new CommandResult() { Success = true, PipelineOutput = flag ? new object[] { pipelineInput } : null, Data = data };
        }
    }
}
