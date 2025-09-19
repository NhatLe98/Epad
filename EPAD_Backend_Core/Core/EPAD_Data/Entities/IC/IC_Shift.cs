using System;
using System.Collections.Generic;

namespace EPAD_Data.Entities
{
    public class IC_Shift : BaseModel
    {
        public DateTime? ShiftDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime ApplyDate { get; set; }
        public ICollection<IC_Employee_Shift> EmployeeShifts { get; set; }
    }
}
