using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Tests.Fixtures;
using System;
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
            nkv.CreateTable<Book>();
            nkv.Save(book);

            helper.AssertRowExists("Book", book.Key);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_single_transaction_committed()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            nkv.CreateTable<Book>();

            var book1 = Book.Generate();
            var book2 = Book.Generate();

            using (var tx = new TransactionScope())
            {
                nkv.Save(book1);
                nkv.Save(book2);
                tx.Complete();

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

            nkv.CreateTable<Book>();

            var book1 = Book.Generate();
            var book2 = Book.Generate();

            using (var tx = new TransactionScope())
            {
                nkv.Save(book1);
                nkv.Save(book2);                
            }

            helper.AssertRowExists("Book", book1.Key, false);
            helper.AssertRowExists("Book", book2.Key, false);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_key_should_be_case_sensitive()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            var book = Book.Generate();

            nkv.CreateTable<Book>();            
            
            book.Key = book.Key.ToLower();
            nkv.Save(book);

            helper.AssertRowExists("Book", book.Key);
            helper.AssertRowExists("Book", book.Key.ToUpper(), false);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_same_key_different_tables()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];
            nkv.CreateTable<Book>();
            nkv.CreateTable<BlogEntry>();

            string key = Guid.NewGuid().ToString();
            var book = Book.Generate();
            book.Key = key;
            var blogEntry = BlogEntry.Generate();
            blogEntry.Key = key;

            nkv.Save(book);
            nkv.Save(blogEntry);

            helper.AssertRowExists("Book", key);
            helper.AssertRowExists("BlogPosts", key);
        }
        
        #endregion

        #region Update

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUpdate()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            nkv.CreateTable<Book>();

            var book = Book.Generate();
            nkv.Save(book);
            nkv.Save(book);
        }

        #endregion
    }
}
