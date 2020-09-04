using System.Collections.Generic;
using System.Web;

namespace SimpleSpider.Command.Commands
{
    public class HtmlDecodeCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "htmldecode";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            if (args.Length == 0)
                return new CommandResult() { Success = true, PipelineOutput = HttpUtility.HtmlDecode(pipelineInput.ToString()) };
            foreach (var item in args)
            {
                if (data.ContainsKey(item))
                    data[item] = HttpUtility.HtmlDecode(data[item]);
            }
            return new CommandResult() { Success = true, PipelineOutput = pipelineInput, Data = data };
        }
    }
}
