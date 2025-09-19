using System;

namespace EPAD_Data.Models
{
    public class IC_UserMasterDTO
    {
        public int Index { get; set; }
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public string NameOnMachine { get; set; }
        public string CardNumber { get; set; }
        public string Password { get; set; }
        public short? Privilege { get; set; }
        public string AuthenMode { get; set; }
        public string FingerData0 { get; set; }
        public string FingerData1 { get; set; }
        public string FingerData2 { get; set; }
        public string FingerData3 { get; set; }
        public string FingerData4 { get; set; }
        public string FingerData5 { get; set; }
        public string FingerData6 { get; set; }
        public string FingerData7 { get; set; }
        public string FingerData8 { get; set; }
        public string FingerData9 { get; set; }
        public string FingerVersion { get; set; }
        // face
        public int? FaceIndex { get; set; }
        public string FaceTemplate { get; set; }
        public string FaceVersion { get; set; }
        //Face V2
        public int? FaceV2_Index { get; set; }
        public int? FaceV2_No { get; set; }
        public int? FaceV2_Valid { get; set; }
        public int? FaceV2_Duress { get; set; }
        public int? FaceV2_Type { get; set; }
        public int? FaceV2_MajorVer { get; set; }
        public int? FaceV2_MinorVer { get; set; }
        public int? FaceV2_Format { get; set; }
        public string FaceV2_TemplateBIODATA { get; set; }
        public int? FaceV2_Size { get; set; }
        public string FaceV2_Content { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
    }
}
