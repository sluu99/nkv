using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Tests.Fixtures;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvSaveTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            nkv.CreateTable<Book>();
            nkv.Insert(Book.Generate());
        }
    }
}
