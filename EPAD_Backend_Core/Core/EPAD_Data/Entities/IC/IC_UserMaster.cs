using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Data.Entities
{
    public class IC_UserMaster
    {
        
        [Column(TypeName = "varchar(100)", Order = 0)]
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public string NameOnMachine { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string CardNumber { get; set; }
        [Column(TypeName = "varchar(20)")]
        [StringLength(20)]
        public string Password { get; set; }
        [StringLength(200)]
        public short? Privilege { get; set; }
        [StringLength(30)]
        public string AuthenMode { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string FingerData0 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData1 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData2 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData3 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData4 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData5 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData6 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData7 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData8 { get; set; }
        [Column(TypeName = "varchar(max)")]
        public string FingerData9 { get; set; }
        [StringLength(10)]
        public string FingerVersion { get; set; }
        // face
        public short? FaceIndex { get; set; }
        [Column(TypeName = "text")]
        public string FaceTemplate { get; set; }
        [StringLength(10)]
        public string FaceVersion { get; set; }

        //Face V2
        public int? FaceV2_No { get; set; }
        public int? FaceV2_Index { get; set; }
        public int? FaceV2_Valid { get; set; }
        public int? FaceV2_Duress { get; set; }
        public int? FaceV2_Type { get; set; }
        public int? FaceV2_MajorVer { get; set; }
        public int? FaceV2_MinorVer { get; set; }
        public int? FaceV2_Format { get; set; }

        [Column(TypeName = "text")]
        public string FaceV2_TemplateBIODATA { get; set; }
        public int? FaceV2_Size { get; set; }

        [Column(TypeName = "text")]
        public string FaceV2_Content { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [StringLength(50)]
        public string UpdatedUser { get; set; }
    }
}
