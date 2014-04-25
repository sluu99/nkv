using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Sql;
using Nkv.Attributes;

namespace Nkv.Tests.SqlTests
{
    [Table]
    internal class TypeWithTableAttr : Entity { }

    [Table("SomethingElse1")]
    internal class TypeWithTableAttrAndConstructorName : Entity { }

    [Table(Name = "SomethingElse2")]
    internal class TypeWithTableAttrAndPropName : Entity { }

    internal class TypeWithoutAttribute : Entity { }

    [TestClass]
    public class SqlNkvTests
    {
        [TestMethod]
        public void TestCreateTable()
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
