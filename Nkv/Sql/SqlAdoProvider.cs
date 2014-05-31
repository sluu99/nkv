using Nkv.Interfaces;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Nkv.Sql
{
    public class SqlAdoProvider : IAdoProvider
    {
        #region Static

        private static bool TemplatesPopulated = false;
        private static object PopulateTemplatePadLock = new object();
        private static string CreateTableTemplate;
        private static string CreateStoredProcsTemplate;
        private static string InsertEntityTemplate;
        private static string DeleteEntityTemplate;
        private static string UpdateEntityTemplate;
        private static string SelectAllTemplate;
        private static string SetLockTimestampTemplate;

        private static void PopulateQueryTemplates()
        {
            if (SqlAdoProvider.TemplatesPopulated)
            {
                return;
            }

            lock (SqlAdoProvider.PopulateTemplatePadLock)
            {
                if (SqlAdoProvider.TemplatesPopulated)
                {
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlCreateTable.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.CreateTableTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlCreateStoredProcs.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.CreateStoredProcsTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlInsertEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.InsertEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlDeleteEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.DeleteEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlUpdateEntity.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.UpdateEntityTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlSelectAll.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.SelectAllTemplate = reader.ReadToEnd().Trim();
                    }
                }

                using (var stream = assembly.GetManifestResourceStream("Nkv.Sql.Queries.SqlSetLockTimestamp.txt"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        SqlAdoProvider.SetLockTimestampTemplate = reader.ReadToEnd().Trim();
                    }
                }

                SqlAdoProvider.TemplatesPopulated = true;
            }
        }

        #endregion

        private string _connectionString;

        public SqlAdoProvider(string connectionString)
        {
            SqlAdoProvider.PopulateQueryTemplates();

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

            string query = "select [Key], [Value], [Timestamp], [Version] from [{0}] where [Key] = @key";
            query = string.Format(query, tableName);

            return query;
        }


        public string[] GetInitQueries(string tableName)
        {
            return new string[]
            {
                string.Format(SqlAdoProvider.CreateTableTemplate, tableName),
                string.Format(SqlAdoProvider.CreateStoredProcsTemplate, tableName),
                string.Format(SqlAdoProvider.InsertEntityTemplate, tableName),
                string.Format(SqlAdoProvider.DeleteEntityTemplate, tableName),
                string.Format(SqlAdoProvider.UpdateEntityTemplate, tableName),
                string.Format(SqlAdoProvider.SetLockTimestampTemplate, tableName)
            };
        }


        public string GetDeleteQuery(string tableName, out string keyParamName, out string versionParamName)
        {
            return InternalGetDeleteQuery(tableName, out keyParamName, out versionParamName, false);
        }

        private static string InternalGetDeleteQuery(string tableName, out string keyParamName, out string versionParamName, bool ignoreLock)
        {
            keyParamName = "@keyInput";
            versionParamName = "@versionInput";

            string query = "exec [nkv_Delete{0}Entity] @key=@keyInput, @version=@versionInput, @ignoreLock = {1}";

            return string.Format(query, tableName, ignoreLock ? 1 : 0);
        }


        public string GetInsertQuery(string tableName, out string keyParamName, out string valueParamName)
        {
            keyParamName = "@keyInput";
            valueParamName = "@valueInput";

            string query = "exec [nkv_Insert{0}Entity] @key=@keyInput, @value=@valueInput";

            return string.Format(query, tableName);
        }


        public string GetUpdateQuery(string tableName, out string keyParamName, out string valueParamName, out string versionParamName)
        {
            keyParamName = "@keyInput";
            valueParamName = "@valueInput";
            versionParamName = "@versionInput";

            string query = "exec [nkv_Update{0}Entity] @key=@keyInput, @value=@valueInput, @version=@versionInput";

            return string.Format(query, tableName);
        }


        public string GetCountQuery(string tableName)
        {
            return string.Format("select count_big(1) from [{0}]", tableName);
        }


        public string GetSelectPrefixQuery(string tableName, ref string prefix, out string prefixParamName)
        {
            prefixParamName = "@prefix";

            prefix = prefix.Replace("%", "[%]");
            prefix = prefix.Replace("_", "[_]");
            prefix += "%";

            string query = "select [Key], [Value], [Timestamp], [Version] from [{0}] where [Key] like @prefix";

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
                "select [Key], [Value], [Timestamp], [Version] from [{0}] where [Key] in ({1})",
                tableName,
                string.Join(",", keyParamNames)
            );
        }


        public string GetSelectAllQuery(string tableName, long skip, int take)
        {
            return string.Format(SqlAdoProvider.SelectAllTemplate, tableName, skip + 1, take + skip);
        }


        public string GetLockQuery(string tableName, out string keyParamName, out string versionParamName)
        {
            keyParamName = "@keyInput";
            versionParamName = "@versionInput";

            string query = "exec [nkv_Set{0}LockTimestamp] @key=@keyInput, @version=@versionInput, @isLock=1";

            return string.Format(query, tableName);
        }

        public string GetUnlockQuery(string tableName, out string keyParamName, out string versionParamName)
        {
            keyParamName = "@keyInput";
            versionParamName = "@versionInput";

            string query = "exec [nkv_Set{0}LockTimestamp] @key=@keyInput, @version=@versionInput, @isLock=0";

            return string.Format(query, tableName);
        }
        
        public string GetForceDeleteQuery(string tableName, out string keyParamName, out string versionParamName)
        {
            return InternalGetDeleteQuery(tableName, out keyParamName, out versionParamName, true);
        }
    }
}
