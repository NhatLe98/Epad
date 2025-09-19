using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_AjustAttendanceLogHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime ProcessedCheckTime { get; set; }
        public DateTime RawCheckTime { get; set; }
        public int CompanyIndex { get; set; }
        public string SerialNumber { get; set; }
        public int Operate { get; set; }
        public short? VerifyMode { get; set; }
        public short InOutMode { get; set; }
        public string Note { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
