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
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IC_EmployeeTypeController : ApiControllerBase
    {
        private IMemoryCache cache;
        IIC_EmployeeTypeService _IC_EmployeeTypeService;
        public IC_EmployeeTypeController(IServiceProvider pProvider) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _IC_EmployeeTypeService = TryResolve<IIC_EmployeeTypeService>();
        }

        [Authorize]
        [ActionName("GetEmployeeTypes")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeTypes(int page, string filter, int pageSize)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _IC_EmployeeTypeService.GetDataByPage(user.CompanyIndex, page, filter, pageSize);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetUsingEmployeeType")]
        [HttpGet]
        public async Task<IActionResult> GetUsingEmployeeType()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var grid = await _IC_EmployeeTypeService.GetUsingEmployeeType(user.CompanyIndex);
            return ApiOk(grid);
        }

        [Authorize]
        [ActionName("GetEmployeeTypesAll")]
        [HttpGet]
        public IActionResult GetEmployeeTypesAll()
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }
            var grid = _IC_EmployeeTypeService.GetDataByCompanyIndex(user.CompanyIndex).Result;
            var dataGridClass = new DataGridClass(grid.Count, grid);
            return ApiOk(dataGridClass);
        }

        [Authorize]
        [ActionName("AddEmployeeType")]
        [HttpPost]
        public async Task<IActionResult> AddEmployeeType(IC_EmployeeType param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var employeeTypeByCode = await _IC_EmployeeTypeService.GetDataByCodeAndCompanyIndex(param.Code, user.CompanyIndex);
            if (employeeTypeByCode != null)
            {
                return ApiConflict("EmployeeTypeCodeExists");
            }

            var employeeTypeByName = await _IC_EmployeeTypeService.GetDataByNameAndCompanyIndex(param.Code, user.CompanyIndex);
            if (employeeTypeByName != null)
            {
                return ApiConflict("EmployeeTypeNameExists");
            }

            var isSuccess = await _IC_EmployeeTypeService.AddEmployeeType(param, user);
            return ApiOk(isSuccess);
        }

        [Authorize]
        [ActionName("UpdateEmployeeType")]
        [HttpPut]
        public async Task<IActionResult> UpdateEmployeeType(IC_EmployeeType param)
        {
            UserInfo user = GetUserInfo();

            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var employeeType = await _IC_EmployeeTypeService.GetDataByIndex(param.Index);
            if (employeeType == null)
            {
                return ApiConflict("EmployeeTypeNotExist");
            }

            var employeeTypeByCode = await _IC_EmployeeTypeService.GetDataByCodeAndCompanyIndex(param.Code, user.CompanyIndex);
            if (employeeTypeByCode != null && employeeTypeByCode.Index != param.Index)
            {
                return ApiConflict("EmployeeTypeCodeExists");
            }

            var employeeTypeByName = await _IC_EmployeeTypeService.GetDataByNameAndCompanyIndex(param.Code, user.CompanyIndex);
            if (employeeTypeByName != null && employeeTypeByName.Index != param.Index)
            {
                return ApiConflict("EmployeeTypeNameExists");
            }

            var isSuccess = await _IC_EmployeeTypeService.UpdateEmployeeType(param, user);

            return ApiOk(isSuccess);
        }


        [Authorize]
        [ActionName("DeleteEmployeeType")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEmployeeType([FromBody] List<int> listIndex)
        {
            UserInfo user = GetUserInfo();
            IActionResult result = ApiUnauthorized();
            if (user == null)
            {
                return result;
            }

            var deleteMessage = await _IC_EmployeeTypeService.DeleteEmployeeTypes(listIndex, user);
            return ApiOk(deleteMessage);
        }
    }
}
