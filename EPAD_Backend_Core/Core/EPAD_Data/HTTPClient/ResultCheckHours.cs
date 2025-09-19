using EPAD_Data.Models.EZHR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.HTTPClient
{
    public class ResultCheckHours
    {
        public string EmployeeATID { get; set; }
        public bool ResultWeek { get; set; }
        public decimal? NumberHoursWeek { get; set; }
        public decimal? NumberHoursWeekByRule { get; set; }
        public bool ResultMonth { get; set; }
        public decimal? NumberHoursMonth { get; set; }
        public decimal? NumberHoursMonthByRule { get; set; }
        public bool ResultOTWeek { get; set; }
        public decimal? NumberHoursOTWeek { get; set; }
        public decimal? NumberHoursOTWeekByRule { get; set; }
        public bool ResultOTMonth { get; set; }
        public decimal? NumberHoursOTMonth { get; set; }
        public decimal? NumberHoursOTMonthByRule { get; set; }
        public List<TA_Shift> ShiftsInfo { get; set; }
    }

    public class ResultCheckHoursReponse
    {
        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public ResultCheckHours Data { get; set; }
    }
}
