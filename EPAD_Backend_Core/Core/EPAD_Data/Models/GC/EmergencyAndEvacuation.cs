using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmergencyAndEvacuation
    {
        public string DepartmentName { get; set; }
        public long DepartmentIndex { get; set; }
        public int InCom { get; set; }
        public int Attendance { get; set; }
        public int Absent { get; set; }
    }
}
