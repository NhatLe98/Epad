using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_UserFaceTemplate_v2
    {
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(50)", Order = 1)]
        public string SerialNumber { get; set; }

        public int CompanyIndex { get; set; }
        public int No { get; set; }
        public int Index { get; set; }
        public int Valid { get; set; }
        public int Duress { get; set; }
        public int Type { get; set; }
        public int MajorVer { get; set; }
        public int MinorVer { get; set; }
        public int Format { get; set; }

        [Column(TypeName = "text")]
        public string TemplateBIODATA { get; set; }
        public int Size { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
