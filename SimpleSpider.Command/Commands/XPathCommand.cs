using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class XPathCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "xpath";
            }
        }

        string getVal(string[] set, HtmlNode node)
        {
            var par = set.Last();
            var val = "";
            if (set.Length == 2)
                val = node.InnerText;
            else if (par == "text")
                val = node.InnerText;
            else if (par == "html")
                val = node.InnerHtml;
            else if (par == "outhtml")
                val = node.OuterHtml;
            else if (par.StartsWith("attr"))
                val = node.Attributes[new Regex(@"\[(\w+)\]").Match(par).Groups[1].Value].Value;
            else
                val = node.InnerText;
            return val;
        }

        string getRule(string arg)
        {
            if (!arg.StartsWith("/"))
            {
                var set = arg.Split('=');
                arg = arg.Substring(set[0].Length + 1);
            }
            return new Regex(@"=(text|html|outhtml|attr\[\w+\])$").Replace(arg, "");
        }


        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(peplineInput.ToString());
            foreach (var item in args)
            {
                if (!item.StartsWith("/"))
                {
                    var set = item.Split('=');
                    var name = set[0];
                    var node = htmlDoc.DocumentNode.SelectSingleNode(getRule(item));
                    if (node == null)
                        return new CommandResult() { Success = false, PeplineOutput = set[1] + " 获取失败" };
                    data[name] = getVal(set, node);
                }
                else if (args.Length == 1)
                {
                    var node = htmlDoc.DocumentNode.SelectSingleNode(getRule(item));
                    peplineInput = getVal(args, node);
                }
            }
            return new CommandResult() { Success = true, Data = data, PeplineOutput = peplineInput };
        }
    }
}
