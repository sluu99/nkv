using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
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
            TestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            nkv.CreateTable<Book>();

            var book = Book.Generate();
            nkv.Save(book);

            var book2 = nkv.Select<Book>(book.Key);

            Assert.AreEqual(book.Key, book2.Key);
            Assert.AreEqual(book.Category, book2.Category);
            Assert.AreEqual(book.Title, book2.Title);
            Assert.AreEqual(book.Timestamp, book2.Timestamp);
            Assert.AreEqual(book.Pages, book2.Pages);
            Assert.AreEqual(book.ReleaseDate, book2.ReleaseDate);
            Assert.AreEqual(book.Abstract, book2.Abstract);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect_not_exists()
        {
            Nkv nkv;
            TestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            nkv.CreateTable<Book>();

            // make sure there's something in the database
            for (int i = 0; i < 10; i++)
            {
                nkv.Save(Book.Generate()); 
            }

            Assert.IsNull(nkv.Select<Book>(Guid.NewGuid().ToString()));
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect_same_key_different_tables()
        {
            Nkv nkv;
            TestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            nkv.CreateTable<Book>();
            nkv.CreateTable<BlogEntry>();

            var book = Book.Generate();
            var blogEntry = BlogEntry.Generate();

            using (var tx = new TransactionScope())
            {
                nkv.Save(book);
                nkv.Save(blogEntry);

                tx.Complete();
            }

            helper.AssertRowExists("Book", book.Key);
            helper.AssertRowExists("BlogPosts", blogEntry.Key);
            helper.AssertRowExists("Book", blogEntry.Key, false);
            helper.AssertRowExists("BlogPosts", book.Key, false);
        }
    }
}
