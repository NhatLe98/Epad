using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_StateLog/[action]")]
    [ApiController]
    public class AC_StateLogController : ApiControllerBase
    {
        private IMemoryCache cache;
        private EPAD_Context context;
        public AC_StateLogController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            context = TryResolve<EPAD_Context>();
        }

        [Authorize]
        [ActionName("AddStateLog")]
        [HttpPost]
        public async Task<IActionResult> AddStateLog([FromBody] StateLogParam Param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (Param != null)
            {
                if (Param.ListStateLog != null && Param.ListStateLog.Count > 0)
                {
                    try
                    {
                        var minDate = Param.ListStateLog.Select(x => x.Time).Min();
                        var maxDate = Param.ListStateLog.Select(x => x.Time).Max();
                        var listState = context.AC_StateLog.Where(t => t.SerialNumber == Param.SerialNumber && t.Time >= minDate && t.Time <= maxDate).ToList();
                        var lst = new List<AC_StateLog>();

                        foreach (var item in Param.ListStateLog)
                        {
                            var check = listState.FirstOrDefault(x => x.SerialNumber == Param.SerialNumber && x.Time == item.Time);
                            if (check == null)
                            {
                                AC_StateLog stateLog = new AC_StateLog()
                                {
                                    SerialNumber = Param.SerialNumber,
                                    Time = item.Time,
                                    Alarm = item.Alarm,
                                    Door = item.Door,
                                    Relay = item.Relay,
                                    Sensor = item.Sensor,
                                };
                                context.Entry(stateLog).State = EntityState.Added;
                                lst.Add(stateLog);
                            }
                        }
                        if (lst.Count > 0)
                        {
                            var listDataLog = (from log in lst
                                               join doorDevice in context.AC_DoorAndDevice.Where(x => x.CompanyIndex == user.CompanyIndex)
                                                on log.SerialNumber equals doorDevice.SerialNumber into temp
                                               from dummy in temp.DefaultIfEmpty()
                                               join dev in context.AC_Door.Where(x => x.CompanyIndex == user.CompanyIndex)
                                               on dummy.DoorIndex equals dev.Index into devlog
                                               from dl in devlog.DefaultIfEmpty()

                                               join devi in context.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex)
                                            on log.SerialNumber equals devi.SerialNumber into deviLog
                                               from dll in deviLog.DefaultIfEmpty()
                                               select new AC_ACOpenDoor
                                               {
                                                   Optime = log.Time.ToString("dd/MM/yyyy hh:mm:ss"),
                                                   DeviceName = dll != null ? dll.AliasName : "",
                                                   DoorName = dl != null ? dl.Name : "",
                                                   Status = log.Sensor == "01" ? "Mở và đóng cửa" : "Mở cửa"
                                               }).ToList();
                            await SendPushAttendanceLog(listDataLog);
                        }
                        context.SaveChanges();
                    }
                    catch
                    {
                    }
                }
            }

            result = Ok();
            return result;
        }

        private async Task SendPushAttendanceLog(List<AC_ACOpenDoor> listLogs)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            var json = JsonConvert.SerializeObject(listLogs, jsonSettings);
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/PostOperationLog", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _Logger.LogError($"PushAttendanceLog: {ex}");
            }
        }

        public class StateLogParam
        {
            public List<AC_StateLog> ListStateLog { get; set; }
            public string SerialNumber { get; set; }
        }
    }
}
