using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_CustomerVehicle
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("vehicle_code")]
        public string VehicleCode { get; set; }
        [JsonProperty("vehicle_number")]
        public string VehicleNumber { get; set; }
        [JsonProperty("vehicle_brand")]
        public string VehicleBrand { get; set; }
        [JsonProperty("date_start")]
        public string DateStart { get; set; }
        [JsonProperty("date_end")]
        public string DateEnd { get; set; }
        [JsonProperty("time_start")]
        public string TimeStart { get; set; }
        [JsonProperty("time_end")]
        public string TimeEnd { get; set; }
    }
}
