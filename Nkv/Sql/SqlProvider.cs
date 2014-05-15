using Nkv.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Nkv.Sql
{
    public class SqlProvider : IProvider
    {
        #region Static

        private static bool TemplatesPopulated = false;
        private static object PopulateTemplatePadLock = new object();
        private static string CreateTableTemplate;
        private static string CreateStoredProcsTemplate;
        private static string InsertEntityTemplate;
        private static string DeleteEntityTemplate;
        private static string UpdateEntityTemplate;
        private static string SelectManyTemplate;

        private static void PopulateQueryTemplates()
        {
            if (SqlProvider.TemplatesPopulated)
            {
                return;
            }

            lock (SqlProvider.PopulateTemplatePadLock)
            {
                if (SqlProvider.TemplatesPopulated)
                {
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlCreateTable.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.CreateTableTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlCreateStoredProcs.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.CreateStoredProcsTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlInsertEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.InsertEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlDeleteEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.DeleteEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlUpdateEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.UpdateEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlSelectAll.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlProvider.SelectManyTemplate = reader.ReadToEnd().Trim();
                    }
                }

                SqlProvider.TemplatesPopulated = true;
            }
        }

        #endregion

        private string _connectionString;

        public SqlProvider(string connectionString)
        {
            SqlProvider.PopulateQueryTemplates();

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


        public string GetSelectQuery(string tableName, out string keyParamName)
        {
            keyParamName = "@key";

            string query = "select [Key], [Value], [Timestamp] from dbo.[{0}] where [Key] = @key";
            query = string.Format(query, tableName);

            return query;
        }


        public string[] GetCreateTableQueries(string tableName)
        {
            return new string[]
            {
                string.Format(SqlProvider.CreateTableTemplate, tableName),
                string.Format(SqlProvider.CreateStoredProcsTemplate, tableName),
                string.Format(SqlProvider.InsertEntityTemplate, tableName),
                string.Format(SqlProvider.DeleteEntityTemplate, tableName),
                string.Format(SqlProvider.UpdateEntityTemplate, tableName),
            };
        }


        public string GetDeleteQuery(string tableName, out string keyParamName, out string timestampParamName)
        {
            keyParamName = "@keyInput";
            timestampParamName = "@timestampInput";

            string query = "exec dbo.[nkv_Delete{0}Entity] @key=@keyInput, @timestamp=@timestampInput";

            return string.Format(query, tableName);
        }


        public string GetInsertQuery(string tableName, out string keyParamName, out string valueParamName)
        {
            keyParamName = "@keyInput";
            valueParamName = "@valueInput";

            string query = "exec dbo.[nkv_Insert{0}Entity] @key=@keyInput, @value=@valueInput";

            return string.Format(query, tableName);
        }


        public string GetUpdateQuery(string tableName, out string keyParamName, out string valueParamName, out string timestampParamName)
        {
            keyParamName = "@keyInput";
            valueParamName = "@valueInput";
            timestampParamName = "@oldTimestampInput";

            string query = "exec dbo.[nkv_Update{0}Entity] @key=@keyInput, @value=@valueInput, @oldTimestamp=@oldTimestampInput";

            return string.Format(query, tableName);
        }


        public string GetCountQuery(string tableName)
        {
            return string.Format("select count_big(1) from dbo.[{0}]", tableName);
        }


        public string GetSelectPrefixQuery(string tableName, ref string prefix, out string prefixParamName)
        {
            prefixParamName = "@prefix";

            prefix = prefix.Replace("%", "[%]");
            prefix = prefix.Replace("_", "[_]");
            prefix += "%";

            string query = "select [Key], [Value], [Timestamp] from dbo.[{0}] where [Key] like @prefix";

            return string.Format(query, tableName);
        }


        public string GetSelectManyQuery(string tableName, int keyCount, out string[] keyParamNames)
        {
            keyParamNames = new string[keyCount];
            for (int i = 0; i < keyCount; i++)
            {
                keyParamNames[i] = "@key" + i.ToString();
            }

            return string.Format(
                "select [Key], [Value], [Timestamp] from dbo.[{0}] where [Key] in ({1})",
                tableName,
                string.Join(",", keyParamNames)
            );
        }


        public string GetSelectAllQuery(string tableName, int skip, int take)
        {
            return string.Format(tableName, skip + 1, take + skip);
        }
    }
}
