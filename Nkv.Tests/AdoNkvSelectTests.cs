using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System;
using System.Transactions;
using System.Linq;
using Nkv.Attributes;

namespace Nkv.Tests
{
    [TestClass]
    public class AdoNkvSelectTests
    {
        public TestContext TestContext { get; set; }

        #region Select

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();

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

                Assert.AreNotEqual(0, book2.Version);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelect_not_exists()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();

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
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();
                session.Init<BlogEntry>();

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
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();
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
                Assert.AreEqual(0, entities.Count(x => x.Version < 1));
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectPrefix_not_exists()
        {
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();
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
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();
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
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();
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
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(books.Select(b => b.Key).Take(5).ToArray());

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(5, selectManyBooks.Length);

                Assert.AreEqual(0, selectManyBooks.Count(x => x.Version < 1));
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectMany_partial_exists()
        {
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(new string[] { books[0].Key, books[1].Key, Guid.NewGuid().ToString() });

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(2, selectManyBooks.Length);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectMany_none_exists()
        {
            AdoNkv nkv = TestConfiguration.CreateNkv(TestContext);

            using (var session = nkv.BeginSession())
            {
                session.Init<Book>();

                var books = new Book[10];

                for (int i = 0; i < books.Length; i++)
                {
                    books[i] = Book.Generate();
                    session.Insert(books[i]);
                }

                var selectManyBooks = session.SelectMany<Book>(new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() });

                Assert.IsNotNull(selectManyBooks);
                Assert.AreEqual(0, selectManyBooks.Length);
            }
        }

        #endregion

        #region SelectAll

        [Table("SelectAllTest")]
        private class SelectAllTestFixture : Entity
        {
            public SelectAllTestFixture()
            {
                Key = Guid.NewGuid().ToString();
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestSelectAll()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);

            var fixtures = new SelectAllTestFixture[10];
            for (int i = 0; i < fixtures.Length; i++)
            {
                fixtures[i] = new SelectAllTestFixture();
                fixtures[i].Key = i.ToString();
            }

            using (var session = nkv.BeginSession())
            {
                session.Init<SelectAllTestFixture>();

                var entities = session.SelectAll<SelectAllTestFixture>(0, int.MaxValue);
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);

                session.Insert(fixtures[0]);

                entities = session.SelectAll<SelectAllTestFixture>(0, int.MaxValue);
                Assert.AreEqual(1, entities.Length);

                entities = session.SelectAll<SelectAllTestFixture>(1, int.MaxValue);
                Assert.IsNotNull(entities);
                Assert.AreEqual(0, entities.Length);

                for (int i = 1; i < fixtures.Length; i++)
                {
                    session.Insert(fixtures[i]);
                }

                entities = session.SelectAll<SelectAllTestFixture>(0, 5);
                Assert.AreEqual(5, entities.Length);
                Assert.AreEqual(5, entities.Count(x => int.Parse(x.Key) >= 0 && int.Parse(x.Key) <= 4));
                Assert.AreEqual(0, entities.Count(x => x.Version < 1));

                entities = session.SelectAll<SelectAllTestFixture>(3, 2);
                Assert.AreEqual(2, entities.Length);
                Assert.AreEqual(2, entities.Count(x => int.Parse(x.Key) >= 3 && int.Parse(x.Key) <= 4));
                Assert.AreEqual(0, entities.Count(x => x.Version < 1));

                entities = session.SelectAll<SelectAllTestFixture>(7, 4);
                Assert.AreEqual(3, entities.Length);
                Assert.AreEqual(3, entities.Count(x => int.Parse(x.Key) >= 7 && int.Parse(x.Key) <= 9));
                Assert.AreEqual(0, entities.Count(x => x.Version < 1));
            }
        }

        #endregion
    }
}
