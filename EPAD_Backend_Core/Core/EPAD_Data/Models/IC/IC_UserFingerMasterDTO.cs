using System;

namespace EPAD_Data.Models
{
    public class IC_UserFingerMasterDTO
    {
        public string EmployeeATID { get; set; }

        public string SerialNumber { get; set; }
        public int CompanyIndex { get; set; }
        public short FingerIndex { get; set; }

        public string FingerData { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }
    }
}
