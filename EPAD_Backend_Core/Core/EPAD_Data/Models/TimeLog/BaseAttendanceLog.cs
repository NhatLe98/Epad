using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models.TimeLog
{
    public class BaseAttendanceLog
    {
        public string EmployeeATID { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }
        public DateTime CheckTime { get; set; }
        public string VerifyMode { get; set; }
        public string FaceMask { get; set; }
        public string BodyTemperature { get; set; }
        public bool IsOverBodyTemperature { get; set; }
        public string InOutMode { get; set; }
        public int CompanyIndex { get; set; }
        public long? DepartmentIndex { get; set; }
        public string Department { get; set; }
        public int? DeviceNumber { get; set; }
        public string DeviceId { get; set; }
        public string AreaName { get; set; }
        public string DoorName { get; set; }
        public int AreaIndex { get; set; }
        public int DoorIndex { get; set; }
    }

}
