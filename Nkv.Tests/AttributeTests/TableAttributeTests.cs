using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nkv.Attributes;

namespace Nkv.Tests.AttributeTests
{
    [Table()]
    internal class TypeWithTableAttr { }

    [Table("SomethingElse1")]
    internal class TypeWithTableAttrAndConstructorName { }

    [Table(Name = "SomethingElse2")]
    internal class TypeWithTableAttrAndPropName { }

    internal class TypeWithoutAttribute { }

    [TestClass]
    public class TableAttributeTests
    {
        [TestMethod]
        public void TestGetTableName()
        {
            Assert.AreEqual("TypeWithTableAttr", TableAttribute.GetTableName(typeof(TypeWithTableAttr)), true);
            Assert.AreEqual("SomethingElse1", TableAttribute.GetTableName(typeof(TypeWithTableAttrAndConstructorName)), true);
            Assert.AreEqual("SomethingElse2", TableAttribute.GetTableName(typeof(TypeWithTableAttrAndPropName)), true);
            Assert.AreEqual("TypeWithoutAttribute", TableAttribute.GetTableName(typeof(TypeWithoutAttribute)), true);
        }
    }
}
