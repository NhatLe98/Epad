using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.WebUtilitys
{
    public class ControllerProcess
    {
        public async static Task<string> SetOnAndAutoOffController(string pLinkApi,string pIpAddress,int pPort,List<int> pListChannel,int pSecondsNumberOff)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(pLinkApi);
            ControllerParam param = new ControllerParam();
            param.IpAddress = pIpAddress;
            param.Port = pPort;
            param.ListChannel = pListChannel;
            param.SetOn = false;
            param.SecondsNumberOff = pSecondsNumberOff;

            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/api/Replay/SetOnAndAutoOff", content);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch(Exception ex)
            {

                return ex.ToString();
            }
            return "";
        }
        public async static Task<string> SetOnOffController(string pLinkApi, string pIpAddress, int pPort, List<int> pListChannel,bool pSetOn)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(pLinkApi);
            ControllerParam param = new ControllerParam();
            param.IpAddress = pIpAddress;
            param.Port = pPort;
            param.ListChannel = pListChannel;
            param.SetOn = pSetOn;
            param.SecondsNumberOff = 0;

            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/api/Replay/SetOnOff", content);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return "";
        }

        public class ControllerParam
        {
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public List<int> ListChannel { get; set; }
            public bool SetOn { get; set; }
            public int SecondsNumberOff { get; set; }
        }
    }
}
