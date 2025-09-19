using System;

namespace EPAD_Data.Models
{
    public class IC_WorkingInfoDTO
    {
        public int Index { get; set; }
        public int CompanyIndex { get; set; }
        public long PositionIndex { get; set; }
        public string EmployeeATID { get; set; }
        public long DepartmentIndex { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsSync { get; set; }
        public bool IsManager { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public short Status { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedUser { get; set; }
        public string FullName { get; set; }
        public string NewDepartmentName { get; set; }
        public string PositionName { get; set; }
        public bool? IsUpdate { get; set; }
        public bool? IsInsert { get; set; }
        public bool? IsNotChange { get; set;}
    }
}
