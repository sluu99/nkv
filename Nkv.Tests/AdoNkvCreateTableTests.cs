using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Attributes;
using Nkv.Tests.Fixtures;

namespace Nkv.Tests
{
    [Table]
    internal class TypeWithTableAttr : Entity { }

    [Table("SomethingElse1")]
    internal class TypeWithTableAttrAndConstructorName : Entity { }

    [Table(Name = "SomethingElse2")]
    internal class TypeWithTableAttrAndPropName : Entity { }

    internal class TypeWithoutAttribute : Entity { }

    [TestClass]
    public class AdoNkvCreateTableTests
    {        
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCreateTable()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<TypeWithTableAttr>();
                helper.AssertTableExists("TypeWithTableAttr");

                session.CreateTable<TypeWithTableAttrAndConstructorName>();
                helper.AssertTableExists("SomethingElse1");

                session.CreateTable<TypeWithTableAttrAndPropName>();
                helper.AssertTableExists("SomethingElse2");

                session.CreateTable<TypeWithoutAttribute>();
                helper.AssertTableExists("TypeWithoutAttribute");
            }

        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCreateTable_already_exists()
        {
            AdoNkv nkv;
            IAdoTestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            using (var session = nkv.BeginSession())
            {
                session.CreateTable<Book>();
                helper.AssertTableExists("Book");

                session.CreateTable<Book>();
                helper.AssertTableExists("Book");
            }
        }
    }
}
