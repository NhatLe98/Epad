using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_ScheduleFixedByEmployeeDTO : TA_ScheduleFixedByEmployee
    {
        public List<string> EmployeeATIDs { get; set; }
        public int Id { get; set; }
    }
    public class ScheduleFixedByEmployeeRequest
    {
        public List<long?> DepartmentIndexes { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ScheduleFixedByEmployeeReponse
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public List<long?> DepartmentList { get; set; }
        public List<string> EmployeeATIDs { get; set; }
        public string FromDateFormat { get; set; }
        public DateTime FromDate { get; set; }
        public string ToDateFormat { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Monday { get; set; }
        public int? Tuesday { get; set; }
        public int? Wednesday { get; set; }
        public int? Thursday { get; set; }
        public int? Friday { get; set; }
        public int? Saturday { get; set; }
        public int? Sunday { get; set; }

        public string MondayShift { get; set; }
        public string TuesdayShift { get; set; }
        public string WednesdayShift { get; set; }
        public string ThursdayShift { get; set; }
        public string FridayShift { get; set; }
        public string SaturdayShift { get; set; }
        public string SundayShift { get; set; }
    }


    public class ScheduleFixedByEmployeeImportExcel : TA_ScheduleFixedByEmployee
    {
        public string EmployeeCode { get; set; } 
        public string FullName { get; set; }
        public string FromDateFormat { get; set; }
        public string ToDateFormat { get; set; }
        public string MondayShift { get; set; }
        public string TuesdayShift { get; set; }
        public string WednesdayShift { get; set; }
        public string ThursdayShift { get; set; }
        public string FridayShift { get; set; }
        public string SaturdayShift { get; set; }
        public string SundayShift { get; set; }
        public string ErrorMessage { get; set; }
    }

}
