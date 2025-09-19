using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    public class Config
    {
        public string EventType { get; set; }
        public List<string> TimePos { get; set; }
        public List<string> Email { get; set; }
        public bool SendMailWhenError { get; set; }

        public bool AlwaysSend { get; set; }
        public int? PreviousDays { get; set; }
        public bool? WriteToDatabase { get; set; }
        public bool? WriteToFile { get; set; }
        public string WriteToFilePath { get; set; }


        public string LinkAPI { get; set; }
        public bool? UsingDatabase { get; set; }
        public bool? IsOverwriteData { get; set; }
        public int? AfterHours { get; set; }
        public bool? SendEmailWithFile { get; set; }
        public bool? DeleteLogAfterSuccess { get; set; }
        public string TitleEmailSuccess { get; set; }
        public string BodyEmailSuccess { get; set; }

        public string TitleEmailError { get; set; }
        public string BodyEmailError { get; set; }
        public List<string> ListSerialNumber { get; set; }
        public bool? AutoIntegrate { get; set; }
        public double? BodyTemperature { get; set; }
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
        public List<string> EmailAllowImportGoogleSheet { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Config()
        {
            TimePos = new List<string>();
            ListSerialNumber = new List<string>();
        }

        public Config(string EventType)
        {
            this.EventType = EventType;
            TimePos = new List<string>();
            Email = new List<string>();
            ListSerialNumber = new List<string>();
            SendMailWhenError = false;
            AlwaysSend = false;
            BodyTemperature = null;
        }
    }
}
