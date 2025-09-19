using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/SystemLog/[action]")]
    [ApiController]
    public class IC_SystemLogController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_SystemLogController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }
        [Authorize]
        [ActionName("GetSystemLog")]
        [HttpGet]
        public IActionResult GetSystemLog(int page, string filter, string fromTime, string toTime)
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


                MongoDBHelper<RemoteProcessLogObject> mongoObject = new MongoDBHelper<RemoteProcessLogObject>("process_log", cache);
                IMongoCollection<RemoteProcessLogObject> collection = mongoObject.GetCollection();
                if(collection == null)
                {
                    return Ok();
                }
                List<RemoteProcessLogObject> listData = null;
                List<RemoteProcessLog> _listData = new List<RemoteProcessLog>();
                if (page <= 1)
                {
                    listData = collection.Find(t => t.LogTime >= _fromTime && t.LogTime <= _toTime).Limit(GlobalParams.ROWS_NUMBER_SYSTEM_LOG).ToList();
                }
                else
                {
                    int fromRow = GlobalParams.ROWS_NUMBER_SYSTEM_LOG * (page - 1);
                    listData = collection.Find(t => t.LogTime >= _fromTime && t.LogTime <= _toTime).Skip(fromRow).Limit(GlobalParams.ROWS_NUMBER_SYSTEM_LOG).ToList();
                }

                for (int i = 0; i < listData.Count; i++)
                {
                    DateTime time = new DateTime(listData[i].LogTime.Year, listData[i].LogTime.Month, listData[i].LogTime.Day, listData[i].LogTime.AddHours(7).Hour, listData[i].LogTime.Minute, listData[i].LogTime.Second);
                    RemoteProcessLog remoteProcessLog = new RemoteProcessLog();
                    remoteProcessLog.Time = time;
                    remoteProcessLog.TimeString = time.ToString("yyyy-MM-dd HH:mm:ss");
                    remoteProcessLog.SDKFunction = listData[i].ListSystemLog[0].SDKFunction;
                    remoteProcessLog.Data = listData[i].ListSystemLog[0].Data;
                    remoteProcessLog.Result = listData[i].ListSystemLog[0].Result;

                    _listData.Add(remoteProcessLog);
                }

                //int record = collection.Find(t => t.LogTime >= _fromTime && t.LogTime <= _toTime).ToList().Count;
                //double totalPage = ConfigObject.CheckDoubleNumber((record / GlobalParams.ROWS_NUMBER_IN_PAGE).ToString());
                
                _listData = _listData.Where(t => t.Result != null && t.Result.Equals("UnExecute") == false && t.Time >= _fromTime && t.Time <= _toTime).ToList();
                int record = _listData.Count;
                DataGridClass dataGrid = new DataGridClass(record, _listData);


                result = Ok(dataGrid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public class RemoteProcessLog
        {
            public ObjectId _id { get; set; }
            public DateTime Time { get; set; }
            public string TimeString { get; set; }
            public string SDKFunction { get; set; }
            public string Data { get; set; }
            public string Result { get; set; }
        }
    }
}
