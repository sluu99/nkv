using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv.Tests.SqlTests
{
    public static class SqlTestHelper
    {
        public static void AssertTableExists(string tableName)
        {
            using (var conn = TestGlobals.SqlConnectionProvider.GetConnection())
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
    }
}
