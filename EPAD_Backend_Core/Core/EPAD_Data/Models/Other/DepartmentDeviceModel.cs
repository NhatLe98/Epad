using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.Other
{
    public class DepartmentDeviceModel
    {
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string SerialNumber { get; set; }
        public bool RootDepartment { get; set; }
        public long RootDepartmentIndex { get; set; }
    }
}
