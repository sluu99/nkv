using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv.Tests
{
    public interface TestHelper
    {
        void AssertTableExists(string tableName);
        void AssertRowExists(string tableName, string key);
    }
}
