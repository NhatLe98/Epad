using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/OperationLog/[action]")]
    [ApiController]
    public class IC_OperationLogController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        public IC_OperationLogController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
        }

        [Authorize]
        [ActionName("AddOperationLog")]
        [HttpPost]
        public IActionResult AddOperationLog([FromBody]OperationLogPram Param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            if (Param != null)
            {
                if (Param.listOperationLog != null && Param.listOperationLog.Count > 0)
                {
                    try
                    {
                        foreach (var item in Param.listOperationLog)
                        {
                            var check = context.IC_OperationLog.Where(t => t.CompanyIndex == user.CompanyIndex && t.SerialNumber == Param.SerialNumber && t.OpTime == item.time).FirstOrDefault();

                            if (check == null)
                            {
                                IC_OperationLog operationLog = new IC_OperationLog()
                                {
                                    SerialNumber = Param.SerialNumber,
                                    OpTime = item.time,
                                    CompanyIndex = user.CompanyIndex,
                                    OpCode = item.opCode,
                                    AdminID = item.adminID,
                                    Reserve1 = item.res1,
                                    Reserve2 = item.res2,
                                    Reserve3 = item.res3,
                                    Reserve4 = item.res4,
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.UserName
                                };
                                context.Entry(operationLog).State = EntityState.Added;

                            }
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

        public class OperationLog
        {
            public short opCode { get; set; }
            public string adminID { get; set; }
            public DateTime time { get; set; }
            public short? res1 { get; set; }
            public short? res2 { get; set; }
            public short? res3 { get; set; }
            public short? res4 { get; set; }
        }

        public class OperationLogPram
        {
            public List<OperationLog> listOperationLog { get; set; }
            public string SerialNumber { get; set; }
        }
    }
}
