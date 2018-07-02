using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
