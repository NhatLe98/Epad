
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Logic;
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
    public class GC_BlackListService : BaseServices<GC_BlackList, EPAD_Context>, IGC_BlackListService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;

        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        private readonly IIC_ConfigLogic _iC_ConfigLogic;
        private readonly IIC_CommandLogic _iC_CommandLogic;
        public GC_BlackListService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();

            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_CustomerInfoService = serviceProvider.GetService<IHR_CustomerInfoService>();
            _IHR_EmployeeLogic = serviceProvider.GetService<IHR_EmployeeLogic>();
            _iC_ConfigLogic = serviceProvider.GetService<IIC_ConfigLogic>();
            _iC_CommandLogic = serviceProvider.GetService<IIC_CommandLogic>();
        }

        public async Task<DataGridClass> GetByFilter(DateTime fromDate, DateTime? toDate,
            string filter, int page, int pageSize, int pCompanyIndex)
        {
            var blackList = DbContext.GC_BlackList.Where(x => x.FromDate.Date >= fromDate.Date);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterList = filter.Split(' ').ToList();
                if (filterList != null && filterList.Count > 0)
                {
                    blackList = blackList.Where(x => filterList.Contains(x.EmployeeATID) || filterList.Contains(x.Nric));
                }
            }

            if (toDate.HasValue)
            {
                blackList = blackList.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date <= toDate.Value.Date));
            }

            if (page < 1) page = 1;
            var totalCount = blackList.Count();
            var result = await blackList.OrderBy(x => x.Index).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(result.Select(x => x.EmployeeATID).ToList(), DateTime.Now, pCompanyIndex);
            var listData = new List<BlackListParams>();
            foreach (var black in blackList)
            {
                var employeeInfo = employeeInfoList.Where(x => x.EmployeeATID == black.EmployeeATID).FirstOrDefault();
                var data = new BlackListParams();
                data.Index = (int)black.Index;
                data.EmployeeATID = black.EmployeeATID;
                data.IsEmployeeSystem = black.IsEmployeeSystem;
                data.Nric = black.Nric;
                data.FromDate = black.FromDate;
                data.FromDateString = black.FromDate.ToddMMyyyy();
                data.ToDate = black?.ToDate;
                data.ToDateString = black?.ToDate != null ? black?.ToDate.Value.ToddMMyyyy() : null;
                data.Reason = black?.Reason;
                data.ReasonRemove = black?.ReasonRemove;
                if (!data.IsEmployeeSystem)
                {
                    data.FullName = employeeInfo?.FullName;
                }
                else
                {
                    data.FullName = black.FullName;
                }
                listData.Add(data);
            }

            var gridClass = new DataGridClass(totalCount, listData);
            return gridClass;
        }


        public bool GetByFilterAndCompanyIndexExcludeThis(bool isEmployeeSystem, string pEmployeeATID, string nRIC, DateTime pFromDate, DateTime? pToDate, long pIndex, int pCompanyIndex)
        {
            var listExist = false;
            if (!isEmployeeSystem)
            {
                listExist = _dbContext.GC_BlackList
                .Any(x => x.CompanyIndex == pCompanyIndex
                && x.Index != pIndex
                && pEmployeeATID == x.EmployeeATID
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date)))));
            }
            else
            {
                listExist = _dbContext.GC_BlackList
                .Any(x => x.CompanyIndex == pCompanyIndex
                && x.Index != pIndex
                && x.Nric == nRIC
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date)))));
            }
            return listExist;
        }

        public List<GC_BlackList> CheckExistBlackList(bool isEmployeeSystem, string pEmployeeATID, string nRIC, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex)
        {
            var listExist = new List<GC_BlackList>();
            if (!isEmployeeSystem)
            {
                listExist = _dbContext.GC_BlackList
                .Where(x => x.CompanyIndex == pCompanyIndex
                && pEmployeeATID == x.EmployeeATID
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date))))).ToList();
            }
            else
            {
                listExist = _dbContext.GC_BlackList
                .Where(x => x.CompanyIndex == pCompanyIndex
                && x.Nric == nRIC
                && ((x.ToDate.HasValue && ((pToDate.HasValue && ((pToDate.Value.Date >= x.FromDate.Date && pFromDate.Date <= x.FromDate.Date)
                    || (pFromDate.Date >= x.FromDate.Date && pToDate.Value.Date <= x.ToDate.Value.Date)
                    || (pFromDate.Date <= x.ToDate.Value.Date && pToDate.Value.Date >= x.ToDate.Value.Date)))
                            || (!pToDate.HasValue && (pFromDate.Date <= x.FromDate.Date
                            || (pFromDate.Date >= x.FromDate.Date && pFromDate.Date <= x.ToDate.Value.Date)))))
                        || (!x.ToDate.HasValue && (!pToDate.HasValue
                            || (pToDate.HasValue && pToDate.Value.Date >= x.FromDate.Date))))).ToList();
            }
            return listExist;
        }

        public async Task<GC_BlackList> GetDataByIndex(long pIndex)
        {
            return await _dbContext.GC_BlackList.FirstOrDefaultAsync(x => x.Index == pIndex);
        }

        public async Task<GC_BlackList> AddBlackList(BlackListParams param, UserInfo user)
        {
            var blackList = new GC_BlackList
            {
                IsEmployeeSystem = param.IsEmployeeSystem,
                EmployeeATID = param.EmployeeATID,
                FullName = param.FullName,
                Nric = param.Nric,
                FromDate = param.FromDate,
                ToDate = param.ToDate,
                Reason = param.Reason,
                CompanyIndex = user.CompanyIndex,
                UpdatedDate = DateTime.Now,
                UpdatedUser = user.UserName
            };
            try
            {

                await _dbContext.GC_BlackList.AddAsync(blackList);
                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(param.EmployeeATID))
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(new List<string> { param.EmployeeATID }, null, null);

                }
                else
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(null, new List<string> { param.Nric }, null);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("AddBlackList: " + ex);
                return null;
            }
            return blackList;
        }


        public async Task DeleteBlackListByCreateCommand(List<long> indexes, UserInfo user)
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.AUTO_DELETE_BLACKLIST.ToString() });
            var lstSerials = new List<string>();
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null && downloadConfig.Any())
            {
                var config = downloadConfig.First();
                List<CommandResult> lstCmd = new List<CommandResult>();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.ListSerialNumber))
                {
                    var lstGroupDevice = config.IntegrateLogParam.ListSerialNumber.Split(';').ToList();
                    if (lstGroupDevice.Count == 0)
                    {
                        return;
                    }

                    var groupDevice = lstGroupDevice.Select(int.Parse).ToList();
                    lstSerials = await _dbContext.IC_GroupDeviceDetails.Where(x => groupDevice.Contains(x.GroupDeviceIndex)).Select(x => x.SerialNumber).ToListAsync();
                    if (lstSerials == null || lstSerials.Count == 0)
                    {
                        return;
                    }
                }
            }

            var now = DateTime.Now.Date;
            var blacklist = await _dbContext.GC_BlackList.Where(x => indexes.Contains(x.Index)).ToListAsync();

            var notValid = blacklist.ToList();
            if (notValid.Count > 0)
            {
                var employee = notValid.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                var nric = notValid.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                var lstBlacklistUpload = new List<string>();
                if (nric != null && nric.Count > 0)
                {
                    var employeeLst = await (from e in _dbContext.HR_User
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Employee || e.EmployeeType == null)
                                             select e.EmployeeATID).ToListAsync();

                    if (employeeLst != null && employeeLst.Count > 0)
                    {
                        lstBlacklistUpload.AddRange(employeeLst);
                    }

                    var customerLst = await (from e in _dbContext.HR_User
                                             join w in _dbContext.HR_CustomerInfo
                                             on e.EmployeeATID equals w.EmployeeATID
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Guest)
                                             select e.EmployeeATID).ToListAsync();
                    if (customerLst != null && customerLst.Count > 0)
                    {
                        lstBlacklistUpload.AddRange(customerLst);
                    }
                }

                if (employee != null && employee.Count > 0)
                {
                    lstBlacklistUpload.AddRange(employee);
                }


                var isMondelez = ClientName.MONDELEZ.ToString() == _configClientName;
                if (lstBlacklistUpload.Count > 0)
                {

                    var listWorkingInfoAll = await _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex
                              && x.FromDate.Date <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate > DateTime.Now.Date)
                               && (!isMondelez || (x.DepartmentIndex != 0))
                              && lstBlacklistUpload.Contains(x.EmployeeATID) && x.Status == (long)TransferStatus.Approve).OrderBy(t => t.FromDate).ToListAsync();

                    var customerLst = (from e in listWorkingInfoAll
                                       join w in _dbContext.IC_DepartmentAndDevice
                                       on e.DepartmentIndex equals w.DepartmentIndex
                                       where lstSerials.Contains(w.SerialNumber) && w.SerialNumber != null
                                       select new
                                       {
                                           EmployeeATID = e.EmployeeATID,
                                           SerialNumber = w.SerialNumber
                                       }).ToList();

                    var customerLstUp = customerLst.GroupBy(x => x.SerialNumber).Select(x => new { Key = x.Key, Value = x }).ToList();

                    foreach (var item in customerLstUp)
                    {
                        var listUser = new List<UserInfoOnMachine>();

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = item.Value.Select(x => x.EmployeeATID).ToList();
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.ListSerialNumber = new List<string> { item.Key };
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.FullInfo = true;
                        listUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);


                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.UploadUsers;
                        commandParam.CommandName = StringHelper.GetCommandType((short)EmployeeType.Employee);
                        commandParam.AuthenMode = null;
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = listUser;
                        commandParam.ListSerialNumber = new List<string> { item.Key };
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = null;

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.UploadUsers.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        }
                    }

                }

            }
        }

        public async Task<GC_BlackList> UpdateBlackList(BlackListParams param, UserInfo user)
        {
            var parkingLotAccessed = await _dbContext.GC_BlackList.FirstOrDefaultAsync(x => x.Index == param.Index);
            parkingLotAccessed.FromDate = param.FromDate;
            parkingLotAccessed.ToDate = param.ToDate;
            parkingLotAccessed.Reason = param.Reason;
            parkingLotAccessed.CompanyIndex = user.CompanyIndex;
            parkingLotAccessed.UpdatedDate = DateTime.Now;
            parkingLotAccessed.UpdatedUser = user.UserName;
            try
            {

                await _dbContext.SaveChangesAsync();
                if (!string.IsNullOrEmpty(param.EmployeeATID))
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(new List<string> { parkingLotAccessed.EmployeeATID }, null, null);

                }
                else
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(null, new List<string> { parkingLotAccessed.Nric }, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateBlackList: " + ex);

            }
            return parkingLotAccessed;
        }

        public async Task<bool> DeleteBlackList(List<long> indexes)
        {
            var result = true;
            try
            {
                var blackList = await _dbContext.GC_BlackList.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (blackList != null && blackList.Count > 0)
                {
                    var empl = blackList.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                    var nric = blackList.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.Nric).ToList();
                    _dbContext.GC_BlackList.RemoveRange(blackList);
                    await _dbContext.SaveChangesAsync();

                    var param = new GC_BlackListIntegrate()
                    {
                        EmployeeATIDs = empl,
                        NRICs = nric,
                    };
                    await _IHR_EmployeeLogic.DeleteBlackList(param, null);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteBlackList: " + ex);
                result = false;
            }
            return result;
        }

        public async Task<List<BlackListParams>> ImportBlackList(List<BlackListParams> param, UserInfo user)
        {
            var result = param;
            try
            {
                var employeeATIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var employees = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);

                string[] formats = { "dd/MM/yyyy" };

                long i = 0;
                foreach (var itemImport in result)
                {
                    ++i;
                    itemImport.RowIndex = i;
                    if (string.IsNullOrWhiteSpace(itemImport.FromDateString))
                    {
                        itemImport.ErrorMessage += "Từ ngày không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(itemImport.FromDateString))
                    {
                        var fromDate = new DateTime();
                        var convertFromDate = DateTime.TryParseExact(itemImport.FromDateString, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fromDate);
                        if (!convertFromDate)
                        {
                            itemImport.ErrorMessage += "Từ ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            itemImport.FromDate = fromDate;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(itemImport.ToDateString))
                    {
                        var fromDate = new DateTime();
                        var convertFromDate = DateTime.TryParseExact(itemImport.ToDateString, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fromDate);
                        if (!convertFromDate)
                        {
                            itemImport.ErrorMessage += "Đến ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            itemImport.ToDate = fromDate;

                            if (itemImport.FromDate > itemImport.ToDate)
                            {
                                itemImport.ErrorMessage += "Từ ngày không thể lớn hơn đến ngày\r\n";
                            }
                        }

                       
                    }

                    //-- 
                    if (string.IsNullOrWhiteSpace(itemImport.Nric) && string.IsNullOrWhiteSpace(itemImport.EmployeeATID))
                    {
                        itemImport.ErrorMessage += "Bắt buộc phải nhập mã người dùng hoặc CMND/CCCD/Passport\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(itemImport.EmployeeATID))
                    {
                        var employee = employees.FirstOrDefault(y => y.EmployeeATID == itemImport.EmployeeATID);
                        if (employee != null)
                        {
                            itemImport.FullName = employee.FullName;
                        }
                        else
                        {
                            itemImport.ErrorMessage += "Nhân viên không tồn tại\r\n";
                        }
                    }
                }

                var listEmpIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var existedBlackList = await _dbContext.GC_BlackList.Where(x => listEmpIDs.Contains(x.EmployeeATID)).ToListAsync();

                var allEmployee = await _HR_CustomerInfoService.GetAllCustomerInfo(new string[0], user.CompanyIndex);

                var noErrorParam = result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
                var indexUpdate = new List<GC_BlackList>();
                if (noErrorParam != null && noErrorParam.Count > 0)
                {
                    var logExistInFile = new List<BlackListParams>();
                    foreach (var item in noErrorParam)
                    {
                        var employeeExistedBlackList = existedBlackList.Where(y => (!string.IsNullOrWhiteSpace(item.EmployeeATID)
                                                                        && y.EmployeeATID == item.EmployeeATID)
                                                                        || ((string.IsNullOrWhiteSpace(item.EmployeeATID) && !string.IsNullOrWhiteSpace(item.Nric)) && y.Nric == item.Nric)).ToList();

                        if (logExistInFile.Any(y => y.RowIndex != item.RowIndex
                        && (!string.IsNullOrWhiteSpace(item.EmployeeATID)
                        && y.EmployeeATID == item.EmployeeATID)
                        || ((string.IsNullOrWhiteSpace(item.EmployeeATID) && !string.IsNullOrWhiteSpace(item.Nric)) && y.Nric == item.Nric)

                        && ((y.ToDate.HasValue && ((item.ToDate.HasValue && ((item.ToDate.Value.Date >= y.FromDate.Date
                        && item.FromDate.Date <= y.FromDate.Date)
                        || (item.FromDate.Date >= y.FromDate.Date && item.ToDate.Value.Date <= y.ToDate.Value.Date)
                        || (item.FromDate.Date <= y.ToDate.Value.Date && item.ToDate.Value.Date >= y.ToDate.Value.Date)))
                                || (!item.ToDate.HasValue && (item.FromDate.Date <= item.FromDate.Date
                                || (item.FromDate.Date >= y.FromDate.Date && item.FromDate.Date <= y.ToDate.Value.Date)))))
                            || (!y.ToDate.HasValue && (!item.ToDate.HasValue
                                || (item.ToDate.HasValue && item.ToDate.Value.Date >= y.FromDate.Date))))))
                        {
                            item.ErrorMessage += "Dữ liệu đăng ký bị trùng trong tập tin\r\n";
                            continue;
                        }


                        if (employeeExistedBlackList != null && employeeExistedBlackList.Count > 0)
                        {
                            var checkExistInTime = employeeExistedBlackList.FirstOrDefault(y => ((y.ToDate.HasValue && ((item.ToDate.HasValue && ((item.ToDate.Value.Date >= y.FromDate.Date
                            && item.FromDate.Date <= y.FromDate.Date)
                        || (item.FromDate.Date >= y.FromDate.Date && item.ToDate.Value.Date <= y.ToDate.Value.Date)
                        || (item.FromDate.Date <= y.ToDate.Value.Date && item.ToDate.Value.Date >= y.ToDate.Value.Date)))
                                || (!item.ToDate.HasValue && (item.FromDate.Date <= item.FromDate.Date
                                || (item.FromDate.Date >= y.FromDate.Date && item.FromDate.Date <= y.ToDate.Value.Date)))))
                                || (!y.ToDate.HasValue && (!item.ToDate.HasValue
                                || (item.ToDate.HasValue && item.ToDate.Value.Date >= y.FromDate.Date)))));

                            if (checkExistInTime != null)
                            {
                                item.ErrorMessage += "Không được khai báo người dùng trong danh sách đen cùng khoảng thời gian\r\n";
                            }
                            else
                            {
                                checkExistInTime = new GC_BlackList
                                {
                                    EmployeeATID = item.EmployeeATID,
                                    FullName = item.FullName,
                                    Nric = !string.IsNullOrWhiteSpace(item.EmployeeATID) ? employees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID)?.NRIC : item.Nric,
                                    FromDate = item.FromDate,
                                    Reason = item.Reason,
                                    ToDate = item.ToDate,
                                    CompanyIndex = user.CompanyIndex,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedUser = user.UserName
                                };
                                _dbContext.GC_BlackList.Add(checkExistInTime);
                                indexUpdate.Add(checkExistInTime);
                            }
                        }
                        else
                        {
                            var blackList = new GC_BlackList
                            {
                                EmployeeATID = item.EmployeeATID,
                                FullName = item.FullName,
                                Nric = !string.IsNullOrWhiteSpace(item.EmployeeATID) ? employees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID)?.NRIC : item.Nric,
                                FromDate = item.FromDate,
                                Reason = item.Reason,
                                ToDate = item.ToDate,
                                CompanyIndex = user.CompanyIndex,
                                UpdatedDate = DateTime.Now,
                                UpdatedUser = user.UserName
                            };
                            _dbContext.GC_BlackList.Add(blackList);
                            indexUpdate.Add(blackList);
                        }
                        logExistInFile.Add(item);
                    }
                    await _dbContext.SaveChangesAsync();
                    await CreateCommandBlacklist(indexUpdate.Select(x => x.Index).ToList(), user);
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(null, null, null);

                }

                var errorList = result.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;


                //}
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLotAccessed: " + ex);
            }
            return result;
        }

        public async Task<bool> RemoveEmployeeInBlackList(RemoveEmployeeInBlackListParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var blackList = await _dbContext.GC_BlackList.FirstOrDefaultAsync(x => x.Index == param.Index);
                blackList.ToDate = param.ToDate;
                blackList.ReasonRemove = param.ReasonRemoveBlackList;
                blackList.CompanyIndex = user.CompanyIndex;
                blackList.UpdatedDate = DateTime.Now;
                blackList.UpdatedUser = user.UserName;
                await _dbContext.SaveChangesAsync();
                if (!string.IsNullOrEmpty(blackList.EmployeeATID))
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(new List<string> { blackList.EmployeeATID }, null, null);

                }
                else
                {
                    await _IHR_EmployeeLogic.IntegrateBlackListToOffline(null, new List<string> { blackList.Nric }, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RemoveEmployeeInBlackList: " + ex);
                result = false;
            }
            return result;
        }

        public async Task CreateCommandBlacklist(List<long> indexs, UserInfo user)
        {
            string timePostCheck = DateTime.Now.ToHHmm();
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.AUTO_DELETE_BLACKLIST.ToString() });
            var lstSerials = new List<string>();
            var downloadConfig = await _iC_ConfigLogic.GetMany(addedParams);
            if (downloadConfig != null && downloadConfig.Any())
            {
                var config = downloadConfig.First();
                List<CommandResult> lstCmd = new List<CommandResult>();
                if (config != null && !string.IsNullOrEmpty(config.TimePos) && !string.IsNullOrEmpty(config.IntegrateLogParam.ListSerialNumber))
                {
                    var lstGroupDevice = config.IntegrateLogParam.ListSerialNumber.Split(';').ToList();
                    if (lstGroupDevice.Count == 0)
                    {
                        return;
                    }

                    var groupDevice = lstGroupDevice.Select(int.Parse).ToList();
                    lstSerials = await _dbContext.IC_GroupDeviceDetails.Where(x => groupDevice.Contains(x.GroupDeviceIndex)).Select(x => x.SerialNumber).ToListAsync();
                    if (lstSerials == null || lstSerials.Count == 0)
                    {
                        return;
                    }
                }
            }
            var now = DateTime.Now.Date;
            var blacklist = await _dbContext.GC_BlackList.Where(x => indexs.Contains(x.Index)).ToListAsync();
            var valid = blacklist.Where(x => x.FromDate.Date <= now && (x.ToDate == null || x.ToDate.Value.Date >= now)).ToList();

            if (valid.Count > 0)
            {
                var employee = valid.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                var nric = valid.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.Nric).ToList();
                var lstBlacklistDelete = new List<string>();
                if (nric != null && nric.Count > 0)
                {
                    var employeeLst = await (from e in _dbContext.HR_User
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Employee || e.EmployeeType == null)
                                             select e.EmployeeATID).ToListAsync();

                    if (employeeLst != null && employeeLst.Count > 0)
                    {
                        lstBlacklistDelete.AddRange(employeeLst);
                    }

                    var customerLst = await (from e in _dbContext.HR_User
                                             join w in _dbContext.HR_CustomerInfo
                                             on e.EmployeeATID equals w.EmployeeATID
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Guest)
                                             select e.EmployeeATID).ToListAsync();
                    if (customerLst != null && customerLst.Count > 0)
                    {
                        lstBlacklistDelete.AddRange(customerLst);
                    }
                }
                if (employee != null && employee.Count > 0)
                {
                    lstBlacklistDelete.AddRange(employee);
                }

                if (lstBlacklistDelete.Count > 0)
                {
                    var lstUser = lstBlacklistDelete.Select(x => new UserInfoOnMachine
                    {
                        EmployeeATID = x,
                        UserID = x
                    }).ToList();

                    IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                    commandParam.IsOverwriteData = false;
                    commandParam.Action = CommandAction.DeleteUserById;
                    commandParam.AuthenMode = null;
                    commandParam.FromTime = new DateTime(2000, 1, 1);
                    commandParam.ToTime = DateTime.Now;
                    commandParam.ListEmployee = lstUser;
                    commandParam.ListSerialNumber = lstSerials;
                    commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                    commandParam.ExternalData = null;

                    List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                    if (lstCmd != null && lstCmd.Count() > 0)
                    {
                        IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                        groupCommand.CompanyIndex = user.CompanyIndex;
                        groupCommand.UserName = user.UserName;
                        groupCommand.ListCommand = lstCmd;
                        groupCommand.GroupName = GroupName.DeleteUserById.ToString();
                        groupCommand.EventType = "";
                        _iC_CommandLogic.CreateGroupCommands(groupCommand);
                    }
                }
            }


            var notValid = blacklist.Where(x => x.ToDate != null && x.ToDate < now).ToList();
            if (notValid.Count > 0)
            {
                var employee = notValid.Where(x => !string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.EmployeeATID).ToList();
                var nric = notValid.Where(x => string.IsNullOrEmpty(x.EmployeeATID)).Select(x => x.Nric).ToList();
                var lstBlacklistUpload = new List<string>();
                if (nric != null && nric.Count > 0)
                {
                    var employeeLst = await (from e in _dbContext.HR_User
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Employee || e.EmployeeType == null)
                                             select e.EmployeeATID).ToListAsync();

                    if (employeeLst != null && employeeLst.Count > 0)
                    {
                        lstBlacklistUpload.AddRange(employeeLst);
                    }

                    var customerLst = await (from e in _dbContext.HR_User
                                             join w in _dbContext.HR_CustomerInfo
                                             on e.EmployeeATID equals w.EmployeeATID
                                             where nric.Contains(e.Nric) && (e.EmployeeType == (int)EmployeeType.Guest)
                                             select e.EmployeeATID).ToListAsync();
                    if (customerLst != null && customerLst.Count > 0)
                    {
                        lstBlacklistUpload.AddRange(customerLst);
                    }
                }

                if (employee != null && employee.Count > 0)
                {
                    lstBlacklistUpload.AddRange(employee);
                }

                var isMondelez = ClientName.MONDELEZ.ToString() == _configClientName;
                if (lstBlacklistUpload.Count > 0)
                {

                    var listWorkingInfoAll = await _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex
                              && x.FromDate.Date <= DateTime.Now && (!x.ToDate.HasValue || x.ToDate > DateTime.Now.Date)
                               && (!isMondelez || (x.DepartmentIndex != 0))
                              && lstBlacklistUpload.Contains(x.EmployeeATID) && x.Status == (long)TransferStatus.Approve).OrderBy(t => t.FromDate).ToListAsync();

                    var customerLst = (from e in listWorkingInfoAll
                                       join w in _dbContext.IC_DepartmentAndDevice
                                       on e.DepartmentIndex equals w.DepartmentIndex
                                       where lstSerials.Contains(w.SerialNumber) && w.SerialNumber != null
                                       select new
                                       {
                                           EmployeeATID = e.EmployeeATID,
                                           SerialNumber = w.SerialNumber
                                       }).ToList();

                    var customerLstUp = customerLst.GroupBy(x => x.SerialNumber).Select(x => new { Key = x.Key, Value = x }).ToList();

                    foreach (var item in customerLstUp)
                    {
                        var listUser = new List<UserInfoOnMachine>();

                        IC_UserinfoOnMachineParam paramUserOnMachine = new IC_UserinfoOnMachineParam();
                        paramUserOnMachine.ListEmployeeaATID = item.Value.Select(x => x.EmployeeATID).ToList();
                        paramUserOnMachine.CompanyIndex = user.CompanyIndex;
                        paramUserOnMachine.ListSerialNumber = new List<string> { item.Key };
                        paramUserOnMachine.AuthenMode = "";
                        paramUserOnMachine.FullInfo = true;
                        listUser = _iC_CommandLogic.GetListUserInfoOnMachine(paramUserOnMachine);


                        IC_CommandParamDTO commandParam = new IC_CommandParamDTO();
                        commandParam.IsOverwriteData = false;
                        commandParam.Action = CommandAction.UploadUsers;
                        commandParam.CommandName = StringHelper.GetCommandType((short)EmployeeType.Employee);
                        commandParam.AuthenMode = null;
                        commandParam.FromTime = new DateTime(2000, 1, 1);
                        commandParam.ToTime = DateTime.Now;
                        commandParam.ListEmployee = listUser;
                        commandParam.ListSerialNumber = new List<string> { item.Key };
                        commandParam.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                        commandParam.ExternalData = null;

                        List<CommandResult> lstCmd = _iC_CommandLogic.CreateListCommands(commandParam);

                        if (lstCmd != null && lstCmd.Count() > 0)
                        {
                            IC_GroupCommandParamDTO groupCommand = new IC_GroupCommandParamDTO();
                            groupCommand.CompanyIndex = user.CompanyIndex;
                            groupCommand.UserName = user.UserName;
                            groupCommand.ListCommand = lstCmd;
                            groupCommand.GroupName = GroupName.UploadUsers.ToString();
                            groupCommand.EventType = "";
                            _iC_CommandLogic.CreateGroupCommands(groupCommand);
                        }
                    }
                }

            }

        }
    }
}
