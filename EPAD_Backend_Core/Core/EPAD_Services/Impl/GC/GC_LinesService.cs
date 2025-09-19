using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
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
    public class GC_LinesService : BaseServices<GC_Lines, EPAD_Context>, IGC_LinesService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public GC_LinesService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<bool> AddLine(LinesParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var line = new GC_Lines
                {
                    Name = param.Name,
                    Description = param.Description,
                    CompanyIndex = user.CompanyIndex,
                    LineForCustomer = param.LineForCustomer,
                    LineForCustomerIssuanceReturnCard = param.LineForCustomerIssuanceReturnCard,
                    LineForDriver = param.LineForDriver,
                    LineForDriverIssuanceReturnCard = param.LineForDriverIssuanceReturnCard,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName
                };

                await _dbContext.GC_Lines.AddAsync(line);
                await _dbContext.SaveChangesAsync();

                //var oldLineGate = await _dbContext.GC_Gates_Lines.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineGate != null && oldLineGate.Count > 0)
                //{
                //    _dbContext.GC_Gates_Lines.RemoveRange(oldLineGate);
                //}
                //if (param.GateIndex != null && param.GateIndex.Count > 0)
                //{
                //    param.GateIndex.ForEach(async x =>
                //    {
                //        var lineGate = new GC_Gates_Lines();
                //        lineGate.LineIndex = line.Index;
                //        lineGate.GateIndex = x;
                //        lineGate.CompanyIndex = user.CompanyIndex;
                //        lineGate.UpdatedUser = user.UserName;
                //        lineGate.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Gates_Lines.AddAsync(lineGate);
                //    });
                //}

                //var oldLineCameraIn = await _dbContext.GC_Lines_CheckInCamera.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineCameraIn != null && oldLineCameraIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInCamera.RemoveRange(oldLineCameraIn);
                //}
                //if (param.CameraInIndex != null && param.CameraInIndex.Count > 0)
                //{
                //    param.CameraInIndex.ForEach(async x =>
                //    {
                //        var lineCameraIn = new GC_Lines_CheckInCamera();
                //        lineCameraIn.LineIndex = line.Index;
                //        lineCameraIn.CameraIndex = x;
                //        lineCameraIn.CompanyIndex = user.CompanyIndex;
                //        lineCameraIn.UpdatedUser = user.UserName;
                //        lineCameraIn.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckInCamera.AddAsync(lineCameraIn);
                //    });
                //}

                //var oldLineCameraOut = await _dbContext.GC_Lines_CheckOutCamera.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineCameraOut != null && oldLineCameraOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutCamera.RemoveRange(oldLineCameraOut);
                //}
                //if (param.CameraOutIndex != null && param.CameraOutIndex.Count > 0)
                //{
                //    param.CameraOutIndex.ForEach(async x =>
                //    {
                //        var lineCameraOut = new GC_Lines_CheckOutCamera();
                //        lineCameraOut.LineIndex = line.Index;
                //        lineCameraOut.CameraIndex = x;
                //        lineCameraOut.CompanyIndex = user.CompanyIndex;
                //        lineCameraOut.UpdatedUser = user.UserName;
                //        lineCameraOut.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckOutCamera.AddAsync(lineCameraOut);
                //    });
                //}

                //var oldLineDeviceIn = await _dbContext.GC_Lines_CheckInDevice.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineDeviceIn != null && oldLineDeviceIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInDevice.RemoveRange(oldLineDeviceIn);
                //}
                //if (param.DeviceInSerial != null && param.DeviceInSerial.Count > 0)
                //{
                //    param.DeviceInSerial.ForEach(async x =>
                //    {
                //        var lineDeviceIn = new GC_Lines_CheckInDevice();
                //        lineDeviceIn.LineIndex = line.Index;
                //        lineDeviceIn.CheckInDeviceSerial = x;
                //        lineDeviceIn.CompanyIndex = user.CompanyIndex;
                //        lineDeviceIn.UpdatedUser = user.UserName;
                //        lineDeviceIn.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckInDevice.AddAsync(lineDeviceIn);
                //    });
                //}

                //var oldLineDeviceOut = await _dbContext.GC_Lines_CheckOutDevice.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineDeviceOut != null && oldLineDeviceOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutDevice.RemoveRange(oldLineDeviceOut);
                //}
                //if (param.DeviceOutSerial != null && param.DeviceOutSerial.Count > 0)
                //{
                //    param.DeviceOutSerial.ForEach(async x =>
                //    {
                //        var lineDeviceOut = new GC_Lines_CheckOutDevice();
                //        lineDeviceOut.LineIndex = line.Index;
                //        lineDeviceOut.CheckOutDeviceSerial = x;
                //        lineDeviceOut.CompanyIndex = user.CompanyIndex;
                //        lineDeviceOut.UpdatedUser = user.UserName;
                //        lineDeviceOut.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckOutDevice.AddAsync(lineDeviceOut);
                //    });
                //}

                //var oldLineRelayControllerIn = await _dbContext.GC_Lines_CheckInRelayController.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineRelayControllerIn != null && oldLineRelayControllerIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInRelayController.RemoveRange(oldLineRelayControllerIn);
                //}
                //var oldLineRelayControllerOut = await _dbContext.GC_Lines_CheckOutRelayController.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineRelayControllerOut != null && oldLineRelayControllerOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutRelayController.RemoveRange(oldLineRelayControllerOut);
                //}
                //if (param.LineControllersIn != null && param.LineControllersIn.Count > 0)
                //{
                //    param.LineControllersIn.ForEach(async x =>
                //    {
                //        var lineRelayControllerIn = new GC_Lines_CheckInRelayController();
                //        lineRelayControllerIn.LineIndex = line.Index;
                //        lineRelayControllerIn.RelayControllerIndex = x.ControllerIndex.Value;
                //        lineRelayControllerIn.OpenDoorChannelIndex = (short)(x?.OpenChannel ?? 0);
                //        lineRelayControllerIn.FailAlarmChannelIndex = (short)(x?.CloseChannel ?? 0);
                //        lineRelayControllerIn.CompanyIndex = user.CompanyIndex;
                //        lineRelayControllerIn.UpdatedUser = user.UserName;
                //        lineRelayControllerIn.UpdatedDate = DateTime.Now;
                //        await _dbContext.GC_Lines_CheckInRelayController.AddAsync(lineRelayControllerIn);
                //    });
                //}
                //if (param.LineControllersOut != null && param.LineControllersOut.Count > 0)
                //{
                //    param.LineControllersOut.ForEach(async x =>
                //    {
                //        var lineRelayControllerOut = new GC_Lines_CheckOutRelayController();
                //        lineRelayControllerOut.LineIndex = line.Index;
                //        lineRelayControllerOut.RelayControllerIndex = x.ControllerIndex.Value;
                //        lineRelayControllerOut.OpenDoorChannelIndex = (short)(x?.OpenChannel ?? 0);
                //        lineRelayControllerOut.FailAlarmChannelIndex = (short)(x?.CloseChannel ?? 0);
                //        lineRelayControllerOut.CompanyIndex = user.CompanyIndex;
                //        lineRelayControllerOut.UpdatedUser = user.UserName;
                //        lineRelayControllerOut.UpdatedDate = DateTime.Now;
                //        await _dbContext.GC_Lines_CheckOutRelayController.AddAsync(lineRelayControllerOut);
                //    });
                //}

                //await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddLine" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateLine(LinesParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var line = await _dbContext.GC_Lines.FirstOrDefaultAsync(x => x.Index == param.Index);
                line.Name = param.Name;
                line.Description = param.Description;
                line.LineForCustomer = param.LineForCustomer;
                line.LineForCustomerIssuanceReturnCard = param.LineForCustomerIssuanceReturnCard;
                line.LineForDriver = param.LineForDriver;
                line.LineForDriverIssuanceReturnCard = param.LineForDriverIssuanceReturnCard;
                line.UpdatedDate = DateTime.Now;
                line.UpdatedUser = user.UserName;

                //var oldLineGate = await _dbContext.GC_Gates_Lines.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineGate != null && oldLineGate.Count > 0)
                //{
                //    _dbContext.GC_Gates_Lines.RemoveRange(oldLineGate);
                //}
                //if (param.GateIndex != null && param.GateIndex.Count > 0)
                //{
                //    param.GateIndex.ForEach(async x =>
                //    {
                //        var lineGate = new GC_Gates_Lines();
                //        lineGate.LineIndex = line.Index;
                //        lineGate.GateIndex = x;
                //        lineGate.CompanyIndex = user.CompanyIndex;
                //        lineGate.UpdatedUser = user.UserName;
                //        lineGate.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Gates_Lines.AddAsync(lineGate);
                //    });
                //}

                //var oldLineCameraIn = await _dbContext.GC_Lines_CheckInCamera.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineCameraIn != null && oldLineCameraIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInCamera.RemoveRange(oldLineCameraIn);
                //}
                //if (param.CameraInIndex != null && param.CameraInIndex.Count > 0)
                //{
                //    param.CameraInIndex.ForEach(async x =>
                //    {
                //        var lineCameraIn = new GC_Lines_CheckInCamera();
                //        lineCameraIn.LineIndex = line.Index;
                //        lineCameraIn.CameraIndex = x;
                //        lineCameraIn.CompanyIndex = user.CompanyIndex;
                //        lineCameraIn.UpdatedUser = user.UserName;
                //        lineCameraIn.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckInCamera.AddAsync(lineCameraIn);
                //    });
                //}

                //var oldLineCameraOut = await _dbContext.GC_Lines_CheckOutCamera.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineCameraOut != null && oldLineCameraOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutCamera.RemoveRange(oldLineCameraOut);
                //}
                //if (param.CameraOutIndex != null && param.CameraOutIndex.Count > 0)
                //{
                //    param.CameraOutIndex.ForEach(async x =>
                //    {
                //        var lineCameraOut = new GC_Lines_CheckOutCamera();
                //        lineCameraOut.LineIndex = line.Index;
                //        lineCameraOut.CameraIndex = x;
                //        lineCameraOut.CompanyIndex = user.CompanyIndex;
                //        lineCameraOut.UpdatedUser = user.UserName;
                //        lineCameraOut.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckOutCamera.AddAsync(lineCameraOut);
                //    });
                //}

                //var oldLineDeviceIn = await _dbContext.GC_Lines_CheckInDevice.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineDeviceIn != null && oldLineDeviceIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInDevice.RemoveRange(oldLineDeviceIn);
                //}
                //if (param.DeviceInSerial != null && param.DeviceInSerial.Count > 0)
                //{
                //    param.DeviceInSerial.ForEach(async x =>
                //    {
                //        var lineDeviceIn = new GC_Lines_CheckInDevice();
                //        lineDeviceIn.LineIndex = line.Index;
                //        lineDeviceIn.CheckInDeviceSerial = x;
                //        lineDeviceIn.CompanyIndex = user.CompanyIndex;
                //        lineDeviceIn.UpdatedUser = user.UserName;
                //        lineDeviceIn.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckInDevice.AddAsync(lineDeviceIn);
                //    });
                //}

                //var oldLineDeviceOut = await _dbContext.GC_Lines_CheckOutDevice.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineDeviceOut != null && oldLineDeviceOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutDevice.RemoveRange(oldLineDeviceOut);
                //}
                //if (param.DeviceOutSerial != null && param.DeviceOutSerial.Count > 0)
                //{
                //    param.DeviceOutSerial.ForEach(async x =>
                //    {
                //        var lineDeviceOut = new GC_Lines_CheckOutDevice();
                //        lineDeviceOut.LineIndex = line.Index;
                //        lineDeviceOut.CheckOutDeviceSerial = x;
                //        lineDeviceOut.CompanyIndex = user.CompanyIndex;
                //        lineDeviceOut.UpdatedUser = user.UserName;
                //        lineDeviceOut.UpdatedDate = DateTime.Now;

                //        await _dbContext.GC_Lines_CheckOutDevice.AddAsync(lineDeviceOut);
                //    });
                //}

                //var oldLineRelayControllerIn = await _dbContext.GC_Lines_CheckInRelayController.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineRelayControllerIn != null && oldLineRelayControllerIn.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckInRelayController.RemoveRange(oldLineRelayControllerIn);
                //}
                //var oldLineRelayControllerOut = await _dbContext.GC_Lines_CheckOutRelayController.Where(x => x.LineIndex == line.Index).ToListAsync();
                //if (oldLineRelayControllerOut != null && oldLineRelayControllerOut.Count > 0)
                //{
                //    _dbContext.GC_Lines_CheckOutRelayController.RemoveRange(oldLineRelayControllerOut);
                //}
                //if (param.LineControllersIn != null && param.LineControllersIn.Count > 0)
                //{
                //    param.LineControllersIn.ForEach(async x =>
                //    {
                //        var lineRelayControllerIn = new GC_Lines_CheckInRelayController();
                //        lineRelayControllerIn.LineIndex = line.Index;
                //        lineRelayControllerIn.RelayControllerIndex = x.ControllerIndex.Value;
                //        lineRelayControllerIn.OpenDoorChannelIndex = (short)(x?.OpenChannel ?? 0);
                //        lineRelayControllerIn.FailAlarmChannelIndex = (short)(x?.CloseChannel ?? 0);
                //        lineRelayControllerIn.CompanyIndex = user.CompanyIndex;
                //        lineRelayControllerIn.UpdatedUser = user.UserName;
                //        lineRelayControllerIn.UpdatedDate = DateTime.Now;
                //        await _dbContext.GC_Lines_CheckInRelayController.AddAsync(lineRelayControllerIn);
                //    });
                //}
                //if (param.LineControllersOut != null && param.LineControllersOut.Count > 0)
                //{
                //    param.LineControllersOut.ForEach(async x =>
                //    {
                //        var lineRelayControllerOut = new GC_Lines_CheckOutRelayController();
                //        lineRelayControllerOut.LineIndex = line.Index;
                //        lineRelayControllerOut.RelayControllerIndex = x.ControllerIndex.Value;
                //        lineRelayControllerOut.OpenDoorChannelIndex = (short)(x?.OpenChannel ?? 0);
                //        lineRelayControllerOut.FailAlarmChannelIndex = (short)(x?.CloseChannel ?? 0);
                //        lineRelayControllerOut.CompanyIndex = user.CompanyIndex;
                //        lineRelayControllerOut.UpdatedUser = user.UserName;
                //        lineRelayControllerOut.UpdatedDate = DateTime.Now;
                //        await _dbContext.GC_Lines_CheckOutRelayController.AddAsync(lineRelayControllerOut);
                //    });
                //}

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateLine" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteLines(List<int> indexes, UserInfo user)
        {
            var result = true;
            try
            {
                var deleteItems = await _dbContext.GC_Lines.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (deleteItems != null)
                {
                    _dbContext.GC_Lines.RemoveRange(deleteItems);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteLines" + ex);
                result = false;
            }
            return result;
        }

        public async Task<GC_Lines> GetLineBySerialNumber(string serialNumber)
        {
            GC_Lines result = null;
            var isDeviceUsing = await _dbContext.GC_Lines_CheckInDevice.AsNoTracking().AnyAsync(x => x.CheckInDeviceSerial == serialNumber);
            if (isDeviceUsing)
            {
                var lineUsing = await _dbContext.GC_Lines_CheckInDevice.AsNoTracking().FirstOrDefaultAsync(x 
                    => x.CheckInDeviceSerial == serialNumber);
                var lineIndex = lineUsing.LineIndex;
                result = await _dbContext.GC_Lines.FirstOrDefaultAsync(x => x.Index == lineIndex);
            }
            
            if(!isDeviceUsing || result == null)
            { 
                isDeviceUsing = await _dbContext.GC_Lines_CheckOutDevice.AsNoTracking().AnyAsync(x => x.CheckOutDeviceSerial == serialNumber);
                if (isDeviceUsing)
                {
                    var lineUsing = await _dbContext.GC_Lines_CheckOutDevice.AsNoTracking().FirstOrDefaultAsync(x
                    => x.CheckOutDeviceSerial == serialNumber);
                    var lineIndex = lineUsing.LineIndex;
                    result = await _dbContext.GC_Lines.FirstOrDefaultAsync(x => x.Index == lineIndex);
                }
            }

            var dataResult = new LineModel().PopulateWith(result);
            var gateOfLine = await _dbContext.GC_Gates_Lines.AsNoTracking().Where(x
                => result.Index == x.LineIndex).ToListAsync();
            if (gateOfLine.Count > 0)
            {
                var lineGateIndexes = gateOfLine.Select(x => x.GateIndex).ToList();
                dataResult.GateIndexes = lineGateIndexes;
            }
            return dataResult;
        }

        public async Task<DataGridClass> GetAllData(int companyIndex)
        {
            var dataQuery = _dbContext.GC_Lines.AsNoTracking().Where(x => x.CompanyIndex == companyIndex);
            int countTotal = dataQuery.Count();
            var data = await dataQuery.ToListAsync();
            var dataResult = new List<LinesParam>();
            if (data != null && data.Count > 0)
            {
                var listLineIndex = data.Select(x => x.Index).ToList();
                var lineGates = await _dbContext.GC_Gates_Lines.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineCameraIns = await _dbContext.GC_Lines_CheckInCamera.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineCameraOuts = await _dbContext.GC_Lines_CheckOutCamera.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineDeviceIns = await _dbContext.GC_Lines_CheckInDevice.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineDeviceOuts = await _dbContext.GC_Lines_CheckOutDevice.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineRelayControllerIns = await _dbContext.GC_Lines_CheckInRelayController.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();
                var lineRelayControllerOuts = await _dbContext.GC_Lines_CheckOutRelayController.Where(x
                    => listLineIndex.Contains(x.LineIndex)).ToListAsync();

                var listCameraIndex = new List<int>();
                var listCamera = new List<IC_Camera>();
                if (lineCameraIns != null && lineCameraIns.Count > 0)
                {
                    listCameraIndex.AddRange(lineCameraIns.Select(x => x.CameraIndex));
                }
                if (lineCameraOuts != null && lineCameraOuts.Count > 0)
                {
                    listCameraIndex.AddRange(lineCameraOuts.Select(x => x.CameraIndex));
                }
                if (listCameraIndex != null && listCameraIndex.Count > 0)
                {
                    listCamera = await _dbContext.IC_Camera.AsNoTracking().Where(x => listCameraIndex.Contains(x.Index)).ToListAsync();
                }

                var listDeviceSerial = new List<string>();
                var listDevice = new List<IC_Device>();
                if (lineDeviceIns != null && lineDeviceIns.Count > 0)
                {
                    listDeviceSerial.AddRange(lineDeviceIns.Select(x => x.CheckInDeviceSerial));
                }
                if (lineDeviceOuts != null && lineDeviceOuts.Count > 0)
                {
                    listDeviceSerial.AddRange(lineDeviceOuts.Select(x => x.CheckOutDeviceSerial));
                }
                if (listDeviceSerial != null && listDeviceSerial.Count > 0)
                {
                    listDevice = await _dbContext.IC_Device.AsNoTracking().Where(x => listDeviceSerial.Contains(x.SerialNumber)).ToListAsync();
                }

                var listControllerIndex = new List<int>();
                var listRelayController = new List<IC_RelayController>();
                if (lineRelayControllerIns != null && lineRelayControllerIns.Count > 0)
                {
                    listControllerIndex.AddRange(lineRelayControllerIns.Select(x => x.RelayControllerIndex));
                }
                if (lineRelayControllerOuts != null && lineRelayControllerOuts.Count > 0)
                {
                    listControllerIndex.AddRange(lineRelayControllerOuts.Select(x => x.RelayControllerIndex));
                }
                if (listControllerIndex != null && listControllerIndex.Count > 0)
                {
                    listRelayController = await _dbContext.IC_RelayController.AsNoTracking().Where(x 
                        => listControllerIndex.Contains(x.Index)).ToListAsync();
                }

                var listdeviceProcess = _dbContext.IC_SystemCommand.Where(x => x.CompanyIndex == companyIndex && x.Excuted == false 
                    && x.ExcutingServiceIndex > 0).AsEnumerable().GroupBy(x => x.SerialNumber);
                var deviceProcessing = listdeviceProcess.ToDictionary(x => x.Key, x => x.ToList());

                data.ForEach(x =>
                {
                    var lineResultData = new LinesParam();
                    lineResultData.Index = x.Index;
                    lineResultData.Name = x.Name;
                    lineResultData.Description = x.Description;
                    lineResultData.LineForCustomer = x.LineForCustomer;
                    lineResultData.LineForCustomerIssuanceReturnCard = x.LineForCustomerIssuanceReturnCard;
                    lineResultData.LineForDriver = x.LineForDriver;
                    lineResultData.LineForDriverIssuanceReturnCard = x.LineForDriverIssuanceReturnCard;
                    lineResultData.LineControllersIn = new List<LineController>();
                    lineResultData.LineControllersOut = new List<LineController>();

                    var listGate = lineGates.Where(y => y.LineIndex == x.Index).ToList();
                    var lineCameraIn = lineCameraIns.Where(y => y.LineIndex == x.Index).ToList();
                    var lineCameraOut = lineCameraOuts.Where(y => y.LineIndex == x.Index).ToList();
                    var lineDeviceIn = lineDeviceIns.Where(y => y.LineIndex == x.Index).ToList();
                    var lineDeviceOut = lineDeviceOuts.Where(y => y.LineIndex == x.Index).ToList();
                    var lineRelayControllerIn = lineRelayControllerIns.Where(y => y.LineIndex == x.Index).ToList();
                    var lineRelayControllerOut = lineRelayControllerOuts.Where(y => y.LineIndex == x.Index).ToList();

                    var lineCameraInIndex = lineCameraIn.Select(x => x.CameraIndex).ToList();
                    var lineCameraOutIndex = lineCameraOut.Select(x => x.CameraIndex).ToList();
                    var cameraIn = listCamera.Where(x => lineCameraInIndex.Contains(x.Index)).ToList();
                    var cameraOut = listCamera.Where(x => lineCameraOutIndex.Contains(x.Index)).ToList();

                    var lineDeviceInSerial = lineDeviceIn.Select(x => x.CheckInDeviceSerial).ToList();
                    var lineDeviceOutSerial = lineDeviceOut.Select(x => x.CheckOutDeviceSerial).ToList();
                    var deviceIn = listDevice.Where(x => lineDeviceInSerial.Contains(x.SerialNumber)).ToList();
                    var deviceOut = listDevice.Where(x => lineDeviceOutSerial.Contains(x.SerialNumber)).ToList();

                    var lineControllersInIndex = lineRelayControllerIn.Select(x => x.RelayControllerIndex).ToList();
                    var lineControllersOutIndex = lineRelayControllerOut.Select(x => x.RelayControllerIndex).ToList();
                    var relayControllerIn = listRelayController.Where(x => lineControllersInIndex.Contains(x.Index)).ToList();
                    var relayControllerOut = listRelayController.Where(x => lineControllersOutIndex.Contains(x.Index)).ToList();

                    lineResultData.CameraInIndex = lineCameraInIndex;
                    lineResultData.CameraOutIndex = lineCameraOutIndex;
                    lineResultData.CameraIn = cameraIn;
                    lineResultData.CameraOut = cameraOut;
                    lineResultData.DeviceInSerial = lineDeviceIn.Select(x => x.CheckInDeviceSerial).ToList();
                    lineResultData.DeviceOutSerial = lineDeviceOut.Select(x => x.CheckOutDeviceSerial).ToList();
                    lineResultData.DeviceOut = new List<IC_DeviceModel>().PopulateWith(deviceOut);
                    if (deviceIn != null && deviceIn.Count > 0)
                    {
                        lineResultData.DeviceIn = deviceIn.Select(x =>
                        {
                            return new IC_DeviceModel().PopulateWith(x);
                        }).ToList();
                        lineResultData.DeviceIn.ForEach(x =>
                        {
                            x.Name = x.AliasName;
                            x.Status = GetDeviceStatus(x, _Cache, deviceProcessing);
                        });
                    }
                    if (deviceOut != null && deviceOut.Count > 0)
                    {
                        lineResultData.DeviceOut = deviceOut.Select(x =>
                        {
                            return new IC_DeviceModel().PopulateWith(x);
                        }).ToList();
                        lineResultData.DeviceOut.ForEach(x =>
                        {
                            x.Name = x.AliasName;
                            x.Status = GetDeviceStatus(x, _Cache, deviceProcessing);
                        });
                    }
                    lineRelayControllerIn.ForEach(e =>
                    {
                        var lineInController = new LineController
                        {
                            ControllerIndex = e.RelayControllerIndex,
                            OpenChannel = e.OpenDoorChannelIndex,
                            CloseChannel = e.FailAlarmChannelIndex,
                        };
                        if (lineInController.OpenChannel == 0)
                        {
                            lineInController.OpenChannel = null;
                        }
                        if (lineInController.CloseChannel == 0)
                        {
                            lineInController.CloseChannel = null;
                        }
                        if (relayControllerIn.Any(y => y.Index == e.RelayControllerIndex))
                        {
                            lineInController = lineInController.PopulateWith(
                                relayControllerIn.FirstOrDefault(y => y.Index == e.RelayControllerIndex));
                        }
                        lineResultData.LineControllersIn.Add(lineInController);

                    });
                    lineRelayControllerOut.ForEach(e =>
                    {
                        var lineOutController = new LineController
                        {
                            ControllerIndex = e.RelayControllerIndex,
                            OpenChannel = e.OpenDoorChannelIndex,
                            CloseChannel = e.FailAlarmChannelIndex,
                        };
                        if (lineOutController.OpenChannel == 0)
                        {
                            lineOutController.OpenChannel = null;
                        }
                        if (lineOutController.CloseChannel == 0)
                        {
                            lineOutController.CloseChannel = null;
                        }
                        if (relayControllerOut.Any(y => y.Index == e.RelayControllerIndex))
                        {
                            lineOutController = lineOutController.PopulateWith(
                                relayControllerOut.FirstOrDefault(y => y.Index == e.RelayControllerIndex));
                        }
                        lineResultData.LineControllersOut.Add(lineOutController);
                    });

                    dataResult.Add(lineResultData);
                });
            }

            var grid = new DataGridClass(countTotal, dataResult);
            return grid;
        }

        private string GetDeviceStatus(IC_Device pDevice, IMemoryCache pCache, Dictionary<string, List<IC_SystemCommand>> pProcessingDevice)
        {
            if (CaculateTime(pDevice.LastConnection, DateTime.Now) < ConfigObject.GetConfig(pCache).LimitedTimeConnection)
            {
                if (pProcessingDevice.ContainsKey(pDevice.SerialNumber))
                {
                    return "Đang xử lý";
                }
                else
                {
                    return "Online";
                }
            }
            else
            {
                return "Offline";
            }
        }

        private double CaculateTime(DateTime? time1, DateTime time2)
        {
            DateTime temp = new DateTime();
            if (time1.HasValue)
            {
                temp = time1.Value;
            }
            else
            {
                temp = new DateTime(2000, 1, 1, 0, 0, 0);
            }
            TimeSpan time = new TimeSpan();
            time = time2 - temp;
            return time.TotalMinutes;
        }

        public async Task<GC_Lines> GetDataByIndex(int index)
        {
            return await _dbContext.GC_Lines.FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<GC_Lines>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _dbContext.GC_Lines.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<GC_Lines> GetDataByNameAndCompanyIndex(string name, int companyIndex)
        {
            return await _dbContext.GC_Lines.FirstOrDefaultAsync(x => x.Name == name && x.CompanyIndex == companyIndex);
        }

        public async Task<bool> CheckLineUsing(List<int> pLineIndexs, int pCompanyIndex)
        {
            var lineIndexLookup = pLineIndexs.ToHashSet();
            var isLineCameraInUsing = await _dbContext.GC_Lines_CheckInCamera.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineCameraInUsing)
            {
                return true;
            }

            var isLineCameraOutUsing = await _dbContext.GC_Lines_CheckOutCamera.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineCameraOutUsing)
            {
                return true;
            }

            var isLineDeviceInUsing = await _dbContext.GC_Lines_CheckInDevice.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineDeviceInUsing)
            {
                return true;
            }

            var isLineDeviceOutUsing = await _dbContext.GC_Lines_CheckOutDevice.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineDeviceOutUsing)
            {
                return true;
            }

            var isLineControllerInUsing = await _dbContext.GC_Lines_CheckInRelayController.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineControllerInUsing)
            {
                return true;
            }

            var isLineControllerOutUsing = await _dbContext.GC_Lines_CheckOutRelayController.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineControllerOutUsing)
            {
                return true;
            }

            var isLineGateUsing = await _dbContext.GC_Gates_Lines.AnyAsync(x
                => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
            if (isLineGateUsing)
            {
                return true;
            }

            return false;
        }

        public async Task<string> TryDeleteLine(List<int> pLineIndexs, int pCompanyIndex)
        {
            var lineIndexLookup = pLineIndexs.ToHashSet();

            _dbContext.Database.BeginTransaction();

            try
            {
                //var allLineCamIn = _dbContext.GC_Lines_CheckInCamera.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckInCamera.RemoveRange(allLineCamIn);

                //var allLineCamOut = _dbContext.GC_Lines_CheckOutCamera.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckOutCamera.RemoveRange(allLineCamOut);

                //var allLineRelayIn = _dbContext.GC_Lines_CheckInRelayController.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckInRelayController.RemoveRange(allLineRelayIn);

                //var allLineRelayOut = _dbContext.GC_Lines_CheckOutRelayController.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckOutRelayController.RemoveRange(allLineRelayOut);

                //var allLineDeviceIn = _dbContext.GC_Lines_CheckInDevice.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckInDevice.RemoveRange(allLineDeviceIn);

                //var allLineDeviceOut = _dbContext.GC_Lines_CheckOutDevice.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Lines_CheckOutDevice.RemoveRange(allLineDeviceOut);

                //var allgate_line = _dbContext.GC_Gates_Lines.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_Gates_Lines.RemoveRange(allgate_line);

                //var allparkinglotdetail = _dbContext.GC_ParkingLotDetail.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_ParkingLotDetail.RemoveRange(allparkinglotdetail);

                var allLine = _dbContext.GC_Lines.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.Index));
                _dbContext.GC_Lines.RemoveRange(allLine);

                _dbContext.SaveChanges();
                _dbContext.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.Database.RollbackTransaction();
                return await Task.FromResult(ex.Message);
            }

            return await Task.FromResult("");
        }
    }
}
