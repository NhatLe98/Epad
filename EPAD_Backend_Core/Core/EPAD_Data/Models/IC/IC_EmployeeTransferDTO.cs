using System;

namespace EPAD_Data.Models
{
    public class IC_EmployeeTransferDTO
    {
        public int? WorkingInfoIndex { get; set; }
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public short? Gender { get; set; }
        public string CardNumber { get; set; }
        public string NameOnMachine { get; set; }
        public string ImageUpload { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? StoppedDate { get; set; }
        public DateTime JoinedDate { get; set; }
        public string IsFromTime { get; set; }
        public string IsToTime { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string Description { get; set; }
        public long NewDepartment { get; set; }
        public string NewDepartmentName { get; set; }
        public string NewDepartmentCode { get; set; }
        public long? OldDepartment { get; set; }
        public string OldDepartmentName { get; set; }
        public string OldDepartmentCode { get; set; }
        public string RemoveFromOldDepartmentName { get; set; }
        public bool? RemoveFromOldDepartment { get; set; }
        public bool? AddOnNewDepartment { get; set; }
        public string AddOnNewDepartmentName { get; set; }
        public string TypeTemporaryTransfer { get; set; }
        public string TransferApprovedDate { get; set; }
        public string TransferApprovedUser { get; set; }
        public short Status { get; set; }
        public string TransferApproveStatus { get; set; }
        public bool TemporaryTransfer { get; set; }
        public bool TransferNow { get; set; }
        public int? EmployeeTypeId { get; set; }
    }
}
