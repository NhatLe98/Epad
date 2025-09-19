using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_AjustTimeAttendanceLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
