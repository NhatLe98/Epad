using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    [Table("IC_Employee")]
    public class IC_EmployeeIntegrate
    {
        public string EmployeeATID { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentCode { get; set; }
        public string PositionCode { get; set; }
        public bool? Status { get; set; }
        public DateTime? DateQuit { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int UserType { get; set; }
        public string CardNumber { get; set; }
        public DateTime? StartedDate { get; set; }
        public string Note { get; set; }
    }
}
