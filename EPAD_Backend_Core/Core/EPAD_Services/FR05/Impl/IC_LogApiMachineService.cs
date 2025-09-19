using EPAD_Common.Extensions;
using EPAD_Data;
using EPAD_Data.Constants;
using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using EPAD_Services.FR05.Interface;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Impl
{
    public class IC_LogApiMachineService : IIC_LogApiMachineService
    {
        private readonly IIC_DeviceService _deviceService;
        ConfigObject _Config;
        IMemoryCache _Cache;
        protected IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public IC_LogApiMachineService(IServiceProvider serviceProvider, ILogger<IC_LogApiMachineService> logger)
        {
            _serviceProvider = serviceProvider;
            _Cache = _serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _deviceService = _serviceProvider.GetService<IIC_DeviceService>();
            _logger = logger;
        }

        public async Task<string> DeleteAllLog( CommandResult pSystemCommand)
        {
            var client = new HttpClient();
            var device = await _deviceService.GetBySerialNumber(pSystemCommand.SerialNumber, 2);
            string startTime = new DateTime(2000, 1, 1).ToString("yyyy-MM-dd HH:mm:ss");
            string toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
            var httpIp = "http://" + pSystemCommand.IPAddress + ":" + pSystemCommand.Port;
            var sendData = new DeleteLogByTime()
            {
                Pass = device.ConnectionCode,
                PersonId = "-1", // set -1 to delete all
                StartTime = startTime,
                EndTime = toDate,
                Model = -1 //all type of records
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(httpIp + "/" + FR05ApiConst.DeleteLogByTime, new FormUrlEncodedContent(values));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<DeleteRecordsResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return item.Success ? "" : item.Msg;

        }

        public async Task<string> DeleteLogFromToTime(CommandResult pSystemCommand)
        {
            var client = new HttpClient();
            var device = await _deviceService.GetBySerialNumber(pSystemCommand.SerialNumber, 2);
            string startTime = pSystemCommand.FromTime.ToString("yyyy-MM-dd HH:mm:ss");
            string toDate = pSystemCommand.ToTime.ToString("yyyy-MM-dd HH:mm:ss");
            var httpIp = "http://" + pSystemCommand.IPAddress + ":" + pSystemCommand.Port;
            var sendData = new DeleteLogByTime()
            {
                Pass = device.ConnectionCode,
                PersonId = "-1", // set -1 to delete all
                StartTime = startTime,
                EndTime = toDate,
                Model = -1 //all type of records
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            HttpResponseMessage response = await client.PostAsync(httpIp + "/" + FR05ApiConst.DeleteLogByTime, new FormUrlEncodedContent(values));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<DeleteRecordsResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            return item.Success ? "" : item.Msg;
        }

     
        public async Task<LogCommandResult> DownloadAllLog(CommandResult pSystemCommand)
        {
            var client = new HttpClient();
            var err = "";
            var logListReturn = new List<LogInfo>();
            var userIdsFailure = new List<string>();
            var userIdsSuccess = new List<string>();
            var httpIp = "http://" + pSystemCommand.IPAddress + ":" + pSystemCommand.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(pSystemCommand.SerialNumber, 2);
                string startTime = new DateTime(2000, 1, 1).ToString("yyyy-MM-dd HH:mm:ss");
                string toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                var sendData = new NewFindRecordsParam()
                {
                    Pass = device.ConnectionCode,
                    Index = 0, //Page number , start from 0 
                    Length = 200000, //maximum number of records per page.Set - 1 will not separate by page
                    PersonId = "-1", // set -1 to get all
                    StartTime = startTime,
                    EndTime = toDate,
                    Model = -1,
                    Order = 1

                };
                var json = JsonConvert.SerializeObject(sendData);
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var _client = new HttpClient();
                HttpResponseMessage response = await _client.PostAsync(httpIp + "/" + FR05ApiConst.NewDownloadAllLog, new FormUrlEncodedContent(values));

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<FindRecordsResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                _logger.LogInformation("DownloadAllLog: " + item.Data.Records.Count().ToString());

                var logList = item.Data.Records;

                if (logList != null && logList.Count > 0)
                {
                    logList = logList.Where(x => x.PersonId != "STRANGERBABY").ToList();
                }

                foreach (var log in logList)
                {
                    var time = log.Time.ConvertUnixTimeToDateTime();
                    var logItem = new LogInfo()
                    {
                        VerifiedMode = 15, //face
                        Time = time,
                        InOutMode = device.DeviceStatus != null ? HandleLogInOutMode(device.DeviceStatus.Value) : log.Direction,
                        UserId = log.PersonId
                    };
                    if (log.Type.Contains("finger")) { logItem.VerifiedMode = 1; }

                    logListReturn.Add(logItem);
                    userIdsSuccess.Add(log.PersonId);
                }

            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            var resultCommand = new LogCommandResult()
            {
                Error = err,
                LogInfos = logListReturn,
                userIdsFailure = userIdsFailure,
                userIdsSuccess = userIdsSuccess
            };

            return resultCommand;
        }

        public int HandleLogInOutMode(int deviceStatus)
        {
            var inOutMode = deviceStatus;
            switch (deviceStatus)
            {
                case (int)DeviceStatus.Output: inOutMode = (int)InOutMode.Output; break;
                case (int)DeviceStatus.Input : inOutMode = (int)InOutMode.Input; break;
            }
            return inOutMode;
        }

        public async Task<LogCommandResult> DownloadLogFromToTime( CommandResult pSystemCommand)
        {
            var err = "";
            var logListReturn = new List<LogInfo>();
            var userIdsFailure = new List<string>();
            var userIdsSuccess = new List<string>();
            var httpIp = "http://" + pSystemCommand.IPAddress + ":" + pSystemCommand.Port;
            try
            {
                var device = await _deviceService.GetBySerialNumber(pSystemCommand.SerialNumber,2);
                string startTime = pSystemCommand.FromTime.ToString("yyyy-MM-dd HH:mm:ss");
                string toDate = pSystemCommand.ToTime.ToString("yyyy-MM-dd HH:mm:ss");
                var sendData = new NewFindRecordsParam()
                {
                    Pass = device.ConnectionCode,
                    Index = 0, //Page number , start from 0 
                    Length = 200000, //maximum number of records per page.Set - 1 will not separate by page
                    PersonId = "-1", // set -1 to get all
                    StartTime = startTime,
                    EndTime = toDate,
                    Model = -1,
                    Order = 1

                };
                var json = JsonConvert.SerializeObject(sendData);
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var _client = new HttpClient();
                HttpResponseMessage response = await _client.PostAsync(httpIp + "/" + FR05ApiConst.NewDownloadAllLog, new FormUrlEncodedContent(values));

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<FindRecordsResult>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                var logList = item.Data.Records;

                if (logList != null && logList.Count > 0)
                {
                    logList = logList.Where(x => x.PersonId != "STRANGERBABY").ToList();
                }

                foreach (var log in logList)
                {
                    var time = log.Time.ConvertUnixTimeToDateTime();
                    
                    var logItem = new LogInfo()
                    {
                        VerifiedMode = 15, //face
                        Time = time,
                        InOutMode = device.DeviceStatus != null ? HandleLogInOutMode(device.DeviceStatus.Value) : log.Direction,
                        UserId = log.PersonId
                    };
                    if (log.Type.Contains("finger")) { logItem.VerifiedMode = 1; }
                    logListReturn.Add(logItem);
                    userIdsSuccess.Add(log.PersonId);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            var resultCommand = new LogCommandResult()
            {
                Error = err,
                LogInfos = logListReturn,
                userIdsFailure = userIdsFailure,
                userIdsSuccess = userIdsSuccess
            };

            return resultCommand;
        }
    }
}
