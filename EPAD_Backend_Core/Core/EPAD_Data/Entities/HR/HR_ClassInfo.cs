using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_ClassInfo
    {
        [Key]
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string Index { get; set; }
        [StringLength(20)]
        public string Code { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(200)]
        public string NameInEng { get; set; }
        public int? GradeIndex { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public  int CompanyIndex { get; set; }
    }
}
