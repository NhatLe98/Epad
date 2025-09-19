using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_Employee_Integrate_AEON
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("loginName")]
        public string LoginName { get; set; }
        [JsonProperty("sapCode")]
        public string SAPCode { get; set; }
        [JsonProperty("fullName")]
        public string FullName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("isActivated")]
        public bool? IsActivated { get; set; }
        [JsonProperty("role")]
        public int Role { get; set; }
        [JsonProperty("gender")]
        public int Gender { get; set; }
    }

    public class ItemEmployee
    {
        [JsonProperty("errorCodes")]
        public List<string> ErrorCodes { get; set; }
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
        [JsonProperty("object")]
        public ItemDetailEmployee Objects { get; set; }
    }

    public class ItemDetailEmployee
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("data")]
        public List<IC_Employee_Integrate_AEON> Data { get; set; }
    }

    public class CheckDuplicate
    {
        public string SAPCode { get; set; }
        public bool IsAdd { get; set; }
    }
}
