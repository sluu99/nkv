using Nkv.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nkv.Sql
{
    public class SqlNkv : Nkv
    {
        public SqlNkv(SqlConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override void CreateTable<T>()
        {
            string tableName = TableAttribute.GetTableName(typeof(T));
            string query =
                @"if not exists (select 1 from sys.tables where name = '{0}')
                begin
                    create table [{0}] (
                        [key] nvarchar(128) collate SQL_Latin1_General_CP1_CS_AS primary key not null, 
                        [value] nvarchar(max), 
                        [timestamp] datetime not null)
                end";
            query = string.Format(query, tableName);
            ExecuteNonQuery(query);
        }

        protected override string Escape(string x)
        {
            return string.Format("[{0}]", x);
        }

        protected override IDbDataParameter CreateParameter(string name, SqlDbType type, object value, int size = 0)
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

        protected override string GetSaveQuery(string tableName, out string keyParamName, out string valueParamName)
        {
            keyParamName = "@key";
            valueParamName = "@value";

            string query = @"
                declare @timestamp datetime = sysutcdatetime()
                insert into [{0}]([key], [value], [timestamp]) values({1}, {2}, @timestamp)
                select @@rowcount [RowCount], @timestamp [Timestamp]";

            return string.Format(query.Trim(), tableName, keyParamName, valueParamName);
        }
    }
}
