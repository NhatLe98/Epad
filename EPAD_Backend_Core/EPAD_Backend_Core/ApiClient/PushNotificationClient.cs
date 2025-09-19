using EPAD_Backend_Core.Controllers;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Controllers.V1Controller;

namespace EPAD_Backend_Core.ApiClient
{
    public class PushNotificationClient
    {
        private static PushNotificationClient pushNotificationClient;

        private HttpClient Client { get; set; }
        public PushNotificationClient(IMemoryCache cache)
        {
            Client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3),
                BaseAddress = new Uri(ConfigObject.GetConfig(cache).PushNotificatioinLink)
            };

        }
        public static PushNotificationClient GetInstance(IMemoryCache cache)
        {
            if (pushNotificationClient == null)
            {
                pushNotificationClient = new PushNotificationClient(cache);
            }
            return pushNotificationClient;
        }

        public async Task<HttpResponseMessage> SendMessageToChannelAsync(string channel, string message)
        {
            return await Client.PostAsync($"api/Notification/SendMessageToChannel?channel={channel}&message={message}", new StringContent(""));
        }

        public async Task SendNotificationToSDKInterfaceAsync(List<string> serialNumbers)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await SendMessageToChannelAsync("executeCommand", string.Join(",", serialNumbers));
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(5000);
                }
            }
        }


        //public async Task SendNotiControllerAsync(RelayTimerController param)
        //{
        //    var json = JsonConvert.SerializeObject(param);
        //    try
        //    {
        //        await SendMessageToChannelAsync("deviceTcpService", json.ToString());
                
        //    }
        //    catch (Exception)
        //    {
        //        await Task.Delay(5000);
        //    }
        //}
    }
}
