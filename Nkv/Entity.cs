using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nkv
{
    public abstract class Entity
    {
        [JsonIgnore]
        public string Key { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; internal set; }
    }
}
