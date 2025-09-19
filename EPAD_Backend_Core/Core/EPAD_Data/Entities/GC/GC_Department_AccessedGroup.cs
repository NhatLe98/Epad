using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EPAD_Data.Entities
{
    public class GC_Department_AccessedGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Index { get; set; }

        public int DepartmentIndex { get; set; }


        [Column(TypeName = "smalldatetime")]
        public DateTime FromDate { get; set; }

        public int CompanyIndex { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? ToDate { get; set; }

        public int? AccessedGroupIndex { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }
    }
}
