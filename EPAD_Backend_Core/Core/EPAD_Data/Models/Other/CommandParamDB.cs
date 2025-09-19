using EPAD_Data.Entities;
using System;
using System.Collections.Generic;

namespace EPAD_Data.Models
{
    public class CommandParamDB
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }

        public List<UserInfoOnMachine> ListUsers { get; set; }
        public List<AC_TimeZone> TimeZones { get; set; }
        public List<AC_AccGroup> AccGroups { get; set; }
        public List<AC_AccHoliday> AccHolidays { get; set; }
        public string TimeZone { get; set; }
        public string Group { get; set; }
        public int AutoOffSecond { get; set; }
        public string ConnectionCode { get; set; }
    }
}