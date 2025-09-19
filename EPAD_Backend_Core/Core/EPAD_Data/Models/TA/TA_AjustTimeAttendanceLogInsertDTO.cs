using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustTimeAttendanceLogInsertDTO
    {
        public TA_AjustTimeAttendanceLogInsertDTO()
        {
            AjustTimeLists = new List<TA_AjustTimeList> { };
        }
        public string EmployeeATID { get; set; }
        public List<TA_AjustTimeList> AjustTimeLists { get; set; }
    }


    public class TA_AjustTimeList
    {
        public DateTime Date { get; set; }
        public string WorkingType { get; set; }
    }
}
