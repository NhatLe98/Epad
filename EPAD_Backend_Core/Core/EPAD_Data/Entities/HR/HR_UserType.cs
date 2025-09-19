using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities.HR
{
    public class HR_UserType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        public string Code { get; set; }

        [StringLength(150)]
        public string Name { get; set; }
       
        [StringLength(150)]
        public string EnglishName { get; set; }

        public short Order { get; set; }

        public short StatusId { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int CompanyIndex { get; set; }

        public string UpdatedUser { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public bool? IsEmployee { get; set; }

        public short UserTypeId { get; set; }

    }
}
