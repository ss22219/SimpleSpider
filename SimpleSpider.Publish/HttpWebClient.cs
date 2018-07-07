using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Publish
{
    public class HttpWebClient
    {
        public string Cookie { get; set; }
        public Encoding Encoding { get; set; }
        public string Referer { get; set; }

        public HttpWebClient()
        {
            Encoding = Encoding.UTF8;
        }

        public PublishResult Result(bool success = true, string message = null)
        {
            return new PublishResult() { Success = success, Message = message };
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

        public async Task<PublishResult> Post(string url, HttpContent content)
        {
            var client = CreateHttpClient();
            var result = await client.PostAsync(url, content);
            if (result.StatusCode != HttpStatusCode.OK)
                return Result(false, "网络提交失败");
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var reponseContent = Encoding.GetString(bytes);
            return Result(true, reponseContent);
        }

        public HttpClient CreateHttpClient()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
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
