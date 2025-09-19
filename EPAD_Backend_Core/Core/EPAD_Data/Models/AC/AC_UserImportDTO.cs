using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_UserImportDTO
    {
        public string EmployeeATID { get; set; }
        public string Timezone { get; set; }
        public string AreaName { get; set; }
        public string DoorName { get; set; }
        public bool isOverwriteUserMaster { get; set; }
        public string ErrorMessage { get; set; }
        public int TimezoneIndex { get; set; }
        public int AreaIndex { get; set; }
        public int DoorIndex { get; set; }
        public string TimezoneUID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public List<int> DoorIndexLst { get; set; }
        public List<int> AreaIndexLst { get; set; }
        public List<string> ListSerial { get; set; }
        public bool IsIntegrateToMachine { get; set; }
    }
}
