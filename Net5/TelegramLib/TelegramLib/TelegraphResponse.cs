using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchBackup.Models.Telegrph
{
    public class TelegraphResponse
    {
        [JsonProperty("src")]  
        public string Src { get; set; }
    }
}
