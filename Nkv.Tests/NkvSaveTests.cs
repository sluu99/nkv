using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Tests.Fixtures;
using System.Transactions;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvSaveTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
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
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
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
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
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
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestInsertion_nested_transaction_scope_committed()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var helper = TestConfiguration.TestHelpers[TestContext.DataRow["Helper"].ToString()];

            nkv.CreateTable<Book>();

            var outterBook = Book.Generate();
            var innerBook = Book.Generate();

            using (var outterTx = new TransactionScope())
            {                
                nkv.Save(outterBook);

                using (var innerTx = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    nkv.Save(innerBook);
                    innerTx.Complete();
                }
            }

            helper.AssertRowExists("Book", outterBook.Key, false);
            helper.AssertRowExists("Book", innerBook.Key, true);
        }
    }
}
