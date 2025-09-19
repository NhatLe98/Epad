using AutoMapper;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace EPAD_Services.Impl
{
    public class GC_TruckDriverLogService : BaseServices<GC_TruckDriverLog, EPAD_Context>, IGC_TruckDriverLogService
    {
        private IServiceScopeFactory _scopeFactory;
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        private readonly IMapper _mapper;
        public GC_TruckDriverLogService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration, IServiceScopeFactory scopeFactory, IMapper mapper) : base(serviceProvider)
        {
            _scopeFactory = scopeFactory;
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_TruckDriverLogService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _mapper = mapper;
        }

        public async Task<IC_PlanDock> GetPlanDockByTripCode(string tripCode)
        {
            return await _dbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x => x.TripId == tripCode);
        }

        public async Task<IC_PlanDock> GetPlanDockByVehiclePlate(string vehiclePlate)
        {
            return await _dbContext.IC_PlanDock.AsNoTracking().OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.TrailerNumber == vehiclePlate && x.StatusDock != "0003");
        }

        public async Task<List<IC_StatusDock>> GetAllStatusDock()
        {
            return await _dbContext.IC_StatusDock.AsNoTracking().ToListAsync();
        }

        public async Task<List<GC_TruckDriverLog>> GetTruckDriverLogByTripCode(string tripCode)
        {
            return await _dbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode).ToListAsync();
        }

        public List<GC_TruckDriverLog> GetActiveTruckDriverLogByTripCode(string tripCode)
        {
            return  _dbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode && !x.IsInactive).ToList();
        }

        public async Task<List<GC_TruckDriverLog>> GetActiveTruckDriverLogByTripCodeAsync(string tripCode)
        {
            return await _dbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode && !x.IsInactive).ToListAsync();
        }

        public async Task<List<GC_TruckDriverLog>> GetActiveTruckDriverLogByCardNumber(string cardNumber)
        {
            return await _dbContext.GC_TruckDriverLog.AsNoTracking().Where(x => x.CardNumber == cardNumber && !x.IsInactive).ToListAsync();
        }

        public async Task<List<GC_TruckExtraDriverLog>> GetExtraTruckDriverLogByTripCode(string tripCode)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode).ToListAsync();
        }

        public async Task<List<GC_TruckExtraDriverLog>> GetExtraTruckDriverLogByListTripCode(List<string> tripCode)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => tripCode.Contains(x.TripCode)).ToListAsync();
        }

        public List<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByTripCode(string tripCode)
        {
            return _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode && !x.IsInactive).ToList();
        }

        public async Task<List<GC_TruckExtraDriverLog>> GetActiveExtraTruckDriverLogByTripCodeAsync(string tripCode)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => x.TripCode == tripCode && !x.IsInactive).ToListAsync();
        }

        public async Task<GC_TruckExtraDriverLog> GetExtraTruckDriverLogByExtraDriverCode(string cccd)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().FirstOrDefaultAsync(x => x.ExtraDriverCode == cccd);
        }

        public async Task<List<GC_TruckExtraDriverLog>> GetListExtraTruckDriverLogByExtraDriverCode(string cccd)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x => x.ExtraDriverCode == cccd).ToListAsync();
        }

        public async Task<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByExtraDriverCode(string cccd)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().FirstOrDefaultAsync(x => !x.IsInactive && x.ExtraDriverCode == cccd);
        }

        public async Task<GC_TruckExtraDriverLog> GetActiveExtraTruckDriverLogByCardNumber(string cardNumber)
        {
            return await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().FirstOrDefaultAsync(x => !x.IsInactive && x.CardNumber == cardNumber);
        }

        public async Task<HR_UserResult> GetDriverByCCCD(string cccd)
        {
            HR_UserResult result = null;
            var listPlanDock = await DbContext.IC_PlanDock.AsNoTracking().Where(x => x.DriverCode == cccd).ToListAsync();
            var listExtraDriverByNRIC = await GetListExtraTruckDriverLogByExtraDriverCode(cccd);
            if (listPlanDock.Count > 0)
            {
                var listTripCode = listPlanDock.Select(x => x.TripId).ToList();
                var anyDriverLogActive = await DbContext.GC_TruckDriverLog.AnyAsync(x
                    => listTripCode.Contains(x.TripCode) && !x.IsInactive);
                if (anyDriverLogActive)
                {
                    var listActiveDriverLog = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x
                        => listTripCode.Contains(x.TripCode) && !x.IsInactive).ToListAsync();
                    if (listActiveDriverLog.Count > 0)
                    {
                        var newestActiveDriverLog = listActiveDriverLog.OrderByDescending(x => x.UpdatedDate.HasValue)
                            .ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
                        var planDock = listPlanDock.FirstOrDefault(x => x.TripId == newestActiveDriverLog.TripCode);
                        result = new HR_UserResult();
                        result.EmployeeATID = planDock.TripId;
                        result.EmployeeCode = planDock.TripId;
                        result.FullName = planDock.DriverName;
                        result.Nric = planDock.DriverCode;
                        result.CardNumber = newestActiveDriverLog.CardNumber;
                        result.EmployeeType = (short)EmployeeType.Driver;
                        result.IsExtraDriver = false;
                        result.IsExpired = false;
                    }
                }
                else
                {
                    var listDriverLog = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x
                        => listTripCode.Contains(x.TripCode)).ToListAsync();
                    if (listDriverLog.Count > 0)
                    {
                        var newestDriverLog = listDriverLog.OrderByDescending(x => x.UpdatedDate.HasValue)
                            .ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
                        var planDock = listPlanDock.FirstOrDefault(x => x.TripId == newestDriverLog.TripCode);
                        result = new HR_UserResult();
                        result.EmployeeATID = planDock.TripId;
                        result.EmployeeCode = planDock.TripId;
                        result.FullName = planDock.DriverName;
                        result.Nric = planDock.DriverCode;
                        result.CardNumber = newestDriverLog.CardNumber;
                        result.EmployeeType = (short)EmployeeType.Driver;
                        result.IsExtraDriver = false;
                        result.IsExpired = true;
                    }
                    else
                    {
                        var planDock = listPlanDock.OrderByDescending(x => x.Index).FirstOrDefault();
                        result = new HR_UserResult();
                        result.EmployeeATID = planDock.TripId;
                        result.EmployeeCode = planDock.TripId;
                        result.FullName = planDock.DriverName;
                        result.Nric = planDock.DriverCode;
                        result.CardNumber = string.Empty;
                        result.EmployeeType = (short)EmployeeType.Driver;
                        result.IsExtraDriver = false;
                        result.IsExpired = null;
                    }
                }
            }

            if (result == null && listExtraDriverByNRIC.Count > 0)
            {
                var anyDriverLogActive = listExtraDriverByNRIC.Any(x
                    => !x.IsInactive);
                if (anyDriverLogActive)
                {
                    var listActiveExtraDriverLog = listExtraDriverByNRIC.Where(x
                    => !x.IsInactive).ToList();
                    if (listActiveExtraDriverLog.Count > 0)
                    {
                        var newestActiveDriverLog = listActiveExtraDriverLog.OrderByDescending(x => x.UpdatedDate.HasValue)
                            .ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
                        result = new HR_UserResult();
                        result.EmployeeATID = newestActiveDriverLog.TripCode;
                        result.EmployeeCode = newestActiveDriverLog.TripCode;
                        result.FullName = newestActiveDriverLog.ExtraDriverName;
                        result.Nric = newestActiveDriverLog.ExtraDriverCode;
                        result.CardNumber = newestActiveDriverLog.CardNumber;
                        result.EmployeeType = (short)EmployeeType.Driver;
                        result.IsExtraDriver = true;
                        result.IsExpired = false;
                    }
                }
                else
                {
                    var newestDriverLog = listExtraDriverByNRIC.OrderByDescending(x => x.UpdatedDate.HasValue)
                        .ThenByDescending(x => x.UpdatedDate).FirstOrDefault();
                    result = new HR_UserResult();
                    result.EmployeeATID = newestDriverLog.TripCode;
                    result.EmployeeCode = newestDriverLog.TripCode;
                    result.FullName = newestDriverLog.ExtraDriverName;
                    result.Nric = newestDriverLog.ExtraDriverCode;
                    result.CardNumber = newestDriverLog.CardNumber;
                    result.EmployeeType = (short)EmployeeType.Driver;
                    result.IsExtraDriver = true;
                    result.IsExpired = true;
                }
            }

            var listTripCodeDriverLog = new List<HR_UserResult>();
            if (result != null && !string.IsNullOrWhiteSpace(result.EmployeeATID))
            {
                var resultDriverLog = await _dbContext.GC_TruckDriverLog.AsNoTracking().FirstOrDefaultAsync(x
                    => x.TripCode == result.EmployeeATID);
                var resultDriverInfo = new IC_PlanDock();
                if (resultDriverLog != null)
                {
                    resultDriverInfo = await _dbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x
                        => x.TripId == resultDriverLog.TripCode);
                    listTripCodeDriverLog.Add(new HR_UserResult
                    {
                        EmployeeATID = resultDriverLog.TripCode,
                        EmployeeCode = resultDriverLog.TripCode,
                        FullName = resultDriverInfo != null ? resultDriverInfo.DriverName : string.Empty,
                        Nric = resultDriverInfo != null ? resultDriverInfo.DriverCode : string.Empty,
                        CardNumber = resultDriverLog.CardNumber,
                        EmployeeType = (short)EmployeeType.Driver,
                        IsExtraDriver = false,
                        IsExpired = resultDriverLog.IsInactive,
                    });
                }

                var resultExtraDriverLog = await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                    => x.TripCode == result.EmployeeATID).ToListAsync();
                if (resultExtraDriverLog.Count > 0)
                {
                    foreach (var item in resultExtraDriverLog)
                    {
                        listTripCodeDriverLog.Add(new HR_UserResult
                        {
                            EmployeeATID = item.TripCode,
                            EmployeeCode = item.TripCode,
                            FullName = item.ExtraDriverName,
                            Nric = item.ExtraDriverCode,
                            CardNumber = item.CardNumber,
                            EmployeeType = (short)EmployeeType.Driver,
                            IsExtraDriver = true,
                            IsExpired = item.IsInactive,
                        });
                    }
                }

                if (listTripCodeDriverLog.Count > 0)
                {
                    listTripCodeDriverLog = listTripCodeDriverLog.Where(x 
                        => !(x.Nric == result.Nric && x.IsExtraDriver == result.IsExtraDriver)).ToList();
                    result.ListLogDriver = listTripCodeDriverLog;
                }
            }
            return result;
        }

        public async Task<HR_UserResult> GetDriverByCardNumber(string cardNumber)
        {
            HR_UserResult result = null;
            var listDriverLog = await DbContext.GC_TruckDriverLog.AsNoTracking().Where(x
                => x.CardNumber == cardNumber && !x.IsInactive).ToListAsync();
            var listExtraDriverLog = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                => x.CardNumber == cardNumber && !x.IsInactive).ToListAsync();
            if (listDriverLog.Count > 0)
            {
                var newestDriverLog = listDriverLog.OrderByDescending(x => x.UpdatedDate.HasValue).ThenByDescending(x => x.UpdatedDate)
                    .FirstOrDefault();
                var driverInfo = await DbContext.IC_PlanDock.FirstOrDefaultAsync(x => x.TripId == newestDriverLog.TripCode);
                if (driverInfo != null)
                {
                    result = new HR_UserResult();
                    result.EmployeeATID = newestDriverLog.TripCode;
                    result.EmployeeCode = newestDriverLog.TripCode;
                    result.FullName = driverInfo.DriverName;
                    result.Nric = driverInfo.DriverCode;
                    result.CardNumber = newestDriverLog.CardNumber;
                    result.EmployeeType = (short)EmployeeType.Driver;
                    result.IsExtraDriver = false;
                    result.IsExpired = false;
                }
            }

            if (result == null && listExtraDriverLog.Count > 0)
            { 
                var newestExtraDriverLog = listExtraDriverLog.OrderByDescending(x => x.UpdatedDate.HasValue).ThenByDescending(x => x.UpdatedDate)
                    .FirstOrDefault();
                result = new HR_UserResult();
                result.EmployeeATID = newestExtraDriverLog.TripCode;
                result.EmployeeCode = newestExtraDriverLog.TripCode;
                result.FullName = newestExtraDriverLog.ExtraDriverName;
                result.Nric = newestExtraDriverLog.ExtraDriverCode;
                result.CardNumber = newestExtraDriverLog.CardNumber;
                result.EmployeeType = (short)EmployeeType.Driver;
                result.IsExtraDriver = true;
                result.IsExpired = false;
            }

            var listTripCodeDriverLog = new List<HR_UserResult>();
            if (result != null && !string.IsNullOrWhiteSpace(result.EmployeeATID)) 
            {
                var resultDriverLog = await _dbContext.GC_TruckDriverLog.AsNoTracking().FirstOrDefaultAsync(x
                    => x.TripCode == result.EmployeeATID);
                var resultDriverInfo = new IC_PlanDock();
                if (resultDriverLog != null)
                {
                    resultDriverInfo = await _dbContext.IC_PlanDock.AsNoTracking().FirstOrDefaultAsync(x
                        => x.TripId == resultDriverLog.TripCode);
                    listTripCodeDriverLog.Add(new HR_UserResult
                    {
                        EmployeeATID = resultDriverLog.TripCode,
                        EmployeeCode = resultDriverLog.TripCode,
                        FullName = resultDriverInfo != null ? resultDriverInfo.DriverName : string.Empty,
                        Nric = resultDriverInfo != null ? resultDriverInfo.DriverCode : string.Empty,
                        CardNumber = resultDriverLog.CardNumber,
                        EmployeeType = (short)EmployeeType.Driver,
                        IsExtraDriver = false,
                        IsExpired = resultDriverLog.IsInactive,
                    });
                }

                var resultExtraDriverLog = await _dbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x
                    => x.TripCode == result.EmployeeATID).ToListAsync();
                if (resultExtraDriverLog.Count > 0)
                {
                    foreach (var item in resultExtraDriverLog)
                    {
                        listTripCodeDriverLog.Add(new HR_UserResult
                        {
                            EmployeeATID = item.TripCode,
                            EmployeeCode = item.TripCode,
                            FullName = item.ExtraDriverName,
                            Nric = item.ExtraDriverCode,
                            CardNumber = item.CardNumber,
                            EmployeeType = (short)EmployeeType.Driver,
                            IsExtraDriver = true,
                            IsExpired = item.IsInactive,
                        });
                    }
                }

                if (listTripCodeDriverLog.Count > 0)
                {
                    listTripCodeDriverLog = listTripCodeDriverLog.Where(x
                        => !(x.Nric == result.Nric && x.IsExtraDriver == result.IsExtraDriver)).ToList();
                    result.ListLogDriver = listTripCodeDriverLog;
                }
            }

            return result;
        }

        public async Task<IC_Company> GetCompanyByUser(UserInfo user)
        {
            return await _dbContext.IC_Company.AsNoTracking().FirstOrDefaultAsync(x => x.Index == user.CompanyIndex);
        }

        public async Task<bool> AddTruckDriverLog(GC_TruckDriverLog param)
        {
            try
            {
                if (param.InOutMode == (short)InOutMode.Output)
                {
                    param.IsInactive = true;
                    var truckDriverLogInByTripCode = await _dbContext.GC_TruckDriverLog.FirstOrDefaultAsync(x => x.TripCode == param.TripCode 
                        && x.InOutMode == (short)InOutMode.Input);
                    if (truckDriverLogInByTripCode != null)
                    {
                        truckDriverLogInByTripCode.IsInactive = true;
                    }

                    var listExtraDriver = await DbContext.GC_TruckExtraDriverLog.Where(x =>
                        x.TripCode == param.TripCode).ToListAsync();
                    if (listExtraDriver.Count > 0)
                    {
                        listExtraDriver.ForEach(x => {
                            x.IsInactive = true;
                            x.UpdatedDate = DateTime.Now;
                            x.UpdatedUser = param.UpdatedUser;
                        });
                        _dbContext.GC_TruckExtraDriverLog.UpdateRange(listExtraDriver);
                    }
                }
                await _dbContext.GC_TruckDriverLog.AddAsync(param);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> AddTruckDriverLog(GC_TruckDriverLog param, List<ExtraTruckDriverLogModel> extraDriver)
        {
            try
            {
                if (param.InOutMode == (short)InOutMode.Output)
                {
                    param.IsInactive = true;
                    var truckDriverLogInByTripCode = await _dbContext.GC_TruckDriverLog.FirstOrDefaultAsync(x => x.TripCode == param.TripCode);
                    if (truckDriverLogInByTripCode != null)
                    {
                        truckDriverLogInByTripCode.IsInactive = true;
                    }

                    var listExtraDriver = await DbContext.GC_TruckExtraDriverLog.Where(x =>
                        x.TripCode == param.TripCode).ToListAsync();
                    if (listExtraDriver.Count > 0)
                    {
                        listExtraDriver.ForEach(x => {
                            x.IsInactive = true;
                            x.UpdatedDate = DateTime.Now;
                            x.UpdatedUser = param.UpdatedUser;
                        });
                        _dbContext.GC_TruckExtraDriverLog.UpdateRange(listExtraDriver);
                    }
                }
                if (param.InOutMode == (short)InOutMode.Input && extraDriver != null && extraDriver.Count > 0)
                {
                    extraDriver.ForEach(async (x) =>
                    {
                        await _dbContext.GC_TruckExtraDriverLog.AddAsync(x);
                    });
                }
                await _dbContext.GC_TruckDriverLog.AddAsync(param);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SaveExtraTruckDriverLog(List<ExtraTruckDriverLogModel> extraDriver)
        {
            try
            {
                var listExtraDriverIndex = extraDriver.Select(x => x.Index).ToList();
                var listExistedExtraDriver = await DbContext.GC_TruckExtraDriverLog.Where(x
                    => listExtraDriverIndex.Contains(x.Index)).ToListAsync();
                if (extraDriver != null && extraDriver.Count > 0)
                {
                    extraDriver.ForEach(async (x) =>
                    {
                        var existedExtraDriver = listExistedExtraDriver.FirstOrDefault(y => y.Index == x.Index);
                        if (existedExtraDriver != null)
                        {
                            existedExtraDriver.ExtraDriverName = x.ExtraDriverName;
                            existedExtraDriver.ExtraDriverCode = x.ExtraDriverCode;
                            existedExtraDriver.CardNumber = x.CardNumber;
                            existedExtraDriver.BirthDay = x.BirthDay;
                            existedExtraDriver.UpdatedUser = x.UpdatedUser;
                            existedExtraDriver.UpdatedDate = x.UpdatedDate;
                            _dbContext.GC_TruckExtraDriverLog.Update(existedExtraDriver);
                        }
                        else
                        {
                            await _dbContext.GC_TruckExtraDriverLog.AddAsync(x);
                        }
                    });
                }
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteExtraTruckDriverLog(int index)
        {
            try
            {
                var extraDriverlog = await _dbContext.GC_TruckExtraDriverLog.FirstOrDefaultAsync(x => x.Index == index);
                if (extraDriverlog != null)
                {
                    _dbContext.GC_TruckExtraDriverLog.Remove(extraDriverlog);
                    await _dbContext.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<TruckHistoryModel>> GetHistoryData(UserInfo user, DateTime fromTime, DateTime toTime, string filter)
        {
            var result = new List<TruckHistoryModel>();
            var histories = await DbContext.GC_TruckDriverLog.AsNoTracking()
              .Where(e => e.Time >= fromTime && e.Time <= toTime).ToListAsync();

            if (histories.Count > 0)
            {
                result = histories.GroupBy(x => x.TripCode).Select(x =>
                {
                    if (x != null && x.Count() > 0) 
                    {
                        //var rs = _mapper.Map<TruckHistoryModel>(x);
                        var rs = new TruckHistoryModel().PopulateWith(x.First());
                        return rs;
                    }
                    return null;
                }).ToList();
                result = result.Where(x => x != null).ToList();
            }

            if (result == null || (result != null && result.Count == 0))
            {
                return new List<TruckHistoryModel>();
            }

            var historiesTripCode = histories.Select(x => x.TripCode).ToList();
            histories = await DbContext.GC_TruckDriverLog.AsNoTracking()
              .Where(e => historiesTripCode.Contains(e.TripCode)).ToListAsync();
            var historiesMachineSerial = histories.Select(x => x.MachineSerial).ToList();
            var listPlanDock = new List<IC_PlanDock>();
            var listStatusDock = new List<IC_StatusDock>();
            var listDevice = new List<IC_Device>();
            var listExtraDriver = new List<GC_TruckExtraDriverLog>();
            if (historiesTripCode != null && historiesTripCode.Count > 0)
            {
                listPlanDock = await DbContext.IC_PlanDock.AsNoTracking().Where(x => historiesTripCode.Contains(x.TripId)).ToListAsync();
                if (listPlanDock.Count > 0)
                {
                    var listPlanDockStatusDock = listPlanDock.Select(x => x.StatusDock).ToList();
                    listStatusDock = await DbContext.IC_StatusDock.AsNoTracking().Where(x =>
                        listPlanDockStatusDock.Contains(x.Key)).ToListAsync();
                }
                listExtraDriver = await DbContext.GC_TruckExtraDriverLog.AsNoTracking().Where(x 
                    => historiesTripCode.Contains(x.TripCode)).ToListAsync();
            }
            if (historiesMachineSerial != null && historiesMachineSerial.Count > 0)
            {
                listDevice = await DbContext.IC_Device.AsNoTracking().Where(x => historiesMachineSerial.Contains(x.SerialNumber)).ToListAsync();
            }

            result.ForEach(x =>
            {
                var planDock = listPlanDock.FirstOrDefault(y => y.TripId == x.TripCode);
                if (planDock != null)
                {
                    x = _mapper.Map(planDock, x);
                }

                x.TimeIn = histories.FirstOrDefault(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Input)?.Time ?? null;
                x.TimeInString = x.TimeIn.HasValue ? x.TimeIn.Value.ToddMMyyyyHHmmssMinus() : string.Empty;

                x.TimeOut = histories.FirstOrDefault(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output)?.Time ?? null;
                x.TimeOutString = x.TimeOut.HasValue ? x.TimeOut.Value.ToddMMyyyyHHmmssMinus() : string.Empty;

                x.MachineSerialIn = histories.FirstOrDefault(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Input)?.MachineSerial ?? string.Empty;
                x.MachineNameIn = !string.IsNullOrWhiteSpace(x.MachineSerialIn)
                    ? (listDevice.FirstOrDefault(y => y.SerialNumber == x.MachineSerialIn)?.AliasName ?? string.Empty) : string.Empty;

                x.MachineSerialOut = histories.FirstOrDefault(y => y.TripCode == x.TripCode
                    && y.InOutMode == (short)InOutMode.Output)?.MachineSerial ?? string.Empty;
                x.MachineNameOut = !string.IsNullOrWhiteSpace(x.MachineSerialOut)
                    ? (listDevice.FirstOrDefault(y => y.SerialNumber == x.MachineSerialOut)?.AliasName ?? string.Empty) : string.Empty;

                x.TimesDockString = x.TimesDock.HasValue ? x.TimesDock.Value.ToddMMyyyyHHmmssMinus() : string.Empty;
                x.EtaString = x.Eta.HasValue ? x.Eta.Value.ToddMMyyyyHHmmssMinus() : string.Empty;
                x.StatusDockName = !string.IsNullOrWhiteSpace(x.StatusDock)
                    ? (listStatusDock.FirstOrDefault(y => y.Key == x.StatusDock)?.Name ?? string.Empty) : string.Empty;

                x.ExtraDriver = listExtraDriver.Any(y => y.TripCode == x.TripCode)
                    ? listExtraDriver.Where(y => y.TripCode == x.TripCode).ToList() : null;

                x.IsException = histories.Any(y => y.TripCode == x.TripCode && y.IsInactive == true);
                x.ReasonException = histories.FirstOrDefault(y => y.TripCode == x.TripCode && !string.IsNullOrWhiteSpace(y.ReasonException))?.ReasonException ?? string.Empty;
            });

            if (!string.IsNullOrWhiteSpace(filter))
            {
                result = result.Where(x => x.TripCode.ToLower().Contains(filter.Trim().ToLower()) || x.DriverCode.ToLower().Contains(filter.Trim().ToLower()) 
                || x.DriverName.ToLower().Contains(filter.Trim().ToLower()) || x.TrailerNumber.ToLower().Contains(filter.Trim().ToLower())).ToList();
            }

            return result;
        }

        public async Task<string> ReturnDriverCard(ReturnDriverCardModel data, UserInfo user)
        {
            var result = string.Empty;
            try
            {
                var activeDriverLog = await DbContext.GC_TruckDriverLog.FirstOrDefaultAsync(x
                    => x.TripCode == data.TripCode && x.CardNumber == data.CardNumber 
                    && x.InOutMode == (short)InOutMode.Input && !x.IsInactive);
                if (activeDriverLog != null)
                {
                    activeDriverLog.IsInactive = true;
                    activeDriverLog.UpdatedDate = DateTime.Now;
                    activeDriverLog.UpdatedUser = user.FullName;
                    var activeDriverLogOut = await DbContext.GC_TruckDriverLog.FirstOrDefaultAsync(x
                        => x.TripCode == data.TripCode && x.CardNumber == data.CardNumber
                        && x.InOutMode == (short)InOutMode.Output);
                    if (activeDriverLogOut != null && !activeDriverLogOut.IsInactive)
                    {
                        activeDriverLogOut.IsInactive = true;
                        activeDriverLogOut.Time = DateTime.Now;
                        activeDriverLogOut.Description = data.Description;
                        activeDriverLogOut.UpdatedDate = DateTime.Now;
                        activeDriverLogOut.UpdatedUser = user.FullName;
                    }
                    else
                    {
                        var newDriverLogOut = new GC_TruckDriverLog().PopulateWith(activeDriverLog);
                        newDriverLogOut.Index = 0;
                        newDriverLogOut.Time = DateTime.Now;
                        newDriverLogOut.InOutMode = (short)InOutMode.Output;
                        newDriverLogOut.Description = data.Description;
                        if (!string.IsNullOrWhiteSpace(data.SerialNumber))
                        { 
                            newDriverLogOut.MachineSerial = data.SerialNumber;
                        }
                        await DbContext.GC_TruckDriverLog.AddAsync(newDriverLogOut);
                    }

                    await DbContext.SaveChangesAsync();
                }
                else
                { 
                    var activeExtraTruckDriverLog = await DbContext.GC_TruckExtraDriverLog.FirstOrDefaultAsync(x
                    => x.TripCode == data.TripCode && x.CardNumber == data.CardNumber && !x.IsInactive);
                    if (activeExtraTruckDriverLog != null)
                    {
                        activeExtraTruckDriverLog.IsInactive = true;
                        activeExtraTruckDriverLog.Description = data.Description;
                        activeExtraTruckDriverLog.UpdatedDate = DateTime.Now;
                        activeExtraTruckDriverLog.UpdatedUser = user.FullName;

                        await DbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }

        public DataGridClass GetPaginationList(IEnumerable<TruckHistoryModel> histories, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = histories.Count();
            var dummy = histories.Skip(skip).Take(pageSize).ToList();
            DataGridClass grid = new DataGridClass(countTotal, dummy);
            return grid;
        }
    }
}
