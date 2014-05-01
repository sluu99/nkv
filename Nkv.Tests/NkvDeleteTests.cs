using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System.Threading;

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

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete_modified_entity()
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

                Thread.Sleep(1000); // make sure the time has changed
                var bookInstance2 = session.Select<Book>(book.Key);                               
                session.Save(bookInstance2);

                try
                {
                    session.Delete(book);
                    Assert.Fail("Expecting an instance of NkvException with AckCode=TIMESTAMP_MISMATCH");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual("TIMESTAMP_MISMATCH", ex.AckCode);
                    Assert.AreEqual(bookInstance2.Timestamp, ex.Timestamp);
                }
            }
        }
    }
}
