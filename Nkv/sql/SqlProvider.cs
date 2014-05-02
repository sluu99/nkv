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
                declare @rowCount int;
                declare @ackCode varchar(32) = 'SUCCESS'
                declare @recordTimestamp datetime
                declare @returnTimestamp datetime

                set @returnTimestamp = @newTimestamp;

                update [{0}] set [value] = @value, [timestamp] = @newTimestamp
                where [key] = @key and [timestamp] = @oldTimestamp;

                set @rowCount = @@rowcount;

                if @rowCount <> 1
                begin
	                set @recordTimestamp = null;
	                select @recordTimestamp = [timestamp] from [{0}] where [key] = @key;

	                if @recordTimestamp is not null -- record found
	                begin
		                if @recordTimestamp <> @oldTimestamp
		                begin
			                set @ackCode = 'TIMESTAMP_MISMATCH';
                            set @returnTimestamp = @recordTimestamp;
		                end
		                else
		                begin
			                set @ackCode = 'UNKNOWN';
		                end
	                end
	                else
	                begin
		                insert into [{0}]([key], [value], [timestamp]) values(@key, @value, @newTimestamp);
		
		                set @rowCount = @@rowCount;
		                if @rowCount <> 1
		                begin
			                set @ackCode = 'UNKNOWN';
		                end
	                end
                end

                select @rowCount [RowCount], @returnTimestamp [Timestamp], @ackCode [AckCode]";

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
            return string.Format(query.Trim(), tableName);
        }


        public string GetDeleteQuery(string tableName, out string keyParamName, out string timestampParamName)
        {
            keyParamName = "@key";
            timestampParamName = "@timestamp";

            string query = @"
                declare @rowTimestamp datetime;
                declare @rowCount int;
                declare @ackCode varchar(32) = 'SUCCESS'

                delete from [{0}] where [key] = @key and [timestamp] = @timestamp;
                
                set @rowCount = @@rowcount;
                if @rowCount <> 1
                begin
                    select @rowTimestamp = [timestamp] from [{0}] where [key] = @key;
                    if @rowTimestamp is null
                    begin
                        set @ackCode = 'NOT_EXISTS';
                    end
                    else
                    begin
                        if @rowTimestamp <> @timestamp
                        begin
                            set @ackCode = 'TIMESTAMP_MISMATCH';            
                        end
                        else
                        begin
                            set @ackCode = 'UNKNOWN';
                        end
                    end
                end

                select @rowCount [RowCount], @rowTimestamp [Timestamp], @ackCode [AckCode]";

            return string.Format(query.Trim(), tableName);
        }


        public string GetInsertQuery(string tableName, out string keyParamName, out string valueParamName)
        {
            keyParamName = "@key";
            valueParamName = "@value";

            string query = @"
                declare @timestamp datetime = null;
                declare @ackCode varchar(32);
                declare @rowCount int;

                select @timestamp = [timestamp]
                from [{0}]
                where [key] = @key;

                if @timestamp is not null
                begin
	                set @ackCode = 'ROW_EXISTS';
	                set @rowCount = 0;
                end
                else
                begin
	                set @timestamp = sysutcdatetime();
	
	                insert into [{0}]([key], [value], [timestamp])
	                values(@key, @value, @timestamp);
	
	                set @rowCount = @@ROWCOUNT;
	                if @rowCount = 1
	                begin
		                set @ackCode = 'SUCCESS';
	                end
	                else
	                begin
		                set @ackCode = 'UNKNOWN';
	                end
                end

                select @rowCount [RowCount], @timestamp [Timestamp], @ackCode [AckCode];";

            return string.Format(query.Trim(), tableName);
        }


        public string GetUpdateQuery(string tableName, out string keyParamName, out string valueParamName, out string timestampParamName)
        {
            keyParamName = "@key";
            valueParamName = "@value";
            timestampParamName = "@oldTimestamp";

            string query = @"
                declare @rowTimestamp datetime = sysutcdatetime();
                declare @rowCount int = 0;
                declare @ackCode varchar(32);

                update [{0}] set
	                [value] = @value,
	                [timestamp] = @rowTimestamp
                where [key] = @key and [timestamp] = @oldTimestamp;

                set @rowCount = @@ROWCOUNT;
                set @ackCode = 'SUCCESS';

                if @rowCount <> 1
                begin
	                set @rowTimestamp = null;
	                select @rowTimestamp = [timestamp] from [{0}] where [key] = @key;
	
	                if @rowTimestamp is null
	                begin
		                set @ackCode = 'NOT_EXISTS';
	                end
	                else
	                begin
		                if @oldTimestamp <> @rowTimestamp
		                begin
			                set @ackCode = 'TIMESTAMP_MISMATCH';
		                end
		                else
		                begin
			                set @ackCode = 'UNKNOWN';
		                end
	                end
                end

                select @rowCount [RowCount], @rowTimestamp [Timestamp], @ackCode [AckCode];";

            return string.Format(query.Trim(), tableName);
        }


        public string GetCountQuery(string tableName)
        {
            return string.Format("select count_big(1) from [{0}]", tableName);
        }
    }
}
