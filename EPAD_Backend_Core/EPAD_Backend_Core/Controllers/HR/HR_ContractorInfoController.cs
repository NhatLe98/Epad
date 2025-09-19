using EPAD_Backend_Core.Base;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class HR_ContractorInfoController : ApiControllerBase
    {
        private readonly IHR_ContractorInfoService _HR_ContractorInfoService;
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CardNumberInfoService _HR_CardNumberInfoService;
        public HR_ContractorInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_ContractorInfoService = TryResolve<IHR_ContractorInfoService>();
            _HR_UserService = TryResolve<IHR_UserService>();
            _HR_CardNumberInfoService = TryResolve<IHR_CardNumberInfoService>();
        }

        [Authorize]
        [ActionName("GetContractorAtPage")]
        [HttpGet]
        public async Task<ActionResult<List<HR_ContractorInfoResult>>> Get([FromQuery] int page, [FromQuery] int pageSize)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allCustomer = await _HR_ContractorInfoService.GetDataGrid(user.CompanyIndex, page, pageSize);
            return ApiOk(allCustomer);
        }

        [Authorize]
        [ActionName("Get_HR_ContractorInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_ContractorInfoResult>>> Get_HR_ContractorInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_ContractorInfoService.GetAllContractorInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_ContractorInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Get_HR_ContractorInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_ContractorInfoService.GetContractorInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }

        [Authorize]
        [ActionName("Post_HR_ContractorInfo")]
        [HttpPost]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Post_HR_ContractorInfo([FromBody] HR_ContractorInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            value.EmployeeATID = value.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0');

            HR_User u = _Mapper.Map<HR_ContractorInfoResult, HR_User>(value);
            u.EmployeeType = (int)EmployeeType.Guest;
            HR_ContractorInfo e = _Mapper.Map<HR_ContractorInfoResult, HR_ContractorInfo>(value);
            HR_CardNumberInfo card = _Mapper.Map<HR_ContractorInfoResult, HR_CardNumberInfo>(value);

            BeginTransaction();
            try
            {
                await _HR_UserService.InsertAsync(u);
                await _HR_ContractorInfoService.InsertAsync(e);
                await _HR_CardNumberInfoService.InsertAsync(card);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_ContractorInfoService.GetContractorInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }

        }

        [Authorize]
        [ActionName("Put_HR_ContractorInfo")]
        [HttpPut("{employeeATID}")]
        public async Task<ActionResult<HR_EmployeeInfoResult>> Put_HR_ContractorInfo(string employeeATID, [FromBody] HR_ContractorInfoResult value)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var u = await _HR_UserService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);
            var e = await _HR_ContractorInfoService.FirstOrDefaultAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex, true);

            BeginTransaction();
            try
            {
                u = _Mapper.Map<HR_ContractorInfoResult, HR_User>(value);
                e = _Mapper.Map<HR_ContractorInfoResult, HR_ContractorInfo>(value);
                _HR_UserService.Update(u);
                _HR_ContractorInfoService.Update(e);
                await SaveChangeAsync();
                CommitTransaction();

                var employee = await _HR_ContractorInfoService.GetContractorInfo(u.EmployeeATID, user.CompanyIndex);
                return ApiOk(employee);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                return ApiError(ex.Message);
            }
        }

        [Authorize]
        [ActionName("Delete_HR_ContractorInfo")]
        [HttpDelete("{employeeATID}")]
        public async Task<IActionResult> Delete_HR_ContractorInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            BeginTransaction();
            try
            {
                await _HR_ContractorInfoService.DeleteAsync(x => x.EmployeeATID == employeeATID && x.CompanyIndex == user.CompanyIndex);
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
        [ActionName("DeleteCustomerMulti")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomerMulti([FromBody] string[] listEmployee)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            var empLookup = listEmployee.ToHashSet();

            BeginTransaction();
            try
            {
                await _HR_ContractorInfoService.DeleteAsync(x => empLookup.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex);
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
