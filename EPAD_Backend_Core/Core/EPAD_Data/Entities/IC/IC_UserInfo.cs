using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EPAD_Data.Entities
{
    public class IC_UserInfo
    {
        [Column(TypeName = "varchar(100)", Order =0)]
        [Key]
        public string EmployeeATID { get; set; }

        [Column(TypeName = "varchar(50)", Order =1)]
        [Key]
        public string SerialNumber { get; set; }

        [Key, Column(Order = 2)]
        public int CompanyIndex { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string UserName { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string CardNumber { get; set; }

        public short? Privilege { get; set; }

        [Column(TypeName = "varchar(20)")]
        [StringLength(20)]
        public string Password { get; set; }

        public string AuthenMode { get; set; }

        [StringLength(50)]
        public string Reserve1 { get; set; }

        public int? Reserve2 { get; set; }

        public int? FingerData0 { get; set; }
        public int? FingerData1 { get; set; }
        public int? FingerData2 { get; set; }
        public int? FingerData3 { get; set; }
        public int? FingerData4 { get; set; }
        public int? FingerData5 { get; set; }
        public int? FingerData6 { get; set; }
        public int? FingerData7 { get; set; }
        public int? FingerData8 { get; set; }
        public int? FingerData9 { get; set; }
        public int? FaceTemplate { get; set; }
        public int? FaceTemplateV2 { get; set; }
        public int? VeinsData1 { get; set; }
        public int? VeinsData2 { get; set; }
        public int? VeinsData3 { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string UpdatedUser { get; set; }

        public string DepartmentName { get; set; }
    }
}