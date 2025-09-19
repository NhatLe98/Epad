using EPAD_Backend_Core.Base;
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
    [Route("api/EmployeeShift/[action]")]
    [ApiController]
    [Authorize]
    public class IC_EmployeeShiftController : ApiControllerBase
    {
        private readonly IIC_ShiftService _IC_ShiftService;
        private readonly IIC_EmployeeShiftService _IC_EmployeeShiftService;
        private readonly IHR_UserService _IHR_UserService;

        public IC_EmployeeShiftController(IServiceProvider provider) : base(provider)
        {
            _IC_ShiftService = TryResolve<IIC_ShiftService>();
            _IC_EmployeeShiftService = TryResolve<IIC_EmployeeShiftService>();
            _IHR_UserService = TryResolve<IHR_UserService>();
        }
        
        [ActionName("GetAllShifts")]
        [HttpGet]
        public async Task<IActionResult> GetAllShiftAsync()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_ShiftService.GetAllAsync(e => e.CompanyIndex.Equals(user.CompanyIndex));// && e.EndTime >= DateTime.Now);

            return ApiOk(result);
        }

        [ActionName("GetAllShiftsByEmployeeId")]
        [HttpGet]
        public async Task<IActionResult> GetAllShiftByEmployeeAsync([FromQuery] string employeeId)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employeeShifts = await _IC_EmployeeShiftService.GetAllAsync(e => e.CompanyIndex.Equals(user.CompanyIndex) && 
                                                                           e.EmployeeATID.Equals(employeeId));
            var shiftIds = employeeShifts.Select(e => e.IC_ShiftId).ToList();
            var result = await _IC_ShiftService.GetAllAsync(e => e.CompanyIndex.Equals(user.CompanyIndex) && shiftIds.Contains(e.Id));

            return ApiOk(result);
        }

        [ActionName("GetAllEmployeesByShiftId")]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployeesByShiftNameAsync([FromQuery] int shiftId)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employeeShifts = await _IC_EmployeeShiftService.GetAllAsync(e => e.CompanyIndex.Equals(user.CompanyIndex) &&
                                                                           e.IC_ShiftId.Equals(shiftId));
            var employeeIds = employeeShifts.Select(e => e.EmployeeATID).ToList();
            var result = await _IHR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeIds, DateTime.Now, user.CompanyIndex);

            return ApiOk(result);
        }

        [ActionName("GetAllEmployeeShifts")]
        [HttpPost]
        public async Task<IActionResult> GetAllEmployeeShifts([FromBody] GetListEmployeeShiftRequest request)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var result = await _IC_EmployeeShiftService.GetAllEmployeeShifts(request, user.CompanyIndex);

            return ApiOk(result);
        }
    }
}
