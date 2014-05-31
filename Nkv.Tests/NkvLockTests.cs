using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System.Threading;

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

                var version = book.Version;
                session.Lock(book);
                Assert.IsTrue(book.Version > version, "Entity version should increase after locking");

                version = book.Version;
                session.Lock(book); // should not throw an exception if the entity is already locked
                Assert.IsTrue(book.Version > version, "Entity version should increase after locking");
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
                
                var version = book.Version;
                session.Unlock(book); // it's okay to unlock an entity that's not locked
                Assert.IsTrue(book.Version > version, "Entity version should increase after unlocking");
                                
                session.Lock(book);

                version = book.Version;
                session.Unlock(book);
                Assert.IsTrue(book.Version > version, "Entity version should increase after unlocking");

                session.Unlock(book); // should not throw an exception if the entity is not locked
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestLock_entity_modified()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);

                var book2 = session.Select<Book>(book.Key);
                book.Pages++;
                session.Update(book2);

                try
                {
                    session.Lock(book);
                    Assert.Fail("Expecting an NkvException with AckCode VersionMismatch");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.VersionMismatch, ex.AckCode);
                }
            }
        }
        
        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUnlock_entity_modified()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);

                Thread.Sleep(500);

                var book2 = session.Select<Book>(book.Key);
                book.Pages++;
                session.Update(book2);

                try
                {
                    session.Unlock(book);
                    Assert.Fail("Expecting an NkvException with AckCode VersionMismatch");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.VersionMismatch, ex.AckCode);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestLock_not_found()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                
                try
                {
                    session.Lock(book);
                    Assert.Fail("Expecting an NkvException with AckCode KeyNotFound");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyNotFound, ex.AckCode);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestUnlock_not_found()
        {
            var nkv = TestConfiguration.CreateNkv(TestContext);
            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                try
                {
                    session.Unlock(book);
                    Assert.Fail("Expecting an NkvException with AckCode KeyNotFound");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyNotFound, ex.AckCode);
                }
            }
        }
    }
}
