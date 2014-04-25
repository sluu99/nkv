using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
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
    public class NkvCreateTableTests
    {        
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCreateTable()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);
            
            nkv.CreateTable<TypeWithTableAttr>();
            helper.AssertTableExists("TypeWithTableAttr");

            nkv.CreateTable<TypeWithTableAttrAndConstructorName>();
            helper.AssertTableExists("SomethingElse1");

            nkv.CreateTable<TypeWithTableAttrAndPropName>();
            helper.AssertTableExists("SomethingElse2");

            nkv.CreateTable<TypeWithoutAttribute>();
            helper.AssertTableExists("TypeWithoutAttribute");

        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "Implementations.xml", "Implementation", DataAccessMethod.Sequential)]
        public void TestCreateTable_already_exists()
        {
            Nkv nkv;
            ITestHelper helper;
            TestConfiguration.ParseContext(TestContext, out nkv, out helper);

            nkv.CreateTable<Book>();
            helper.AssertTableExists("Book");

            nkv.CreateTable<Book>();
            helper.AssertTableExists("Book");
        }
    }
}
