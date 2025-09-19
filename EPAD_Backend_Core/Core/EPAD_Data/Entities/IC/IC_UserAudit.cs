using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class IC_UserAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get;set;}
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string Nric { get; set; }
        public string DepartmentName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int Operation { get; set; }
        public bool IsCreateFace { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public string CreatedUser { get; set; }
    }
}
