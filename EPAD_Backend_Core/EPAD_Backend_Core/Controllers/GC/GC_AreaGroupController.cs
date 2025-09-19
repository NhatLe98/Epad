using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_AreaGroupController : ApiControllerBase
    {
        private IMemoryCache cache;
        IGC_AreaGroupService _GC_AreaGroupService;
        IGC_AreaGroup_GroupDeviceService _GC_AreaGroup_GroupDeviceService;
        public GC_AreaGroupController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _GC_AreaGroupService = TryResolve<IGC_AreaGroupService>();
            _GC_AreaGroup_GroupDeviceService = TryResolve<IGC_AreaGroup_GroupDeviceService>();
        }

        [Authorize]
        [ActionName("GetAreaGroups")]
        [HttpGet]
        public async Task<IActionResult> GetAreaGroups(int page, string filter, int pageSize)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var dataResult = await _GC_AreaGroupService.GetAreaGroup(page, filter, pageSize, user);
            var dataGridClass = new DataGridClass(dataResult.Item2, dataResult.Item1);
            return ApiOk(dataGridClass);
        }

        [Authorize]
        [ActionName("GetAreaGroupAll")]
        [HttpGet]
        public async Task<IActionResult> GetAreaGroupAll()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = await _GC_AreaGroupService.GetDataByCompanyIndex(user.CompanyIndex);
            var dataGridClass = new DataGridClass(grid.Count, grid);
            return ApiOk(dataGridClass);
        }

        [Authorize]
        [ActionName("AddAreaGroup")]
        [HttpPost]
        public async Task<IActionResult> AddAreaGroup(AreaGroupRequestModel model)
        {
            UserInfo user = GetUserInfo();

            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var param = model.AreaGroup;
            var area = await _GC_AreaGroupService.GetDataByCodeAndCompanyIndex(param.Code, user.CompanyIndex);
            if (area != null)
            {
                return ApiConflict("AreaCodeIsDuplicate");
            }

            var isAddSuccess = await _GC_AreaGroupService.AddAreaGroup(model, user);

            return ApiOk(isAddSuccess);
        }

        [Authorize]
        [ActionName("UpdateAreaGroup")]
        [HttpPost]
        public async Task<IActionResult> UpdateAreaGroup(AreaGroupRequestModel model)
        {
            UserInfo user = GetUserInfo();

            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var param = model.AreaGroup;
            var area = await _GC_AreaGroupService.GetDataByIndex(param.Index);
            if (area == null)
            {
                return ApiConflict("AreaGroupNotFound");
            }

            var isUpdateSuccess = await _GC_AreaGroupService.UpdateAreaGroup(model, user);

            return ApiOk(isUpdateSuccess);
        }

        [Authorize]
        [ActionName("DeleteAreaGroups")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAreaGroups([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            var result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var isDeleteSuccess = await _GC_AreaGroupService.DeleteAreaGroup(listIndex, user);

            return ApiOk(isDeleteSuccess);
        }

        [Authorize]
        [ActionName("GetAreaGroupByCode")]
        [HttpGet]
        public IActionResult GetAreaGroupByCode(string code)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var dummy = _GC_AreaGroupService.GetDataByCodeAndCompanyIndex(code, user.CompanyIndex).Result;

            return ApiOk(dummy);
        }
    }
}
