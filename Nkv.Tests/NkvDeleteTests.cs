﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Tests.Fixtures;
using System.Threading;

namespace Nkv.Tests
{
    [TestClass]
    public class NkvDeleteTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book);
                helper.AssertRowExists("Book", book.Key);

                session.Delete(book);
                helper.AssertRowExists("Book", book.Key, false);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete_modified_entity()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book);
                helper.AssertRowExists("Book", book.Key);

                Thread.Sleep(500); // make sure the time has changed
                var bookInstance2 = session.Select<Book>(book.Key);                               
                session.Update(bookInstance2);

                try
                {
                    session.Delete(book);
                    Assert.Fail("Expecting an instance of NkvException with AckCode=VersionMismatch");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.VersionMismatch, ex.AckCode);
                    Assert.AreEqual(bookInstance2.Timestamp, ex.Timestamp);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete_non_existent()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book);
                helper.AssertRowExists("Book", book.Key);
                                
                session.Delete(book);
                helper.AssertRowExists("Book", book.Key, false);

                try
                {
                    session.Delete(book);
                    Assert.Fail("Expecting an instance of NkvException thrown with AckCode=NOT_EXISTS");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.KeyNotFound, ex.AckCode);
                }
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestDelete_entity_locked()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book);
                session.Lock(book);

                book.Pages++;

                try
                {
                    session.Delete(book);
                    Assert.Fail("Expecting an NkvException with AckCode=EntityLocked");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.EntityLocked, ex.AckCode);
                }

                helper.AssertRowExists("Book", book.Key);
                
                session.Unlock(book);
                session.Delete(book);

                helper.AssertRowExists("Book", book.Key, false);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestForceDelete()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book1 = Book.Generate();
            var book2 = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                session.Insert(book1);
                session.Insert(book2);

                session.Lock(book2);

                try
                {
                    session.Delete(book2);
                    Assert.Fail("Expecting an NkvException with AckCode=EntityLocked");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.EntityLocked, ex.AckCode);
                }

                session.ForceDelete(book1); // book1 is NOT locked, make sure ForceDelete works on unlocked entities
                session.ForceDelete(book2);

                Assert.IsNull(session.Select<Book>(book1.Key));
                Assert.IsNull(session.Select<Book>(book2.Key));

                helper.AssertRowExists("Book", book1.Key, false);
                helper.AssertRowExists("Book", book2.Key, false);
            }
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestForceDelete_modified_entity()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            var book = Book.Generate();

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();

                session.Insert(book);
                helper.AssertRowExists("Book", book.Key);

                Thread.Sleep(500); // make sure the time has changed
                var bookInstance2 = session.Select<Book>(book.Key);
                session.Update(bookInstance2);

                try
                {
                    session.ForceDelete(book);
                    Assert.Fail("Expecting an instance of NkvException with AckCode=VersionMismatch");
                }
                catch (NkvException ex)
                {
                    Assert.AreEqual(NkvAckCode.VersionMismatch, ex.AckCode);
                    Assert.AreEqual(bookInstance2.Timestamp, ex.Timestamp);
                }
            }
        }
    }
}
