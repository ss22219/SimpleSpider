using Newtonsoft.Json.Linq;
using SimpleSpider.Command;
using SimpleSpider.Config;
using SimpleSpider.Config.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider
{
    class Program
    {
        static StreamWriter logWriter;
        static void Main(string[] args)
        {
            if (args.Any(a => a.ToLower() == "-log"))
            {
                var logStream = File.Open($"{DateTime.Now.ToString("yyyy-MM-dd")}.log", FileMode.OpenOrCreate);
                logWriter = new StreamWriter(logStream);
            }
            typeof(CommandManage).Assembly
                .GetTypes()
                .Where(t => t.GetInterfaces().Any(t2 => t2 == typeof(ICommand)))
                .Select(t3 => typeof(CommandManage).Assembly.CreateInstance(t3.FullName)).
                ToList().ForEach(c => CommandManage.Regist((ICommand)c));

            ExcuteScript(args[0]);
        }

        static void ExcuteScript(string configFile)
        {
            var configFiles = new string[] { configFile };
            if (Path.GetFileName(configFile).IndexOf("*") != -1)
            {
                configFiles = Directory.GetFiles(Path.GetDirectoryName(configFile), Path.GetFileName(configFile));
            }

            foreach (var config in configFiles)
            {
                var engine = new Engine(logWriter);
                engine.ExcuteFile(config);
            }
        }
    }
}
