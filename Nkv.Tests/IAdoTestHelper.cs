using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv.Tests
{
    public interface IAdoTestHelper
    {
        void AssertTableExists(string tableName);
        void AssertRowExists(string tableName, string key, bool exists = true);
    }
}
