using Nkv.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nkv.Sql
{
    public class SqlNkv : Nkv
    {
        public SqlNkv(SqlConnectionProvider connectionProvider) : base(connectionProvider)
        {
        }

        public override void CreateTable<T>()
        {
            string tableName = TableAttribute.GetTableName(typeof(T));
            string query = 
                @"if not exists (select 1 from sys.tables where name = '{0}')
                begin
                    create table [{0}] ([key] nvarchar(128) primary key not null, [value] nvarchar(max), [timestamp] datetime not null)
                end";
            query = string.Format(query, tableName);
            this.ExecuteNonQuery(query);
        }

    }
}
