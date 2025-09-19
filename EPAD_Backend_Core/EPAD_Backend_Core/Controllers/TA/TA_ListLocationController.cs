using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TA_ListLocationController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly ITA_ListLocationService _TA_ListLocationService;
        public TA_ListLocationController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_ListLocationService = TryResolve<ITA_ListLocationService>();
        }

        [Authorize]
        [ActionName("GetListLocationAtPage")]
        [HttpGet]
        public IActionResult GetListLocationAtPage(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _TA_ListLocationService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddLocation")]
        [HttpPost]
        public async Task<IActionResult> AddLocation(TA_ListLocation data)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = await _TA_ListLocationService.AddLocation(data, user);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("UpdateLocation")]
        [HttpPut]
        public async Task<IActionResult> UpdateLocation(TA_ListLocationDTO data)
        {
            UserInfo user = GetUserInfo();
            if (user == null)
            {
                return ApiUnauthorized();
            }

            var existLocation = await _TA_ListLocationService.GetLocationByIndex(data.LocationIndex);
            if (existLocation == null)
            {
                return ApiError("UpdateLocationFail");
            }

            var isSuccess = await _TA_ListLocationService.UpdateLocation(data, user, existLocation);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteLocation")]
        [HttpDelete]
        public IActionResult DeleteLocation([FromBody] List<int> data)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var deleteLocation = _TA_ListLocationService.DeleteLocation(data);
            if (deleteLocation != null)
            {
                return ApiOk(deleteLocation);
            }
            return ApiOk();
        }
    }
}