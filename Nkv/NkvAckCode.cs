using System;

namespace Nkv
{
    public enum NkvAckCode
    {
        Unknown = -1,
        KeyExists = 1,
        KeyNotFound = 2,
        TimestampMismatch = 3,
    }
}
