using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;


namespace EPAD_Data.Models
{
    public class TA_BusinessRegistrationDTO : TA_BusinessRegistration
    {
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public long DepartmentIndex { get; set; }
        public string DepartmentName { get; set; }
        public string BusinessDateString { get; set; }
        public string FromTimeString { get; set; }
        public string ToTimeString { get; set; }
    }

    public class BusinessRegistrationModel : TA_BusinessRegistrationDTO
    { 
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Filter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public List<long> ListDepartmentIndex { get; set; }
        public List<string> ListEmployeeATID { get; set; }
        public string BusinessTypeName { get; set; }
        public float TotalWork { get; set; }
        public string ErrorMessage { get; set; }
    }
}
