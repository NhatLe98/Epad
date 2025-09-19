using EPAD_Common.Enums;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class HR_EmployeeLogic : IHR_EmployeeLogic
    {
        private ezHR_Context _dbIntergrateContext;
        private EPAD_Context _dbContext;
        private static IMemoryCache cache;
        IConfiguration _configuration;
        private ConfigObject _config;
        private string mCommunicateToken;
        private ILogger _logger;
        private readonly IIC_ConfigLogic _iC_ConfigLogic;

        public HR_EmployeeLogic(ezHR_Context dbIntergrateContext, IMemoryCache pCache, EPAD_Context dbContext, IConfiguration configuration, ILogger<IHR_EmployeeLogic> logger, IIC_ConfigLogic iC_ConfigLogic)
        {
            _configuration = configuration;
            _dbIntergrateContext = dbIntergrateContext;
            cache = pCache;
            _dbContext = dbContext;
            _config = ConfigObject.GetConfig(cache);
            mCommunicateToken = _configuration.GetValue<string>("CommunicateToken");
            _logger = logger;
            _iC_ConfigLogic = iC_ConfigLogic;
        }

        public async Task<HR_Employee> GetByEmployeeATIDAndCompanyIndex(string empATID, int companyIndex)
        {
            var data = await _dbIntergrateContext.HR_Employee.FirstOrDefaultAsync(x => x.EmployeeATID == empATID && x.CompanyIndex == companyIndex);
            if (data.Image != null && data.Image.Length > 0)
            {
                try
                {
                    data.Avatar = Convert.ToBase64String(data.Image);
                }
                catch (Exception)
                {
                }
            }
            return data;
        }

        public async Task IntegrateBlackListToOffline(List<string> employeeATIDs, List<string> nricLst, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var listBlack = new List<GC_BlackList>();
                if ((employeeATIDs != null && employeeATIDs.Count > 0) || (nricLst != null && nricLst.Count > 0))
                {
                    listBlack = await _dbContext.GC_BlackList.Where(x => employeeATIDs.Contains(x.EmployeeATID) || nricLst.Contains(x.Nric)).ToListAsync();
                }
                else
                {
                    listBlack = await _dbContext.GC_BlackList.ToListAsync();
                }

                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(listBlack);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateBlackList", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateBlackListToOffline: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var listBlack = new List<GC_BlackList>();
                        if ((employeeATIDs != null && employeeATIDs.Count > 0) || (nricLst != null && nricLst.Count > 0))
                        {
                            listBlack = await _dbContext.GC_BlackList.Where(x => employeeATIDs.Contains(x.EmployeeATID) || nricLst.Contains(x.Nric)).ToListAsync();
                        }
                        else
                        {
                            listBlack = await _dbContext.GC_BlackList.ToListAsync();
                        }

                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(listBlack);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateBlackList", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }


                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateBlackList: {ex}");
                }
            }

        }

        public async Task IntegrateUserToOfflineEmployee(List<string> employeeATIDs)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    await IntegrateUserToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateEmployeeToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateWorkingInfo(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateCardToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateUserToOfflineEmployee: {ex}");
            }

        }

        public async Task IntegrateUserToOfflineCustomer(List<string> employeeATIDs)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    await IntegrateUserToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateCustmerToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateWorkingInfo(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                    await IntegrateCardToOffline(employeeATIDs, config.IntegrateLogParam.LinkAPIIntegrate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateUserToOfflineEmployee: {ex}");
            }

        }



        public async Task IntegrateCustomerCardToOffline(List<string> employeeATIDs, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                try
                {

                    var customerCards = new List<HR_CustomerCard>();
                    if (employeeATIDs != null && employeeATIDs.Count > 0)
                    {
                        customerCards = await _dbContext.HR_CustomerCard.Where(x => employeeATIDs.Contains(x.CardID)).ToListAsync();
                    }
                    else
                    {
                        customerCards = await _dbContext.HR_CustomerCard.ToListAsync();
                    }
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(api);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(customerCards);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateCustomerCardToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateCustomerCardToOffline: {ex}");
                }

            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var customerCards = new List<HR_CustomerCard>();
                        if (employeeATIDs != null && employeeATIDs.Count > 0)
                        {
                            customerCards = await _dbContext.HR_CustomerCard.Where(x => employeeATIDs.Contains(x.CardID)).ToListAsync();
                        }
                        else
                        {
                            customerCards = await _dbContext.HR_CustomerCard.ToListAsync();
                        }
                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(customerCards);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateCustomerCardToOffline", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateCustomerCardToOffline: {ex}");
                }
            }
        }

        public async Task IntegrateCardToOffline(List<string> employeeATIDs, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var customerCards = new List<HR_CardNumberInfo>();
                if (employeeATIDs != null && employeeATIDs.Count > 0)
                {
                    customerCards = await _dbContext.HR_CardNumberInfo.Where(x => employeeATIDs.Contains(x.EmployeeATID) && x.IsActive == true).ToListAsync();
                }
                else
                {
                    customerCards = await _dbContext.HR_CardNumberInfo.Where(x => x.IsActive == true).ToListAsync();
                }

                var employeead = customerCards.Select(x => x.EmployeeATID).ToList();

                if (employeeATIDs != null)
                {
                    var adds = employeeATIDs.Where(x => !employeead.Contains(x)).Select(x => new HR_CardNumberInfo { CardNumber = "0", EmployeeATID = x, CompanyIndex = _config.CompanyIndex }).ToList();
                    if (adds.Any())
                    {
                        customerCards.AddRange(adds);

                    }
                }


                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(customerCards);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateCardToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateCardToOffline: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var customerCards = new List<HR_CardNumberInfo>();
                        if (employeeATIDs != null && employeeATIDs.Count > 0)
                        {
                            customerCards = await _dbContext.HR_CardNumberInfo.Where(x => employeeATIDs.Contains(x.EmployeeATID) && x.IsActive == true).ToListAsync();
                        }
                        else
                        {
                            customerCards = await _dbContext.HR_CardNumberInfo.Where(x => x.IsActive == true).ToListAsync();
                        }


                        var employeead = customerCards.Select(x => x.EmployeeATID).ToList();

                        if (employeeATIDs != null)
                        {
                            var adds = employeeATIDs.Where(x => !employeead.Contains(x)).Select(x => new HR_CardNumberInfo { CardNumber = "0", EmployeeATID = x, CompanyIndex = _config.CompanyIndex }).ToList();
                            if (adds.Any())
                            {
                                customerCards.AddRange(adds);

                            }
                        }
                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(customerCards);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateCardToOffline", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateCardToOffline: {ex}");
                }
            }
        }


        public async Task IntegrateDepartmentToOffline(List<long> indexes, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var department = new List<IC_Department>();
                if (indexes != null && indexes.Count > 0)
                {
                    department = await (from e in _dbContext.IC_Department
                                        join w in _dbContext.IC_Department
                                         on e.ParentIndex equals w.Index into deptGroup
                                        from dept in deptGroup.DefaultIfEmpty()
                                        where indexes.Contains(e.Index)
                                        select new IC_Department
                                        {
                                            Name = e.Name,
                                            Code = e.Code,
                                            ParentCode = dept != null ? dept.Code : null,
                                            CompanyIndex = e.CompanyIndex,
                                            IsContractorDepartment = e.IsContractorDepartment,
                                            IsDriverDepartment = e.IsDriverDepartment

                                        }).ToListAsync();

                }
                else
                {
                    department = await (from e in _dbContext.IC_Department
                                        join w in _dbContext.IC_Department
                                         on e.ParentIndex equals w.Index into deptGroup
                                        from dept in deptGroup.DefaultIfEmpty()

                                        select new IC_Department
                                        {
                                            Name = e.Name,
                                            Code = e.Code,
                                            ParentCode = dept != null ? dept.Code : null,
                                            CompanyIndex = e.CompanyIndex,
                                            IsContractorDepartment = e.IsContractorDepartment,
                                            IsDriverDepartment = e.IsDriverDepartment

                                        }).ToListAsync();
                }

                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(department);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateDepartmentToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateDepartmentToOffline: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var department = new List<IC_Department>();
                        if (indexes != null && indexes.Count > 0)
                        {
                            department = await (from e in _dbContext.IC_Department
                                                join w in _dbContext.IC_Department
                                                 on e.ParentIndex equals w.Index into deptGroup
                                                from dept in deptGroup.DefaultIfEmpty()
                                                where indexes.Contains(e.Index)
                                                select new IC_Department
                                                {
                                                    Name = e.Name,
                                                    Code = e.Code,
                                                    ParentCode = dept != null ? dept.Code : null,
                                                    CompanyIndex = e.CompanyIndex,
                                                    IsContractorDepartment = e.IsContractorDepartment,
                                                    IsDriverDepartment = e.IsDriverDepartment

                                                }).ToListAsync();

                        }
                        else
                        {
                            department = await (from e in _dbContext.IC_Department
                                                join w in _dbContext.IC_Department
                                                 on e.ParentIndex equals w.Index into deptGroup
                                                from dept in deptGroup.DefaultIfEmpty()

                                                select new IC_Department
                                                {
                                                    Name = e.Name,
                                                    Code = e.Code,
                                                    ParentCode = dept != null ? dept.Code : null,
                                                    CompanyIndex = e.CompanyIndex,
                                                    IsContractorDepartment = e.IsContractorDepartment,
                                                    IsDriverDepartment = e.IsDriverDepartment
                                                }).ToListAsync();
                        }

                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(department);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateDepartmentToOffline", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }



                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateDepartmentToOffline: {ex}");
                }
            }

        }

        public async Task IntegrateUserToOffline(List<string> employeeATIDs, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var users = new List<HR_User>();
                if (employeeATIDs != null && employeeATIDs.Count > 0)
                {
                    users = await _dbContext.HR_User.Where(x => employeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
                }
                else
                {
                    users = await _dbContext.HR_User.ToListAsync();
                }

                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(users);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateUserToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateUserToOffline: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var users = new List<HR_User>();
                        if (employeeATIDs != null && employeeATIDs.Count > 0)
                        {
                            users = await _dbContext.HR_User.Where(x => employeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
                        }
                        else
                        {
                            users = await _dbContext.HR_User.ToListAsync();
                        }

                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(users);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateUserToOffline", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }




                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateUserToOffline: {ex}");
                }

            }
        }

        public async Task IntegrateWorkingInfo(List<string> employeeATIDs, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var users = new List<IC_WorkingInfoIntegrate>();
                if (employeeATIDs != null && employeeATIDs.Count > 0)
                {
                    users = await (from e in _dbContext.IC_WorkingInfo
                                   join w in _dbContext.IC_Department
                                    on e.DepartmentIndex equals w.Index into deptGroup
                                   from dept in deptGroup.DefaultIfEmpty()
                                   where employeeATIDs.Contains(e.EmployeeATID)
                                && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                   select new IC_WorkingInfoIntegrate
                                   {
                                       CompanyIndex = e.CompanyIndex,
                                       DepartmentIndex = e.DepartmentIndex,
                                       FromDate = e.FromDate,
                                       ToDate = e.ToDate,
                                       PositionIndex = e.PositionIndex,
                                       Status = e.Status,
                                       IsManager = e.IsManager,
                                       DepartmentCode = dept != null ? dept.Code : null,
                                       EmployeeATID = e.EmployeeATID,

                                   }).ToListAsync();

                }
                else
                {
                    users = await (from e in _dbContext.IC_WorkingInfo
                                   join w in _dbContext.IC_Department
                                    on e.DepartmentIndex equals w.Index into deptGroup
                                   from dept in deptGroup.DefaultIfEmpty()
                                   where e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                   select new IC_WorkingInfoIntegrate
                                   {
                                       CompanyIndex = e.CompanyIndex,
                                       DepartmentIndex = e.DepartmentIndex,
                                       FromDate = e.FromDate,
                                       ToDate = e.ToDate,
                                       PositionIndex = e.PositionIndex,
                                       Status = e.Status,
                                       IsManager = e.IsManager,
                                       DepartmentCode = dept != null ? dept.Code : null,
                                       EmployeeATID = e.EmployeeATID,

                                   }).ToListAsync();
                }


                var listSplitEmployeeID = CommonUtils.SplitList(users, 500);
                foreach (var item in listSplitEmployeeID)
                {
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(api);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(item);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateWorkingInfo", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"IntegrateWorkingInfo: {ex}");
                    }
                }

            }
            else
            {

                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var users = new List<IC_WorkingInfoIntegrate>();
                        if (employeeATIDs != null && employeeATIDs.Count > 0)
                        {
                            users = await (from e in _dbContext.IC_WorkingInfo
                                           join w in _dbContext.IC_Department
                                            on e.DepartmentIndex equals w.Index into deptGroup
                                           from dept in deptGroup.DefaultIfEmpty()
                                           where employeeATIDs.Contains(e.EmployeeATID)
                                        && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                           select new IC_WorkingInfoIntegrate
                                           {
                                               CompanyIndex = e.CompanyIndex,
                                               DepartmentIndex = e.DepartmentIndex,
                                               FromDate = e.FromDate,
                                               ToDate = e.ToDate,
                                               PositionIndex = e.PositionIndex,
                                               Status = e.Status,
                                               IsManager = e.IsManager,
                                               DepartmentCode = dept != null ? dept.Code : null,
                                               EmployeeATID = e.EmployeeATID,

                                           }).ToListAsync();

                        }
                        else
                        {
                            users = await (from e in _dbContext.IC_WorkingInfo
                                           join w in _dbContext.IC_Department
                                            on e.DepartmentIndex equals w.Index into deptGroup
                                           from dept in deptGroup.DefaultIfEmpty()
                                           where e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                           select new IC_WorkingInfoIntegrate
                                           {
                                               CompanyIndex = e.CompanyIndex,
                                               DepartmentIndex = e.DepartmentIndex,
                                               FromDate = e.FromDate,
                                               ToDate = e.ToDate,
                                               PositionIndex = e.PositionIndex,
                                               Status = e.Status,
                                               IsManager = e.IsManager,
                                               DepartmentCode = dept != null ? dept.Code : null,
                                               EmployeeATID = e.EmployeeATID,

                                           }).ToListAsync();
                        }

                        var listSplitEmployeeID = CommonUtils.SplitList(users, 500);
                        foreach (var item in listSplitEmployeeID)
                        {
                            var client1 = new HttpClient();
                            client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                            client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                            var json = JsonConvert.SerializeObject(item);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            try
                            {
                                HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateWorkingInfo", content);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    response.EnsureSuccessStatusCode();
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"IntegrateWorkingInfo: {ex}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"IntegrateUserToOffline: {ex}");
                }
            }

        }

        public async Task IntegrateEmployeeToOffline(List<string> employeeATIDs, string api)
        {
            var users = new List<HR_EmployeeInfo>();
            if (employeeATIDs != null && employeeATIDs.Count > 0)
            {
                users = await _dbContext.HR_EmployeeInfo.Where(x => employeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            }
            else
            {
                users = await _dbContext.HR_EmployeeInfo.ToListAsync();
            }

            var client1 = new HttpClient();
            client1.BaseAddress = new Uri(api);
            client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
            var json = JsonConvert.SerializeObject(users);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateEmployeeToOffline", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateEmployeeToOffline: {ex}");
            }
        }

        public async Task IntegrateCustmerToOffline(List<string> employeeATIDs, string api)
        {
            var users = new List<HR_CustomerInfo>();
            if (employeeATIDs != null && employeeATIDs.Count > 0)
            {
                users = await _dbContext.HR_CustomerInfo.Where(x => employeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            }
            else
            {
                users = await _dbContext.HR_CustomerInfo.ToListAsync();
            }

            var client1 = new HttpClient();
            client1.BaseAddress = new Uri(api);
            client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
            var json = JsonConvert.SerializeObject(users);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/IntegrateCustmerToOffline", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"IntegrateCustmerToOffline: {ex}");
            }
        }

        public async Task DeleteBlackList(GC_BlackListIntegrate blackList, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(blackList);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteBlackList", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteBlackList: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(blackList);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteBlackList", content);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            response.EnsureSuccessStatusCode();
                        }


                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteBlackList: {ex}");
                }
            }
        }

        public async Task DeleteCustomerCardToOffline(List<string> employeeATIDs, string api)
        {
            if (!string.IsNullOrEmpty(api))
            {
                var client1 = new HttpClient();
                client1.BaseAddress = new Uri(api);
                client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                var json = JsonConvert.SerializeObject(employeeATIDs);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteCustomerCardToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteCustomerCardToOffline: {ex}");
                }
            }
            else
            {
                var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
                var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
                try
                {
                    var config = downloadConfig.FirstOrDefault();
                    if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                    {
                        var client1 = new HttpClient();
                        client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                        client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                        var json = JsonConvert.SerializeObject(employeeATIDs);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        try
                        {
                            HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteCustomerCardToOffline", content);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response.EnsureSuccessStatusCode();
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"DeleteCustomerCardToOffline: {ex}");
                        }


                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteCustomerCardToOffline: {ex}");
                }

            }
        }

        public async Task DeleteCardToOffline(List<HR_CardNumberInfo> employeeATIDs, string api)
        {
            var client1 = new HttpClient();
            client1.BaseAddress = new Uri(api);
            client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
            var json = JsonConvert.SerializeObject(employeeATIDs);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteCardToOffline", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteCardToOffline: {ex}");
            }
        }

        public async Task DeleteDepartmentToOffline(List<string> codes)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(codes);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteDepartmentToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteDepartmentToOffline: {ex}");
            }

        }



        public async Task DeleteUserToOffline(List<string> codes)
        {
            var addedParams = new List<AddedParam>
            {
                new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex },
                new AddedParam { Key = "EventType", Value = ConfigAuto.INTEGRATE_INFO_TO_OFFLINE.ToString() }
            };
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            try
            {
                var config = downloadConfig.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.LinkAPIIntegrate))
                {
                    var client1 = new HttpClient();
                    client1.BaseAddress = new Uri(config.IntegrateLogParam.LinkAPIIntegrate);
                    client1.DefaultRequestHeaders.Add("api-token", mCommunicateToken);
                    var json = JsonConvert.SerializeObject(codes);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client1.PostAsync("api/IC_Offline/DeleteUserToOffline", content);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteUserToOffline: {ex}");
            }

        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfo(int pCompanyIndex)
        {
            string query =
                  @"SELECT  null as Avatar, HR_Employee.EmployeeATID, HR_Employee.EmployeeCode, HR_Employee.CardNumber, 
                HR_Employee.LastName + ' ' + HR_Employee.MidName + ' ' + HR_Employee.FirstName AS FullName,  
                HR_Employee.FirstName,HR_Employee.MidName,HR_Employee.LastName,HR_EmployeeContactInfo.Email,HR_EmployeeContactInfo.MobilePhone as Phone, 
                HR_Employee.Gender, HR_Employee.JoinedDate, HR_WorkingInfo.DepartmentIndex, HR_Department.[Name] AS Department,
                HR_WorkingInfo.PositionIndex,HR_Position.[Name] AS Position,HR_WorkingInfo.TitlesIndex as TitleIndex,HR_Titles.Name as Title, 
                HR_WorkingInfo.EmployeeKind as EmployeeKindIndex, HR_KindOfEmployee.[Name] as EmployeeKind, 
                HR_WorkingInfo.IsManager,HR_WorkingInfo.ManagedDepartment, HR_WorkingInfo.ManagedOtherDepartments as ManagedOtherDepartment,HR_WorkingInfo.DirectManager, 
                HR_WorkingInfo.FromDate, HR_WorkingInfo.ToDate,  
                HR_Employee.DayOfBirth, HR_Employee.MonthOfBirth, HR_Employee.YearOfBirth,  
                HR_Employee.TaxNumber,HR_Employee.NRIC, 
                HR_Employee.SocialInsNo, HR_WorkingInfo.EmployeeKind as WorkingEmployeeKind,HR_Employee.CompanyIndex 
                FROM  HR_Employee LEFT OUTER JOIN HR_WorkingInfo ON HR_WorkingInfo.EmployeeATID = HR_Employee.EmployeeATID  AND HR_WorkingInfo.CompanyIndex = HR_Employee.CompanyIndex 
                and (HR_WorkingInfo.ToDate is null OR  
                (Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0)) 
                LEFT OUTER JOIN HR_Department ON HR_Department.[Index] = HR_WorkingInfo.DepartmentIndex  
                LEFT OUTER JOIN HR_Position ON HR_WorkingInfo.PositionIndex = HR_Position.[Index] 
                LEFT OUTER JOIN HR_Titles ON HR_WorkingInfo.TitlesIndex = HR_Titles.[Index] 
                and (HR_WorkingInfo.ToDate is null OR  
                (Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0)) 
                LEFT OUTER JOIN HR_EmployeeContactInfo on HR_EmployeeContactInfo.EmployeeATID = HR_Employee.EmployeeATID AND HR_EmployeeContactInfo.CompanyIndex = HR_Employee.CompanyIndex 
                LEFT OUTER JOIN HR_KindOfEmployee on HR_WorkingInfo.EmployeeKind=HR_KindOfEmployee.[Index] 
                WHERE (HR_Employee.MarkForDelete = 0) 
                AND HR_Employee.CompanyIndex = @CompanyIndex";

            var lstParam = new List<SqlParameter>();
            lstParam.Add(new SqlParameter("CompanyIndex", pCompanyIndex));

            var result = _dbIntergrateContext.EmployeeFullInfo.FromSqlRaw(query, lstParam.Select(x => (object)x).ToArray());

            List<EmployeeFullInfo> listData = await result.OrderBy(t => t.EmployeeATID).ToListAsync();
            return listData;
        }

        public List<IC_EmployeeDTO> GetMany(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();
            int companyIndexCo = _config.CompanyIndex;

            var addparamEmployeeTranfer = addedParams.FirstOrDefault(e => e.Key == "ListEmployeeTransferATID");

            string select = "SELECT e.CompanyIndex, e.EmployeeATID,e.EmployeeCode,e.LastName + ' ' + e.MidName + ' ' + e.FirstName as FullName, ";
            select += "\n" + "e.Gender, e.NickName as NameOnMachine, e.CardNumber, e.MarkForDelete,e.[Image] as Avatar,  e.UpdatedDate, e.UpdatedUser,e.JoinedDate,";
            select += "\n" + " w.[Index] as WorkingInfoIndex, w.DepartmentIndex,w.FromDate,w.ToDate,w.Synched as IsSync, ";
            select += "\n" + " d.Name as DepartmentName, d.Code as DepartmentCode, " +
                "(SELECT TOP 1 stoped.StartedDate FROM  HR_EmployeeStoppedWorkingInfo stoped WHERE stoped.EmployeeATID = e.EmployeeATID and stoped.ReturnedDate is null ORDER BY stoped.StartedDate DESC) as StoppedDate";

            string from = "\n" + " FROM HR_Employee e ";
            from += "\n" + " LEFT JOIN HR_WorkingInfo w on e.EmployeeATID = w.EmployeeATID and e.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Department d on d.[Index] = w.DepartmentIndex and d.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Position p on p.[Index] = w.PositionIndex and p.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Titles t on t.[Index] = w.TitlesIndex and t.CompanyIndex = w.CompanyIndex ";

            string where = "\n" + $"WHERE e.MarkForDelete = 0 AND ";

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                var filterBy = filter.Split(" ").ToList();
                                where += " (e.EmployeeCode like '%" + filter + "%' OR ";
                                where += " e.EmployeeATID like '%" + filter + "%' OR ";
                                where += " e.NickName like '%" + filter + "%' OR ";
                                if (filterBy.Count > 0)
                                {
                                    where += " e.EmployeeATID in ('" + string.Join("','", filterBy) + "') OR ";
                                }
                                where += " e.CardNumber like '%" + filter + "%') AND ";

                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                companyIndexCo = Convert.ToInt32(param.Value);
                                where += " e.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {

                                int departmentID = Convert.ToInt32(param.Value);
                                where += "w.DepartmentIndex = " + departmentID + " AND ";
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                where += "(w.DepartmentIndex IN (" + string.Join(",", departments) + ") OR w.[Index] is null ) AND ";

                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "e.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;

                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "e.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;
                        case "IsSync":
                            if (param.Value != null)
                            {
                                bool isSync = Convert.ToBoolean(param.Value);
                                where += " e.IsSync = " + isSync.ToString() + " AND ";
                            }
                            break;
                        case "IsWorking":
                            if (param.Value != null)
                            {
                                var listWorking = (List<int>)param.Value;
                                if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                {
                                    if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                    {
                                        where += "e.EmployeeATID NOT IN (Select distinct EmployeeATID From HR_EmployeeStoppedWorkingInfo s WHERE s.StartedDate <=  GETDATE() AND s.ReturnedDate is null) AND " +
                                            "((w.ToDate is null) OR (Datediff(day, w.ToDate, GetDate()) <= 0) OR  w.DepartmentIndex = 0 OR  w.DepartmentIndex is null ) AND";
                                    }
                                    if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stop working
                                    {
                                        where += "(e.EmployeeATID IN (Select distinct EmployeeATID From HR_EmployeeStoppedWorkingInfo s WHERE s.StartedDate <=  GETDATE() AND s.ReturnedDate is null)) AND ";
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            where = where.Substring(0, where.LastIndexOf("AND"));
            var query = select + from + where;
            string conn = _configuration.GetConnectionString("connectionStringOtherDB");
            var resutlData = new List<IC_EmployeeDTO>();
            using (var connection = new SqlConnection(conn))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            var index = 0;
                            foreach (DataRow dtRow in dataTable.Rows)
                            {
                                // On all tables' columns
                                var dataRow = new IC_EmployeeDTO();
                                dataRow.Index = index++;
                                dataRow.CompanyIndex = (dtRow["CompanyIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["CompanyIndex"].ToString());
                                dataRow.WorkingInfoIndex = (dtRow["WorkingInfoIndex"] is DBNull) ? (long?)null : Convert.ToInt64(dtRow["WorkingInfoIndex"].ToString());
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.DepartmentIndex = (dtRow["DepartmentIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["DepartmentIndex"].ToString());
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.Gender = (dtRow["Gender"] is DBNull) ? (short?)null : Convert.ToInt16(dtRow["Gender"]);
                                dataRow.TransferStatus = -1;
                                dataRow.DepartmentName = (dtRow["DepartmentName"] is DBNull) ? "Không có phòng ban" : dtRow["DepartmentName"].ToString();
                                dataRow.DepartmentCode = (dtRow["DepartmentCode"] is DBNull) ? "NoDepartment" : dtRow["DepartmentCode"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.IsSync = (dtRow["IsSync"] is DBNull) ? (bool?)null : Convert.ToBoolean(dtRow["IsSync"]);
                                dataRow.FromDate = (dtRow["FromDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["FromDate"].ToString());
                                dataRow.ToDate = (dtRow["ToDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["ToDate"].ToString());
                                dataRow.Avatar = (dtRow["Avatar"] is DBNull) ? "" : dtRow["Avatar"].ToString();
                                dataRow.Status = ((dtRow["ToDate"] is DBNull || DateTime.Parse(dtRow["ToDate"].ToString()) > DateTime.Now) && (dtRow["StoppedDate"] is DBNull || DateTime.Parse(dtRow["StoppedDate"].ToString()) > DateTime.Now.Date)) ? "IsWorking" : "StoppedWork";
                                resutlData.Add(dataRow);
                            }
                        }
                    }
                }
            }

            var res = GetUserMasterFromIntegrate(resutlData, 2);
            return res;
        }

        public List<IC_EmployeeDTO> GetManyStoppedWorking(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new List<IC_EmployeeDTO>();

            var query = _dbIntergrateContext.HR_EmployeeStoppedWorkingInfo.AsEnumerable();

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null)
                            {
                                string filter = param.Value.ToString();
                                if (!string.IsNullOrWhiteSpace(filter))
                                {
                                    query = query.Where(u => u.EmployeeATID.Contains(filter));
                                }
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "ToDay":
                            if (param.Value != null)
                            {
                                var today = Convert.ToDateTime(param.Value.ToString());
                                query = query.Where(u => u.StartedDate.Value.Date <= DateTime.Now.Date && u.ReturnedDate == null);
                            }
                            break;
                        case "StartedDate":
                            if (param.Value != null)
                            {
                                var startedDate = Convert.ToDateTime(param.Value.ToString());
                                query = query.Where(u => u.StartedDate.Value.Date == startedDate.Date);
                            }
                            break;
                        case "ReturnedDate":
                            if (param.Value != null)
                            {
                                var returnedDate = Convert.ToDateTime(param.Value.ToString());
                                query = query.Where(u => u.ReturnedDate != null && u.ReturnedDate.Value.Date == returnedDate.Date);
                            }
                            break;
                        case "Synched":
                            if (param.Value != null)
                            {
                                var synched = Convert.ToInt32(param.Value);
                                query = query.Where(u => u.Synched == synched);
                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null)
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                            }
                            break;
                    }
                }
            }
            // var totalcount = query.Count();
            var result = query.GroupBy(e => e.EmployeeATID).Select(e => e.First()).Select(e => new IC_EmployeeDTO { EmployeeATID = e.EmployeeATID, CompanyIndex = e.CompanyIndex }).ToList();
            // var data = result
            return result;
        }

        private bool? GetIsync(short? itemCheck)
        {
            if (itemCheck.HasValue)
            {
                return itemCheck.Value == 1 ? true : false;
            }
            return null;
        }

        public ListDTOModel<IC_EmployeeDTO> GetPage(List<AddedParam> addedParams)
        {
            if (addedParams == null)
                return new ListDTOModel<IC_EmployeeDTO>();

            string select = "SELECT e.CompanyIndex, e.EmployeeATID,e.EmployeeCode,e.FirstName + ' ' + e.MidName + ' ' + e.LastName as FullName, ";
            select += "\n" + "e.Gender, e.NickName as NameOnMachine, e.CardNumber, e.MarkForDelete,e.[Image] as Avatar,  e.UpdatedDate, e.UpdatedUser,e.JoinedDate,";
            select += "\n" + " w.[Index] as WorkingInfoIndex, w.DepartmentIndex,w.FromDate,w.ToDate,w.Synched as IsSync, ";
            select += "\n" + " d.Name as DepartmentName , d.Code as DepartmentCode";

            string from = "\n" + " FROM HR_Employee e ";
            from += "\n" + " LEFT JOIN HR_WorkingInfo w on e.EmployeeATID = w.EmployeeATID and e.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Department d on d.[Index] = w.DepartmentIndex and d.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Position p on p.[Index] = w.PositionIndex and p.CompanyIndex = w.CompanyIndex ";
            from += "\n" + " LEFT JOIN HR_Titles t on t.[Index] = w.TitlesIndex and t.CompanyIndex = w.CompanyIndex ";

            string where = "\n" + $"WHERE e.MarkForDelete = 0 AND e.EmployeeATID NOT IN (Select distinct EmployeeATID From HR_EmployeeStoppedWorkingInfo s WHERE s.StartedDate <=  GETDATE() AND s.ReturnedDate is null) AND ";

            if (addedParams != null)
            {
                foreach (AddedParam param in addedParams)
                {
                    switch (param.Key)
                    {
                        case "Filter":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                string filter = param.Value.ToString();
                                where += " (e.FullName like '%" + filter + "%' OR ";
                                where += " e.EmployeeCode like '%" + filter + "%' OR ";
                                where += " e.EmployeeATID like '%" + filter + "%' OR ";
                                where += " d.[Name] like '%" + filter + "%' OR ";
                                where += " e.NickName like '%" + filter + "%' OR ";
                                where += " e.CardNumber like '%" + filter + "%') AND ";
                            }
                            break;
                        case "CompanyIndex":
                            if (param.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(param.Value);
                                where += " e.CompanyIndex = " + companyIndex.ToString() + " AND ";
                            }
                            break;
                        case "DepartmentIndex":
                            if (param.Value != null)
                            {

                                int departmentID = Convert.ToInt32(param.Value);
                                where += "w.DepartmentIndex = " + departmentID + " AND ";
                            }
                            break;
                        case "ListDepartment":
                            if (param.Value != null)
                            {
                                IList<long> departments = (IList<long>)param.Value;
                                where += "w.DepartmentIndex IN (" + string.Join(",", departments) + ") AND ";

                            }
                            break;
                        case "ListEmployeeATID":
                            if (param.Value != null && !string.IsNullOrWhiteSpace(param.Value.ToString()))
                            {
                                IList<string> listEmployeeID = (IList<string>)param.Value;
                                where += "e.EmployeeATID IN ('" + string.Join("','", listEmployeeID) + "') AND ";
                            }
                            break;

                        case "EmployeeATID":
                            if (param.Value != null)
                            {
                                string employeeID = param.Value.ToString();
                                where += "e.EmployeeATID = '" + employeeID + "' AND ";
                            }
                            break;
                        case "IsSync":
                            if (param.Value != null)
                            {
                                bool isSync = Convert.ToBoolean(param.Value);
                                where += " e.IsSync = " + isSync.ToString() + " AND ";
                            }
                            break;
                        case "IsCurrentWorking":
                            if (param.Value != null)
                            {
                                where += "((w.ToDate is null and Datediff(day, w.FromDate, GetDate()) >= 0 ) OR (Datediff(day, w.ToDate, GetDate()) <= 0 AND Datediff(day, w.FromDate, GetDate()) >= 0) OR  w.DepartmentIndex = 0 OR  w.DepartmentIndex is null) AND ";
                            }
                            break;
                    }
                }
            }
            where = where.Substring(0, where.LastIndexOf("AND"));
            var query = select + from + where;
            string conn = _configuration.GetConnectionString("connectionStringOtherDB");
            var resutlData = new List<IC_EmployeeDTO>();
            using (var connection = new SqlConnection(conn))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(reader);
                            var index = 0;
                            foreach (DataRow dtRow in dataTable.Rows)
                            {
                                // On all tables' columns
                                var dataRow = new IC_EmployeeDTO();
                                dataRow.Index = index++;
                                dataRow.CompanyIndex = (dtRow["CompanyIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["CompanyIndex"].ToString());
                                dataRow.WorkingInfoIndex = (dtRow["WorkingInfoIndex"] is DBNull) ? (long?)null : Convert.ToInt64(dtRow["WorkingInfoIndex"].ToString());
                                dataRow.EmployeeATID = (dtRow["EmployeeATID"] is DBNull) ? null : dtRow["EmployeeATID"].ToString();
                                dataRow.DepartmentIndex = (dtRow["DepartmentIndex"] is DBNull) ? 0 : Convert.ToInt32(dtRow["DepartmentIndex"].ToString());
                                dataRow.EmployeeCode = (dtRow["EmployeeCode"] is DBNull) ? null : dtRow["EmployeeCode"].ToString();
                                dataRow.FullName = (dtRow["FullName"] is DBNull) ? null : dtRow["FullName"].ToString();
                                dataRow.Gender = (dtRow["Gender"] is DBNull) ? (short?)null : Convert.ToInt16(dtRow["Gender"]);
                                dataRow.TransferStatus = -1;
                                dataRow.DepartmentName = (dtRow["DepartmentName"] is DBNull) ? "Không có phòng ban" : dtRow["DepartmentName"].ToString();
                                dataRow.DepartmentCode = (dtRow["DepartmentCode"] is DBNull) ? "NoDepartment" : dtRow["DepartmentCode"].ToString();
                                dataRow.NameOnMachine = (dtRow["NameOnMachine"] is DBNull) ? null : dtRow["NameOnMachine"].ToString();
                                dataRow.CardNumber = (dtRow["CardNumber"] is DBNull) ? null : dtRow["CardNumber"].ToString();
                                dataRow.IsSync = (dtRow["IsSync"] is DBNull) ? (bool?)null : Convert.ToBoolean(dtRow["IsSync"]);
                                dataRow.FromDate = (dtRow["FromDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["FromDate"].ToString());
                                dataRow.ToDate = (dtRow["ToDate"] is DBNull) ? (DateTime?)null : DateTime.Parse(dtRow["ToDate"].ToString());
                                dataRow.Avatar = (dtRow["Avatar"] is DBNull) ? "" : dtRow["Avatar"].ToString();
                                resutlData.Add(dataRow);
                            }
                        }
                    }
                }
            }

            int pageIndex = 1;
            int pageSize = GlobalParams.ROWS_NUMBER_IN_PAGE;
            var pageIndexParam = addedParams.FirstOrDefault(u => u.Key == "PageIndex");
            var pageSizeParam = addedParams.FirstOrDefault(u => u.Key == "PageSize");
            if (pageIndexParam != null && pageIndexParam.Value != null)
            {
                pageIndex = Convert.ToInt32(pageIndexParam.Value);
            }
            if (pageSizeParam != null && pageSizeParam.Value != null)
            {
                pageSize = Convert.ToInt32(pageSizeParam.Value);
            }

            ListDTOModel<IC_EmployeeDTO> mv = new ListDTOModel<IC_EmployeeDTO>();
            mv.TotalCount = resutlData.Count();
            resutlData = resutlData.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            mv.PageIndex = pageIndex;
            mv.Data = resutlData;
            return mv;
        }

        public List<IC_EmployeeDTO> GetAll()
        {
            var query = _dbIntergrateContext.HR_Employee.AsQueryable();
            var data = query.Where(u => u.CompanyIndex == 2).Select(u => new IC_EmployeeDTO { EmployeeATID = u.EmployeeATID }).ToList();
            return data;
        }

        public List<IC_EmployeeDTO> GetUserMasterMachineInfo(List<string> listEmployeeATID, string filter, UserInfo user)
        {
            List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeATID });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            List<IC_EmployeeDTO> listEmployee = GetMany(addedParams);

            if (listEmployee != null)
            {
                var listEmployeeATIDResult = listEmployee.Select(e => e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                List<IC_UserMaster> listUser = _dbContext.IC_UserMaster.Where(t => t.CompanyIndex == user.CompanyIndex && listEmployeeATIDResult.Contains(t.EmployeeATID)).ToList();

                int index = 0;
                for (int i = 0; i < listEmployee.Count; i++)
                {
                    IC_EmployeeDTO data = null;
                    if (listUser != null)
                    {
                        IC_UserMaster userMasterInfo = listUser.FirstOrDefault(t => t.EmployeeATID == listEmployee[i].EmployeeATID);
                        if (userMasterInfo != null)
                        {
                            data = new IC_EmployeeDTO();
                            data.Index = index + 1;
                            CreateBasicInfo(ref data, listEmployee[i]);

                            data.CardNumber = userMasterInfo.CardNumber;
                            data.Privilege = int.Parse(userMasterInfo.Privilege.ToString());
                            data.Password = userMasterInfo.Password;
                            data.FaceTemplate = !string.IsNullOrEmpty(userMasterInfo.FaceTemplate) ? userMasterInfo.FaceTemplate.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_Content) ? userMasterInfo.FaceV2_Content.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_TemplateBIODATA) ? userMasterInfo.FaceV2_TemplateBIODATA.Length : 0;
                            data.Finger1 = string.IsNullOrEmpty(userMasterInfo.FingerData0) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger2 = string.IsNullOrEmpty(userMasterInfo.FingerData1) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger3 = string.IsNullOrEmpty(userMasterInfo.FingerData2) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger4 = string.IsNullOrEmpty(userMasterInfo.FingerData3) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger5 = string.IsNullOrEmpty(userMasterInfo.FingerData4) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger6 = string.IsNullOrEmpty(userMasterInfo.FingerData5) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger7 = string.IsNullOrEmpty(userMasterInfo.FingerData6) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger8 = string.IsNullOrEmpty(userMasterInfo.FingerData7) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger9 = string.IsNullOrEmpty(userMasterInfo.FingerData8) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger10 = string.IsNullOrEmpty(userMasterInfo.FingerData9) ? 0 : userMasterInfo.FingerData0.Length;
                            listData.Add(data);
                            index++;
                        }
                        else
                        {
                            data = new IC_EmployeeDTO();
                            data.Index = index;

                            CreateBasicInfo(ref data, listEmployee[i]);
                            listData.Add(data);
                            index++;
                        }
                    }
                }
            }
            return listData;
        }
        private string GetPrivilegeName(int privilege)
        {
            switch (privilege)
            {
                case GlobalParams.DevicePrivilege.PUSHAdminRole:
                case GlobalParams.DevicePrivilege.SDKAdminRole:
                    return "Quản trị viên"; // Recommend using language key instead of.
                case GlobalParams.DevicePrivilege.SDKUserRegisterRole:
                    return "Quyền đăng ký";
            }
            return "Người dùng";
        }

        public List<IC_EmployeeDTO> GetUserMasterFromIntegrate(List<IC_EmployeeDTO> listEmployee, int companyIndex)
        {
            if (listEmployee != null && listEmployee.Count > 0)
            {
                var listEmployeeATIDResult = listEmployee.Select(e => e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                List<IC_UserMaster> listUser = _dbContext.IC_UserMaster.Where(t => t.CompanyIndex == companyIndex && listEmployeeATIDResult.Contains(t.EmployeeATID)).ToList();

                for (int i = 0; i < listEmployee.Count; i++)
                {
                    if (listUser != null)
                    {
                        IC_UserMaster userMasterInfo = listUser.FirstOrDefault(t => t.EmployeeATID == listEmployee[i].EmployeeATID);
                        if (userMasterInfo != null)
                        {
                            listEmployee[i].CardNumber = !string.IsNullOrEmpty(listEmployee[i].CardNumber) ? listEmployee[i].CardNumber : userMasterInfo.CardNumber;
                            listEmployee[i].Privilege = int.Parse(userMasterInfo.Privilege.ToString());
                            listEmployee[i].Password = userMasterInfo.Password;
                            listEmployee[i].FaceTemplate = !string.IsNullOrEmpty(userMasterInfo.FaceTemplate) ? userMasterInfo.FaceTemplate.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_Content) ? userMasterInfo.FaceV2_Content.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_TemplateBIODATA) ? userMasterInfo.FaceV2_TemplateBIODATA.Length : 0;
                            listEmployee[i].Finger1 = string.IsNullOrEmpty(userMasterInfo.FingerData0) ? 0 : userMasterInfo.FingerData0.Length;
                            listEmployee[i].Finger2 = string.IsNullOrEmpty(userMasterInfo.FingerData1) ? 0 : userMasterInfo.FingerData1.Length;
                            listEmployee[i].Finger3 = string.IsNullOrEmpty(userMasterInfo.FingerData2) ? 0 : userMasterInfo.FingerData2.Length;
                            listEmployee[i].Finger4 = string.IsNullOrEmpty(userMasterInfo.FingerData3) ? 0 : userMasterInfo.FingerData3.Length;
                            listEmployee[i].Finger5 = string.IsNullOrEmpty(userMasterInfo.FingerData4) ? 0 : userMasterInfo.FingerData4.Length;
                            listEmployee[i].Finger6 = string.IsNullOrEmpty(userMasterInfo.FingerData5) ? 0 : userMasterInfo.FingerData5.Length;
                            listEmployee[i].Finger7 = string.IsNullOrEmpty(userMasterInfo.FingerData6) ? 0 : userMasterInfo.FingerData6.Length;
                            listEmployee[i].Finger8 = string.IsNullOrEmpty(userMasterInfo.FingerData7) ? 0 : userMasterInfo.FingerData7.Length;
                            listEmployee[i].Finger9 = string.IsNullOrEmpty(userMasterInfo.FingerData8) ? 0 : userMasterInfo.FingerData8.Length;
                            listEmployee[i].Finger10 = string.IsNullOrEmpty(userMasterInfo.FingerData9) ? 0 : userMasterInfo.FingerData9.Length;
                            listEmployee[i].PrivilegeName = GetPrivilegeName(int.Parse(userMasterInfo.Privilege.ToString()));
                        }
                        else
                        {
                            listEmployee[i].CardNumber = !string.IsNullOrEmpty(listEmployee[i].CardNumber) ? listEmployee[i].CardNumber : "0";
                            listEmployee[i].SerialNumber = !string.IsNullOrEmpty(listEmployee[i].SerialNumber) ? listEmployee[i].SerialNumber : "";

                        }
                    }
                }
            }
            return listEmployee;
        }

        public List<IC_EmployeeDTO> GetUserMasterMachineInfoCompare(List<string> listEmployeeATID, string filter, UserInfo user)
        {
            List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeATID });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            List<IC_EmployeeDTO> listEmployee = GetMany(addedParams);

            if (listEmployee != null)
            {
                var listEmployeeATIDResult = listEmployee.Select(e => e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                List<IC_UserMaster> listUser = _dbContext.IC_UserMaster.Where(t => t.CompanyIndex == user.CompanyIndex && listEmployeeATIDResult.Contains(t.EmployeeATID)).ToList();

                int index = 0;
                for (int i = 0; i < listEmployee.Count; i++)
                {
                    IC_EmployeeDTO data = null;
                    if (listUser != null)
                    {
                        IC_UserMaster userMasterInfo = listUser.FirstOrDefault(t => t.EmployeeATID == listEmployee[i].EmployeeATID);
                        if (userMasterInfo != null)
                        {
                            data = new IC_EmployeeDTO();
                            data.Index = index + 1;
                            CreateBasicInfo(ref data, listEmployee[i]);

                            data.CardNumber = userMasterInfo.CardNumber;
                            data.Privilege = int.Parse(userMasterInfo.Privilege.ToString());
                            data.Password = userMasterInfo.Password;
                            data.FaceTemplate = !string.IsNullOrEmpty(userMasterInfo.FaceTemplate) ? userMasterInfo.FaceTemplate.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_Content) ? userMasterInfo.FaceV2_Content.Length : !string.IsNullOrEmpty(userMasterInfo.FaceV2_TemplateBIODATA) ? userMasterInfo.FaceV2_TemplateBIODATA.Length : 0;
                            data.Finger1 = string.IsNullOrEmpty(userMasterInfo.FingerData0) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger2 = string.IsNullOrEmpty(userMasterInfo.FingerData1) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger3 = string.IsNullOrEmpty(userMasterInfo.FingerData2) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger4 = string.IsNullOrEmpty(userMasterInfo.FingerData3) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger5 = string.IsNullOrEmpty(userMasterInfo.FingerData4) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger6 = string.IsNullOrEmpty(userMasterInfo.FingerData5) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger7 = string.IsNullOrEmpty(userMasterInfo.FingerData6) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger8 = string.IsNullOrEmpty(userMasterInfo.FingerData7) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger9 = string.IsNullOrEmpty(userMasterInfo.FingerData8) ? 0 : userMasterInfo.FingerData0.Length;
                            data.Finger10 = string.IsNullOrEmpty(userMasterInfo.FingerData9) ? 0 : userMasterInfo.FingerData0.Length;
                            listData.Add(data);
                            index++;
                        }
                        else
                        {
                            data = new IC_EmployeeDTO();
                            data.Index = index;

                            CreateBasicInfo(ref data, listEmployee[i]);
                            data.CardNumber = "0";
                            data.SerialNumber = "";
                            listData.Add(data);
                            index++;
                        }
                    }
                }
            }
            return listData;
        }

        public List<IC_EmployeeDTO> GetUserMachineInfo(List<string> listEmployeeATID, List<string> listDevice, string filter, UserInfo user)
        {
            List<IC_EmployeeDTO> listData = new List<IC_EmployeeDTO>();
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "Filter", Value = filter });
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
            addedParams.Add(new AddedParam { Key = "ListDepartment", Value = user.ListDepartmentAssigned });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = listEmployeeATID });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            List<IC_EmployeeDTO> listEmployee = GetMany(addedParams);

            if (listEmployee != null)
            {
                var listEmployeeATIDResult = listEmployee.Select(e => e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0')).ToList();
                List<IC_UserInfo> listUser = _dbContext.IC_UserInfo.Where(t => t.CompanyIndex == user.CompanyIndex && listEmployeeATIDResult.Contains(t.EmployeeATID) && listDevice.Contains(t.SerialNumber)).ToList();

                int index = 0;
                var listResultDevice = _dbContext.IC_Device.Where(x => x.CompanyIndex == user.CompanyIndex).ToList();
                for (int i = 0; i < listEmployee.Count; i++)
                {
                    IC_EmployeeDTO data = null;
                    List<IC_UserInfo> listUserByEmployeeATID = listUser.Where(t => t.EmployeeATID == listEmployee[i].EmployeeATID).ToList();
                    if (listUserByEmployeeATID != null && listUserByEmployeeATID.Count > 0)
                    {
                        foreach (IC_UserInfo item in listUserByEmployeeATID)
                        {
                            data = new IC_EmployeeDTO();
                            data.Index = index + 1;

                            CreateBasicInfo(ref data, listEmployee[i]);

                            data.CardNumber = item.CardNumber;
                            data.Privilege = int.Parse(item.Privilege.ToString());
                            data.Password = item.Password;
                            data.FaceTemplate = item.FaceTemplate.HasValue ? item.FaceTemplate.Value != 0 ? item.FaceTemplate : item.FaceTemplateV2 : item.FaceTemplateV2;
                            data.Finger1 = item.FingerData0.HasValue ? item.FingerData0 : 0;
                            data.Finger2 = item.FingerData1.HasValue ? item.FingerData1 : 0;
                            data.Finger3 = item.FingerData2.HasValue ? item.FingerData2 : 0;
                            data.Finger4 = item.FingerData3.HasValue ? item.FingerData3 : 0;
                            data.Finger5 = item.FingerData4.HasValue ? item.FingerData4 : 0;
                            data.Finger6 = item.FingerData5.HasValue ? item.FingerData5 : 0;
                            data.Finger7 = item.FingerData6.HasValue ? item.FingerData6 : 0;
                            data.Finger8 = item.FingerData7.HasValue ? item.FingerData7 : 0;
                            data.Finger9 = item.FingerData8.HasValue ? item.FingerData8 : 0;
                            data.Finger10 = item.FingerData9.HasValue ? item.FingerData9 : 0;
                            data.DepartmentName = item.DepartmentName;
                            data.PrivilegeName = GetPrivilegeName(item.Privilege ?? 0);
                            data.NameOnMachine = item.UserName;
                            data.SerialNumber = item.SerialNumber;
                            data.IPAddress = listResultDevice.FirstOrDefault(x => x.SerialNumber == item.SerialNumber)?.IPAddress ?? string.Empty;
                            listData.Add(data);
                            index++;
                        }
                    }
                    else
                    {
                        data = new IC_EmployeeDTO();
                        data.Index = index;

                        CreateBasicInfo(ref data, listEmployee[i]);
                        listData.Add(data);
                        index++;
                    }
                }
            }
            return listData;
        }

        public List<HR_EmployeeReport> GetAllEmployeeReport()
        {
            string query =
                "SELECT HR_Employee.EmployeeATID, HR_Employee.CompanyIndex, HR_Employee.EmployeeCode, HR_Employee.CardNumber,HR_Employee.LastName + ' ' + HR_Employee.MidName + ' ' + HR_Employee.FirstName AS FullName," +
                " HR_Employee.NickName, HR_Employee.Gender, '' as NameOnMachine,HR_WorkingInfo.DepartmentIndex as DepartmentIndex,dep.[Name] as DepartmentName, HR_Employee.JoinedDate " +
                " FROM HR_Employee" +
                " LEFT JOIN HR_WorkingInfo ON HR_WorkingInfo.EmployeeATID = HR_Employee.EmployeeATID" +
                " and ((HR_WorkingInfo.ToDate is null and  Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0)" +
                " OR(Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))" +
                " LEFT OUTER JOIN HR_Department ON HR_Department.[Index] = HR_WorkingInfo.DepartmentIndex" +
                " LEFT OUTER JOIN HR_Position ON HR_WorkingInfo.PositionIndex = HR_Position.[Index]" +
                " LEFT OUTER JOIN HR_Titles ON HR_WorkingInfo.TitlesIndex = HR_Titles.[Index]" +
                " and(HR_WorkingInfo.ToDate is null OR" +
                " (Datediff(day, HR_WorkingInfo.ToDate, getdate()) <= 0 AND Datediff(day, HR_WorkingInfo.FromDate, getdate()) >= 0))" +
                " left join HR_Department dep on dep.[Index]= HR_WorkingInfo.DepartmentIndex " +
                " WHERE (HR_Employee.MarkForDelete = 0) AND HR_Employee.CompanyIndex = " + _config.CompanyIndex;
            List<HR_EmployeeReport> listEmployee;
            listEmployee = _dbIntergrateContext.HR_EmployeeReport.FromSqlRaw(query).ToList();

            List<AddedParam> addedParams1 = new List<AddedParam>();
            addedParams1.Add(new AddedParam { Key = "CompanyIndex", Value = _config.CompanyIndex });
            addedParams1.Add(new AddedParam { Key = "ToDay", Value = DateTime.Now });
            var listStoppedWorking = GetManyStoppedWorking(addedParams1);
            var listEmployeeATIDStopped = new List<string>();
            if (listStoppedWorking != null)
            {
                listEmployeeATIDStopped = listStoppedWorking.Select(e => e.EmployeeATID).ToList();
            }
            if (listEmployeeATIDStopped != null && listEmployeeATIDStopped.Count > 0)
            {
                listEmployee = listEmployee.Where(e => !listEmployeeATIDStopped.Contains(e.EmployeeATID)).ToList();

            }
            foreach (var item in listEmployee)
            {
                item.EmployeeATID = item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0');
            }
            return listEmployee;
        }

        private void CreateBasicInfo(ref IC_EmployeeDTO data, IC_EmployeeDTO pEmployee)
        {
            data.EmployeeATID = pEmployee.EmployeeATID;
            data.EmployeeCode = pEmployee.EmployeeCode;
            data.FullName = pEmployee.FullName;
            data.DepartmentName = pEmployee.DepartmentName;
            data.CardNumber = pEmployee.CardNumber;
            data.SerialNumber = pEmployee.SerialNumber;
        }

        private void CreateListFingerData(List<IC_UserFinger> listFingerByEmp, ref IC_EmployeeDTO data)
        {
            for (int i = 0; i < listFingerByEmp.Count; i++)
            {
                if (listFingerByEmp[i].FingerData == null)
                    continue;

                switch (listFingerByEmp[i].FingerIndex)
                {
                    case 0:
                        data.Finger1 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 1:
                        data.Finger2 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 2:
                        data.Finger3 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 3:
                        data.Finger4 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 4:
                        data.Finger5 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 5:
                        data.Finger6 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 6:
                        data.Finger7 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 7:
                        data.Finger8 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 8:
                        data.Finger9 = listFingerByEmp[i].FingerData.Length;
                        break;
                    case 9:
                        data.Finger10 = listFingerByEmp[i].FingerData.Length;
                        break;
                }

            }
        }

    }


    public interface IHR_EmployeeLogic
    {
        List<IC_EmployeeDTO> GetAll();
        List<IC_EmployeeDTO> GetUserMasterMachineInfo(List<string> listEmployeeATID, string filter, UserInfo user);
        List<IC_EmployeeDTO> GetUserMasterMachineInfoCompare(List<string> listEmployeeATID, string filter, UserInfo user);
        List<IC_EmployeeDTO> GetUserMachineInfo(List<string> listEmployeeATID, List<string> listDevice, string filter, UserInfo user);
        List<IC_EmployeeDTO> GetMany(List<AddedParam> addedParams);
        ListDTOModel<IC_EmployeeDTO> GetPage(List<AddedParam> addedParams);
        Task<List<EmployeeFullInfo>> GetEmployeeCompactInfo(int pCompanyIndex);
        Task<HR_Employee> GetByEmployeeATIDAndCompanyIndex(string empATID, int companyIndex);
        List<IC_EmployeeDTO> GetManyStoppedWorking(List<AddedParam> addedParams);
        List<HR_EmployeeReport> GetAllEmployeeReport();
        Task IntegrateBlackListToOffline(List<string> employeeATIDs, List<string> nricLst, string api);
        Task IntegrateCustomerCardToOffline(List<string> employeeATIDs, string api);
        Task IntegrateCardToOffline(List<string> employeeATIDs, string api);
        Task IntegrateDepartmentToOffline(List<long> indexes, string api);
        Task IntegrateUserToOffline(List<string> employeeATIDs, string api);
        Task IntegrateWorkingInfo(List<string> employeeATIDs, string api);
        Task IntegrateEmployeeToOffline(List<string> employeeATIDs, string api);
        Task IntegrateCustmerToOffline(List<string> employeeATIDs, string api);
        Task DeleteBlackList(GC_BlackListIntegrate blackList, string api);
        Task DeleteCustomerCardToOffline(List<string> employeeATIDs, string api);
        Task DeleteCardToOffline(List<HR_CardNumberInfo> employeeATIDs, string api);
        Task DeleteDepartmentToOffline(List<string> codes);
        Task DeleteUserToOffline(List<string> codes);
        Task IntegrateUserToOfflineEmployee(List<string> employeeATIDs);
        Task IntegrateUserToOfflineCustomer(List<string> employeeATIDs);
    }
}
