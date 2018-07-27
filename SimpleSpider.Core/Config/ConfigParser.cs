using SimpleSpider.Config.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Config
{
    public class ConfigParseException : Exception
    {
        public ConfigParseException(int line, int word, string message) : base(message)
        {
            this.Line = line;
            this.Word = word;
        }
        public int Line { get; set; }
        public int Word { get; set; }

        public override string ToString()
        {
            return $"line {Line},{Word} :{Message}";
        }
    }

    public class ConfigParser
    {
        string[] innerCommands = new string[] { "include" };

        public List<string> CommandNames;
        public string IncludePath;
        public ConfigParser(string includePath = "")
        {
            this.IncludePath = includePath;
        }
        public ConfigParser(List<string> commandNames, string includePath = "")
        {
            this.CommandNames = commandNames;
            this.IncludePath = includePath;
        }

        public const string ENGINE = "engine";
        public const char COMMENT_CAHR = '#';
        static char[] spaceChar = new char[] { '\r', '\n', '\t', ' ' };

        enum LineStatus
        {
            Start,
            TreeStart,
            NameEnd,
            ArgumentStart,
            ArgumentEnd
        }

        public Node Parse(string content)
        {
            using (var reader = new StringReader(content))
            {
                return Parse(reader);
            }
        }

        public Node Parse(System.IO.TextReader reader)
        {
            var root = new Node();
            var last = root;
            int lineNum = 0;
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                lineNum++;
                if (lineNum == 1 && line != ENGINE)
                    throw new ConfigParseException(lineNum, 0, "配置文件应该从`engin`开始");
                else if (lineNum == 1)
                {
                    root.Name = ENGINE;
                    continue;
                }

                Node node = null;
                if (!string.IsNullOrWhiteSpace(line))
                    node = new Node();
                else
                    continue;

                NodeParse(line, lineNum, node);
                if (node.Name == null)
                    continue;

                if (CommandNames != null && (!CommandNames.Contains(node.Name) && !innerCommands.Contains(node.Name)))
                    throw new ConfigParseException(lineNum, node.Indent, $"{node.Name} 命令不存在！");
                if (node.Indent > last.Indent)
                {
                    last.Childs.Add(node);
                    node.Parent = last;
                }
                else if (node.Indent == last.Indent)
                {
                    if (last.Parent == null)
                        throw new ConfigParseException(lineNum, 0, "不能出现多个根节点");
                    last.Parent.Childs.Add(node);
                    node.Parent = last.Parent;
                }
                else
                {
                    var tmpNode = last;
                    while (tmpNode.Indent != node.Indent)
                    {
                        if (tmpNode.Parent != null)
                            tmpNode = tmpNode.Parent;
                        else
                            throw new ConfigParseException(lineNum, 0, "与同级缩进不一致！");
                    }
                    if (tmpNode.Parent == null)
                        throw new ConfigParseException(lineNum, 0, "不能出现多个根节点");
                    node.Parent = tmpNode.Parent;
                    tmpNode.Parent.Childs.Add(node);
                }
                if (node.Name == "include")
                {
                    includeParse(node);
                    node = last;
                }
                last = node;
            }
            return root;
        }

        private void resetIndent(Node node, int indent)
        {
            foreach (var item in node.Childs)
            {
                resetIndent(item, node.Indent - node.Parent.Indent + indent);
            }
            node.Indent = indent;
        }

        private void includeParse(Node includeNode)
        {
            if (includeNode.Args.Count == 0)
                throw new ConfigParseException(includeNode.Line, 0, "参数不正确");
            var file = IncludePath + "\\" + includeNode.Args[0];
            if (!File.Exists(file))
                throw new ConfigParseException(includeNode.Line, 0, $"引入文件不存在 {file}");

            var content = File.ReadAllText(file);
            var root = Parse(content);
            if (root.Childs.Count > 0)
            {
                foreach (var item in root.Childs)
                {
                    includeNode.Parent.Childs.Add(item);
                    resetIndent(item, includeNode.Indent);
                }
            }
            includeNode.Parent.Childs.Remove(includeNode);
        }

        enum NodeParseStatus
        {
            Indent,
            NameStart,
            NameEnd,
            ArgumentStart,
            ArgumentEnd
        }

        public void NodeParse(string content, int line, Node node)
        {
            node.Line = line;
            NodeParseStatus status = NodeParseStatus.Indent;
            int indent = 0;
            string name = null;
            string argument = null;

            bool converChar = false;
            List<string> args = new List<string>();
            for (int word = 0; word < content.Length; word++)
            {
                if (spaceChar.Contains(content[word]))
                {
                    switch (status)
                    {
                        case NodeParseStatus.Indent:
                            indent++;
                            break;
                        case NodeParseStatus.NameStart:
                            node.Name = name;
                            name = null;
                            status = NodeParseStatus.NameEnd;
                            break;
                        case NodeParseStatus.NameEnd:
                            break;
                        case NodeParseStatus.ArgumentStart:
                            if (!converChar)
                            {
                                args.Add(argument);
                                status = NodeParseStatus.ArgumentEnd;
                                argument = null;
                            }
                            else
                            {
                                converChar = false;
                                argument = argument.Substring(0, argument.Length - 1) + content[word];
                            }
                            break;
                        default:
                            converChar = false;
                            break;
                    }
                }
                else
                {
                    converChar = false;
                    if (content[word] == '\\')
                        converChar = true;

                    if (content[word] == COMMENT_CAHR)
                        goto End;

                    switch (status)
                    {
                        case NodeParseStatus.Indent:
                            status = NodeParseStatus.NameStart;
                            name = content[word].ToString();
                            node.Indent = indent;
                            break;
                        case NodeParseStatus.NameStart:
                            name += content[word];
                            break;
                        case NodeParseStatus.NameEnd:
                        case NodeParseStatus.ArgumentEnd:
                            argument = content[word].ToString();
                            status = NodeParseStatus.ArgumentStart;
                            break;
                        case NodeParseStatus.ArgumentStart:
                            argument += content[word];
                            break;
                        default:
                            break;
                    }
                }
            }

            End:
            if (!string.IsNullOrEmpty(argument))
                args.Add(argument);
            if (!string.IsNullOrEmpty(name))
                node.Name = name;
            node.Args = args;
        }
    }
}
