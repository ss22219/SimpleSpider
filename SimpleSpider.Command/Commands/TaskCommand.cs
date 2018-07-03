using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class TaskCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "task";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            data["task"] = args[0];
            return new CommandResult() { Success = true, Data = data, PipelineOutput = pipelineInput };
        }
    }
}
