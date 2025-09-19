using EPAD_Backend_Core.Base;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_ClassInfoController : ApiControllerBase
    {
        private readonly IHR_ClassInfoService _HR_ClassInfoService;
        public HR_ClassInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_ClassInfoService = TryResolve<IHR_ClassInfoService>();
        }
        [Authorize]
        [ActionName("GetHRClassInfoAtPage")]
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] string filter, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            //var allEmployee = await _HR_EmployeeInfoService.GetDataGrid("", d , user.CompanyIndex, page, pageSize);

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "PageIndex", Value = page });
            addedParams.Add(new AddedParam { Key = "PageSize", Value = pageSize });
            var allEmployee = await _HR_ClassInfoService.GetPage(addedParams, user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Post_HR_ClassInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_ClassInfoResult>> Post_HR_ClassInfo([FromBody] HR_ClassInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var check = await _HR_ClassInfoService.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex && x.Code == value.Code);
            if (check != null)
                return ApiError("ClassInfoExist");
            try
            {
                HR_ClassInfo c = _Mapper.Map<HR_ClassInfoResult, HR_ClassInfo>(value);
                c.Index = Guid.NewGuid().ToString();
                await _HR_ClassInfoService.InsertAsync(c);
                await SaveChangeAsync();
                return ApiOk(c);
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Put_HR_ClassInfo")]
        [HttpPut("{classIndex}")]
        public async Task<ActionResult<HR_ClassInfoResult>> Put_HR_ClassInfo(string classIndex, [FromBody] HR_ClassInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var c = await _HR_ClassInfoService.FirstOrDefaultAsync(x => x.Index == classIndex && x.CompanyIndex == user.CompanyIndex, true);
            try
            {
                c = _Mapper.Map(value, c);
                _HR_ClassInfoService.Update(c);
                await SaveChangeAsync();
                return ApiOk(c);
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_ClassInfo")]
        [HttpDelete("{classIndex}")]
        public async Task<IActionResult> Delete_HR_ClassInfo(string classIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            try
            {
                await _HR_ClassInfoService.DeleteAsync(x => x.Index == classIndex && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();

                return ApiOk();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("DeleteClassInfoMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteClassInfoMulti([FromBody] string[] listClassIndex)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var classLookup = listClassIndex.ToHashSet();

            try
            {
                await _HR_ClassInfoService.DeleteAsync(x => classLookup.Contains(x.Index) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();

                return ApiOk();
            }
            catch (Exception ex)
            {
                return ApiError(ex.Message);
            }
        }

        //[Authorize]
        [ActionName("Get_HR_ClassInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_ClassInfo>>> Get_HR_ClassInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_ClassInfoService.GetAllClassInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_ClassInfo")]
        [HttpGet("{classID}")]
        public async Task<ActionResult<HR_ClassInfo>> Get_HR_ClassInfo(string classID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var classInfo = await _HR_ClassInfoService.GetAllClassInfoByClassID(classID, user.CompanyIndex);
            return ApiOk(classInfo);
        }

        [Authorize]
        [ActionName("Get_HR_ClassInfoByCode")]
        [HttpGet("{code}")]
        public async Task<ActionResult<HR_ClassInfo>> Get_HR_ClassInfoByCode(string code)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var classInfo = await _HR_ClassInfoService.GetAllClassInfoByClassCode(code, user.CompanyIndex);
            return ApiOk(classInfo);
        }

        [Authorize]
        [ActionName("Get_HR_ClassInfoByNanny")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_ClassInfo>> Get_HR_ClassInfoByNanny(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_ClassInfoService.GetClassInfoByNanny(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }
    }
}
