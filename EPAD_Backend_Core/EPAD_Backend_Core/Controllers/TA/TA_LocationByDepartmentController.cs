using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
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
    public class TA_LocationByDepartmentController : ApiControllerBase
    {
        private IMemoryCache cache;
        private readonly ITA_LocationByDepartmentService _TA_LocationByDepartmentService;
        public TA_LocationByDepartmentController(IConfiguration configuration, IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _TA_LocationByDepartmentService = TryResolve<ITA_LocationByDepartmentService>();
        }

        [Authorize]
        [ActionName("GetLocationByDepartmentAtPage")]
        [HttpGet]
        public IActionResult GetLocationByDepartmentAtPage(int page, int limit, string filter)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _TA_LocationByDepartmentService.GetDataGrid(user.CompanyIndex, page, limit, filter);
            return ApiOk(result);   
        }

        [Authorize]
        [ActionName("AddLocationByDepartment")]
        [HttpPost]
        public async Task<IActionResult> AddLocationByDepartment(TA_LocationByDepartmentDTO data)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = await _TA_LocationByDepartmentService.AddListLocationByDepartment(data, user);

            return ApiOk(result);
        }

        [Authorize]
        [ActionName("UpdateLocationByDepartment")]
        [HttpPut]
        public async Task<IActionResult> UpdateLocationByDepartment(TA_LocationByDepartmentDTO data)
        {
            UserInfo user = GetUserInfo();
            if (user == null)
            {
                return ApiUnauthorized();
            }

            var existLocation = await _TA_LocationByDepartmentService.GetLocationByDepartmentByIndex(data.DepartmentIndexDTO);
            if (existLocation == null)
            {
                return ApiError("UpdateLocationFail");
            }

            var isSuccess = await _TA_LocationByDepartmentService.UpdateLocationByDepartment(data, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("DeleteLocationByDepartment")]
        [HttpDelete]
        public IActionResult DeleteLocationByDepartment([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var deleteDepartment = _TA_LocationByDepartmentService.DeleteListLocationByDepartment(listIndex);
            return ApiOk();
        }
    }
}
