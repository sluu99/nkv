using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System;
using System.Transactions;
using System.Linq;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvSelectTests
    {
        public TestContext TestContext { get; set; }

        #region Select

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

        #endregion

        #region SelectPrefix

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectPrefix()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                var book = Book.Generate();

                session.Insert(book);

                for (int i = 0; i < 10; i++)
                {
                    var b2 = Book.Generate();
                    b2.Key = book.Key + "_" + b2.Key;
                    session.Insert(b2);
                }

                var entities = session.SelectPrefix<Book>(book.Key);
                Assert.IsNotNull(entities);
                Assert.AreEqual(11, entities.Length); // 11, including the original book itself

                entities = session.SelectPrefix<Book>(book.Key + "_");
                Assert.IsNotNull(entities);
                Assert.AreEqual(10, entities.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectPrefix_not_exists()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                var book = Book.Generate();

                session.Insert(book);

                for (int i = 0; i < 10; i++)
                {
                    var b2 = Book.Generate();
                    b2.Key = book.Key + "_" + b2.Key;
                    session.Insert(b2);
                }

                var entities = session.SelectPrefix<Book>(Guid.NewGuid().ToString());
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectPrefix_wildcard_prefix()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                var book = Book.Generate();

                session.Insert(book);

                var entities = session.SelectPrefix<Book>(book.Key);
                Assert.IsNotNull(entities);
                Assert.AreEqual(1, entities.Length);

                entities = session.SelectPrefix<Book>("%" + book.Key);
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);

                entities = session.SelectPrefix<Book>("_" + book.Key.Substring(1));
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectPrefix_wildcard_suffix()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                var book = Book.Generate();

                session.Insert(book);

                var entities = session.SelectPrefix<Book>(book.Key);
                Assert.IsNotNull(entities);
                Assert.AreEqual(1, entities.Length);

                entities = session.SelectPrefix<Book>(book.Key + "%");
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);

                entities = session.SelectPrefix<Book>(book.Key.Substring(0, book.Key.Length - 1) + "_");
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);
            }
        }

        #endregion

        #region SelectMany

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectMany()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(books.Select(b => b.Key).Take(5).ToArray());

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(5, selectManyBooks.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectMany_partial_exists()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(books[0].Key, books[1].Key, Guid.NewGuid().ToString());

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(2, selectManyBooks.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectMany_none_exists()
        {
            Nkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(0, selectManyBooks.Length);
            }
        }

        #endregion
    }
}
