using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.TimeLog;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_ControllerService : BaseServices<IC_Controller, EPAD_Context>, IIC_ControllerService
    {
        private ILogger _logger;
        private readonly ILoggerFactory _LoggerFactory;
        private IIC_ModbusReplayControllerLogic _IIC_ModbusReplayControllerLogic;
        private IIC_ClientTCPControllerLogic _IIC_ClientTCPControllerLogic;
        private string _linkControllerApi;
        public IC_ControllerService(IConfiguration configuration, IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<IC_ControllerService>();
            _IIC_ModbusReplayControllerLogic = serviceProvider.GetService<IIC_ModbusReplayControllerLogic>();
            _IIC_ClientTCPControllerLogic = serviceProvider.GetService<IIC_ClientTCPControllerLogic>();
            _linkControllerApi = configuration.GetValue<string>("ControllerApi");
        }

        public async Task<bool> SetOnAndAutoOffController(RelayControllerParam param)
        {
            var isSuccess = true;
            try
            {
                var controller = DbContext.IC_RelayController.FirstOrDefault(t => t.Index == param.ControllerIndex);
                if (controller == null)
                {
                    //return BadRequest("ControllerIndexNotExists");
                    _logger.LogError("ControllerIndexNotExists");
                }
                var controllerChannel = DbContext.IC_RelayControllerChannel
                    .FirstOrDefault(t => t.RelayControllerIndex == param.ControllerIndex && param.ListChannel.Contains(t.ChannelIndex));

                string error = "";

                //_logger.LogError($"Type {controller.RelayType} - {param.AutoOff} {param.ControllerIndex} - {string.Join(',', param.ListChannel)} - {param.SetOn}");

                if (controller.RelayType == RelayType.ModbusTCP.ToString())
                {
                    //_logger.LogError("param.AutoOff");
                    if (param.AutoOff)
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannel = new List<ChannelParam>();
                            foreach (var item in param.ListChannel)
                            {
                                listChannel.Add(new ChannelParam() { Index = Convert.ToInt16(item) });
                            }

                            if (controllerChannel != null)
                            {
                                var result = await _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, 
                                    controllerChannel?.NumberOfSecondsOff ?? 4);
                                _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                                //_logger.LogError($"SetOnAndAutoOffController true: {controllerChannel.NumberOfSecondsOff} {result}");
                            }
                        }
                    }
                    else
                    {
                        if (await _IIC_ModbusReplayControllerLogic.ConnectToModbusTCPDevie(controller.IpAddress, Convert.ToUInt16(controller.Port)))
                        {
                            var listChannel = new List<ChannelParam>();
                            foreach (var item in param.ListChannel)
                            {
                                listChannel.Add(new ChannelParam() { Index = Convert.ToInt16(item), ChannelStatus = param.SetOn });
                                //_logger.LogError($"param.ListChannel: {item} - {param.SetOn}");
                            }
                            var result = _IIC_ModbusReplayControllerLogic.SetOnAndAutoOffController(listChannel, 
                                controllerChannel?.NumberOfSecondsOff ?? 4);
                            //var result = _IIC_ModbusReplayControllerLogic.OpenChannel(listChannel);
                            _IIC_ModbusReplayControllerLogic.DisconnectModbusTCPDevice();
                            //_logger.LogError($"SetOnAndAutoOffController false: {controllerChannel?.NumberOfSecondsOff} {result}");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"_linkControllerApi {_linkControllerApi}");
                    if (param.AutoOff)
                    {

                        error = await _IIC_ClientTCPControllerLogic.SetOnAndAutoOffController(controller.IpAddress, controller.Port, param.ListChannel, controllerChannel?.NumberOfSecondsOff ?? 4);
                        //ControllerProcess.SetOnAndAutoOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, controllerChannel == null ? 4 : controllerChannel.NumberOfSecondsOff);
                        _logger.LogError($"error123:{error} - {_linkControllerApi} - NumberOfSecondsOff:{controllerChannel?.NumberOfSecondsOff}");
                    }
                    else
                    {
                        //ControllerProcess.SetOnOffController(_linkControllerApi, controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                        error = await _IIC_ClientTCPControllerLogic.SetOnOffController(controller.IpAddress, controller.Port, param.ListChannel, param.SetOn);
                        _logger.LogError($"error456:{error} {_linkControllerApi} IP:{controller.IpAddress} {string.Join(",", param.ListChannel.ToArray())} SetOn:{param.SetOn}");
                    }
                }
                if (error != "")
                {
                    isSuccess = false;
                    _logger.LogError("Controller: " + error);
                    //return StatusCode(500, error);
                }

            }
            catch (Exception ex)
            {
                isSuccess = false;
                _logger.LogError($"SetOnAndAutoOffController: {ex}");
            }
            return isSuccess;
        }

        public async Task<List<IC_Controller>> GetByFilter(string pFilter, int pCompanyIndex)
        {
            var allController = Where(x => x.CompanyIndex == pCompanyIndex);
            if(pFilter != "")
            {
                allController = allController.Where(x => x.Name.ContainsIgnoreCase(pFilter)
                    || x.IPAddress.ContainsIgnoreCase(pFilter)
                    || x.Port.ToString().ContainsIgnoreCase(pFilter));
            }

            return await Task.FromResult(allController.ToList());
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, int pCompanyIndex, int pPage, int limit)
        {
            var allController = Where(x => x.CompanyIndex == pCompanyIndex);
            if (string.IsNullOrEmpty(pFilter) == false)
            {
                allController = allController.Where(x => x.Name.ContainsIgnoreCase(pFilter)
                    || x.IPAddress.ContainsIgnoreCase(pFilter)
                    || x.Port.ToString().ContainsIgnoreCase(pFilter));
            }

            //var rs = allController.Select(x => new
            //{
            //    x.Index,
            //    x.Name,
            //    x.Port,
            //    x.IPAddress,
            //    x.IDController
            //});

            if (pPage <= 1) pPage = 1;
            var countPage = allController.Count();
            int fromRow = limit * (pPage - 1);

            var lscontroller = allController.OrderBy(t => t.Name).Skip(fromRow).Take(limit).ToList();
            var dataGrid = new DataGridClass(countPage, lscontroller);
            return await Task.FromResult(dataGrid);
        }

        public async Task<IC_Controller> GetByIndex(int pIndex)
        {
            return await FirstOrDefaultAsync(x => x.Index == pIndex);
        }
    }
}
