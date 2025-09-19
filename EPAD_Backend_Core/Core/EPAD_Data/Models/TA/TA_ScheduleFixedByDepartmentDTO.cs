using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_ScheduleFixedByDepartmentDTO : TA_ScheduleFixedByDepartment
    {
        public List<long> DepartmentList { get; set; }
        public int Id { get; set; }

    }
    public class ScheduleFixedByDepartmentRequest
    {
        public List<long> DepartmentIndexes { get; set; }
        public DateTime FromDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ScheduleFixedByDepartmentReponse
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public long DepartmentIndex { get; set; }
        public List<long> DepartmentList { get; set; }
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

    public class ScheduleFixedByDepartmentImportExcel : TA_ScheduleFixedByDepartment
    {
        public string DepartmentName { get; set; }
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
