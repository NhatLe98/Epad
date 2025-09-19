using System;

namespace EPAD_Data.Sync_Entities
{
    public class Employee_Shift_Integrate
    {
        public string EmployeeId { get; set; }
        public string ShiftName { get;set; }
        public DateTime ShiftDate { get; set; }
        public DateTime ShiftFromTime { get; set; }
        public DateTime ShiftToTime { get; set; }
        public DateTime ShiftApplyDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
