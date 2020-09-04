using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimpleSpider.Command.Commands
{
    public class XPathCommand : ICommand
    {
        class Rule
        {
            public string Name { get; set; }
            public string Xpath { get; set; }
            public string ValueType { get; set; }
        }

        public string Name
        {
            get
            {
                return "xpath";
            }
        }

        string getVal(Rule rule, HtmlNode node)
        {
            var val = "";
            if (string.IsNullOrEmpty(rule.ValueType))
                val = node.InnerText;
            else if (rule.ValueType == "text")
                val = node.InnerText;
            else if (rule.ValueType == "html")
                val = node.InnerHtml;
            else if (rule.ValueType == "outhtml")
                val = node.OuterHtml;
            else if (rule.ValueType.StartsWith("attr"))
                val = node.Attributes[new Regex(@"\[(\w+)\]").Match(rule.ValueType).Groups[1].Value].Value;
            else
                val = node.InnerText;
            return val;
        }

        Rule getRule(string arg)
        {
            var match = new Regex(@"^([^=/]+=)?(.+?)(=(text|html|outhtml|attr\[\w+\]))*$").Match(arg);
            return new Rule() { Name = match.Groups[1].Value.TrimEnd('='), Xpath = match.Groups[2].Value, ValueType = match.Groups[4].Value };
        }


        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            HtmlNode root = null;
            var html = string.Empty;
            if (pipelineInput is HtmlNode)
                html = ((HtmlNode)pipelineInput).OuterHtml;
            else
                html = pipelineInput.ToString();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            root = htmlDoc.DocumentNode;

            foreach (var item in args)
            {
                var rule = getRule(item);
                if (!item.StartsWith("/"))
                {
                    var name = rule.Name;
                    var node = root.SelectSingleNode(rule.Xpath);
                    if (node == null)
                        data[name] = null;
                    else
                        data[name] = getVal(rule, node);
                }
                else if (args.Length == 1)
                    pipelineInput = root.SelectNodes(rule.Xpath);
            }
            return new CommandResult() { Success = true, Data = data, PipelineOutput = pipelineInput };
        }
    }
}
