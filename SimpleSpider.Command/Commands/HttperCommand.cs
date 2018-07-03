using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class HttperCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "httper";
            }
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            try
            {
                var client = new WebClient();
                if (args.Select(a => a.ToLower()).Contains("gbk"))
                    client.Encoding = Encoding.GetEncoding("gbk");
                else if (args.Select(a => a.ToLower()).Contains("utf-8"))
                    client.Encoding = Encoding.UTF8;
                var settings = args.Where(a => a.IndexOf("=") != -1).Select(i => { var set = i.Split('='); return new KeyValuePair<string, string>(set[0], set[1]); });
                foreach (var setting in settings)
                {
                    client.Headers[setting.Key] = setting.Value;
                }
                var result = client.DownloadString(pipelineInput.ToString());
                return new CommandResult() { Success = true, PipelineOutput = result };
            }
            catch (Exception ex)
            {
                return new CommandResult() { Success = false, PipelineOutput = ex.ToString() };
            }
        }
    }
}
