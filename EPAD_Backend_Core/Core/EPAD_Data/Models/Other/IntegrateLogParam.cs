using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class IntegrateLogParam
    {
        public bool WriteToDatabase { get; set; }
        public bool WriteToFile { get; set; }
        public string LinkAPI { get; set; }
        public bool UsingDatabase { get; set; }
        public bool AutoIntegrate { get; set; }
        public bool IsOverwriteData { get; set; }
        public int AfterHours { get; set; } = 0;
        public bool SendEmailWithFile { get; set; }
        public string WriteToFilePath { get; set; }
        public string ListSerialNumber { get; set; }
        public int? DepartmentIndex { get; set; }
        public int? RemoveStoppedWorkingEmployeesType { get; set; }
        public int? RemoveStoppedWorkingEmployeesDay { get; set; }
        public int? RemoveStoppedWorkingEmployeesWeek { get; set; }
        public int? RemoveStoppedWorkingEmployeesMonth { get; set; }
        public DateTime? RemoveStoppedWorkingEmployeesTime { get; set; }
        public int? ShowStoppedWorkingEmployeesType { get; set; }
        public int? ShowStoppedWorkingEmployeesDay { get; set; }
        public int? ShowStoppedWorkingEmployeesWeek { get; set; }
        public int? ShowStoppedWorkingEmployeesMonth { get; set; }
        public DateTime? ShowStoppedWorkingEmployeesTime { get; set; }
        public int? SoftwareType { get; set; }
        public string LinkAPIIntegrate { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? FileType { get; set; }
        public bool? IntegrateWhenNotInclareDepartment { get; set; }
        public bool AutoCreateDepartmentImportEmployee { get; set; }
        public string EmailAllowImportGoogleSheet { get; set; }

    }

    public class IntegratedShiftConfiguration : IntegrateLogParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
