using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command
{
    public class CommandManage
    {
        static CommandManage()
        {
            Commands = new Dictionary<string, ICommand>();
        }
        public static void Regist(ICommand command)
        {
            Commands[command.Name] = (command);
        }

        public static Dictionary<string, ICommand> Commands { get; private set; }
    }
}
