using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_SystemCommand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [Column(TypeName = "varchar(50)")]
        [Required]
        public string SerialNumber { get; set; }

        [StringLength(50)]
        public string CommandName { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Command { get; set; }

        public string Params { get; set; }
        public string EmployeeATIDs { get; set; }

        public DateTime? RequestedTime { get; set; }

        public DateTime? ExcutedTime { get; set; }

        public bool Excuted { get; set; }
        public string Error { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        [Required]
        public int CompanyIndex { get; set; }

        public int GroupIndex { get; set; }
        public int ExcutingServiceIndex { get; set; }

        public bool IsOverwriteData { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<IC_Audit> IC_Audits { get; set; }

    }
}