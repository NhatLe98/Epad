using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class ViolationShift
    {
        public string EmployeeATID { get; set; }
        public int Status { get; set; }
        public string Error { get; set; }
        public bool Success { get; set; }
    }
}
