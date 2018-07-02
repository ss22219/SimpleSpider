using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ExportJsonCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "export-json";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            if (args.Length > 1)
            {
                Dictionary<string, string> edata = new Dictionary<string, string>();
                var names = args[1].Split(',');
                foreach (var item in names)
                {
                    if (data.ContainsKey(item))
                        edata[item] = data[item];
                }
                File.AppendAllText(args[0], JsonConvert.SerializeObject(edata) + "\r\n");
            }
            return new CommandResult() { Success = true, Data = data, PeplineOutput = peplineInput };
        }
    }
}
