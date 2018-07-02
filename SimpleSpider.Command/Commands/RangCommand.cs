using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class RangCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "rang";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            var start = int.Parse(args[0]);
            var end = int.Parse(args[1]);
            List<int> list = new List<int>();
            for (int i = start; i <= end; i++)
            {
                list.Add(i);
            }
            return new CommandResult() { Success = true, PeplineOutput = list };
        }
    }
}
