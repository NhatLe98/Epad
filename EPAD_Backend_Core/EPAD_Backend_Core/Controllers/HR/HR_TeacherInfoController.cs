using EPAD_Backend_Core.Base;
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
    public class HR_TeacherInfoController : ApiControllerBase
    {
        private readonly IHR_TeacherInfoService _HR_TeacherInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        public HR_TeacherInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_TeacherInfoService = TryResolve<IHR_TeacherInfoService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
        }

        [Authorize]
        [ActionName("GetTeacherAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_EmployeeInfoResult>>> Get([FromQuery] string[] c, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allTeacher = await _HR_TeacherInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allTeacher);
        }

        [Authorize]
        [ActionName("Get_HR_TeacherInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_TeacherInfoResult>>> Get_HR_TeacherInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_TeacherInfoService.GetAllTeacherInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_TeacherInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_TeacherInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_TeacherInfoService.GetTeacherInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("Post_HR_TeacherInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_TeacherInfo([FromBody] HR_TeacherInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');
            HR_User u = _Mapper.Map<HR_TeacherInfoResult, HR_User>(value);
            u.EmployeeType = (int)EmployeeType.Student;
            HR_TeacherInfo e = _Mapper.Map<HR_TeacherInfoResult, HR_TeacherInfo>(value);
            HR_CardNumberInfo card = _Mapper.Map<HR_TeacherInfoResult, HR_CardNumberInfo>(value);

            BeginTransaction();
            try
            {
                await _HR_UserService.InsertAsync(u);
                await _HR_TeacherInfoService.InsertAsync(e);
                await _HR_CardNumberInfoService.InsertAsync(card);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_TeacherInfoService.GetTeacherInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }

        }

        [Authorize]
        [ActionName("Put_HR_TeacherInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_TeacherInfo(string employeeATID, [FromBody] HR_TeacherInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var u = await _HR_UserService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var e = await _HR_TeacherInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);

            BeginTransaction();
            try
            {
                u = _Mapper.Map<HR_TeacherInfoResult, HR_User>(value);
                e = _Mapper.Map<HR_TeacherInfoResult, HR_TeacherInfo>(value);
                _HR_UserService.Update(u);
                _HR_TeacherInfoService.Update(e);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_TeacherInfoService.GetTeacherInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_TeacherInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_TeacherInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_TeacherInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("DeleteTeacherMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteTeacherMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_TeacherInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await _HR_UserService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
                await SaveChangeAsync();
                CommitTransaction();

                return ApiOk();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }
    }
}
