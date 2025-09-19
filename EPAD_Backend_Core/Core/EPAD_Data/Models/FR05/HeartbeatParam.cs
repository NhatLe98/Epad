using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.FR05
{
    public class HeartbeatParam
    {
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("personCount")]
        public string PersonCount { get; set; }
        [JsonProperty("faceCount")]
        public string FaceCount { get; set; }
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }

    }
}
