using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_UserFaceTemplate
    {
        [Column(TypeName = "varchar(100)",Order =0)]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(50)",Order =1)]
        public string SerialNumber { get; set; }

        public short FaceIndex { get; set; }

        public int CompanyIndex { get; set; }

        [Column(TypeName = "text")]
        public string FaceTemplate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}