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

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            try
            {
                var client = new WebClient();
                if (args.Select(a => a.ToLower()).Contains("gbk"))
                    client.Encoding = Encoding.GetEncoding("gbk");
                else if (args.Select(a => a.ToLower()).Contains("utf8"))
                    client.Encoding = Encoding.UTF8;
                var result = client.DownloadString(peplineInput.ToString());
                return new CommandResult() { Success = true, PeplineOutput = result };
            }
            catch (Exception ex)
            {
                return new CommandResult() { Success = false, PeplineOutput = ex.ToString() };
            }
        }
    }
}
