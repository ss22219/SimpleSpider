using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class SelectCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "select";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            List<string> ls = new List<string>();
            foreach (var item in (IEnumerable)peplineInput)
            {
                foreach (var arg in args)
                {
                    ls.Add(arg.Replace("{$0}", item.ToString()));
                }
            }
            return new CommandResult() { Success = true, PeplineOutput = ls };
        }
    }
}
