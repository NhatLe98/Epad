using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities.HR
{
    public class HR_PositionInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(50)]
        public string Code { get; set; }
        [StringLength(200)]
        public string NameInEng { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
        [StringLength(100)]
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
    }
}
