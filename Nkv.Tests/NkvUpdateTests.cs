using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System.Threading;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvUpdateTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUpdate()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                var book = Book.Generate();
                
                session.Insert(book);
                helper.AssertRowExists("Book", book.Key);
                DateTime timestamp = book.Timestamp;

                Thread.Sleep(1000);

                session.Update(book);
                helper.AssertRowExists("Book", book.Key);
                Assert.AreNotEqual(timestamp, book.Timestamp);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUpdate_entity_modified()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book); // insert
                helper.AssertRowExists("Book", book.Key);

                var bookInstance2 = session.Select<Book>(book.Key);

                Thread.Sleep(1000); // make sure the time changes                
                session.Update(bookInstance2);
                Assert.AreNotEqual(book.Timestamp, bookInstance2.Timestamp);

                try
                {
                    session.Update(book);
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=TIMESTAMP_MISMATCH");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.TimestampMismatch, ex.AckCode);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUpdate_entity_not_exists()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                try
                {
                    session.Update(book);
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=NOT_EXISTS");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyNotFound, ex.AckCode);
                }
            }
        }
    }
}
