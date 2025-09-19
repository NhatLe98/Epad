using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(200)]
        [Required]
        public string Name { get; set; }
        [StringLength(200)]
        public string Location { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        public int? ParentIndex { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }
        public int? OrgUnitID { get; set; }
        public int? OrgUnitParentNode { get; set; }
        public int? OVNID { get; set; }
        public bool? IsInactive { get; set; }
        public int? JobGradeGrade { get; set; }
        public bool IsStore { get; set; }
        public string ParentCode { get; set; }
        public int Type { get; set; }
        public bool? IsDriverDepartment { get; set; }
        public bool? IsContractorDepartment { get; set; }
    }
}