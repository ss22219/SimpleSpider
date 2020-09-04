using System.Collections.Generic;

namespace SimpleSpider.Command.Commands
{
    public class RangCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "range";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            var start = int.Parse(args[0]);
            var end = int.Parse(args[1]);
            List<int> list = new List<int>();
            for (int i = start; i <= end; i++)
            {
                list.Add(i);
            }
            return new CommandResult() { Success = true, PipelineOutput = list };
        }
    }
}
