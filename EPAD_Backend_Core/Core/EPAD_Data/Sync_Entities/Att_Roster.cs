using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Att_Roster
    {
        public string CodeEmp { get; set; }
        public string ProfileName { get; set; }
        public string ShiftCode { get; set; }
        public string ShiftName { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateUpdate { get; set; }
    }

    public class Att_RosterApiResult
    {
        public bool success { get; set; }
        public List<Att_Roster> data { get; set; }
        public string Message { get; set; }
    }
}
