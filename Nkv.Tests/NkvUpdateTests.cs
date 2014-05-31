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
                
                long version = book.Version;                               

                session.Update(book);
                helper.AssertRowExists("Book", book.Key);
                Assert.AreNotEqual(version, book.Version);
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

                session.Update(bookInstance2);
                Assert.AreNotEqual(book.Version, bookInstance2.Version);

                try
                {
                    session.Update(book);
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=VersionMismatch");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.VersionMismatch, ex.AckCode);
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
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=KeyNotFound");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyNotFound, ex.AckCode);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUpdate_entity_locked()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);
                session.Lock(book);

                book.Pages++;

                try
                {
                    session.Update(book);
                    Assert.Fail("Expecting an NkvException with AckCode=EntityLocked");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.EntityLocked, ex.AckCode);
                }

                Assert.AreEqual(book.Pages - 1, session.Select<Book>(book.Key).Pages); // make sure the value did not change

                session.Unlock(book);
                session.Update(book);
            }
        }
    }
}
