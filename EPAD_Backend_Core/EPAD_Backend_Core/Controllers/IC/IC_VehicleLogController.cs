using EPAD_Backend_Core.Base;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/IC_VehicleLog/[action]")]
    [ApiController]
    public class IC_VehicleLogController : ApiControllerBase
    {
        private readonly IIC_VehicleLogService _IC_VehicleLogService;
        private string mCommunicateToken;
        public IC_VehicleLogController(IServiceProvider pProvider, IConfiguration configuration) : base(pProvider)
        {
            _IC_VehicleLogService = TryResolve<IIC_VehicleLogService>();
            mCommunicateToken = configuration.GetValue<string>("CommunicateToken");
        }

        [AllowAnonymous]
        [ActionName("AddVehicleLog")]
        [HttpPost]
        public async Task<IActionResult> AddVehicleLog([FromBody] List<IC_VehicleLog> param)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }
            await _IC_VehicleLogService.IntegrateAttendanceLog(param);
            return Ok();
        }

        [AllowAnonymous]
        [ActionName("AddVehicleLogException")]
        [HttpPost]
        public async Task<IActionResult> AddVehicleLogException([FromBody] List<IC_VehicleLog> param)
        {
            string token = "";
            for (int i = 0; i < HttpContext.Request.Headers.Keys.Count; i++)
            {
                if (HttpContext.Request.Headers.Keys.ElementAt(i).ToLower() == "api-token")
                {
                    token = HttpContext.Request.Headers.Values.ElementAt(i)[0].ToString();
                    break;
                }
            }

            if (token != mCommunicateToken)
            {
                return BadRequest("Token invalid");
            }
            await _IC_VehicleLogService.IntegrateAttendanceLogError(param);
            return Ok();
        }

    }
}
