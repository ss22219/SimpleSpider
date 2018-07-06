using Newtonsoft.Json.Linq;
using SimpleSpider.Command;
using SimpleSpider.Config;
using SimpleSpider.Config.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider
{
    class Program
    {
        static Node errorNode;

        static void Main(string[] args)
        {

            typeof(CommandManage).Assembly
                .GetTypes()
                .Where(t => t.GetInterfaces().Any(t2 => t2 == typeof(ICommand)))
                .Select(t3 => typeof(CommandManage).Assembly.CreateInstance(t3.FullName)).
                ToList().ForEach(c => CommandManage.Regist((ICommand)c));

            ExcuteTask(args[0]);
        }

        static void ExcuteTask(string configFile)
        {
            var configFiles = new string[] { configFile };
            var parser = new ConfigParser(CommandManage.Commands.Select(c => c.Key).ToList(), Path.GetDirectoryName(configFile));
            foreach (var config in configFiles)
            {
                errorNode = null;
                try
                {
                    var root = parser.Parse(File.ReadAllText(config));
                    var br = false;
                    var result = Excute(root, ref br);
                    if (!result.Success)
                    {
                        Console.WriteLine($"错误：{result.PipelineOutput.ToString()}");
                        Console.WriteLine($"Command：{errorNode.Name}, 配置文件：{config} 行{errorNode.Line}");
                    }
                }
                catch (ConfigParseException ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        static void Log(Node node)
        {
            Console.WriteLine($"{new string('-', node.Indent * 4)} {node.Name} {(node.Args.Count > 0 ? node.Args.Aggregate((a, b) => a + " " + b) : null)}");
        }

        static void Log(Node node, CommandResult result)
        {
            var content = GetString(result.PipelineOutput);
            if (content.Length > 200)
                content = content.Substring(0, 200);
            content = content.Replace("\r", "").Replace("\n", "");
            Console.WriteLine($"{new string('-', node.Indent * 4)}> {content}");
        }

        static string GetString(object obj)
        {
            if (obj != null)
            {
                if (obj is string)
                    return $"\"{(string)obj}\"";
                else if (obj is JToken)
                    return "JToken";
                else if (obj is IEnumerable)
                {
                    var content = "[";
                    foreach (var item in (IEnumerable)obj)
                    {
                        content += GetString(item) + ",";
                    }
                    content = content.TrimEnd(',') + "]";
                    return content;
                }
                else
                    return obj.ToString();
            }
            return string.Empty;
        }

        static Dictionary<string, string> Clone(Dictionary<string, string> dic)
        {
            var clone = new Dictionary<string, string>();
            if (dic == null)
                return clone;
            foreach (var item in dic)
            {
                clone[item.Key] = item.Value;
            }
            return clone;
        }

        static Dictionary<string, string> GetResultData(Dictionary<string, string> inputData, Dictionary<string, string> data)
        {
            var clone = data == null ? new Dictionary<string, string>() : data;
            var parent = new Dictionary<string, string>();
            if (inputData == null)
                return data;

            foreach (var item in inputData)
            {
                if (!clone.ContainsKey(item.Key))
                    clone[item.Key] = item.Value;
            }
            return clone.Count == 0 ? null : clone;
        }

        static CommandResult ExcuteChild(List<Node> childs, IEnumerable list, Dictionary<string, string> data)
        {
            foreach (var item in list)
            {
                bool @break = false;
                var pipeline = item;
                foreach (var child in childs)
                {
                    var result = Excute(child, ref @break, pipeline, data);
                    pipeline = result.PipelineOutput;
                    data = result.Data;

                    if (!result.Success)
                        return result;

                    if (@break)
                    {
                        @break = false;
                        result.PipelineOutput = pipeline;
                        return result;
                    }
                }
            }
            return new CommandResult() { Success = true };
        }

        static CommandResult Excute(Node node, ref bool @break, object pipeline = null, Dictionary<string, string> data = null)
        {
            if (!CommandManage.Commands.ContainsKey(node.Name))
                throw new ConfigParseException(node.Line, node.Indent, $"{node.Name} 命令不存在");
            var command = CommandManage.Commands[node.Name];

            data = Clone(data);

            Log(node);
            var result = command.Excute(pipeline, data, node.Args.ToArray());
            Log(node, result);

            result.Data = GetResultData(data, result.Data);
            data = result.Data;
            pipeline = result.PipelineOutput;

            if (node.Name == "break")
            {
                @break = true;
                return result;
            }

            if (result.Success)
                if (node.Childs.Count != 0 && pipeline != null)
                    ExcuteChild(node.Childs, pipeline is IEnumerable ? (IEnumerable)pipeline : new object[] { pipeline }, data);
                else
                    errorNode = node;

            return result;
        }
    }
}
