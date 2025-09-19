using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_CustomerTypeParam
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } 
    }
}
