using System.Collections.Generic;

namespace SimpleSpider.Command.Commands
{
    public class ForeachCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "foreach";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            return new CommandResult() { Success = true, PipelineOutput = pipelineInput, Data = data };
        }
    }
}
