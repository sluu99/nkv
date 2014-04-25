using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv.Tests
{
    public interface ITestHelper
    {
        void AssertTableExists(string tableName);
    }
}
