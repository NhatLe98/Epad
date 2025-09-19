using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/IC_DeviceHistory/[action]")]
    [ApiController]
    public class IC_DeviceHistoryController : ApiControllerBase
    {
        private ILogger _logger;
        private IMemoryCache cache;
        private IIC_DeviceHistoryService _IC_DeviceHistoryService;
        public IC_DeviceHistoryController(IServiceProvider pProvider, ILogger<IC_DeviceHistoryController> logger) : base(pProvider)
        {
            cache = TryResolve<IMemoryCache>();
            _IC_DeviceHistoryService = TryResolve<IIC_DeviceHistoryService>();
            _logger = logger;
        }

        [Authorize]
        [ActionName("GetDeviceHistory")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceHistory(int page, string filter, string fromTime, string toTime, int limit)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                DateTime _fromTime = DateTime.ParseExact(fromTime, "dd/MM/yyyy HH:mm:ss", null);
                DateTime _toTime = DateTime.ParseExact(toTime, "dd/MM/yyyy HH:mm:ss", null);


                var dataGrid = await _IC_DeviceHistoryService.GetDeviceHistory(page, filter, _fromTime, _toTime, user.CompanyIndex, limit);


                result = Ok(dataGrid);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDeviceHistory: {ex}");
                return null;
            }
            return result;
        }

        [Authorize]
        [ActionName("GetDeviceHistoryLast7Days")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceHistoryLast7Days()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                var dataGrid = await _IC_DeviceHistoryService.GetDeviceHistoryLast7Days();

                result = Ok(dataGrid);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDeviceHistoryLast7Days: {ex}");
                return null;
            }
            return result;
        }
    }
}
