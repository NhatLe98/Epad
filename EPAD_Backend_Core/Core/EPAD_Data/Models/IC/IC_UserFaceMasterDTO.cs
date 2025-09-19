using System;

namespace EPAD_Data.Models
{
    public class IC_UserFaceMasterDTO
    {
        public string EmployeeATID { get; set; }

        public string SerialNumber { get; set; }

        public short FaceIndex { get; set; }

        public int CompanyIndex { get; set; }

        public string FaceTemplate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedUser { get; set; }
    }
}
