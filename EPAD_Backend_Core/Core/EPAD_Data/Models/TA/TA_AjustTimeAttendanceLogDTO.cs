using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustTimeAttendanceLogDTO
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public long DepartmentIndex { get; set; }
        public string EmployeeCode { get; set; }
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime Date { get; set; }
        public double TotalWorkingDay { get; set; }
        public double TotalWorkingTime { get; set; }
        public double TotalWorkingTimeNormal { get; set; }
        public double TotalDayOff { get; set; }
        public double TotalHoliday { get; set; }
        public double TotalOverTime { get; set; }
        public double TotalBusinessTrip { get; set; }
        public string Status { get; set; }
        public string LeaveType { get; set; }
        public bool IsHoliday { get; set; }
        public float? TheoryWorkedTimeByShift { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public bool IsHolidayPaid { get; set; }
        public bool IsPaidLeave { get; set; }
        public bool IsWorkedTimeHoliday { get; set; }
        public int LeaveDurationType { get; set; }
        public double CheckInLate { get; set; }
        public double CheckOutEarly { get; set; }
        //--OT
        public double TotalOverTimeNormal { get; set; }
        public double TotalOverTimeNightNormal { get; set; }
        public double TotalOverTimeDayOff { get; set; }
        public double TotalOverTimeNightDayOff { get; set; }
        public double TotalOverTimeHoliday { get; set; }
        public double TotalOverTimeNightHoliday { get; set; }
    }
}
