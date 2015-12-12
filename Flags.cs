using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inst8085
{
    class Flags
    {
        [JsonProperty("all")]
        public string All { get; set; }
        [JsonProperty("none")]
        public string None { get; set; }
        [JsonProperty("cy")]
        public string CY { get; set; }
        [JsonProperty("z")]
        public string Z { get; set; }
        [JsonProperty("ac")]
        public string AC { get; set; }
        [JsonProperty("s")]
        public string S { get; set; }
        [JsonProperty("p")]
        public string P { get; set; }

        public Flags()
        {
            All = null;
            None = null;
            CY = null;
            Z = null;
            AC = null;
            S = null;
            P = null;
        }
    }
}
