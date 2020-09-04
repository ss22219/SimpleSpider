using System.Collections.Generic;

namespace SimpleSpider.Command
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandResult
    {
        public bool Success { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public object PipelineOutput { get; set; }
    }

    public class CommandResult<T> : CommandResult
    {
        public new T PipelineOutput { get; set; }
    }

    public interface ICommand
    {
        string Name { get; }
        CommandResult Excute(object pipelineInput, Dictionary<string, string> data, string[] args);
    }
}
