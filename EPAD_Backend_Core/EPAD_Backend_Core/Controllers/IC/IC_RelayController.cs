using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/Relay/[action]")]
    [ApiController]
    public class IC_RelayController : ApiControllerBase
    {
        private EPAD_Context context;
        private IMemoryCache cache;
        private ILogger _logger;
        private IIC_RelayControllerService _IIC_RelayControllerService;
        private IIC_ModbusReplayControllerLogic _IIC_ModbusReplayControllerLogic;
        private IIC_ClientTCPControllerLogic _IIC_ClientTCPControllerLogic;

        public IC_RelayController(IServiceProvider provider, ILogger<IC_RelayController> logger) : base(provider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();
            _logger = logger;
            _IIC_RelayControllerService = TryResolve<IIC_RelayControllerService>();
            _IIC_ModbusReplayControllerLogic = TryResolve<IIC_ModbusReplayControllerLogic>();
            _IIC_ClientTCPControllerLogic = TryResolve<IIC_ClientTCPControllerLogic>();
        }

        [Authorize]
        [ActionName("GetAllController")]
        [HttpGet]
        public IActionResult GetAllController()
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                var listController = context.IC_RelayController.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();
                var listChannel = context.IC_RelayControllerChannel.Where(t => t.CompanyIndex == user.CompanyIndex).ToList();

                var listData = new List<ControllerParam>();
                for (int i = 0; i < listController.Count; i++)
                {
                    var data = new ControllerParam();
                    data.Index = listController[i].Index;
                    data.Name = listController[i].Name;
                    data.IPAddress = listController[i].IpAddress;
                    data.Port = listController[i].Port;
                    data.Description = listController[i].Description;
                    data.RelayType = listController[i].RelayType;
                    data.SignalType = listController[i].SignalType;
                    data.ListChannel = new List<ChannelParam>();

                    var listChannelByController = listChannel.Where(t => t.RelayControllerIndex == data.Index).ToList();
                    foreach (var item in listChannelByController)
                    {
                        data.ListChannel.Add(new ChannelParam() { Index = item.ChannelIndex, NumberOfSecondsOff = item.NumberOfSecondsOff, SignalType = item.SignalType });
                    }

                    listData.Add(data);
                }
                result = Ok(listData);
            }
            catch (Exception ex)
            {
                result = BadRequest("BadRequest");
                _logger.LogError($"{ex}");
            }
            return result;
        }

        [Authorize]
        [ActionName("AddController")]
        [HttpPost]
        public IActionResult AddController([FromBody] ControllerParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (ControllerParam)StringHelper.RemoveWhiteSpace(param);

            var controller = new EPAD_Data.Entities.IC_RelayController();

            try
            {
                controller.CompanyIndex = user.CompanyIndex;
                controller.Name = param.Name;
                controller.IpAddress = param.IPAddress;
                controller.Port = param.Port;
                controller.Description = param.Description;
                controller.RelayType = param.RelayType;
                controller.SignalType = Convert.ToInt32(param.SignalType);

                controller.CreatedDate = DateTime.Now;
                controller.UpdatedDate = DateTime.Now;
                controller.UpdatedUser = user.UserName;

                context.IC_RelayController.Add(controller);
                context.SaveChanges();
                result = Ok(controller);
            }
            catch (Exception ex)
            {
                result = BadRequest("BadRequest");
                _logger.LogError($"{ex}");
            }
            return result;
        }

        [Authorize]
        [ActionName("UpdateController")]
        [HttpPost]
        public IActionResult UpdateController([FromBody] ControllerParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            param = (ControllerParam)StringHelper.RemoveWhiteSpace(param);

            try
            {
                var controller = context.IC_RelayController.Where(t => t.Index == param.Index).FirstOrDefault();
                if (controller == null)
                {
                    return NotFound("ControllerNotExist");
                }

                controller.Name = param.Name;
                controller.IpAddress = param.IPAddress;
                controller.Port = param.Port;
                controller.Description = param.Description;
                controller.RelayType = param.RelayType;
                controller.SignalType = Convert.ToInt32(param.SignalType);

                controller.UpdatedDate = DateTime.Now;
                controller.UpdatedUser = user.UserName;
                //update channel
                context.IC_RelayControllerChannel.RemoveRange(context.IC_RelayControllerChannel.Where(t => t.RelayControllerIndex == param.Index));
                for (int i = 0; i < param.ListChannel.Count; i++)
                {
                    IC_RelayControllerChannel channel = new IC_RelayControllerChannel();
                    channel.RelayControllerIndex = param.Index;
                    channel.ChannelIndex = param.ListChannel[i].Index;
                    channel.CompanyIndex = user.CompanyIndex;
                    channel.NumberOfSecondsOff = param.ListChannel[i].NumberOfSecondsOff;
                    channel.SignalType = param.ListChannel[i].SignalType;
                    channel.UpdatedDate = DateTime.Now;
                    channel.UpdatedUser = user.UserName;

                    context.IC_RelayControllerChannel.Add(channel);
                }
                context.SaveChanges();
                result = Ok();
            }
            catch (Exception ex)
            {
                result = BadRequest("BadRequest");
                _logger.LogError($"{ex}");
            }
            return result;
        }

        [Authorize]
        [ActionName("RemoveController")]
        [HttpPost]
        public IActionResult RemoveController([FromBody] ControllerParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            try
            {
                var controller = context.IC_RelayController.Where(t => t.Index == param.Index).FirstOrDefault();
                if (controller == null)
                {
                    return NotFound("ControllerNotExist");
                }
                context.IC_RelayController.Remove(controller);

                //remove channel
                context.IC_RelayControllerChannel.RemoveRange(context.IC_RelayControllerChannel.Where(t => t.RelayControllerIndex == param.Index));

                context.SaveChanges();
                result = Ok();
            }
            catch (Exception ex)
            {
                result = BadRequest("BadRequest");
                _logger.LogError($"{ex}");
            }
            return result;
        }

        [Authorize]
        [ActionName("SetOnOrOffDevice")]
        [HttpPost]
        public async Task<IActionResult> SetOnOrOffDevice([FromBody] ControllerParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                if (param.RelayType == RelayType.ModbusTCP.ToString())
                {
                    if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(param.IPAddress, Convert.ToUInt16(param.Port)))
                    {
                        _IIC_ModbusReplayControllerLogic.OpenChannel(param.ListChannel);
                    }
                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                }
                else
                {
                    _IIC_ClientTCPControllerLogic.SetOnOrOffController(param);
                }
                result = Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex}");
                result = BadRequest("DeviceNotFound");
            }
            return result;
        }

        [Authorize]
        [ActionName("GetChannelStatus")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceStatus([FromBody] ControllerParam param)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = Unauthorized();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }
            try
            {
                if (param.RelayType == RelayType.ModbusTCP.ToString())
                {
                    if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(param.IPAddress, Convert.ToUInt16(param.Port)))
                    {
                        param = _IIC_ModbusReplayControllerLogic.GetChannelStatus(param);
                    }
                    _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                }
                else {
                    param = _IIC_ClientTCPControllerLogic.GetChannelStatus(param);
                }
                result = Ok(param);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex}");
                result = BadRequest("DeviceNotFound");
            }
            return result;
        }

        [Authorize]
        [ActionName("TelnetMultipleRelayController")]
        [HttpPost]
        public async Task<IActionResult> TelnetMultipleRelayController(List<int> lsparam)
        {
            UserInfo user = UserInfo.GetFromCache(cache, User.Identity.Name);
            IActionResult result = ApiOk();
            if (user == null)
            {
                return Unauthorized(PublicFunctions.CreateHttpErrorContent("TokenExpired"));
            }

            var listController = await _IIC_RelayControllerService.GetControllerByIndexes(lsparam);
            if (listController != null && listController.Count > 0)
            {
                var telnetResult = await _IIC_RelayControllerService.MultipleTelnetRelayController(listController);
                return ApiOk(telnetResult);
            }

            return result;
        }
    }
}
