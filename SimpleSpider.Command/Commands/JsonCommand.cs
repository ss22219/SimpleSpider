using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class JsonCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "json";
            }
        }

        int indexOf(string[] args, string val)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == val)
                    return i;
            }
            return -1;
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            string content = null;
            if (args.Contains("-from"))
            {
                var index = indexOf(args, "-from");
                content = data[args[index + 1]];
                var ls = args.ToList(); ls.RemoveAt(index); ls.RemoveAt(index + 1); args = ls.ToArray();
            }
            else if (!(peplineInput is JToken))
                content = peplineInput.ToString();
            try
            {
                var token = content != null ? JsonConvert.DeserializeObject<JToken>(content) : (JToken)peplineInput;
                if (args.Length != 0)
                {
                    foreach (var item in args)
                    {
                        if (item.IndexOf("=") != -1)
                        {
                            var set = item.Split('=');

                            if (set[1].StartsWith("$"))
                            {
                                var str = set[1].TrimStart('$');
                                foreach (Match match in new Regex(@"\{(\w+)\}").Matches(str))
                                {
                                    var name = match.Groups[1].Value;
                                    var value = "";
                                    if (data.ContainsKey(name))
                                        value = data[name];
                                    else
                                        value = token.SelectToken(name).ToString();
                                    str = str.Replace(match.Groups[0].Value, value);
                                }
                                data[set[0]] = str;
                            }
                            else
                                data[set[0]] = token.SelectToken(set[1]).ToString();
                        }
                    }
                    var outputToken = args.ToList().Where(a => a.IndexOf("=") == -1);
                    if (outputToken.Count() == 1)
                    {
                        token = outputToken.First() == "." ? token : token.SelectToken(outputToken.First());
                    }
                }

                return new CommandResult() { Success = true, PeplineOutput = token.Type == JTokenType.Array ? (object)token.ToArray() : token, Data = data };
            }
            catch (Exception ex)
            {
                return new CommandResult() { Success = false, PeplineOutput = ex.ToString() };
            }

        }
    }
}
