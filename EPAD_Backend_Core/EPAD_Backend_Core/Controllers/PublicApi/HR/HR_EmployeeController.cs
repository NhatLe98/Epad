using EPAD_Backend_Core.Base;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers.HR
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_EmployeeController : ApiControllerBase
    {
        private readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        public HR_EmployeeController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_EmployeeInfoService = TryResolve<IHR_EmployeeInfoService>();
        }


        [ActionName("GetAllEmployee")]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployee(int page, int limit)
        {
            var user = GetUserInfo();
            // if (user == null) return ApiUnauthorized();

            var rs = await _HR_EmployeeInfoService.GetAllEmployeeInfo(new string[0], 1);

            return ApiOkPublic(rs);

        }
    }
}
