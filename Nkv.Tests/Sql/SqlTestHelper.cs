using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nkv.Tests.Sql
{
    public class SqlTestHelper : IAdoTestHelper
    {
        #region Static fields

        public static string SqlConnectionString { get; private set; }
        public static string SqlDatabase { get; private set; }

        private static string SqlMasterConnectionString { get; set; }

        static SqlTestHelper()
        {
            SqlConnectionString =
                Environment.GetEnvironmentVariable("Nkv_Tests_SqlConnectionString") ??
                "server=localhost;database=nkv_test;trusted_connection=true";
            SqlDatabase = Environment.GetEnvironmentVariable("Nkv_Tests_SqlDatabase") ?? "nkv_test";
            SqlMasterConnectionString =
                Environment.GetEnvironmentVariable("Nkv_Tests_SqlMasterConnectionString") ??
                "server=localhost;database=master;trusted_connection=true";
        }


        private SqlAdoProvider _connectionProviderField;
        private object _padLock = new object();

        public SqlAdoProvider ConnectionProvider
        {
            get
            {
                if (_connectionProviderField == null)
                {
                    lock (_padLock)
                    {
                        if (_connectionProviderField == null)
                        {
                            _connectionProviderField = new SqlAdoProvider(SqlConnectionString);
                        }
                    }
                }
                return _connectionProviderField;
            }
            set { _connectionProviderField = value; }
        }

        #endregion

        #region ITestHelper

        public void AssertTableExists(string tableName)
        {
            using (var conn = new SqlConnection(SqlConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("select 1 from sys.tables where name = '{0}'", tableName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        Assert.IsTrue(reader.Read(), string.Format("Cannot find table {0}", tableName));
                    }
                }
            }
        }


        public void AssertRowExists(string tableName, string key, bool exists = true)
        {
            using (var conn = new SqlConnection(SqlConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("select 1 from [{0}] where [Key] = @key", tableName);
                    cmd.Parameters.Add("@key", System.Data.SqlDbType.NVarChar, 128).Value = key;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (exists)
                        {
                            Assert.IsTrue(reader.Read(), string.Format("Cannot find row; table = {0}, key = {1}", tableName, key));
                        }
                        else
                        {
                            Assert.IsFalse(reader.Read(), string.Format("Row not expected to exist; table = {0}, key = {1}", tableName, key));
                        }
                    }
                }
            }
        }


        #endregion

        private void ExecuteSqlMasterQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(SqlMasterConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DropDatabase()
        {
            if (string.Equals(SqlDatabase, "master", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string query =
                @"if exists (select 1 from dbo.sysdatabases where name = '{0}') 
                begin
                    alter database [{0}] set single_user with rollback immediate
                    drop database [{0}]
                end";
            query = string.Format(query, SqlDatabase);
            ExecuteSqlMasterQuery(query);
        }

        public void CreateDatabase()
        {
            if (string.Equals(SqlDatabase, "master", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ExecuteSqlMasterQuery(string.Format("create database [{0}]", SqlDatabase));
        }
    }
}
