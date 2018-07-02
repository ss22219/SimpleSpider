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
            var tags = new List<string>();

            if (args.Length > 0)
            {
                foreach (var item in args[0].Split(','))
                {
                    doc.LoadHtml(data[item]);
                    if (args.Length > 1)
                        foreach (var tag in args[1].Split(','))
                        {
                            foreach (var el in doc.DocumentNode.Descendants(tag).ToArray())
                                el.Remove();
                        }
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
