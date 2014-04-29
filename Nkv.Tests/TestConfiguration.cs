using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using Nkv.Sql;
using System.Collections.Generic;
using Nkv.Tests.Sql;
using Nkv.Interfaces;

namespace Nkv.Tests
{
    [TestClass]
    public class TestConfiguration
    {
        public static Dictionary<string, TestHelper> TestHelpers { get; set; }
        public static Dictionary<string, IProvider> Providers { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var sqlTestHelper = new SqlTestHelper();
            sqlTestHelper.DropDatabase();
            sqlTestHelper.CreateDatabase();

            TestHelpers = new Dictionary<string, TestHelper>();
            TestHelpers["Nkv.Tests.Sql.SqlTestHelper"] = sqlTestHelper;

            Providers = new Dictionary<string, IProvider>();
            Providers["Nkv.Sql.SqlProvider"] = sqlTestHelper.ConnectionProvider;
        }

        public static Nkv CreateNkv(TestContext context)
        {
            var providerName = context.DataRow["Provider"].ToString();
            var provider = TestConfiguration.Providers[providerName];
            return new Nkv(provider);
        }

        public static void ParseContext(TestContext context, out Nkv nkv, out TestHelper helper)
        {
            nkv = CreateNkv(context);
            helper = TestHelpers[context.DataRow["Helper"].ToString()];
        }
    }
}
