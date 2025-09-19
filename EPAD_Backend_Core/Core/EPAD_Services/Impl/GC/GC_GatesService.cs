using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_GatesService : BaseServices<GC_Gates, EPAD_Context>, IGC_GatesService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public GC_GatesService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_GatesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<GC_Gates>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _dbContext.GC_Gates.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<GC_Gates> GetDataByIndex(int index)
        {
            return await _dbContext.GC_Gates.FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<GC_Gates> GetDataByNameAndCompanyIndex(string name, int companyIndex)
        {
            return await _dbContext.GC_Gates.Where(x => x.Name == name && x.CompanyIndex == companyIndex).FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetListIndexByGateMandatory(int companyIndex)
        {
            return await _dbContext.GC_Gates.Where(x => x.CompanyIndex == companyIndex && x.IsMandatory).Select(e => e.Index).ToListAsync();
        }

        public async Task<bool> AddGates(GatesModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var gates = new GC_Gates();
                gates.Name = param.Name;
                gates.Description = param.Description;
                gates.CompanyIndex = user.CompanyIndex;
                gates.CreatedDate = DateTime.Now;
                gates.UpdatedDate = DateTime.Now;
                gates.UpdatedUser = user.UserName;
                gates.IsMandatory = param.IsMandatory;

                await _dbContext.GC_Gates.AddAsync(gates);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddGates: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateGates(GatesModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var gates = await _dbContext.GC_Gates.FirstOrDefaultAsync(x => x.Index == param.Index);
                gates.Name = param.Name;
                gates.Description = param.Description;
                gates.CompanyIndex = user.CompanyIndex;
                gates.UpdatedDate = DateTime.Now;
                gates.UpdatedUser = user.UserName;
                gates.IsMandatory = param.IsMandatory;

                _dbContext.GC_Gates.Update(gates);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateGates: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteGates(List<int> indexes)
        {
            var result = true;
            try
            {
                var gates = await _dbContext.GC_Gates.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (gates != null && gates.Count > 0)
                {
                    _dbContext.GC_Gates.RemoveRange(gates);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteGates: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> CheckGateUsing(List<int> indexes)
        {
            var result = await _dbContext.GC_Gates_Lines.AnyAsync(x => indexes.Contains(x.GateIndex));

            return result;
        }

        public async Task<bool> UpdateGateLineDevice(GatesModel param, UserInfo user)
        {
            var result = true;
            try
            {
                if (param.Index > 0)
                {
                    var gateLines = await _dbContext.GC_Gates_Lines.Where(x => x.GateIndex == param.Index).ToListAsync();
                    if (gateLines != null && gateLines.Count > 0)
                    {
                        _dbContext.GC_Gates_Lines.RemoveRange(gateLines);
                    }

                    if (param.Lines != null && param.Lines.Count > 0)
                    {
                        foreach (var line in param.Lines)
                        {
                            var gateLine = new GC_Gates_Lines();
                            gateLine.GateIndex = param.Index;
                            gateLine.LineIndex = line;
                            gateLine.CompanyIndex = user.CompanyIndex;
                            gateLine.UpdatedUser = user.UserName;
                            gateLine.UpdatedDate = DateTime.Now;
                            await _dbContext.GC_Gates_Lines.AddAsync(gateLine);
                        }
                    }
                }

                if (param.LineDevice != null && param.LineDevice.Index > 0)
                {
                    var lineCameraIn = await DbContext.GC_Lines_CheckInCamera.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lineCameraIn != null && lineCameraIn.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckInCamera.RemoveRange(lineCameraIn);
                    }

                    foreach (var camera in param.LineDevice.CameraInIndex)
                    {
                        var lineCamera = new GC_Lines_CheckInCamera();
                        lineCamera.LineIndex = param.LineDevice.Index;
                        lineCamera.CameraIndex = camera;
                        lineCamera.CompanyIndex = user.CompanyIndex;
                        lineCamera.UpdatedUser = user.UserName;
                        lineCamera.UpdatedDate = DateTime.Now;
                        await _dbContext.GC_Lines_CheckInCamera.AddAsync(lineCamera);
                    }

                    var lineCameraOut = await _dbContext.GC_Lines_CheckOutCamera.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lineCameraOut != null && lineCameraOut.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckOutCamera.RemoveRange(lineCameraOut);
                    }

                    foreach (var camera in param.LineDevice.CameraOutIndex)
                    {
                        var lineCamera = new GC_Lines_CheckOutCamera();
                        lineCamera.LineIndex = param.LineDevice.Index;
                        lineCamera.CameraIndex = camera;
                        lineCamera.CompanyIndex = user.CompanyIndex;
                        lineCamera.UpdatedUser = user.UserName;
                        lineCamera.UpdatedDate = DateTime.Now;
                        await _dbContext.GC_Lines_CheckOutCamera.AddAsync(lineCamera);
                    }

                    var lineDeviceIn = await _dbContext.GC_Lines_CheckInDevice.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lineDeviceIn != null && lineDeviceIn.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckInDevice.RemoveRange(lineDeviceIn);
                    }

                    foreach (var device in param.LineDevice.DeviceInSerial)
                    {
                        var lineDevice = new GC_Lines_CheckInDevice();
                        lineDevice.LineIndex = param.LineDevice.Index;
                        lineDevice.CheckInDeviceSerial = device;
                        lineDevice.CompanyIndex = user.CompanyIndex;
                        lineDevice.UpdatedUser = user.UserName;
                        lineDevice.UpdatedDate = DateTime.Now;
                        await _dbContext.GC_Lines_CheckInDevice.AddAsync(lineDevice);
                    }

                    var lineDeviceOut = await _dbContext.GC_Lines_CheckOutDevice.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lineDeviceOut != null && lineDeviceOut.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckOutDevice.RemoveRange(lineDeviceOut);
                    }

                    foreach (var device in param.LineDevice.DeviceOutSerial)
                    {
                        var lineDevice = new GC_Lines_CheckOutDevice();
                        lineDevice.LineIndex = param.LineDevice.Index;
                        lineDevice.CheckOutDeviceSerial = device;
                        lineDevice.CompanyIndex = user.CompanyIndex;
                        lineDevice.UpdatedUser = user.UserName;
                        lineDevice.UpdatedDate = DateTime.Now;
                        await _dbContext.GC_Lines_CheckOutDevice.AddAsync(lineDevice);
                    }

                    var lineControllerIn = await _dbContext.GC_Lines_CheckInRelayController.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lineControllerIn != null && lineControllerIn.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckInRelayController.RemoveRange(lineControllerIn);
                    }

                    foreach (var controller in param.LineDevice.LineControllersIn)
                    {
                        if (controller.ControllerIndex > 0)
                        {
                            var lineController = new GC_Lines_CheckInRelayController();
                            lineController.LineIndex = param.LineDevice.Index;
                            lineController.RelayControllerIndex = controller.ControllerIndex.Value;
                            lineController.OpenDoorChannelIndex = (short)(controller.OpenChannel ?? 0);
                            lineController.FailAlarmChannelIndex = (short)(controller.CloseChannel ?? 0);
                            lineController.CompanyIndex = user.CompanyIndex;
                            lineController.UpdatedUser = user.UserName;
                            lineController.UpdatedDate = DateTime.Now;
                            await _dbContext.GC_Lines_CheckInRelayController.AddAsync(lineController);
                        }
                    }

                    var lienControllerOut = await _dbContext.GC_Lines_CheckOutRelayController.Where(x
                        => x.LineIndex == param.LineDevice.Index).ToListAsync();
                    if (lienControllerOut != null && lienControllerOut.Count > 0)
                    {
                        _dbContext.GC_Lines_CheckOutRelayController.RemoveRange(lienControllerOut);
                    }

                    foreach (var controller in param.LineDevice.LineControllersOut)
                    {
                        if (controller.ControllerIndex > 0)
                        {
                            var lineController = new GC_Lines_CheckOutRelayController();
                            lineController.LineIndex = param.LineDevice.Index;
                            lineController.RelayControllerIndex = controller.ControllerIndex.Value;
                            lineController.OpenDoorChannelIndex = (short)(controller.OpenChannel ?? 0);
                            lineController.FailAlarmChannelIndex = (short)(controller.CloseChannel ?? 0);
                            lineController.CompanyIndex = user.CompanyIndex;
                            lineController.UpdatedUser = user.UserName;
                            lineController.UpdatedDate = DateTime.Now;
                            await _dbContext.GC_Lines_CheckOutRelayController.AddAsync(lineController);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteGates: " + ex);
                result = false;
            }
            finally
            {
                Misc.LoadMonitoringDeviceList(_dbContext, _Cache);
            }
            return result;
        }
    }
}
