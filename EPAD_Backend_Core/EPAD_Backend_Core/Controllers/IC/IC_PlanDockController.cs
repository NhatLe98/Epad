using Chilkat;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/IC_PlanDock/[action]")]
    [ApiController]
    public class IC_PlanDockController : ApiControllerBase
    {
        private EPAD_Context _context;
        private IMemoryCache _cache;
        private readonly IIC_PlanDockService _IC_PlanDockService;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string mCommunicateToken;
        public IC_PlanDockController(IServiceProvider pProvider, ILoggerFactory loggerFactory, IConfiguration configuration) : base(pProvider)
        {
            _context = TryResolve<EPAD_Context>();
            _cache = TryResolve<IMemoryCache>();
            _IC_PlanDockService = TryResolve<IIC_PlanDockService>();
            _webHostEnvironment = TryResolve<IWebHostEnvironment>();
            _logger = loggerFactory.CreateLogger<IC_PlanDockController>();
            mCommunicateToken = configuration.GetValue<string>("CommunicateToken");
        }

        [ActionName("AddDataToPlanDock")]
        [HttpPost]
        public async Task<IActionResult> AddDataToPlanDock([FromBody] List<IC_PlanDockIntegrate> planDock)
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
            await _IC_PlanDockService.AddDataToPlanDock(planDock);
            return StatusCode(StatusCodes.Status201Created, planDock);
        }

        [ActionName("GetAllDataPlanDock")]
        [HttpGet]
        public async Task<IActionResult> GetAllDataPlanDock()
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
            var planDock = await _DbContext.IC_PlanDock.ToListAsync();
            return Ok(planDock);
        }


        [ActionName("UpdateDataPlanDock")]
        [HttpPost]
        public async Task<IActionResult> UpdateDataPlanDock([FromBody] List<IC_PlanDockIntegrate> planDock)
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
            await _IC_PlanDockService.UpdateDataPlanDock(planDock);
            return Ok(planDock);
        }

    }
}
