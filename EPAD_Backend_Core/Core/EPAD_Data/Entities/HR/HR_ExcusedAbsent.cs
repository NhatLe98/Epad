using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_ExcusedAbsent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string EmployeeATID { get; set; } 
        public DateTime AbsentDate { get; set; } 
        public int ExcusedAbsentReasonIndex { get; set; } 
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
    }
}
