using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_TimeAttendanceProccess
    {
        public List<string> EmployeeATIDs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class SyntheticAttendanceRequest
    {
        public List<long> Departments { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Filter { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class SyntheticAttendanceInfo
    {
        public string DepartmentName { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public DateTime Date { get; set; }
    }

    public class CaculateAttendanceDataReponse
    {
        public string DepartmentName { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string ShiftName { get; set; }
        public string Date { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public long? DepartmentIndex { get; set; }
        public double TotalWorkingDay { get; set; } //Tổng công
        public double TotalWorkingTime { get; set; } //TỔNG GIỜ LÀM VIỆC
        public double TotalDayOff { get; set; } //CÔNG NGÀY NGHỈ
        public double TotalHoliday { get; set; } //CÔNG NGÀY LỄ
        public double TotalOverTime { get; set; } //CÔNG TĂNG CA
        public double TotalBusinessTrip { get; set; } //CÔNG TĂNG CA
        public double TotalWorkingTimeNormal { get; set; } //TỔNG GIỜ LÀM BÌNH THƯỜNG
        public double TotalOverTimeNormal { get; set; } //TĂNG CA NGÀY THƯỜNG
        public double TotalOverTimeNightNormal { get; set; } //TĂNG CA ĐÊM NGÀY THƯỜNG
        public double TotalOverTimeDayOff { get; set; } //TĂNG CA NGÀY NGHỈ
        public double TotalOverTimeNightDayOff { get; set; } //TĂNG CA ĐÊM NGÀY NGHỈ
        public double TotalOverTimeHoliday { get; set; } //TĂNG CA NGÀY LỄ
        public double TotalOverTimeNightHoliday { get; set; } //TĂNG CA ĐÊM NGÀY LỄ
        public double TotalWorkingTimeNight { get; set; } //TĂNG CA ĐÊM NGÀY LỄ
        public double CheckInLate { get; set; } //VÀO TRỄ (PHÚT)
        public double CheckOutEarly { get; set; } //RA SỚM (PHÚT)
    }
}
