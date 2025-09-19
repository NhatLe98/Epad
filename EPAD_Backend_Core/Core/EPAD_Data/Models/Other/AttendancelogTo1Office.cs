using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class AttendancelogTo1Office
    {
        public string key { get; set; }
        public List<AttendancelogDataTo1Office> data { get; set; }
    }

    public class AttendancelogDataTo1Office
    {
        public string code { get; set; }
        public string time { get; set; }
    }
}
