﻿using System;
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
            string query = 
                @"if exists (select 1 from master.dbo.sysdatabases where name = '{0}') 
                begin
                    alter database [{0}] set single_user with rollback immediate
                    drop database [{0}]
                end";
            query = string.Format(query, TestGlobals.SqlDatabase);
            ExecuteSqlMasterQuery(query);
        }

        private static void CreateDatabase()
        {
            ExecuteSqlMasterQuery(string.Format("create database [{0}]", TestGlobals.SqlDatabase));
        }
    }
}
