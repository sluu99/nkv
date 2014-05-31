using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvLockTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestLock()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);

                session.Lock(book);
                session.Lock(book); // should not throw an exception if the entity is already locked
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUnlock()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);

                session.Unlock(book); // it's okay to unlock an entity that's not locked

                session.Lock(book);

                session.Unlock(book);
                session.Unlock(book); // should not throw an exception if the entity is not locked
            }
        }
    }
}
