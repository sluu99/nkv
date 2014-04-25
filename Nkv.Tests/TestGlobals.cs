using Nkv.Sql;

namespace Nkv.Tests
{
    public class TestGlobals
    {
        public const string SqlMasterConnectionString = "server=localhost;database=master;trusted_connection=true";
        public const string SqlConnectionString = "server=localhost;database=nkv_test;trusted_connection=true";
        public const string SqlDatabase = "nkv_test";

        public static SqlConnectionProvider SqlConnectionProvider;
    }
}
