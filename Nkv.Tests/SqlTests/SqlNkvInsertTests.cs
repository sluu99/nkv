using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Tests.Fixtures;

namespace Nkv.Tests.SqlTests
{
    [TestClass]
    public class SqlNkvInsertTests
    {
        [TestMethod]
        public void TestInsertion()
        {
            Nkv nkv = new SqlNkv(TestGlobals.SqlConnectionProvider);
            nkv.CreateTable<Book>();
            nkv.Insert(Book.Generate());
        }
    }
}
