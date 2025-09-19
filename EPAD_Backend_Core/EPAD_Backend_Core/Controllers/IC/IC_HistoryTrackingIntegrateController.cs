using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/HistoryTrackingIntegrate/[action]")]
    [ApiController]
    public class IC_HistoryTrackingIntegrateController : ApiControllerBase
    {
        private ILogger _logger;
        private IMemoryCache cache;
        private readonly IIC_HistoryTrackingIntegrateService _IC_HistoryTrackingIntegrateService;
        public IC_HistoryTrackingIntegrateController(IServiceProvider pProvider, ILogger<IC_HistoryTrackingIntegrateController> logger) : base(pProvider)
        {
            _logger = logger;
            cache = TryResolve<IMemoryCache>();
            _IC_HistoryTrackingIntegrateService = TryResolve<IIC_HistoryTrackingIntegrateService>();
        }

        [Authorize]
        [ActionName("GetHistoryTrackingIntegrate")]
        [HttpGet]
        public async Task<IActionResult> GetHistoryTrackingIntegrate(int page, string filter, string fromTime, string toTime, int limit)
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


                var dataGrid = await _IC_HistoryTrackingIntegrateService.GetHistoryTrackingIntegrateInfo(page, filter, _fromTime, _toTime, user.CompanyIndex, limit);


                result = Ok(dataGrid);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetHistoryTrackingIntegrate: {ex}");
                return null;
            }
            return result;
        }
    }
}
