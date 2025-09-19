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
    [Route("api/AC_DepartmentAccessedGroup/[action]")]
    [ApiController]
    public class AC_DepartmentAccessedGroupController : ApiControllerBase
    {
        private IMemoryCache cache;
        private EPAD_Context context;
        //private readonly IAC_AccessedGroupService _AC_AccessedGroupService;
        private readonly IAC_DepartmentAccessedGroupService _AC_DepartmentAccessedGroupService;
        private readonly IHR_UserService _HR_UserService;
        protected readonly IIC_DepartmentService _IC_DepartmentService;
        public AC_DepartmentAccessedGroupController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            //_AC_AccessedGroupService = TryResolve<IAC_AccessedGroupService>();
            _AC_DepartmentAccessedGroupService = TryResolve<IAC_DepartmentAccessedGroupService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _IC_DepartmentService = TryResolve<IIC_DepartmentService>();
        }

        [Authorize]
        [ActionName("GetDepartmentAccessedGroupAtPage")]
        [HttpPost]
        public IActionResult GetDepartmentAccessedGroupAtPage([FromBody] AC_AccessedGroupParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            var result = _AC_DepartmentAccessedGroupService.GetDataGrid(user.CompanyIndex, param.page, param.limit, param.filter, param.groups, param.departments);
            return ApiOk(result);
        }

        [Authorize]
        [ActionName("AddDepartmentAccessedGroup")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeAccessedGroup([FromBody] Departement_AccessedGroupRequest request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var departmentList = _IC_DepartmentService.Where(x => request.DepartmentIDs.Contains(x.Index)).ToList();
            var departmentExist = "";
            var isCheck = false;
            foreach (int department in request.DepartmentIDs)
            {
                var check = await _AC_DepartmentAccessedGroupService.GetByDepartmentAndFromToDate(department, user, request.GroupIndex.Value);
                if (check != null)
                {
                    departmentExist += "<p>  - " + departmentList.FirstOrDefault(x => x.Index == department)?.Name + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    isCheck = true;
                }
                else
                {
                    var data = new AC_DepartmentAccessedGroup()
                    {
                        DepartmentIndex = department,
                        GroupIndex = request.GroupIndex.Value,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,
                        CompanyIndex = user.CompanyIndex
                    };

                    _AC_DepartmentAccessedGroupService.Insert(data);
                }
            }

            if (!isCheck)
            {
                SaveChange();
            }
            else
            {
                return ApiOk(departmentExist);
            }


            return ApiOk();
        }

        [Authorize]
        [ActionName("UpdateDepartmentAccessedGroup")]
        [HttpPut]
        public async Task<IActionResult> UpdateDepartmentAccessedGroup([FromBody] AC_DepartmentAccessedGroup request)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var data = _AC_DepartmentAccessedGroupService.FirstOrDefault(x => x.CompanyIndex == user.CompanyIndex
            && x.DepartmentIndex == request.DepartmentIndex);

            if (data == null) return ApiError("MSG_ObjectNotExisted");

            var departmentList = _IC_DepartmentService.Where(x => request.DepartmentIndex == x.Index).ToList();
            var check = await _AC_DepartmentAccessedGroupService.GetByDepartmentAndFromToDateEdit(request.DepartmentIndex, user, request.GroupIndex);
            if (check != null)
            {
                var employeeExist = "<p>  - " + departmentList.FirstOrDefault(x => x.Index == check.DepartmentIndex)?.Name + "</p>" + "<br>";
                return ApiOk(employeeExist);
            }

            data.GroupIndex = request.GroupIndex;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedUser = user.UserName;

            var rs = _AC_DepartmentAccessedGroupService.Update(data);
            SaveChange();
            return ApiOk();

        }

        [Authorize]
        [ActionName("DeleteDepartmentAccessedGroup")]
        [HttpDelete]
        public IActionResult DeleteDepartmentAccessedGroup([FromBody] List<int> listItem)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            try
            {
                var dataDelete = _DbContext.AC_DepartmentAccessedGroup.Where(x => listItem.Contains(x.Index)).ToList();
                _DbContext.AC_DepartmentAccessedGroup.RemoveRange(dataDelete);
                _DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
            return ApiOk();
        }


        public class Departement_AccessedGroupRequest
        {
            public List<int> DepartmentIDs { get; set; }
            public int? GroupIndex { get; set; }
        }

    }
}
