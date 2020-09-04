using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            var filename = args.Length > 0 ? args[0] : DateTime.Now.ToString("yyyy.MM.dd_hh_mm_ss") + ".json";
            File.WriteAllText(filename, JsonConvert.SerializeObject(ResultCommand.Rows));
            return new CommandResult() { Success = true, Data = data, PipelineOutput = pipelineInput };
        }
    }
}
