using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Attributes;

namespace Nkv.Tests.SqlTests
{
    [Table]
    internal class TypeWithTableAttr { }

    [Table("SomethingElse1")]
    internal class TypeWithTableAttrAndConstructorName { }

    [Table(Name = "SomethingElse2")]
    internal class TypeWithTableAttrAndPropName { }

    internal class TypeWithoutAttribute { }

    [TestClass]
    public class SqlNkvTests
    {
        [TestMethod]
        public void CreateTableTests()
        {
            Nkv nkv = new SqlNkv(TestGlobals.SqlConnectionProvider);
            
            nkv.CreateTable<TypeWithTableAttr>();
            SqlTestHelper.AssertTableExists("TypeWithTableAttr");

            nkv.CreateTable<TypeWithTableAttrAndConstructorName>();
            SqlTestHelper.AssertTableExists("SomethingElse1");

            nkv.CreateTable<TypeWithTableAttrAndPropName>();
            SqlTestHelper.AssertTableExists("SomethingElse2");

            nkv.CreateTable<TypeWithoutAttribute>();
            SqlTestHelper.AssertTableExists("TypeWithoutAttribute");
        }
    }
}
