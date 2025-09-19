using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IC_CommandRequestDTO
    {
        public string Action { get; set; }
        public List<string> ListSerial { get; set; }
        public List<string> ListUser { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string ServiceType { get; set; }
        public string Privilege { get; set; }
        public List<string> AuthenMode { get; set; }
        public bool IsOverwriteData { get; set; }
        public int EmployeeType { get; set; }
        public bool? IsDownloadFull { get; set; }
        public bool? IsDeleteAll { get; set; }
        public bool? IsUsingTimeZone { get; set; }
        public List<string> TimeZone { get; set; }
        public int Group { get; set; }
        public bool? IsUsingArea { get; set; }
        public List<int> AreaLst { get; set; }
        public List<int> DoorLst { get; set; }
        public int? AutoOffSecond { get; set; }
        public string TimezoneStr { get; set; }
        public List<int> EmployeeAccessedGroup { get; set; }
        public List<string> ListDepartment { get; set; }

    }
}
