using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using Nkv.Sql;
using System.Collections.Generic;
using Nkv.Tests.Sql;

namespace Nkv.Tests
{
    [TestClass]
    public class TestConfiguration
    {
        public static Dictionary<string, IAdoTestHelper> TestHelpers { get; set; }
        public static Dictionary<string, IAdoProvider> Providers { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var sqlTestHelper = new SqlTestHelper();
            sqlTestHelper.DropDatabase();
            sqlTestHelper.CreateDatabase();

            TestHelpers = new Dictionary<string, IAdoTestHelper>();
            TestHelpers["Nkv.Tests.Sql.SqlTestHelper"] = sqlTestHelper;

            Providers = new Dictionary<string, IAdoProvider>();
            Providers["Nkv.Sql.SqlProvider"] = sqlTestHelper.ConnectionProvider;
        }

        public static AdoNkv CreateNkv(TestContext context)
        {
            var providerName = context.DataRow["Provider"].ToString();
            var provider = TestConfiguration.Providers[providerName];
            return new AdoNkv(provider);
        }

        public static void ParseContext(TestContext context, out AdoNkv nkv, out IAdoTestHelper helper)
        {
            nkv = CreateNkv(context);
            helper = TestHelpers[context.DataRow["Helper"].ToString()];
        }
    }
}
