using System;
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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GC_AccessedGroupController : ApiControllerBase
    {
        IGC_AccessedGroupService _GC_AccessedGroupService;
        public GC_AccessedGroupController(IServiceProvider pServiceProvider) : base(pServiceProvider)
        {
            _GC_AccessedGroupService = TryResolve<IGC_AccessedGroupService>();
        }

        [Authorize]
        [ActionName("GetAccessedGroup")]
        [HttpGet]
        public async Task<IActionResult> GetAccessedGroup(string filter, int page, int pageSize)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_AccessedGroupService.GetAccessedGroup(page, pageSize, filter, user);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("AddAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> AddAccessedGroup(GC_AccessedGroup param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var accessedGroup = await _GC_AccessedGroupService.GetDataByNameAndCompanyIndex(param.Name, user.CompanyIndex);
            if (accessedGroup != null)
            {
                return ApiError("AccessedGroupExists");
            }

            if (param.IsGuestDefaultGroup)
            {
                var checkGuest = _GC_AccessedGroupService.CheckExistGuestGroup(user.CompanyIndex, 0);
                if (checkGuest)
                {
                    return ApiError("GuestAccessedGroupIsExist");
                }
            }

            if (param.IsDriverDefaultGroup)
            {
                var checkGuest = _GC_AccessedGroupService.CheckExistDriverGroup(user.CompanyIndex, 0);
                if (checkGuest)
                {
                    return ApiError("DriverAccessedGroupIsExist");
                }
            }

            var statusInsert = await _GC_AccessedGroupService.AddAreaGroup(param, user);
            if (!statusInsert)
            {
                return ApiError("AddAccessedGroup_Fail");
            }
            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateAccessedGroup")]
        [HttpPost]
        public IActionResult UpdateAccessedGroup(GC_AccessedGroup param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var accessGroup = _GC_AccessedGroupService.GetDataByIndex(param.Index);
            if (accessGroup == null)
            {
                return ApiConflict("AreaGroupNotFound");
            }

            if (param.IsGuestDefaultGroup)
            {
                var checkGuest = _GC_AccessedGroupService.CheckExistGuestGroup(user.CompanyIndex, param.Index);
                if (checkGuest)
                {
                    return ApiError("GuestAccessedGroupIsExist");
                }
            }

            if (param.IsDriverDefaultGroup)
            {
                var checkGuest = _GC_AccessedGroupService.CheckExistDriverGroup(user.CompanyIndex, param.Index);
                if (checkGuest)
                {
                    return ApiError("DriverAccessedGroupIsExist");
                }
            }

            accessGroup.Name = param.Name;
            accessGroup.NameInEng = param.NameInEng;
            accessGroup.Description = param.Description;
            accessGroup.CompanyIndex = user.CompanyIndex;
            accessGroup.UpdatedDate = DateTime.Now;
            accessGroup.UpdatedUser = user.UserName;
            accessGroup.GeneralAccessRuleIndex = param.GeneralAccessRuleIndex;
            accessGroup.ParkingLotRuleIndex = param.ParkingLotRuleIndex;
            accessGroup.IsDriverDefaultGroup = param.IsDriverDefaultGroup;
            accessGroup.IsGuestDefaultGroup = param.IsGuestDefaultGroup;
            _GC_AccessedGroupService.Update(accessGroup);
            SaveChange();
            return ApiOk();
        }


        [Authorize]
        [ActionName("DeleteAccessedGroup")]
        [HttpDelete]
        public IActionResult DeleteAccessedGroup([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            _GC_AccessedGroupService.Delete(x => listIndex.Contains(x.Index));
            SaveChange();
            return ApiOk();
        }

        [Authorize]
        [ActionName("GetDataRulesParkingLot")]
        [HttpGet]
        public async Task<IActionResult> GetDataRulesParkingLot()
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_AccessedGroupService.GetDataRulesParkingLotByCompanyIndex(user.CompanyIndex);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetDataRulesGeneralAccess")]
        [HttpGet]
        public async Task<IActionResult> GetDataRulesGeneralAccess()
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_AccessedGroupService.GetDataRulesGeneralAccessByCompanyIndex(user.CompanyIndex);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetAccessedGroupAll")]
        [HttpGet]
        public async Task<IActionResult> GetAccessedGroupAll()
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_AccessedGroupService.GetDataByCompanyIndex(user.CompanyIndex);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetAccessedGroupNormal")]
        [HttpGet]
        public async Task<IActionResult> GetAccessedGroupNormal()
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _GC_AccessedGroupService.GetAccessedGroupNormal(user.CompanyIndex);
            return ApiOk(grid);
        }
    }
}
