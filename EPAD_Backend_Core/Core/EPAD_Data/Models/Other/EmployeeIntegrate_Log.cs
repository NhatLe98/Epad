using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmployeeIntegrate_Log
    {
        public int ServiceId { get; set; }
        public DateTime IntegrateTime { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public int RowErrorIndex { get; set; }
        public List<EmployeeIntegrate> ListEmployee { get; set; }

    }
}
