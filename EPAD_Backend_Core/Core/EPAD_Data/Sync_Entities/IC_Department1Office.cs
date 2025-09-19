using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Sync_Entities
{
    public class IC_Department1Office
    {
        public string ID { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("parentID")]
        public string ParentID { get; set; }
        

    }


    public class IC_Department1Office_Return
    {
        [JsonProperty("error")]
        public bool Error { get; set; }
        [JsonProperty("data")]
        public List<IC_Department1Office> Data { get; set; }
        [JsonProperty("total_item")]
        public string Total_Item { get; set; }
    }
}
