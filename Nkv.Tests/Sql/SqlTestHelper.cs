using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nkv.Tests.Sql
{
    public class SqlTestHelper : ITestHelper
    {
        #region Static fields
                
        public const string SqlConnectionString = "server=localhost;database=nkv_test;trusted_connection=true";
        public const string SqlDatabase = "nkv_test";

        private const string SqlMasterConnectionString = "server=localhost;database=master;trusted_connection=true";

        private SqlConnectionProvider _connectionProviderField;
        private object _padLock = new object();

        public SqlConnectionProvider ConnectionProvider
        {
            get
            {
                if (_connectionProviderField == null)
                {
                    lock (_padLock)
                    {
                        if (_connectionProviderField == null)
                        {
                            _connectionProviderField = new SqlConnectionProvider(SqlConnectionString);
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
            string query =
                @"if exists (select 1 from master.dbo.sysdatabases where name = '{0}') 
                begin
                    alter database [{0}] set single_user with rollback immediate
                    drop database [{0}]
                end";
            query = string.Format(query, SqlDatabase);
            ExecuteSqlMasterQuery(query);
        }

        public void CreateDatabase()
        {
            ExecuteSqlMasterQuery(string.Format("create database [{0}]", SqlDatabase));
        }
    }
}
