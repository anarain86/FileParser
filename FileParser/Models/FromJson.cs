using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileParser.Models
{
    public class FromJson
    {
        //public string Language { get; set; } = null!;
        //public Dictionary<string, string> Group { get; set; } = null!;
        [JsonPropertyName("Data")]
        public Dictionary<string, CaseDictionary<string>> Notices { get; set; } = null!;
    }
}
