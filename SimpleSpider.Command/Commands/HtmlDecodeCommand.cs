using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleSpider.Command.Commands
{
    public class HtmlDecodeCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "htmldecode";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            if (args.Length == 0)
                return new CommandResult() { Success = true, PeplineOutput = HttpUtility.HtmlDecode(peplineInput.ToString()) };
            foreach (var item in args)
            {
                if (data.ContainsKey(item))
                    data[item] = HttpUtility.HtmlDecode(data[item]);
            }
            return new CommandResult() { Success = true, PeplineOutput = peplineInput, Data = data };
        }
    }
}
