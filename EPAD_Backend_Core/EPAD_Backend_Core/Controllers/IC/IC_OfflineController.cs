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
    [Route("api/IC_Offline/[action]")]
    [ApiController]
    public class IC_OfflineController : ApiControllerBase
    {
        private EPAD_Context _context;
        private IMemoryCache _cache;
        private readonly IIC_OfflineService _IIC_OfflineService;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string mCommunicateToken;
        public IC_OfflineController(IServiceProvider pProvider, ILoggerFactory loggerFactory, IConfiguration configuration) : base(pProvider)
        {
            _context = TryResolve<EPAD_Context>();
            _cache = TryResolve<IMemoryCache>();
            _IIC_OfflineService = TryResolve<IIC_OfflineService>();
            _webHostEnvironment = TryResolve<IWebHostEnvironment>();
            _logger = loggerFactory.CreateLogger<IC_OfflineController>();
            mCommunicateToken = configuration.GetValue<string>("CommunicateToken");
        }

        [ActionName("IntegrateBlackList")]
        [HttpPost]
        public async Task<IActionResult> IntegrateBlackList([FromBody] List<GC_BlackList> blackLists)
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
            try
            {
                await _IIC_OfflineService.IntegrateBlackList(blackLists);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateBlackList: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("DeleteBlackList")]
        [HttpPost]
        public async Task<IActionResult> DeleteBlackList([FromBody] GC_BlackListIntegrate blackList)
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
            try
            {
                await _IIC_OfflineService.DeleteBlackList(blackList.EmployeeATIDs, blackList.NRICs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteBlackList: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateCustomerCardToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateCustomerCardToOffline([FromBody] List<HR_CustomerCard> employeeATIDs)
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
            try
            {
                await _IIC_OfflineService.IntegrateCustomerCardToOffline(employeeATIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateCustomerCardToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("DeleteCustomerCardToOffline")]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomerCardToOffline([FromBody] List<string> employeeATIDs)
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
            try
            {
                await _IIC_OfflineService.DeleteCustomerCardToOffline(employeeATIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteCustomerCardToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateCardToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateCardToOffline([FromBody] List<HR_CardNumberInfo> employeeATIDs)
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
            try
            {
                await _IIC_OfflineService.IntegrateCardToOffline(employeeATIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateCardToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("DeleteCardToOffline")]
        [HttpPost]
        public async Task<IActionResult> DeleteCardToOffline([FromBody] List<HR_CardNumberInfo> employeeATIDs)
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
            try
            {
                await _IIC_OfflineService.DeleteCardToOffline(employeeATIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteCardToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateDepartmentToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateDepartmentToOffline([FromBody] List<IC_Department> departments)
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
            try
            {
                await _IIC_OfflineService.IntegrateDepartmentToOffline(departments);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateDepartmentToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("DeleteDepartmentToOffline")]
        [HttpPost]
        public async Task<IActionResult> DeleteDepartmentToOffline([FromBody] List<string> codes)
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
            try
            {
                await _IIC_OfflineService.DeleteDepartmentToOffline(codes);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteDepartmentToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateUserToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateUserToOffline(List<HR_User> users)
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
            try
            {
                await _IIC_OfflineService.IntegrateUserToOffline(users);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateUserToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("DeleteUserToOffline")]
        [HttpPost]
        public async Task<IActionResult> DeleteUserToOffline(List<string> employeeATIDs)
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
            try
            {
                await _IIC_OfflineService.DeleteUserToOffline(employeeATIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteUserToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateWorkingInfo")]
        [HttpPost]
        public async Task<IActionResult> IntegrateWorkingInfo(List<IC_WorkingInfoIntegrate> workingInfoList)
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
            try
            {
                await _IIC_OfflineService.IntegrateWorkingInfo(workingInfoList);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateWorkingInfo: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateEmployeeToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateEmployeeToOffline(List<HR_EmployeeInfo> userList)
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
            try
            {
                await _IIC_OfflineService.IntegrateEmployeeToOffline(userList);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateEmployeeToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        [ActionName("IntegrateCustmerToOffline")]
        [HttpPost]
        public async Task<IActionResult> IntegrateCustmerToOffline(List<HR_CustomerInfo> userList)
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
            try
            {
                await _IIC_OfflineService.IntegrateCustmerToOffline(userList);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateCustmerToOffline: {ex}");
                return BadRequest(ex.Message);
            }

        }

        public class GC_BlackListIntegrate
        {
            public List<string> EmployeeATIDs { get; set; }
            public List<string> NRICs { get; set; }
        }
    }
}