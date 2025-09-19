using System;

namespace EPAD_Data.Models
{
    public class IC_DepartmentAndDeviceDTO
    {
        public int DepartmentIndex { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyIndex { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
    }
}
