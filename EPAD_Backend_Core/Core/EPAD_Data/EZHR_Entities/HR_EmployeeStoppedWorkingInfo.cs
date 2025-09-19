using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Entities
{
    public partial class HR_EmployeeStoppedWorkingInfo
    {
        public long Index { get; set; }
        public int CompanyIndex { get; set; }
        public string EmployeeATID { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public int? StoppedReasonIndex { get; set; }
        public DateTime? ReturnHealthInsCard { get; set; }
        public DateTime? ReturnSocialInsCard { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
        public string NumberOfDoc { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ViewedInSystemToDate { get; set; }
        public DateTime? ApplymentDate { get; set; }
        public string SignedPersion { get; set; }
        public string AttachedFile { get; set; }
        public string SynchErrorDevices { get; set; }
        public string ApprovedEmployeeATID { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? Synched { get; set; }
        public bool? SalaryDetained { get; set; }
    }
}
