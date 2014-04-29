using Nkv.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Nkv.Sql
{
    public class SqlProvider : IProvider
    {
        private string _connectionString;

        public SqlProvider(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("SQL Server connection string is required");
            }

            _connectionString = connectionString;
        }

        public System.Data.IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public string Escape(string x)
        {
            return string.Format("[{0}]", x);
        }

        public IDbDataParameter CreateParameter(string name, SqlDbType type, object value, int size = 0)
        {
            SqlParameter p;
            if (size != 0)
            {
                p = new SqlParameter(name, type, size);
            }
            else
            {
                p = new SqlParameter(name, type);
            }
            p.Value = value;

            return p;
        }

        public string GetSaveQuery(string tableName, out string keyParamName, out string valueParamName, out string timestampParamName)
        {
            keyParamName = "@key";
            valueParamName = "@value";
            timestampParamName = "@oldTimestamp";

            string query = @"
                declare @newTimestamp datetime = sysutcdatetime();

                merge into [{0}] as [Target]
                using (select @key, @oldTimestamp) as [Source] ([key], [timestamp])
                on ([Target].[key] = [Source].[key] and [Target].[timestamp] = [Source].[timestamp])
                when matched then
	                update set [value] = @value, [timestamp] = sysutcdatetime()	
                when not matched by target then
	                insert([key], [value], [timestamp])
	                values(@key, @value, @newTimestamp);

                select @@rowcount [RowCount], @newTimestamp [Timestamp]";

            return string.Format(query.Trim(), tableName);
        }

        public string GetSelectQuery(string tableName, out string keyParamName)
        {
            keyParamName = "@key";

            string query = "select [value], [timestamp] from [{0}] where [key] = @key";
            query = string.Format(query, tableName);

            return query;
        }


        public string GetCreateTableQuery(string tableName)
        {
            string query =
                @"if not exists (select 1 from sys.tables where name = '{0}')
                begin
                    create table [{0}] (
                        [key] nvarchar(128) collate SQL_Latin1_General_CP1_CS_AS primary key not null, 
                        [value] nvarchar(max), 
                        [timestamp] datetime not null)
                end";
            return string.Format(query, tableName);
        }
    }
}
