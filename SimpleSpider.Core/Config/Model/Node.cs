using System.Collections.Generic;

namespace SimpleSpider.Config.Model
{
    public class Node
    {
        public Node()
        {
            Childs = new List<Node>();
            Args = new List<string>();
        }
        public int Line { get; set; }
        public int Indent { get; set; }
        public string Name { get; set; }
        public List<string> Args { get; set; }
        public List<Node> Childs { get; set; }
        public Node Parent { get; set; }
    }
}
