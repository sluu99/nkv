using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv
{
    public class NkvException : Exception
    {
        public NkvException() : base() { }
        public NkvException(string message) : base(message) { }
        public NkvException(string message, Exception innerException) : base(message, innerException) { }

        public string AckCode { get; set; }
        public int RowCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
