using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    [Table("IC_AttendanceLog")]
    public class IC_AttendanceLogIntegrate
    {
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public string SerialNumber { get; set; }
        public DateTime CheckTime { get; set; }
        public short? VerifyMode { get; set; }
        public short InOutMode { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsSend { get; set; }
    }
}
