using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using Nkv.Sql;

namespace Nkv.Tests
{
    [TestClass]
    public class TestConfiguration
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            DropDatabase();
            CreateDatabase();

            TestGlobals.SqlConnectionProvider = new SqlConnectionProvider(TestGlobals.SqlConnectionString);
        }

        private static void ExecuteSqlMasterQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(TestGlobals.SqlMasterConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void DropDatabase()
        {
            string query = string.Format("if exists (select 1 from master.dbo.sysdatabases where name = '{0}') drop database [{0}]", TestGlobals.SqlDatabase);
            ExecuteSqlMasterQuery(query);
        }

        private static void CreateDatabase()
        {
            ExecuteSqlMasterQuery(string.Format("create database [{0}]", TestGlobals.SqlDatabase));
        }
    }
}
