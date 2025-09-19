using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Att_OverTimePlan
    {
        public string CodeEmp { get; set; }
        public string CodeAttendance { get; set; }
        public DateTime WorkDateRoot { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public string Status { get; set; }
    }

    public class Att_OverTimePlan_ApiResult
    {
        public bool success { get; set; }
        public List<Att_OverTimePlan> data { get; set; }
        public string Message { get; set; }
    }
}
