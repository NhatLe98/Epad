using EPAD_Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPAD_Data.Models 
{
    public class CommandResult : ICloneable
    {
        public string ID { get; set; }
        public string GroupIndex { get; set; }
        public string Command { get; set; }
        public string CommnadName { get; set; }

        public string IPAddress { get; set; }
        public int Port { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }

        public List<UserInfoOnMachine> ListUsers { get; set; }
        public List<AC_TimeZone> TimeZones { get; set; }
        public List<AC_AccGroup> AccGroups { get; set; }
        public List<AC_AccHoliday> AccHolidays { get; set; }
        public string SerialNumber { get; set; }
        public int ExcutingServiceIndex { get; set; }
        public string ExternalData { get; set; }
        public bool IsOverwriteData { get; set; }
        public string TimeZone { get; set; }
        public string Group { get; set; }
        public int AutoOffSecond { get; set; }

        [JsonIgnore]
        public DateTime CreatedTime { get; set; }
        [JsonIgnore]
        public DateTime ExecutedTime { get; set; }
        [JsonIgnore]
        public string Status { get; set; }
        [JsonIgnore]
        public string Error { get; set; }

        public DateTime? AppliedTime { get; set; }
        public int ErrorCounter { get; set; }
        public int DeviceModel { get; set; }
        public string ConnectionCode { get; set; }

        public CommandResult()
        {
        }

        public CommandResult(CommandAction pCommand, CommandStatus pStatus)
        {
            Status = pStatus.ToString();
            Command = pCommand.ToString();
        }
        public CommandResult(string pCommand, CommandStatus pStatus)
        {
            Status = pStatus.ToString();
            Command = pCommand.ToString();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}