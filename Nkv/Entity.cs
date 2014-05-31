using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv
{
    public abstract class Entity
    {
        internal static readonly DateTime DefaultTimestamp = DateTime.Parse("1990-09-01");
        internal const int MaxKeySize = 128;

        private string _key;

        public Entity()
        {
            Timestamp = DefaultTimestamp;
            Version = 0;
        }

        [JsonIgnore]
        public string Key
        {
            get { return _key; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length > MaxKeySize)
                {
                    throw new ArgumentException("Key must not be null or whitespace and max size = " + MaxKeySize);
                }

                _key = value;
            }
        }

        [JsonIgnore]
        public DateTime Timestamp { get; internal set; }

        [JsonIgnore]
        public long Version { get; internal set; }
    }
}
