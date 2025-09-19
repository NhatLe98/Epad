using System;

namespace EPAD_Data.Models
{
    public class IC_EmployeeDTO 
    {
        // Current Working Info
        public long? WorkingInfoIndex { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public short? TransferStatus { get; set; }
        public bool? IsSync { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        // Student info
        public string ClassName { get; set; }
        // parent indfo
        public string StudentName { get; set; }
        // contact info 
        public string NRIC { get; set; }
        // Employee Info
        public string Avatar { get; set; }
        public long? Index { get; set; }
        public string EmployeeATID { get; set; }
        public int CompanyIndex { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public short? Gender { get; set; }
        public string CardNumber { get; set; }
        public string NameOnMachine { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime? StoppedDate { get; set; }
        public bool? MarkForDelete { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }

        public long? DepartmentIndex { get; set; }
        public string AliasName { get; set; }
        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
        public int? Privilege { get; set; }
        public string PrivilegeName { get; set; }
        public string Password { get; set; }
        public string PositionName { get; set; }

        public int? Finger1 { get; set; }
        public int? Finger2 { get; set; }
        public int? Finger3 { get; set; }
        public int? Finger4 { get; set; }
        public int? Finger5 { get; set; }
        public int? Finger6 { get; set; }
        public int? Finger7 { get; set; }
        public int? Finger8 { get; set; }
        public int? Finger9 { get; set; }
        public int? Finger10 { get; set; }
        public int? FaceTemplate { get; set; }
        public string ImageUpload { get; set; }
        public bool? IsUpdate { get; set; }
        public bool? IsInsert { get; set; }

        public string BirthDay { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; } 
        public string EmployeeTypeName { get; set; }
        public bool IsAllowPhone { get; set; }
        public string FromDateStr { get; set; }

        public string ToDateStr { get; set; }

        public string FromTime { get; set; }

        public string ToTime { get; set; }
        public string CompanyName { get; set; }
        public string ContactDepartment { get; set; }
        public string ContactPerson { get; set; }
        public string WorkingContent { get; set; }


        public IC_EmployeeDTO()
        {
            Finger1 = Finger2 = Finger3 = Finger4 = Finger5 = Finger6 = Finger7 = Finger8 = Finger9 = Finger10 = 0;
            FaceTemplate = 0;
        }
    }
}
