using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Attributes;

namespace Nkv.Tests
{
    [TestClass]
    public class AdoNkvCountTests
    {
        [Table("CountTest")]
        private class CountTestFixture : Entity
        {
            private static Random _rand = new Random();

            public CountTestFixture()
            {
                Key = Guid.NewGuid().ToString();
                Value = _rand.Next();
            }

            public int Value { get; set; }
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCount()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            Random rand = new Random();
            long count = rand.Next(4, 200);

            using (var session = nkv.BeginSession())
            {
                session.Init<CountTestFixture>();

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
