using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Models
{
    public class IC_EmployeeStoppedDTO : IC_EmployeeStopped
    {
        public List<string> EmployeeATIDs { get; set; }
        public string StoppedDateString { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set; }
    }
}
