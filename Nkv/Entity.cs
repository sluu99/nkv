using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv
{
    public abstract class Entity
    {
        private static readonly DateTime DefaultTimestamp = DateTime.Parse("1990-09-01");

        public Entity()
        {
            Timestamp = DefaultTimestamp;
        }

        [JsonIgnore]
        public string Key { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; internal set; }
    }
}
