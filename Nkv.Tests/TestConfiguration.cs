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
        public static Dictionary<string, IConnectionProvider> ConnectionProviders { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var sqlTestHelper = new SqlTestHelper();
            sqlTestHelper.DropDatabase();
            sqlTestHelper.CreateDatabase();

            TestHelpers = new Dictionary<string, TestHelper>();
            TestHelpers["Nkv.Tests.Sql.SqlTestHelper"] = sqlTestHelper;

            ConnectionProviders = new Dictionary<string, IConnectionProvider>();
            ConnectionProviders["Nkv.Sql.SqlConnectionProvider"] = sqlTestHelper.ConnectionProvider;
        }

        public static Nkv CreateNkv(TestContext context)
        {
            var className = context.DataRow["ClassName"].ToString();
            var connectionProviderName = context.DataRow["ConnectionProvider"].ToString();

            var nkvType = Type.GetType(className);
            var connectionProvider = TestConfiguration.ConnectionProviders[connectionProviderName];

            return Activator.CreateInstance(nkvType, connectionProvider) as Nkv;
        }

        public static void ParseContext(TestContext context, out Nkv nkv, out TestHelper helper)
        {
            nkv = CreateNkv(context);
            helper = TestHelpers[context.DataRow["Helper"].ToString()];
        }
    }
}
