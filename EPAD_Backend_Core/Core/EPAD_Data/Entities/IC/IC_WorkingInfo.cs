using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EPAD_Data.Entities
{
    public class IC_WorkingInfo
    {

        [Key]   
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Required]
        public int CompanyIndex { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string EmployeeATID { get; set; }
        public long PositionIndex { get; set; }

        [Required]
        public long DepartmentIndex { get; set; }

        public int? TeamIndex { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool? IsSync { get; set; }

        public bool IsManager { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        public short Status { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(50)]
        public string ApprovedUser { get; set; }

    }
}
