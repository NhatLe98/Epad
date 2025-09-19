using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class TA_LeaveRegistration
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public string Description { get; set; }
        public int CompanyIndex { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LeaveDate { get; set; } 
        public int LeaveDateType { get; set; }
        public int LeaveDurationType { get; set; }
        public int? HaftLeaveType { get; set; }
    }
}
