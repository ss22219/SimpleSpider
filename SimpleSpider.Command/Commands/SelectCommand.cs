using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            if (pipelineInput is IEnumerable)
            {
                List<string> ls = new List<string>();
                foreach (var item in (IEnumerable)pipelineInput)
                {
                    foreach (var arg in args)
                    {
                        string str = arg;
                        foreach (Match match in new Regex(@"\{([$\w]+)\}").Matches(str))
                        {
                            var name = match.Groups[1].Value;
                            var value = "";
                            if (data.ContainsKey(name))
                                value = data[name];
                            else if (name == "$0")
                                value = item.ToString();

                            str = str.Replace(match.Groups[0].Value, value);
                        }
                        ls.Add(str);
                    }
                }
                return new CommandResult() { Success = true, PipelineOutput = ls };
            }
            else
            {
                return new CommandResult() { Success = false, PipelineOutput = "不支持foreach后操作！" };
            }
        }
    }
}
