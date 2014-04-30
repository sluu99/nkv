using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvDeleteTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Save(book);
                helper.AssertRowExists("Book", book.Key);

                session.Delete(book);
                helper.AssertRowExists("Book", book.Key, false);
            }
        }
    }
}
