using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Lovad
{
    public class IC_LogInOut
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int Total { get; set; }
        public List<IC_LogDetail> LogList { get; set; }
    }

    public class IC_LogDetail
    {
        public string Id { get; set; }
        public string CustomerCode { get; set; }
        public string VehicleNumber { get; set; }
        public string RegisterVehicleNumber { get; set; }
        public string VehicleCode { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int LaneIn { get; set; }
        public int LaneOut { get; set;}
        public string Reason { get; set; }
    }
}
