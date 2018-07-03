using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ListCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "list";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            return new CommandResult() { Success = true, PipelineOutput = args };
        }
    }
}
