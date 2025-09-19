using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class AC_AccessedGroupImportDTO
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Group { get; set; }
        public bool IsIntegrateToMachine { get; set; }
        public int GroupIndex { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ListDevices { get; set; }
        public List<int> DoorIndexLst { get; set; }
        public string Timezone { get; set; }
    }

}
