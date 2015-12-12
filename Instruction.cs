using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inst8085
{
    class Instruction
    {
        [JsonProperty("mnemonic")]
        public string Name { get; set; }
        [JsonProperty("opcode")]
        public string OpCode { get; set; }
        [JsonProperty("code")]
        public string hexCode { get; set; }
        [JsonProperty("operand")]
        public string Operand { get; set; }
        [JsonProperty("mCycles")]
        public string McyclesCount { get; set; }
        [JsonProperty("machineCycles")]
        public string Mcycles { get; set; }
        [JsonProperty("tStates")]
        public string TstatesCount { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
        [JsonProperty("bytes")]
        public string Bytes { get; set; }
        [JsonProperty("addressingMode")]
        public string AddressingMode { get; set; }
        [JsonProperty("instruction")]
        public string instruction { get; set; }
        [JsonProperty("notes")]
        public string Description { get; set; }
        [JsonProperty("flagsAffected")]
        public Flags flgs { get; set; }

        public Instruction()
        {
            Name = null;
            Operand = null;
            Mcycles = null;
            McyclesCount = null;
            Group = null;
            TstatesCount = null;
            Bytes = null;
            AddressingMode = null;
            Description = null;
        }
    }
}
