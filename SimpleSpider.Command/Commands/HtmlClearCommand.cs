using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class HtmlClearCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "htmlclear";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            if (args.Length > 0)
            {
                foreach (var item in args)
                {
                    doc.LoadHtml(data[item]);
                    foreach (var script in doc.DocumentNode.Descendants("script").ToArray())
                        script.Remove();
                    foreach (var style in doc.DocumentNode.Descendants("style").ToArray())
                        style.Remove();
                    foreach (var style in doc.DocumentNode.Descendants("a").ToArray())
                        style.Remove();
                    data[item] = doc.DocumentNode.OuterHtml;
                }
            }
            return new CommandResult() { Success = true, Data = data, PeplineOutput = peplineInput };
        }
    }
}
