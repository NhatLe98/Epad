using System;

namespace EPAD_Data.Models
{
    public class IC_UserInfoDTO
    {
        public string EmployeeATID { get; set; }

        public string SerialNumber { get; set; }

        public int CompanyIndex { get; set; }

        public string UserName { get; set; }

        public string CardNumber { get; set; }

        public short? Privilege { get; set; }

        public string Password { get; set; }

        public string Reserve1 { get; set; }

        public int? Reserve2 { get; set; }

        public string AuthenMode { get; set; }
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

        public string UpdatedUser { get; set; }

    }
}
