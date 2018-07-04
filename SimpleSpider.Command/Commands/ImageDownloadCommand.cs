using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ImageDownloadCommand : ICommand
    {
        public Unreal.QCloud.Api.CosCloud cosApi;
        private string bucketName;

        public string Name
        {
            get
            {
                return "image-download";
            }
        }
        private string appid;
        private string region;


        protected List<string> GetImages(string content)
        {
            var list = new List<string>();
            foreach (Match item in new Regex(@"<img\s+[^<>]*?src=['""]?([^'""<>]+)['""]?[^<>]*?>", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(content))
            {
                list.Add(item.Groups[1].Value);
            }
            return list;
        }

        protected async Task<ImageInfo> UploadImage(ImageInfo info)
        {
            var uri = new Uri(info.Url);
            var data = new WebClient().DownloadData(uri);
            using (var ms = new MemoryStream(data))
            {
                try
                {
                    var path = "simplespider" + FileMetadataUtility.GetSavePath(DateTime.Now.Ticks, ".png");
                    start:
                    var result = await cosApi.UploadFile(bucketName, path, ms);

                    if (result.code == 0)
                    {
                        info.UploadUrl = $"http://{bucketName}-{appid}.pic{region}.myqcloud.com/{path}";
                        return info;
                    }
                    else if (result.code == -177)
                        goto start;
                    else
                        info.Message = result.message;
                }
                catch (Exception ex)
                {
                    info.Message = ex.ToString();
                }
            }
            return info;
        }

        public CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args)
        {
            appid = ConfigurationManager.AppSettings["appId"];
            region = ConfigurationManager.AppSettings["region"];
            bucketName = ConfigurationManager.AppSettings["bucketName"];
            var secretId = ConfigurationManager.AppSettings["secretId"];
            var secretKey = ConfigurationManager.AppSettings["secretKey"];

            cosApi = new Unreal.QCloud.Api.CosCloud(int.Parse(appid), secretId, secretKey, region);

            var names = args[0].Split(',');
            var urlPrefxi = args.Length > 1 ? args[1] : data["url"].Substring(0, data["url"].LastIndexOf("/"));

            var tasks = new List<Task<ImageInfo>>();

            foreach (var name in names)
            {
                var content = data[name];
                var images = GetImages(content);
                foreach (var image in images)
                {
                    var imageUrl = image;
                    if (imageUrl.StartsWith("//"))
                        imageUrl = "http:" + imageUrl;
                    else if (imageUrl.StartsWith("/"))
                        imageUrl = urlPrefxi + imageUrl;
                    if (imageUrl.StartsWith("http"))
                    {
                        tasks.Add(UploadImage(new ImageInfo() { Url = imageUrl, SourceUrl = image }));
                    }
                    else
                        return new CommandResult() { Success = false, PipelineOutput = image + " 地址无法解析" };
                }
                if (tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                    foreach (var item in tasks)
                    {
                        if (!string.IsNullOrEmpty(item.Result.UploadUrl))
                            content = content.Replace(item.Result.SourceUrl, item.Result.UploadUrl);
                        else
                            return new CommandResult() { Success = false, PipelineOutput = item.Result.Message };
                    }
                    data[name] = content;
                }
            }

            return new CommandResult() { Success = true, PipelineOutput = pipelineInput, Data = data };
        }
    }

    public class ImageInfo
    {
        public string SourceUrl { get; set; }
        public string Url { get; set; }
        public string UploadUrl { get; set; }
        public string Message { get; set; }
    }
    public class FileMetadataUtility
    {
        private static Dictionary<char, int> charMaps = new Dictionary<char, int>();
        private static Dictionary<int, char> numMaps = new Dictionary<int, char>();

        public static string GetSavePath(long id, string ext)
        {
            if (id == 0)
                return null;
            var str64 = To64String(id);
            str64 = str64.PadLeft(12, '0');
            string path = "";
            for (var i = 0; i < str64.Length / 2; i++)
            {
                path += str64.Substring(i * 2, 2) + "/";
            }
            return $"/{path.TrimEnd('/')}{ext}";
        }

        static FileMetadataUtility()
        {
            //0-9 : 48-57
            //a-z : 97-122
            //A-Z : 65-90
            //._  : 46 95
            for (int i = 0; i < 10; i++)
            {
                charMaps[(char)(i + 48)] = i;
            }

            for (int i = 0; i < 26; i++)
            {
                charMaps[(char)(i + 97)] = i + 10;
            }


            for (int i = 0; i < 26; i++)
            {
                charMaps[(char)(i + 65)] = i + 10 + 26;
            }
            charMaps['.'] = 10 + 26 + 26;
            charMaps['_'] = charMaps['.'] + 1;

            foreach (var item in charMaps)
            {
                numMaps[item.Value] = item.Key;
            }
        }

        public static string To64String(long value)
        {
            string str = "";
            while (true)
            {
                if (value < 64)
                {
                    str = numMaps[(int)value] + str;
                    break;
                }
                else
                {
                    str = numMaps[(int)(value % 64)] + str;
                    value = value / 64;
                }
            }
            return str;
        }

        public static long From64String(string str)
        {
            long value = 0;
            for (int i = 0; i < str.Length; i++)
            {
                value += (charMaps[str[str.Length - i - 1]] * (long)Math.Pow(64, i));
            }
            return value;
        }
    }
}
