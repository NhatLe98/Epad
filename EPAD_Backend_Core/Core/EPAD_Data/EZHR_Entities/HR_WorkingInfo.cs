using System;
using System.Collections.Generic;

namespace EPAD_Data.Entities
{
    public partial class HR_WorkingInfo
    {
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public long? DepartmentIndex { get; set; }
        public long? PositionIndex { get; set; }
        public long? TitlesIndex { get; set; }
        public bool? IsManager { get; set; }
        public long? ManagedDepartment { get; set; }
        public string DirectManager { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public bool? CreateSeparatedSalary { get; set; }
        public string ManagedOtherDepartments { get; set; }
        public string Note { get; set; }
        public int? EmployeeKind { get; set; }
        public short? Synched { get; set; }
        public string JobDescription { get; set; }
        public string NumberOfDocument { get; set; }
        public string AttachmentFile { get; set; }
        public string SignedPerson { get; set; }
        public DateTime? SignedDate { get; set; }
        public int? ReasonIndex { get; set; }
        public string EmployeeCodeByWorking { get; set; }
        public string SynchErrorDevices { get; set; }
        public int? ProductProcessIndex { get; set; }
    }
}
