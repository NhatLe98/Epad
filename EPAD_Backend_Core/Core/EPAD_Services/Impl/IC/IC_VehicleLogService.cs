using DocumentFormat.OpenXml.Spreadsheet;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_VehicleLogService : BaseServices<IC_VehicleLog, EPAD_Context>, IIC_VehicleLogService
    {
        private readonly EPAD_Context _epadContext;
        private ILogger _logger;
        private readonly ConfigObject _config;
        private readonly IIC_ConfigLogic _iC_ConfigLogic;
        public IC_VehicleLogService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _epadContext = serviceProvider.GetService<EPAD_Context>();
            _logger = loggerFactory.CreateLogger<IC_VehicleLogService>();
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _config = ConfigObject.GetConfig(_Cache);
            _iC_ConfigLogic = serviceProvider.GetService<IIC_ConfigLogic>();
        }

        public async Task IntegrateAttendanceLog(List<IC_VehicleLog> logs)
        {
            try
            {
                var ids = logs.Select(x => x.IntegrateString).ToList();
                var logDb = await _epadContext.IC_VehicleLog.Where(x => ids.Contains(x.IntegrateString)).ToListAsync();
                foreach (var log in logs)
                {
                    if (log.IsDelete == 1)
                    {
                        var logDelete = await _epadContext.IC_VehicleLog.FirstOrDefaultAsync(x => x.IntegrateString == log.IntegrateString);
                        if (logDelete != null)
                        {
                            _epadContext.IC_VehicleLog.Remove(logDelete);
                        }
                        continue;
                    }
                    if (logDb.Any(x => x.IntegrateString == log.IntegrateString))
                    {
                        var logUpdate = logDb.FirstOrDefault(x => x.IntegrateString == log.IntegrateString);
                        logUpdate.FromDate = log.FromDate;
                        logUpdate.ToDate = log.ToDate;
                        logUpdate.Plate = log.Plate;
                        logUpdate.ComputerIn = log.ComputerIn;
                        logUpdate.ComputerOut = log.ComputerOut;
                        logUpdate.VehicleTypeId = log.VehicleTypeId;
                        logUpdate.EmployeeATID = log.EmployeeATID;
                        logUpdate.IsDelete = log.IsDelete;
                        _epadContext.IC_VehicleLog.Update(logUpdate);
                    }
                    else
                    {
                        var logUpdate = new IC_VehicleLog()
                        {
                            FromDate = log.FromDate,
                            ToDate = log.ToDate,
                            Plate = log.Plate,
                            ComputerIn = log.ComputerIn,
                            ComputerOut = log.ComputerOut,
                            VehicleTypeId = log.VehicleTypeId,
                            EmployeeATID = log.EmployeeATID,
                            IntegrateString = log.IntegrateString,
                            IsDelete = log.IsDelete
                        };
                        await _epadContext.IC_VehicleLog.AddAsync(logUpdate);
                    }
                }

                await _epadContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public async Task IntegrateEmployeeToLovad(List<string> employeeATIDs)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.RE_PROCESSING_REGISTERCARD.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    var lstDepartmentIndex = (from e in _epadContext.HR_User
                                      join w in _epadContext.IC_WorkingInfo.Where(w => w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                                       on e.EmployeeATID equals w.EmployeeATID
                                      where employeeATIDs.Contains(e.EmployeeATID)
                                      select w.DepartmentIndex
                                      ).ToList();
                    var lstContent = await _epadContext.IC_Department.Where(x => lstDepartmentIndex.Contains(x.Index)).ToListAsync();
                    foreach (var item in lstContent)
                    {
                        try
                        {
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                            var json = SerializeDepartmentToString(item);
                            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                            var byteContent = new ByteArrayContent(buffer);
                            HttpResponseMessage response = await client.PostAsync("api/ths/v1/department/save", byteContent);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response.EnsureSuccessStatusCode();
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"IntegrateVehicle: {ex}");
                        }
                    }
                    var now = DateTime.Now;
                    var blackLists = _epadContext.GC_BlackList.Where(x => x.FromDate.Date <= now.Date && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
                    var employeeLst = (from e in _epadContext.HR_User
                                       join w in _epadContext.IC_WorkingInfo.Where(w => w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                                        on e.EmployeeATID equals w.EmployeeATID into WorkingIn
                                       from worrk in WorkingIn.DefaultIfEmpty()
                                       join d in _epadContext.IC_Department.Where(x => x.IsInactive != true)
                                       on worrk.DepartmentIndex equals d.Index into deptGroup
                                       from dept in deptGroup.DefaultIfEmpty()
                                       where employeeATIDs.Contains(e.EmployeeATID)
                                       select new IC_CustomerParam
                                       {
                                           Code = e.EmployeeATID,
                                           CustomerTypeCode = (e.EmployeeType == null ? (int)EmployeeType.Employee : e.EmployeeType).ToString(),
                                           DeparmentCode = worrk != null && worrk.DepartmentIndex != 0 ? worrk.DepartmentIndex.ToString() : "0",
                                           Name = e.FullName,
                                           Blacklist = blackLists.Any(x => x.EmployeeATID == e.EmployeeATID || (!string.IsNullOrEmpty(x.Nric) && x.Nric == e.Nric)) ? 1 : 0
                                       }).ToList();



                    var listSplitEmployeeID = CommonUtils.SplitList(employeeLst, 100);
                    foreach (var item in listSplitEmployeeID)
                    {
                        try
                        {
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                            var json = JsonConvert.SerializeObject(item);
                            json = "list_data=" + json;
                            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                            var byteContent = new ByteArrayContent(buffer);
                            HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer/save_list", byteContent);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response.EnsureSuccessStatusCode();
                                var result = await response.Content.ReadAsStringAsync();
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"IntegrateCustomer: {ex}");
                        }
                    }

                    var employeeLst1 = (from emp in _epadContext.HR_User
                                       join pkla in _epadContext.GC_EmployeeVehicle
                                   on emp.EmployeeATID equals pkla.EmployeeATID
                                   into pke
                                       from pkeResult in pke.DefaultIfEmpty()

                                       join wk in _epadContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                                           && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date)))
                                       on emp.EmployeeATID equals wk.EmployeeATID
                                       into pkw
                                       from pkwResult in pkw.DefaultIfEmpty()
                                       where emp.EmployeeType != (int)EmployeeType.Guest && employeeATIDs.Contains(emp.EmployeeATID)
                                       select new IC_CustomerVehicle
                                       {
                                           Code = emp.EmployeeATID,
                                           VehicleBrand = pkeResult != null ? pkeResult.Branch : "",
                                           VehicleCode = pkeResult != null ? (pkeResult.Type + 1).ToString() : "1",
                                           VehicleNumber = pkeResult != null ? pkeResult.Plate : "",
                                           DateStart = pkeResult != null ? pkeResult.FromDate.ToddMMyyyy() : (pkwResult != null ? pkwResult.FromDate.ToddMMyyyy() : DateTime.Now.ToddMMyyyy()),
                                           DateEnd = pkeResult != null && pkeResult.ToDate.HasValue ? pkeResult.ToDate.Value.ToddMMyyyy() : (pkwResult != null && pkwResult.ToDate.HasValue ? pkwResult.ToDate.Value.ToddMMyyyy() : ""),
                                           TimeStart = "00:00",
                                           TimeEnd = "23:59"
                                       }).ToList();

                    var employeeLst2 = (from emp in _epadContext.HR_User
                                        join pkla in _epadContext.GC_CustomerVehicle
                                    on emp.EmployeeATID equals pkla.EmployeeATID
                                    into pke
                                        from pkeResult in pke.DefaultIfEmpty()
                                        join cus in _epadContext.HR_CustomerInfo
                                      on emp.EmployeeATID equals cus.EmployeeATID
                                      into cuss
                                        from cusResult in cuss.DefaultIfEmpty()
                                        where emp.EmployeeType == (int)EmployeeType.Guest && employeeATIDs.Contains(emp.EmployeeATID)
                                        select new IC_CustomerVehicle
                                        {
                                            Code = emp.EmployeeATID,
                                            VehicleBrand = pkeResult != null ? pkeResult.Branch : "",
                                            VehicleCode = pkeResult != null ? (pkeResult.Type + 1).ToString() : "1",
                                            VehicleNumber = pkeResult != null ? pkeResult.Plate : "",
                                            DateStart = cusResult != null ? cusResult.FromTime.ToddMMyyyy() : "",
                                            DateEnd = cusResult != null ? cusResult.ToTime.ToddMMyyyy() : "",
                                            TimeStart = cusResult != null ? cusResult.FromTime.ToHHmm() : "",
                                            TimeEnd = cusResult != null ? cusResult.ToTime.ToHHmm() : ""
                                        }).ToList();

                    employeeLst1.AddRange(employeeLst2);

                    var listSplitEmployeeID1 = CommonUtils.SplitList(employeeLst1, 100);
                    foreach (var item in listSplitEmployeeID1)
                    {
                        try
                        {
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                            var json = JsonConvert.SerializeObject(item);
                            json = "list_data=" + json;
                            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                            var byteContent = new ByteArrayContent(buffer);
                            HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer_vehicle/save_list", byteContent);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response.EnsureSuccessStatusCode();
                                var result = await response.Content.ReadAsStringAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"IntegrateVehicles: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateEmployeeToLovad: {ex}");
            }
        }

        public string SerializeDepartmentToString(IC_Department customerTypes)
        {
            var json = "code=" + customerTypes.Index + "&name=" + customerTypes.Name + "&parentcode=" + customerTypes.ParentIndex ?? "";
            return json;
        }

        public async Task DeleteEmployeeToLovad(List<string> employeeATIDs)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.RE_PROCESSING_REGISTERCARD.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    foreach (var item in employeeATIDs)
                    {
                        try
                        {
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                            string json = "code=" + item;
                            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                            var byteContent = new ByteArrayContent(buffer);
                            HttpResponseMessage response = await client.PostAsync("api/ths/v1/customer/delete", byteContent);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response.EnsureSuccessStatusCode();
                                var result = await response.Content.ReadAsStringAsync();
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"DeleteEmployeeToLovad: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteEmployeeToLovad: {ex}");
            }
        }

        public async Task IntegrateAttendanceLogError(List<IC_VehicleLog> logs)
        {
            try
            {
                var ids = logs.Select(x => x.IntegrateString).ToList();
                var logDb = _epadContext.IC_VehicleLog.Where(x => ids.Contains(x.IntegrateString)).ToList();
                foreach (var log in logs)
                {
                    if (log.IsDelete == 1)
                    {
                        var logDelete = await _epadContext.IC_VehicleLog.FirstOrDefaultAsync(x => x.IntegrateString == log.IntegrateString);
                        if (logDelete != null)
                        {
                            _epadContext.IC_VehicleLog.Remove(logDelete);
                        }
                        continue;
                    }
                    if (logDb.Any(x => x.IntegrateString == log.IntegrateString))
                    {
                        var logUpdate = logDb.FirstOrDefault(x => x.IntegrateString == log.IntegrateString);
                        logUpdate.FromDate = log.FromDate;
                        logUpdate.ToDate = log.ToDate;
                        logUpdate.Plate = log.Plate;
                        logUpdate.ComputerIn = log.ComputerIn;
                        logUpdate.ComputerOut = log.ComputerOut;
                        logUpdate.VehicleTypeId = log.VehicleTypeId;
                        logUpdate.Reason = log.Reason;
                        logUpdate.UpdatedUser = log.UpdatedUser;
                        logUpdate.IsDelete = log.IsDelete;
                        logUpdate.EmployeeATID = log.EmployeeATID;
                        _epadContext.IC_VehicleLog.Update(logUpdate);
                    }
                    else
                    {
                        var logUpdate = new IC_VehicleLog()
                        {
                            FromDate = log.FromDate,
                            ToDate = log.ToDate,
                            Plate = log.Plate,
                            ComputerIn = log.ComputerIn,
                            ComputerOut = log.ComputerOut,
                            VehicleTypeId = log.VehicleTypeId,
                            Reason = log.Reason,
                            UpdatedUser = log.UpdatedUser,
                            EmployeeATID = log.EmployeeATID,
                            IntegrateString = log.IntegrateString,
                            IsDelete = log.IsDelete,
                        };
                        await _epadContext.IC_VehicleLog.AddAsync(logUpdate);
                    }
                }

                await _epadContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }

        public List<IC_VehicleLog> GetLogInNotOutByEmployeeATID(string employeeATID)
        {
            return DbContext.IC_VehicleLog.AsNoTracking().Where(x
                => employeeATID == x.EmployeeATID && string.IsNullOrEmpty(x.ComputerOut) && !x.ToDate.HasValue).ToList();
        }
    }
}
