using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System;
using System.Transactions;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvSelectTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                var book = Book.Generate();
                session.Insert(book);

                var book2 = session.Select<Book>(book.Key);

                Assert.AreEqual(book.Key, book2.Key);
                Assert.AreEqual(book.Category, book2.Category);
                Assert.AreEqual(book.Title, book2.Title);
                Assert.AreEqual(book.Timestamp, book2.Timestamp);
                Assert.AreEqual(book.Pages, book2.Pages);
                Assert.AreEqual(book.ReleaseDate, book2.ReleaseDate);
                Assert.AreEqual(book.Abstract, book2.Abstract);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect_not_exists()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                // make sure there's something in the database
                for (int i = 0; i < 10; i++)
                {
                    session.Insert(Book.Generate());
                }

                Assert.IsNull(session.Select<Book>(Guid.NewGuid().ToString()));
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect_keys_on_different_tables()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.CreateTable<BlogEntry>();

                var book = Book.Generate();
                var blogEntry = BlogEntry.Generate();                

                session.Insert(book);
                session.Insert(blogEntry);
                
                Assert.IsNotNull(session.Select<Book>(book.Key));
                Assert.IsNotNull(session.Select<BlogEntry>(blogEntry.Key));
                Assert.IsNull(session.Select<Book>(blogEntry.Key));
                Assert.IsNull(session.Select<BlogEntry>(book.Key));
            }
        }
    }
}
