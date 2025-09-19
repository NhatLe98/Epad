using ClosedXML.Excel;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.IC;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static EPAD_Backend_Core.Controllers.IC_ServiceController;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/HR_DormRoom/[action]")]
    [ApiController]
    public class HR_DormRoomController : ApiControllerBase
    {
        private EPAD_Context context;
        private ezHR_Context otherContext;
        private IMemoryCache cache;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHR_DormRoomService _iHR_DormRoomService;
        private readonly IHR_ExcusedAbsentService _iHR_ExcusedAbsentService;
        private readonly IIC_AuditLogic _iIC_AuditLogic;
        private List<long> Ids { get; set; }

        public HR_DormRoomController(IServiceProvider provider, IHostingEnvironment hostingEnvironment) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            otherContext = TryResolve<ezHR_Context>();
            _iHR_DormRoomService = TryResolve<IHR_DormRoomService>();
            _iHR_ExcusedAbsentService = TryResolve<IHR_ExcusedAbsentService>();
            _hostingEnvironment = hostingEnvironment;
            _iIC_AuditLogic = TryResolve<IIC_AuditLogic>();
        }

        [Authorize]
        [ActionName("GetAllFloorLevel")]
        [HttpGet]
        public async Task<IActionResult> GetAllFloorLevel(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _iHR_DormRoomService.GetAllFloorlevel(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetAllDormRoom")]
        [HttpGet]
        public async Task<IActionResult> GetAllDormRoom(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _iHR_DormRoomService.GetAllDormRoom(user);

            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("GetDormRoomAtPage")]
        [HttpGet]
        public async Task<IActionResult> GetDormRoomAtPage(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var dataResult = await _iHR_DormRoomService.GetDormRoom(user, page, limit, filter);
            
            result = Ok(dataResult);
            return result;
        }

        [Authorize]
        [ActionName("AddDormRoom")]
        [HttpPost]
        public async Task<IActionResult> AddDormRoom(HR_DormRoom param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var existedListDormRoom = await _iHR_DormRoomService.GetByCodeOrName(user, param.Code, param.Name);
            if (existedListDormRoom != null)
            {
                if (existedListDormRoom.Any(x => x.Code == param.Code))
                { 
                    return Conflict("DormRoomCodeIsExisted");
                }
                if (existedListDormRoom.Any(x => x.Name == param.Name))
                {
                    return Conflict("DormRoomNameIsExisted");
                }
            }

            var isSuccess = await _iHR_DormRoomService.AddDormRoom(user, param);

            return Ok(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateDormRoom")]
        [HttpPost]
        public async Task<IActionResult> UpdateDormRoom(HR_DormRoom param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var existedListDormRoom = await _iHR_DormRoomService.GetByCodeOrName(user, param.Code, param.Name);
            if (existedListDormRoom != null)
            {
                if (existedListDormRoom.Any(x => x.Name == param.Name && x.Index != param.Index))
                {
                    return Conflict("DormRoomNameIsExisted");
                }
            }

            var isSuccess = await _iHR_DormRoomService.UpdateDormRoom(user, param);

            return Ok(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteDormRoom")]
        [HttpDelete]
        public async Task<IActionResult> DeleteDormRoom([FromBody] List<int> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var usingDormRooms = await _iHR_DormRoomService.GetDormRoomUsingByIndexes(lsparam);
            if (usingDormRooms != null && usingDormRooms.Count > 0)
            {
                return Conflict("DormRoomIsUsing");
            }

            var isSuccess = await _iHR_DormRoomService.DeleteDormRoom(lsparam);

            result = Ok(isSuccess);
            return result;
        }
    }
}
