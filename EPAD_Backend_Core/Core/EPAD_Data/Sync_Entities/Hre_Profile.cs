using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public class Hre_Profile
    {
        public string ID { get; set; }
        public string CodeEmp { get; set; }
        public string ProfileName { get; set; }
        public string CodeAttendance { get; set; }
        public string OrgStructureCode { get; set; }
        public string PositionCode { get; set; }
        public string StatusSyn { get; set; }
        public DateTime? DateQuit { get; set; }
        public DateTime? DateUpdate { get; set; }
        public DateTime? datecreate { get; set; }
        public string Flag { get; set; }
    }

    public class Hre_ProfileApiReturn
    {
        public bool success { get; set; }
        public List<Hre_Profile> data { get; set; }
        public string Message { get; set; }
    }
}
