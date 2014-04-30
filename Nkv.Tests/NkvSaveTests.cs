using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System;
using System.Threading;
using System.Transactions;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvSaveTests
    {
        public TestContext TestContext { get; set; }

        #region Insertion

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Save(book);

                helper.AssertRowExists("Book", book.Key);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_single_transaction_committed()
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
                    session.Save(book1);
                    session.Save(book2);
                    tx.Complete();
                }
            }

            helper.AssertRowExists("Book", book1.Key);
            helper.AssertRowExists("Book", book2.Key);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_single_transaction_not_committed()
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
                    session.Save(book1);
                    session.Save(book2);
                }
            }

            helper.AssertRowExists("Book", book1.Key, false);
            helper.AssertRowExists("Book", book2.Key, false);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_nested_transaction_scope_committed()
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
                            innerSession.Save(innerBook);
                            innerTx.Complete();
                        }
                    }

                    session.Save(outterBook);
                }
            }

            helper.AssertRowExists("Book", outterBook.Key, false);
            helper.AssertRowExists("Book", innerBook.Key, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_key_should_be_case_sensitive()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                book.Key = book.Key.ToLower();
                session.Save(book);

                helper.AssertRowExists("Book", book.Key);
                helper.AssertRowExists("Book", book.Key.ToUpper(), false);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_same_key_different_tables()
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

                session.Save(book);
                session.Save(blogEntry);

                helper.AssertRowExists("Book", key);
                helper.AssertRowExists("BlogPosts", key);
            }
        }

        #endregion

        #region Update

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
                session.Save(book);
                session.Save(book);
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

                session.Save(book); // insert
                helper.AssertRowExists("Book", book.Key);

                var bookInstance2 = session.Select<Book>(book.Key);

                Thread.Sleep(1000); // make sure the time changes                
                session.Save(bookInstance2);
                Assert.AreNotEqual(book.Timestamp, bookInstance2.Timestamp);

                try
                {
                    session.Save(book);
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=TIMESTAMP_MISMATCH");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual("TIMESTAMP_MISMATCH", ex.AckCode, ignoreCase: true);
                }
            }
            
        }

        #endregion
    }
}
