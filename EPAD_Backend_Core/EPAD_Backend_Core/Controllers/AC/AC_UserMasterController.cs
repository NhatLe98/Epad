using EPAD_Backend_Core.Base;
using EPAD_Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using EPAD_Common.Extensions;
using EPAD_Services.Interface;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/AC_UserMaster/[action]")]
    [ApiController]
    public class AC_UserMasterController : ApiControllerBase
    {
        private readonly IAC_UserMasterService _IAC_UserMasterService;
        public AC_UserMasterController(IServiceProvider pProvider) : base(pProvider)
        {
            _IAC_UserMasterService = TryResolve<IAC_UserMasterService>();
        }

        [Authorize]
        [ActionName("GetACSync")]
        [HttpPost]
        public async Task<IActionResult> GetACSync(GetACAttendanceLogInfo req)
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();

            DateTime fromTime = req.fromDate.TryGetDateTime();
            DateTime toTime = req.toDate.TryGetDateTime();
            var attendanceLog = await _IAC_UserMasterService.GetACSync(req.filter, fromTime, toTime, 
                req.departmentIds, user.CompanyIndex, req.page, req.limit, req.listDoor, req.listArea,
                req.viewMode, req.viewOperation);
            return ApiOk(attendanceLog);
        }

        [Authorize]
        [ActionName("GetACOperation")]
        [HttpGet]
        public IActionResult GetACOperation()
        {
            UserInfo user = GetUserInfo();
            if (user == null) return ApiUnauthorized();
            if (user == null)
            {
                return ApiUnauthorized();
            }

            var dataResult = _IAC_UserMasterService.GetACOperation();

            var result = Ok(dataResult);
            return result;
        }
    }
}
