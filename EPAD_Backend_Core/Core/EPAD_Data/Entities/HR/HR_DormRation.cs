using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class HR_DormRation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(200)]
        public string UpdatedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyIndex { get; set; }
    }
}
