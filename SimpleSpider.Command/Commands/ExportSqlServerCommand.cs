using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using SqlMeta.Data.Repositories;

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
            var repository = new MetaRepository(connectionString);
            var tables = repository.GetTableInfo();
            var tableExists = tables.Exists(t => t.TableName == tableName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (!tableExists)
                {
                    var createSql = $@"CREATE TABLE [dbo].[{tableName}](
    [id] [int] PRIMARY KEY IDENTITY(1, 1) NOT NULL,
    [title] [nvarchar](255) NULL,
	[content] [text] NULL,
	[tag] [nvarchar](50) NULL,
	[source] [nvarchar](25) NULL,
	[publish] [bit] default(0) NOT NULL)";
                    connection.Execute(createSql);
                }
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
