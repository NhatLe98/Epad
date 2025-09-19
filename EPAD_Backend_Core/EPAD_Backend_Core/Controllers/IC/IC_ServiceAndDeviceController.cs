using System;
using System.Collections.Generic;
using System.Linq;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/ServiceAndDevice/[action]")]
    [ApiController]
    public class IC_ServiceAndDeviceController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private IIC_ServiceAndDeviceLogic _iC_ServiceAndDeviceLogic;
        public IC_ServiceAndDeviceController(IServiceProvider provider):base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _iC_ServiceAndDeviceLogic = TryResolve<IIC_ServiceAndDeviceLogic>();
        }
        [Authorize]
        [ActionName("GetMany")]
        [HttpPost]
        public IActionResult GetMany([FromBody] List<AddedParam> addedParams) {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var listService = new List<IC_ServiceAndDeviceDTO>();
            if (addedParams != null) {
                addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = user.CompanyIndex });
                listService = _iC_ServiceAndDeviceLogic.GetMany(addedParams).ToList() ;
            }
            var resultData = listService.Select(u => new { u.SerialNumber, u.ServiceType, u.IPAddress, u.AliasName }).Distinct();//.Select(c => new { c.Key, Count = c.Count() }).ToList();
            result = Ok(resultData);
            return result;
        }
    }
}
