using EPAD_Backend_Core.Base;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HR_NannyInfoController : ApiControllerBase
    {
        private readonly IHR_NannyService _HR_NannyService;
        public HR_NannyInfoController(IServiceProvider pProvider) : base(pProvider)
        {
            _HR_NannyService = TryResolve<IHR_NannyService>();
        }

        [Authorize]
        [ActionName("Get_HR_NannyInfos")]
        [HttpGet]
        public async Task<ActionResult<List<HR_NannyInfoResult>>> Get_HR_NannyInfos()
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var allEmployee = await _HR_NannyService.GetAllNannyInfo(new string[0], user.CompanyIndex);
            return ApiOk(allEmployee);
        }

        [Authorize]
        [ActionName("Get_HR_NannyInfo")]
        [HttpGet("{employeeATID}")]
        public async Task<ActionResult<HR_NannyInfoResult>> Get_HR_NannyInfo(string employeeATID)
        {
            var user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            var employee = await _HR_NannyService.GetNannyInfo(employeeATID, user.CompanyIndex);
            return ApiOk(employee);
        }
    }
}
