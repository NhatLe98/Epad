using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class TA_AjustAttendanceLogInsertDTO : TA_AjustAttendanceLog
    {
        public string AttendanceDate { get; set; }
        public string AttendanceTime { get; set; }
      
    }
}
