using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
namespace SimpleSpider.Command.Commands
{
    public class ExportSqlServerCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "export-sqlserver";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            var connectionString = args[0];
            var tableName = args[1];
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var item in ResultCommand.Rows)
                {
                    string cloumnStr = "";
                    string paramStr = "";
                    var sqlParm = new DynamicParameters();
                    foreach (var cloumn in item)
                    {
                        cloumnStr += $"[{cloumn.Key}],";
                        paramStr += $"@{cloumn.Key},";
                        sqlParm.Add(cloumn.Key, cloumn.Value);
                    }
                    cloumnStr = cloumnStr.TrimEnd(',');
                    paramStr = paramStr.TrimEnd(',');
                    var sql = $"DELETE [{tableName}] WHERE title=@title and source=@source;INSERT INTO [{tableName}] ({cloumnStr}) VALUES ({paramStr})";
                    connection.Execute(sql, sqlParm);
                }
            }
            return new CommandResult() { Success = true };
        }
    }
}
