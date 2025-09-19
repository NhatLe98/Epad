using System;

namespace EPAD_Data.Entities
{
    public class IC_Employee_Shift_Integrate
    {
        public long Index { get; set; }
        public string EmployeeId { get; set; }
        public string ShiftName { get;set; }
        public DateTime ShiftDate { get; set; }
        public DateTime ShiftFromTime { get; set; }
        public DateTime ShiftToTime { get; set; }
        public DateTime ShiftApplyDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
