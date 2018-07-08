using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                var client = new HttpWebClient();
                if (args.Select(a => a.ToLower()).Contains("gbk"))
                    client.Encoding = Encoding.GetEncoding("gbk");
                else if (args.Select(a => a.ToLower()).Contains("utf-8"))
                    client.Encoding = Encoding.UTF8;
                var settings = args.Where(a => a.IndexOf("=") != -1).Select(i => { var set = i.Split('='); return new KeyValuePair<string, string>(set[0], set[1]); });
                foreach (var setting in settings)
                {
                    client.Headers[setting.Key] = setting.Value;
                }
                var task = client.Get(pipelineInput.ToString());
                task.Wait();
                return new CommandResult() { Success = true, PipelineOutput = task.Result };
            }
            catch (Exception ex)
            {
                return new CommandResult() { Success = false, PipelineOutput = ex.ToString() };
            }
        }
    }


    public class HttpWebClient
    {
        public string Cookie { get; set; }
        public Encoding Encoding { get; set; }
        public string Referer { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpWebClient()
        {
            Headers = new Dictionary<string, string>();
            Encoding = Encoding.UTF8;
        }

        public async Task<string> Get(string url)
        {
            var client = CreateHttpClient();
            var result = await client.GetAsync(url);
            if (result.StatusCode != HttpStatusCode.OK)
                return null;
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var reponseContent = Encoding.GetString(bytes);
            return reponseContent;
        }

        public async Task<string> Post(string url, HttpContent content)
        {
            var client = CreateHttpClient();
            var result = await client.PostAsync(url, content);
            if (result.StatusCode != HttpStatusCode.OK)
                return null;
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var reponseContent = Encoding.GetString(bytes);
            return reponseContent;
        }

        public HttpClient CreateHttpClient()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                UseCookies = false,
            };
            var client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36");
            if (!string.IsNullOrEmpty(Referer))
                client.DefaultRequestHeaders.Add("Referer", Referer);
            if (!string.IsNullOrWhiteSpace(Cookie))
                client.DefaultRequestHeaders.Add("Cookie", Cookie);
            return client;
        }
    }
}
