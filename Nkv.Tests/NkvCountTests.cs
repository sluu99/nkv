using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Attributes;

namespace Nkv.Tests
{
    [Table("CountTest")]
    internal class CountTestFixture : Entity
    {
        private static Random _rand = new Random();

        public CountTestFixture()
        {
            Key = Guid.NewGuid().ToString();
            Value = _rand.Next();
        }

        public int Value { get; set; }
    }

    [TestClass]
    public class NkvCountTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCount()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            Random rand = new Random();
            long count = rand.Next(4, 200);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<CountTestFixture>();

                Assert.AreEqual(0, session.Count<CountTestFixture>());
                               
                for (int i = 0; i < count; i++)
                {
                    session.Insert(new CountTestFixture());
                }

                Assert.AreEqual(count, session.Count<CountTestFixture>());
            }
        }
    }
}
