using System.Collections.Generic;

namespace SimpleSpider.Command.Commands
{
    public class EngineCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "engine";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            return new CommandResult() { Success = true, PipelineOutput = new string[] { "v0.2" }, Data = data };
        }
    }
}
