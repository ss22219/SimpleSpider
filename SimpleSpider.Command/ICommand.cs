using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command
{
    public class CommandResult
    {
        public bool Success { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public object PeplineOutput { get; set; }
    }

    public class CommandResult<T> : CommandResult
    {
        public new T PeplineOutput { get; set; }
    }

    public interface ICommand
    {
        string Name { get; }
        CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args);
    }
}
