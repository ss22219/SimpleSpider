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
            var filename = args.Length > 0 ? args[0] : DateTime.Now.ToString("yyyy.MM.dd_hh_mm_ss") + ".json";
            File.WriteAllText(filename, JsonConvert.SerializeObject(ResultCommand.Rows));
            return new CommandResult() { Success = true, Data = data, PeplineOutput = peplineInput };
        }
    }
}
