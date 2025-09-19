using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static EPAD_Data.Models.IC_SignalRDTO;

namespace EPAD_Logic
{
    public class IC_SignalRLogic : IIC_SignalRLogic
    {
        public EPAD_Context _dbContext;
        private readonly ILogger _logger;
        private static IMemoryCache _cache;
        private ConfigObject _config;

        public IC_SignalRLogic(EPAD_Context context, IMemoryCache cache)
        {
            _dbContext = context;
            _cache = cache;
            _config = ConfigObject.GetConfig(_cache);
        }
        public void PostPushNotification(int CompanyIndex, List<SerialNumberCommandParam> pNotification)
        {
            Notification notification = new Notification()
            {
                CompanyIndex = CompanyIndex,
                Message = pNotification
            };
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_config.PushNotificatioinLink);
            var json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("api/Notification", content).Result;
        }

        public void PostPushUserFingerDevice(int CompanyIndex, UserFingerDeviceParam pUserFinger)
        {
            pUserFinger.CompanyIndex = CompanyIndex;

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_config.RealTimeServerLink);
            var json = JsonConvert.SerializeObject(pUserFinger);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("api/UserFingerDevice", content).Result;
        }
    }

    public interface IIC_SignalRLogic
    {
        void PostPushUserFingerDevice(int CompanyIndex, UserFingerDeviceParam pUserFinger);
        void PostPushNotification(int CompanyIndex, List<SerialNumberCommandParam> pNotification);
    }
}
