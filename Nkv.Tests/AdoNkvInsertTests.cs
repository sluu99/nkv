using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System.Transactions;

namespace Nkv.Tests
{
    [TestClass]
    public class AdoNkvInsertTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);

                helper.AssertRowExists("Book", book.Key);

                Assert.AreEqual(1, book.Version);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_single_transaction_committed()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            var book1 = Book.Generate();
            var book2 = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
            }

            using (var tx = new TransactionScope())
            {
                using (var session = nkv.BeginSession())
                {
                    session.Insert(book1);
                    session.Insert(book2);
                    tx.Complete();
                }
            }

            helper.AssertRowExists("Book", book1.Key);
            helper.AssertRowExists("Book", book2.Key);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_single_transaction_not_committed()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            var book1 = Book.Generate();
            var book2 = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
            }

            using (var tx = new TransactionScope())
            {
                using (var session = nkv.BeginSession())
                {
                    session.Insert(book1);
                    session.Insert(book2);
                }
            }

            helper.AssertRowExists("Book", book1.Key, false);
            helper.AssertRowExists("Book", book2.Key, false);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_nested_transaction_scope_committed()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            var outterBook = Book.Generate();
            var innerBook = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
            }

            using (var outterTx = new TransactionScope())
            {
                using (var session = nkv.BeginSession())
                {
                    using (var innerTx = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        using (var innerSession = nkv.BeginSession())
                        {
                            innerSession.Insert(innerBook);
                            innerTx.Complete();
                        }
                    }

                    session.Insert(outterBook);
                }
            }

            helper.AssertRowExists("Book", outterBook.Key, false);
            helper.AssertRowExists("Book", innerBook.Key, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_key_should_be_case_sensitive()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                book.Key = book.Key.ToLower();
                session.Insert(book);

                helper.AssertRowExists("Book", book.Key);
                helper.AssertRowExists("Book", book.Key.ToUpper(), false);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_same_key_different_tables()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.CreateTable<BlogEntry>();

                string key = Guid.NewGuid().ToString();
                var book = Book.Generate();
                book.Key = key;
                var blogEntry = BlogEntry.Generate();
                blogEntry.Key = key;

                session.Insert(book);
                session.Insert(blogEntry);

                helper.AssertRowExists("Book", key);
                helper.AssertRowExists("BlogPosts", key);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsert_duplicate_key()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book1 = Book.Generate();
            var book2 = Book.Generate();
            book2.Key = book1.Key; // duplicate key

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book1);
                helper.AssertRowExists("Book", book1.Key);

                try
                {
                    session.Insert(book2);
                    Assert.Fail("Expecting an instance of NkvException with AckCode=ROW_EXISTS");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyExists, ex.AckCode);
                }
            }
        }
    }
}
