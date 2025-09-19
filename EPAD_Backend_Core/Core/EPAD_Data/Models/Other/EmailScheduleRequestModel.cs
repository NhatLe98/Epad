using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class EmailScheduleRequestModel
    {
        public int Index { get; set; }
        public DateTime Time { get; set; }
        public int DayOfWeekIndex { get; set; }
        public int RulesWarningIndex { get; set; }
        public int CompanyIndex { get; set; }
    }

    public class SendEmailByTimeLog
    {
        public string Email { get; set; }
        public GC_TimeLog TimeLog { get; set; }
    }
}
