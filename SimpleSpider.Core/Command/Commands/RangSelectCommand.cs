using System.Collections;
using System.Collections.Generic;

namespace SimpleSpider.Command.Commands
{
    public class RangSelectCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "range-select";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            List<string> list = new List<string>();
            var start = int.Parse(args[0]);
            var end = int.Parse(args[1]);
            IEnumerable e = pipelineInput is IEnumerable ? (IEnumerable)pipelineInput:  new List<string>() { pipelineInput.ToString() }; 
            foreach (var item in e)
            {
                for (int i = start; i <= end; i++)
                {
                    list.Add(args[2].Replace("{$0}", item.ToString()).Replace("{$1}", i.ToString()));
                }
            }
            return new CommandResult() { Success = true, PipelineOutput = list };
        }
    }
}
