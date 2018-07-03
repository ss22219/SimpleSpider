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
            var parser = new ConfigParser(CommandManage.Commands.Select(c => c.Key).ToList());
            foreach (var config in configFiles)
            {
                errorNode = null;
                var root = parser.Parse(File.ReadAllText(config));
                var result = Excute(root.Childs);
                if (!result.Success)
                {
                    Console.WriteLine($"错误：{result.PipelineOutput.ToString()}");
                    Console.WriteLine($"Command：{errorNode.Name}, 配置文件：{config} 行{errorNode.Line}");
                    Console.ReadKey();
                }
            }
        }

        static void Log(Node node, CommandResult result)
        {
            Console.WriteLine($"{new string('-', node.Indent * 4)} {node.Name} {(node.Args.Count > 0 ? node.Args.Aggregate((a, b) => a + " " + b) : null)}");
            if (result.PipelineOutput != null)
                Console.WriteLine($"{new string('-', node.Indent * 4)}> {result.PipelineOutput}");
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

        static CommandResult Excute(List<Node> nodes, object pipeline = null, Dictionary<string, string> data = null)
        {
            foreach (var node in nodes)
            {
                if (!CommandManage.Commands.ContainsKey(node.Name))
                    throw new ConfigParseException(node.Line, node.Indent, $"{node.Name} 命令不存在");
                var command = CommandManage.Commands[node.Name];
                data = Clone(data);
                var result = command.Excute(pipeline, data, node.Args.ToArray());

                Log(node, result);
                result.Data = GetResultData(data, result.Data);
                data = result.Data;
                pipeline = result.PipelineOutput;
                if (result.Success)
                {
                    if (node.Childs.Count != 0)
                    {
                        foreach (var item in (IEnumerable)result.PipelineOutput)
                        {
                            result = Excute(node.Childs, item, data);
                            if (!result.Success)
                                return result;
                        }
                    }
                }
                else
                {
                    errorNode = node;
                    return result;
                }
            }
            return new CommandResult() { Success = true };
        }
    }
}
