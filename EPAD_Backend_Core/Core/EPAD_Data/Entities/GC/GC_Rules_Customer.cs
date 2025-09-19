using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Rules_Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        [StringLength(200)]
        [Required]
        public string Name { get; set; }
        [StringLength(200)]
        public string NameInEng { get; set; }
        //[Column(TypeName = "smalldatetime")]
        //public DateTime AppliedDate { get; set; }
        //[Column(TypeName = "smalldatetime")]
        //public DateTime? ToDate { get; set; }
        //[StringLength(200)]
        //[Required]
        //public string ViolateRulesType { get; set; }
        public short NumberOfConnect { get; set; }
        public int NumberOfDaysForSaveData { get; set; }
        //[StringLength(200)]
        //[Required]
        //public string VipCustomerInfo { get; set; }
        public bool UseDefault { get; set; }
        [Required]
        public int CompanyIndex { get; set; }
        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
