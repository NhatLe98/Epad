using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.MainProcess;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/PrivilegeMachineRealtime/[action]")]
    [ApiController]
    public class IC_PrivilegeMachineRealtimeController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private readonly ILogger _logger;
        private readonly IIC_PrivilegeMachineRealtimeService _IC_PrivilegeMachineRealtimeService;
        public IC_PrivilegeMachineRealtimeController(IServiceProvider provider, 
            ILogger<IC_PrivilegeMachineRealtimeController> logger) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _logger = logger;
            _IC_PrivilegeMachineRealtimeService = TryResolve<IIC_PrivilegeMachineRealtimeService>();
        }

        [Authorize]
        [ActionName("GetAllPrivilegeMachineRealtime")]
        [HttpGet]
        public async Task<IActionResult> GetAllPrivilegeMachineRealtime([FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var listPrivilegeMachineRealtime = await _IC_PrivilegeMachineRealtimeService.GetPrivilegeMachineRealtime(filter, page, pageSize, 
                user.CompanyIndex);

            return ApiOk(listPrivilegeMachineRealtime);
        }

        [Authorize]
        [ActionName("GetPrivilegeMachineRealtimeByUser")]
        [HttpGet]
        public IActionResult GetPrivilegeMachineRealtimeByUser()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var listPrivilegeMachineRealtime = _IC_PrivilegeMachineRealtimeService.GetPrivilegeMachineRealtimeByUserName(user.UserName, user.CompanyIndex);

            return ApiOk(listPrivilegeMachineRealtime);
        }


        [Authorize]
        [ActionName("AddPrivilegeMachineRealtime")]
        [HttpPost]
        public async Task<IActionResult> AddPrivilegeMachineRealtime([FromBody] IC_PrivilegeMachineRealtimeDTO param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_PrivilegeMachineRealtimeService.AddPrivilegeMachineRealtime(param, user);
            if(result != null && result.Count > 0)
            {
                return ApiOk(result);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdatePrivilegeMachineRealtime")]
        [HttpPost]
        public async Task<IActionResult> UpdatePrivilegeMachineRealtime([FromBody] IC_PrivilegeMachineRealtimeDTO param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_PrivilegeMachineRealtimeService.UpdatePrivilegeMachineRealtime(param, user);
            if (result != null && result.Count > 0)
            {
                return ApiOk(result);
            }

            return ApiOk();
        }

        [Authorize]
        [ActionName("DeletePrivilegeMachineRealtime")]
        [HttpPost]
        public async Task<IActionResult> DeletePrivilegeMachineRealtime([FromBody] List<IC_PrivilegeMachineRealtimeDTO> param)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_PrivilegeMachineRealtimeService.DeletePrivilegeMachineRealtime(param, user);
            if (result != null && result.Count > 0)
            {
                return ApiOk(result);
            }

            return ApiOk();
        }
    }
}
