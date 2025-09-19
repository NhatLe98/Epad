using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Lovad
{
    public class IC_SendLogToLovad
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("log_time")]
        public string LogTime { get; set; }
        [JsonProperty("lane")]
        public int Lane { get; set; }
        [JsonProperty("flow")]
        public int Flow { get; set; }
    }
}
