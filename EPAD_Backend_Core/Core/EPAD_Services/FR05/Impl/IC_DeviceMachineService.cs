using EPAD_Data.Constants;
using EPAD_Data.Models;
using EPAD_Data.Models.FR05;
using EPAD_Services.FR05.Interface;
using EPAD_Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Impl
{
    public class IC_DeviceMachineService : IIC_DeviceMachineService
    {
        private readonly IIC_DeviceService _deviceService;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private readonly IIC_UserApiMachineService _userMachineService;
        private readonly IIC_LogApiMachineService _logMachineService;

        public IC_DeviceMachineService(IServiceProvider serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _deviceService = serviceProvider.GetService<IIC_DeviceService>();
            _userMachineService = serviceProvider.GetService<IIC_UserApiMachineService>();
            _logMachineService = serviceProvider.GetService<IIC_LogApiMachineService>();
        }
        public async Task<DeviceCapacityParam> GetDeviceCapacityInfo(CommandResult pSystemCommand)
        {
            throw new NotImplementedException();
        }

        public async Task<DeviceParamInfo> GetDeviceInfo(CommandResult pSystemCommand)
        {
            var resultUser = await _userMachineService.DownloadAllUser(pSystemCommand, null);

            var logInfos = await _logMachineService.DownloadAllLog(pSystemCommand);

            var faceCount = resultUser.UserInfos.Where(x => x.Face != null && !string.IsNullOrEmpty(x.Face.FaceTemplate)).Count();
            var fingerCount = resultUser.UserInfos.Where(x => x.FingerPrints != null && x.FingerPrints.Count() > 0).Count();

            var deviceCapacityParam = new DeviceParamInfo()
            {
                AttendanceLogCount = logInfos.LogInfos.Count,
                UserCount = resultUser.UserInfos.Count,
                FaceCount = faceCount,
                SerialNumber = pSystemCommand.SerialNumber,
                FingerCount = fingerCount
            };

            return deviceCapacityParam;


        }

        public async Task RestartDevice( CommandResult param)
        {
            try { 
            HttpClient httpClient = new HttpClient();

            var device = await _deviceService.GetBySerialNumber(param.SerialNumber, 2);
            var sendData = new CommonModel()
            {
                Pass = device.ConnectionCode
            };
            var json = JsonConvert.SerializeObject(sendData);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            var httpIp = "http://" + param.IPAddress + ":" + param.Port;

            HttpResponseMessage response = await httpClient.PostAsync(httpIp + "/" + FR05ApiConst.RestartDevice, new FormUrlEncodedContent(values));
            }catch (Exception ex) { }
        }

        public async Task SetDeviceTime(CommandResult param)
        {
            HttpClient httpClient = new HttpClient();
            var device = await _deviceService.GetBySerialNumber(param.SerialNumber,2);
            var sendData = new SetTimeParam()
            {
                Pass = device.ConnectionCode,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString()
            };
            var json = JsonConvert.SerializeObject(sendData);
             var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var httpIp = "http://" + param.IPAddress + ":" + param.Port;
            HttpResponseMessage response = await httpClient.PostAsync(httpIp + "/" + FR05ApiConst.SetTime, new FormUrlEncodedContent(values));
        }
    }
}
