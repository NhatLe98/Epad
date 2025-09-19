using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using GCS_API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_AccessedGroup/[action]")]
    [ApiController]
    public class AC_AccessedGroupController : ApiControllerBase
    {
        private IMemoryCache cache;
        private EPAD_Context context;
        private readonly IAC_AccessedGroupService _AC_AccessedGroupService;
        private readonly IHR_UserService _HR_UserService;
        public AC_AccessedGroupController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _AC_AccessedGroupService = TryResolve<IAC_AccessedGroupService>();
            _HR_UserService = TryResolve<IHR_UserService>();
        }

        [Authorize]
        [ActionName("GetAccessedGroupAtPage")]
        [HttpPost]
        public IActionResult GetAccessedGroupAtPage([FromBody] AC_AccessedGroupParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _AC_AccessedGroupService.GetDataGrid(user.CompanyIndex, param.page, param.limit, param.filter, param.groups, param.departments);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddEmployeeAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeAccessedGroup([FromBody] Employee_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var userInfoList = _HR_UserService.Where(x => request.EmployeeATIDs.Contains(x.EmployeeATID)).ToList();
            var employeeExist = "";
            var isCheck = false;
            foreach (string employeeATID in request.EmployeeATIDs)
            {
                var check = await _AC_AccessedGroupService.GetByEmployeeAndFromToDate(employeeATID, user, request.GroupIndex.Value);
                if (check != null)
                {
                    employeeExist += "<p>  - " + employeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == employeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    isCheck = true;
                }
                else
                {
                    var data = new AC_AccessedGroup()
                    {
                        EmployeeATID = employeeATID,
                        GroupIndex = request.GroupIndex.Value,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        CompanyIndex = user.CompanyIndex
                    };

                    _AC_AccessedGroupService.Insert(data);
                }
            }

            if (!isCheck)
            {
                SaveChange();
            }
            else
            {
                return ApiOk(employeeExist);
            }


            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateEmployeeAccessedGroup")]
        [HttpPut]
        public async Task<IActionResult> UpdateEmployeeAccessedGroup([FromBody] AC_AccessedGroup request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = _AC_AccessedGroupService.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex
            && x.EmployeeATID == request.EmployeeATID);

            if (data == null) return ApiError("MSG_ObjectNotExisted");

            var userInfoList = _HR_UserService.Where(x => x.EmployeeATID == request.EmployeeATID).ToList();
            var check = await _AC_AccessedGroupService.GetByEmployeeAndFromToDateEdit(request.EmployeeATID, user, request.GroupIndex);
            if (check != null)
            {
                var employeeExist = "<p>  - " + check.EmployeeATID + " " + userInfoList.FirstOrDefault(x => x.EmployeeATID == check.EmployeeATID)?.FullName + "</p>" + "<br>";
                return ApiOk(employeeExist);
            }

            data.GroupIndex = request.GroupIndex;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = user.UserName;

            var rs = _AC_AccessedGroupService.Update(data);
            SaveChange();
            return ApiOk();

        }

        [Authorize]
        [ActionName("DeleteEmployeeAccessedGroup")]
        [HttpDelete]
        public IActionResult DeleteEmployeeAccessedGroup([FromBody] List<int> listItem)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            try
            {
                var dataDelete = _DbContext.AC_AccessedGroup.Where(x => listItem.Contains(x.Index)).ToList();
                _DbContext.AC_AccessedGroup.RemoveRange(dataDelete);
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
            return ApiOk();
        }


        public class Employee_AccessedGroupRequest
        {
            public List<int> DepartmentIDs { get; set; }
            public List<string> EmployeeATIDs { get; set; }
            public int? GroupIndex { get; set; }
        }

    }
}
