using EPAD_Backend_Core.WebUtilitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Threading.Tasks;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Common.Utility;
using EPAD_Common.Types;
using EPAD_Common;
using EPAD_Backend_Core.Base;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/LogProcess/[action]")]
    [ApiController]
    public class IC_LogProcessController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_LogProcessController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }
        [Authorize]
        [ActionName("AddRemoteProcessLog")]
        [HttpPost]
        public async Task<IActionResult> AddRemoteProcessLog([FromBody]RemoteProcessLogParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            RemoteProcessLogObject data = new RemoteProcessLogObject();

            data.SerialNumber = param.SerialNumber;
            data.IPAddress = param.IPAddress;
            data.Port = param.Port;
            data.TaskName = param.TaskName;

            data.ListSystemLog = param.ListSystemLog;
            data.CompanyIndex = user.CompanyIndex;
            data.ServiceIndex = user.Index;
            data.ServiceName = user.ServiceName;
            data.LogTime = DateTime.Now;
            data.Action = "AddRemoteProcessLog";

            MongoDBHelper<RemoteProcessLogObject> mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", cache);
            mongoObject.AddDataToCollection(data, true);

            result = Ok();
            return result;
        }

        [ActionName("AddListRemoteProcessLog")]
        [HttpPost]
        public async Task<IActionResult> AddListRemoteProcessLog([FromBody]List<RemoteProcessLogParam> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            List<RemoteProcessLogObject> lsdata = new List<RemoteProcessLogObject>();
            foreach (var param in lsparam)
            {
                RemoteProcessLogObject data = new RemoteProcessLogObject();
                data.SerialNumber = param.SerialNumber;
                data.IPAddress = param.IPAddress;
                data.Port = param.Port;
                data.TaskName = param.TaskName;

                data.ListSystemLog = param.ListSystemLog;
                data.CompanyIndex = user.CompanyIndex;
                data.ServiceIndex = user.Index;
                data.ServiceName = user.ServiceName;
                data.LogTime = DateTime.Now;
                data.Action = "AddRemoteProcessLog";

                lsdata.Add(data);
            }
            MongoDBHelper<RemoteProcessLogObject> mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", cache);

            mongoObject.AddListDataToCollection(lsdata, true);

            result = Ok();
            return result;
        }

        [Authorize]
        [ActionName("GetHistoryUserByFromToTime")]
        [HttpGet]
        public IActionResult GetHistoryUserByFromToTime(int page, string fromTime, string toTime)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                DateTime _fromTime = DateTime.ParseExact(fromTime, "dd/MM/yyyy HH:mm:ss", null);
                DateTime _toTime = DateTime.ParseExact(toTime, "dd/MM/yyyy HH:mm:ss", null);

                MongoDBHelper<UserAccountLogObject> mongoObject = new MongoDBHelper<UserAccountLogObject>("user_account_log", cache);
                IMongoCollection<UserAccountLogObject> collection = mongoObject.GetCollection();
                List<UserAccountLogObject> listData = null;

                List<UserAccountLogObject> _listData = new List<UserAccountLogObject>();

                if (page <= 1)
                {
                    listData = collection.Find(t => t.Time >= _fromTime && t.Time <= _toTime).Limit(GlobalParams.ROWS_NUMBER_SYSTEM_LOG).ToList();
                }
                else
                {
                    int fromRow = GlobalParams.ROWS_NUMBER_SYSTEM_LOG * (page - 1);
                    listData = collection.Find(t => t.Time >= _fromTime && t.Time <= _toTime).Skip(fromRow).Limit(GlobalParams.ROWS_NUMBER_SYSTEM_LOG).ToList();
                }

               

                foreach (UserAccountLogObject item in listData)
                {
                    DateTime time = new DateTime(item.Time.Year, item.Time.Month, item.Time.Day, item.Time.AddHours(7).Hour, item.Time.Minute, item.Time.Second);

                    UserAccountLogObject userAccountLog = new UserAccountLogObject()
                    {
                        _id = item._id,
                        Time = time,
                        TimeString = time.ToString("yyyy-MM-dd HH:mm:ss"),
                        UserName = item.UserName,
                        Action = item.Action,
                        FormName = item.FormName,
                        Detail = ""
                    };
                    _listData.Add(userAccountLog);
                }
                int record = collection.Find(t => t.Time >= _fromTime && t.Time <= _toTime).ToList().Count;
                double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
                DataGridClass dataGrid = new DataGridClass(totalPage, _listData);

                result = Ok(dataGrid);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
                throw ex;
            }
            return result;
        }
    }
}
