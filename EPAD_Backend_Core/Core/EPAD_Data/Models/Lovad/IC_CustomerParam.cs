using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_CustomerParam
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("customer_type_code")]
        public string CustomerTypeCode { get; set; }

        [JsonProperty("department_code")]
        public string DeparmentCode { get; set; }

        [JsonProperty("blacklist")]
        public int Blacklist { get; set; }
    }
}
