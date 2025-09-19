using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_AccessedGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        [StringLength(200)]
        public string NameInEng { get; set; }

        public string Description { get; set; }

        public int GeneralAccessRuleIndex { get; set; }
        public int ParkingLotRuleIndex { get; set; }
        public bool IsGuestDefaultGroup { get; set; }
        public bool IsDriverDefaultGroup { get; set; }

        [Required]
        public int CompanyIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
