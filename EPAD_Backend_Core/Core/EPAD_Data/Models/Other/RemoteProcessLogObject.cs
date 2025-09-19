using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Data.Models
{
    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class RemoteProcessLogObject
    {
        public string SerialNumber { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string TaskName { get; set; }

        public List<SystemLog> ListSystemLog { get; set; }
        public int CompanyIndex { get; set; }
        public int ServiceIndex { get; set; }
        public string ServiceName { get; set; }

        public DateTime LogTime { get; set; }
        public string Action { get; set; }
        public string Error { get; set; }

        public RemoteProcessLogObject()
        {

        }
        public RemoteProcessLogObject(UserInfo pUser, CommandResult pCommand, string pSDKFunction, string pResult, DateTime pNow)
        {
            ServiceIndex = pUser.Index;
            ServiceName = pUser.ServiceName;
            CompanyIndex = pUser.CompanyIndex;
            LogTime = pNow;

            SerialNumber = pCommand.SerialNumber;
            IPAddress = pCommand.IPAddress;
            Port = pCommand.Port;
            TaskName = pCommand.Command;

            Error = pCommand.Error;

            SystemLog log = new SystemLog();
            log.SDKFunction = pSDKFunction;
            log.Result = pResult;
            ListSystemLog = new List<SystemLog>() { log };
        }
    }
}
