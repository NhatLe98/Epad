using ClosedXML.Excel;
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
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Services.Impl
{
    public class TA_BusinessRegistrationService : BaseServices<TA_BusinessRegistration, EPAD_Context>, ITA_BusinessRegistrationService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        IHR_UserService _HR_UserService;
        public TA_BusinessRegistrationService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_BusinessRegistrationService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }

        public async Task<DataGridClass> GetBusinessRegistration(BusinessRegistrationModel param, UserInfo user)
        {
            var filterBy = new List<string>();
            if (!string.IsNullOrWhiteSpace(param.Filter) && param.Filter.Contains(" "))
            {
                filterBy = param.Filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            var query = from ea in DbContext.TA_BusinessRegistration.Where(x => x.CompanyIndex == user.CompanyIndex)
                        join u in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex)
                        on ea.EmployeeATID equals u.EmployeeATID
                        join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
                            && w.Status == (short)TransferStatus.Approve
                            && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue
                                || (w.ToDate.HasValue && w.ToDate.Value.Date > DateTime.Now.Date)))
                        on u.EmployeeATID equals w.EmployeeATID into workingInfo
                        from wkResult in workingInfo.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex && x.IsInactive != true)
                        on wkResult.DepartmentIndex equals d.Index into depInfo
                        from depResult in depInfo.DefaultIfEmpty()
                        select new BusinessRegistrationModel
                        {
                            Index = ea.Index,
                            EmployeeATID = ea.EmployeeATID,
                            EmployeeCode = u.EmployeeCode,
                            FullName = u.FullName,
                            Description = ea.Description,
                            DepartmentIndex = depResult != null ? depResult.Index : 0,
                            DepartmentName = depResult != null ? depResult.Name : string.Empty,
                            CreatedDate = ea.CreatedDate,
                            UpdatedDate = ea.UpdatedDate,
                            UpdatedUser = ea.UpdatedUser,
                            BusinessDate = ea.BusinessDate,
                            BusinessDateString = ea.BusinessDate.ToddMMyyyy(),
                            BusinessType = ea.BusinessType,
                            BusinessTypeName = ((BusinessType)ea.BusinessType).ToString(),
                            FromTime = ea.FromTime,
                            FromTimeString = ea.FromTime.HasValue ? ea.FromTime.Value.ToHHmm() : string.Empty,
                            ToTime = ea.ToTime,
                            ToTimeString = ea.ToTime.HasValue ? ea.ToTime.Value.ToHHmm() : string.Empty,
                            WorkPlace = ea.WorkPlace,
                        };

            if (param.FromDate.HasValue)
            {
                query = query.Where(x => x.BusinessDate.Date >= param.FromDate.Value.Date);
            }
            if (param.ToDate.HasValue)
            {
                query = query.Where(x => x.BusinessDate.Date <= param.ToDate.Value.Date);
            }
            if (param.ListDepartmentIndex != null && param.ListDepartmentIndex.Count > 0)
            {
                query = query.Where(x => param.ListDepartmentIndex.Contains(x.DepartmentIndex));
            }
            if (param.ListEmployeeATID != null && param.ListEmployeeATID.Count > 0)
            {
                query = query.Where(x => param.ListEmployeeATID.Contains(x.EmployeeATID));
            }
            if (!string.IsNullOrWhiteSpace(param.Filter))
            {
                query = query.Where(x => filterBy.Contains(x.EmployeeATID) || param.Filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.EmployeeCode) && (param.Filter.ToLower().Contains(x.EmployeeCode.ToLower())
                || param.Filter.ToLower() == x.EmployeeCode.ToLower() || x.EmployeeCode.ToLower().Contains(param.Filter.ToLower())))
                || (!string.IsNullOrWhiteSpace(x.FullName) && (param.Filter.ToLower().Contains(x.FullName.ToLower())
                || param.Filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(param.Filter.ToLower()))));
            }

            var skip = (param.Page - 1) * param.PageSize;
            if (skip < 0)
            {
                skip = 0;
            }

            int countTotal = query.Count();
            var dataResult = query.Skip(skip).Take(param.PageSize).ToList();

            if (dataResult != null && dataResult.Count > 0)
            {
                var listEmployeeATID = dataResult.Select(x => x.EmployeeATID).ToList();
                var minFromDate = dataResult.Min(x => x.BusinessDate);
                var maxToDate = dataResult.Max(x => x.BusinessDate);

                var listShiftIndex = new List<int>();

                var scheduleByDepartments = new List<TA_ScheduleFixedByDepartment>();
                if (dataResult.Any(x => x.DepartmentIndex > 0))
                {
                    var listDepartment = dataResult.Where(x => x.DepartmentIndex > 0).Select(x
                        => x.DepartmentIndex).ToList();
                    scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                        => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                        && minFromDate.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                        && maxToDate.Date <= x.ToDate.Value.Date))).ToListAsync();
                    if (scheduleByDepartments != null && scheduleByDepartments.Count > 0)
                    {
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Monday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Tuesday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Wednesday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Thursday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Friday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Saturday));
                        listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Sunday));
                    }
                }
                var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && minFromDate.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    && maxToDate.Date <= x.ToDate.Value.Date))).ToListAsync();
                if (scheduleByEmployees != null && scheduleByEmployees.Count > 0)
                {
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Monday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Tuesday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Wednesday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Thursday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Friday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Saturday));
                    listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Sunday));
                }
                var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && x.Date.Date >= minFromDate.Date && x.Date.Date <= maxToDate.Date).ToListAsync();
                if (employeeShifts != null && employeeShifts.Count > 0)
                {
                    listShiftIndex.AddRange(employeeShifts.Where(x => x.ShiftIndex.HasValue).Select(x => x.ShiftIndex.Value));
                }

                var listShift = new List<TA_Shift>();
                if (listShiftIndex != null && listShiftIndex.Count > 0)
                {
                    listShift = await _dbContext.TA_Shift.AsNoTracking().Where(x => listShiftIndex.Contains(x.Index)).ToListAsync();
                }

                foreach (var item in dataResult)
                {
                    var shiftIndex = 0;
                    var employeeShift = employeeShifts.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.Date.Date == item.BusinessDate.Date);
                    if (employeeShift != null)
                    {
                        shiftIndex = employeeShift.ShiftIndex.HasValue ? employeeShift.ShiftIndex.Value : 0;
                    }
                    var scheduleByEmployee = scheduleByEmployees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                        && x.FromDate.Date <= item.BusinessDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= item.BusinessDate.Date)));
                    if (shiftIndex == 0 && scheduleByEmployee != null)
                    {
                        switch (item.BusinessDate.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                shiftIndex = scheduleByEmployee.Monday;
                                break;
                            case DayOfWeek.Tuesday:
                                shiftIndex = scheduleByEmployee.Tuesday;
                                break;
                            case DayOfWeek.Wednesday:
                                shiftIndex = scheduleByEmployee.Wednesday;
                                break;
                            case DayOfWeek.Thursday:
                                shiftIndex = scheduleByEmployee.Thursday;
                                break;
                            case DayOfWeek.Friday:
                                shiftIndex = scheduleByEmployee.Friday;
                                break;
                            case DayOfWeek.Saturday:
                                shiftIndex = scheduleByEmployee.Saturday;
                                break;
                            case DayOfWeek.Sunday:
                                shiftIndex = scheduleByEmployee.Sunday;
                                break;
                            default:
                                shiftIndex = 0;
                                break;
                        }
                    }
                    if (shiftIndex == 0 && item.DepartmentIndex > 0)
                    {
                        var scheduleByDepartment = scheduleByDepartments.FirstOrDefault(x => x.DepartmentIndex == item.DepartmentIndex
                            && x.FromDate.Date <= item.BusinessDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= item.BusinessDate.Date)));
                        if (scheduleByDepartment != null)
                        {
                            switch (item.BusinessDate.DayOfWeek)
                            {
                                case DayOfWeek.Monday:
                                    shiftIndex = scheduleByDepartment.Monday;
                                    break;
                                case DayOfWeek.Tuesday:
                                    shiftIndex = scheduleByDepartment.Tuesday;
                                    break;
                                case DayOfWeek.Wednesday:
                                    shiftIndex = scheduleByDepartment.Wednesday;
                                    break;
                                case DayOfWeek.Thursday:
                                    shiftIndex = scheduleByDepartment.Thursday;
                                    break;
                                case DayOfWeek.Friday:
                                    shiftIndex = scheduleByDepartment.Friday;
                                    break;
                                case DayOfWeek.Saturday:
                                    shiftIndex = scheduleByDepartment.Saturday;
                                    break;
                                case DayOfWeek.Sunday:
                                    shiftIndex = scheduleByDepartment.Sunday;
                                    break;
                                default:
                                    shiftIndex = 0;
                                    break;
                            }
                        }
                    }

                    if (item.BusinessType == (short)BusinessType.BusinessAllShift)
                    {
                        item.TotalWork = 1;
                    }
                    else if (item.BusinessType == (short)BusinessType.BusinessFromToTime 
                        && shiftIndex > 0 && listShift.Any(x => x.Index == shiftIndex))
                    {
                        var shift = listShift.FirstOrDefault(x => x.Index == shiftIndex);
                        var shiftTotalMinutes = (int)(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                            shift.CheckOutTime.Value.Hour, shift.CheckOutTime.Value.Minute, 0))
                            .Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                            shift.CheckInTime.Value.Hour, shift.CheckInTime.Value.Minute, 0)).TotalMinutes;
                        var businessTotalMinutes = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            item.ToTime.Value.Hour, item.ToTime.Value.Minute, 0)).Subtract(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            item.FromTime.Value.Hour, item.FromTime.Value.Minute, 0)).TotalMinutes;
                        item.TotalWork = (float)(businessTotalMinutes / shiftTotalMinutes);
                    }
                }
            }

            var result = new DataGridClass(countTotal, dataResult);

            return result;
        }

        public async Task<List<TA_BusinessRegistration>> GetBusinessRegistrationByListIndex(List<int> index)
        {
            return await _dbContext.TA_BusinessRegistration.AsNoTracking().Where(x => index.Contains(x.Index)).ToListAsync();
        }

        public async Task<List<TA_LeaveRegistration>> GetLeaveRegistrationByEmployeeATIDsAndDates(List<string> employeeATIDs, 
            List<DateTime> dates, int companyIndex) 
        {
            var minDate = dates.Min(x => x);
            var maxDate = dates.Max(x => x);
            return await _dbContext.TA_LeaveRegistration.AsNoTracking().Where(x
                => employeeATIDs.Contains(x.EmployeeATID) && x.LeaveDate.Date >= minDate.Date && x.LeaveDate.Date <= maxDate.Date)
                .ToListAsync();
        }

        public async Task<bool> AddBusinessRegistration(BusinessRegistrationModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var listDate = DateTimeExtension.GetListDate(param.FromDate.Value, param.ToDate.Value);
                foreach (var id in param.ListEmployeeATID)
                {
                    foreach (var date in listDate)
                    {
                        await _dbContext.TA_BusinessRegistration.AddAsync(new TA_BusinessRegistration
                        {
                            EmployeeATID = id,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName,
                            Description = param.Description,
                            BusinessDate = date,
                            BusinessType = param.BusinessType,
                            WorkPlace = param.WorkPlace,
                            FromTime = param.FromTime,
                            ToTime = param.ToTime
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateBusinessRegistration(BusinessRegistrationModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var existData = await _dbContext.TA_BusinessRegistration.FirstOrDefaultAsync(x => x.Index == param.Index);
                if (existData != null)
                {
                    existData.Description = param.Description;
                    existData.BusinessDate = param.BusinessDate;
                    existData.BusinessType = param.BusinessType;
                    existData.WorkPlace = param.WorkPlace;
                    existData.FromTime = param.FromTime;
                    existData.ToTime = param.ToTime;

                    _dbContext.TA_BusinessRegistration.Update(existData);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteBusinessRegistration(List<int> indexex)
        {
            var result = true;
            try
            {
                var existData = await _dbContext.TA_BusinessRegistration.Where(x => indexex.Contains(x.Index)).ToListAsync();
                if (existData != null && existData.Count > 0)
                {
                    _dbContext.TA_BusinessRegistration.RemoveRange(existData);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<DateTime> GetLockAttendanceTimeValidDate(UserInfo user)
        {
            var now = DateTime.Now;
            var rule = await _dbContext.TA_Rules_Global.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);
            var result = new DateTime();
            if (now.Day <= rule.LockAttendanceTime)
            {
                result = new DateTime(now.Year, now.Month, rule.LockAttendanceTime).AddMonths(-1).AddDays(1);
            }
            else
            {
                result = new DateTime(now.Year, now.Month, rule.LockAttendanceTime).AddDays(1);
            }
            return result;
        }

        public async Task<Dictionary<string, HashSet<string>>> CheckRuleBusinessRegister(BusinessRegistrationModel param, UserInfo user)
        {
            var result = new Dictionary<string, HashSet<string>>();
            var dicEmployeeShift = new Dictionary<string, List<Tuple<DateTime, int>>>();
            var listDate = DateTimeExtension.GetListDate(param.FromDate.Value, param.ToDate.Value);
            var listYear = listDate.Select(x => x.Year).ToHashSet();
            var listEmployeeATID = param.ListEmployeeATID.ToHashSet();
            var employeeInfos = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(param.ListEmployeeATID, new DateTime(), user.CompanyIndex);
            var scheduleByDepartments = new List<TA_ScheduleFixedByDepartment>();
            if (employeeInfos != null && employeeInfos.Count > 0 && employeeInfos.Any(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0))
            {
                var listDepartment = employeeInfos.Where(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0).Select(x
                    => x.DepartmentIndex).ToList();
                scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                    => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                    && param.FromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    && param.ToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
            }
            var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                && param.FromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                && param.ToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
            var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                && x.Date.Date >= param.FromDate.Value.Date && x.Date.Date <= param.ToDate.Value.Date).ToListAsync();

            var listLeaveRegistered = await GetLeaveRegistrationByEmployeeATIDsAndDates(listEmployeeATID.ToList(), listDate, user.CompanyIndex);

            foreach (var id in listEmployeeATID)
            {
                var employeeInfo = employeeInfos.FirstOrDefault(x => x.EmployeeATID == id);
                var listShift = new List<Tuple<DateTime, int>>();
                foreach (var date in listDate)
                {
                    if (listLeaveRegistered.Any(x => x.EmployeeATID == id && x.LeaveDate.Date == date.Date))
                    {
                        var leaveRegistered = listLeaveRegistered.FirstOrDefault(x => x.EmployeeATID == id && x.LeaveDate.Date == date.Date);
                        if (leaveRegistered.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift)
                        {
                            if (result.ContainsKey(id))
                            {
                                result[id].Add("EmployeeHaveAllDayLeaveRegistered");
                            }
                            else
                            {
                                result.Add(id, new HashSet<string> { "EmployeeHaveAllDayLeaveRegistered" });
                            }
                        }
                        else if (leaveRegistered.LeaveDurationType == (short)LeaveDurationType.LeaveHaftShift 
                            && param.BusinessType == (short)BusinessType.BusinessAllShift)
                        {
                            if (result.ContainsKey(id))
                            {
                                result[id].Add("EmployeeHaveLeaveRegistered");
                            }
                            else
                            {
                                result.Add(id, new HashSet<string> { "EmployeeHaveLeaveRegistered" });
                            }
                        }
                    }

                    var shiftIndex = 0;
                    var employeeShift = employeeShifts.FirstOrDefault(x => x.EmployeeATID == id && x.Date.Date == date.Date);
                    if (employeeShift != null)
                    {
                        shiftIndex = employeeShift.ShiftIndex.HasValue ? employeeShift.ShiftIndex.Value : 0;
                        continue;
                    }
                    var scheduleByEmployee = scheduleByEmployees.FirstOrDefault(x => x.EmployeeATID == id
                        && x.FromDate.Date <= date.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= date.Date)));
                    if (scheduleByEmployee != null)
                    {
                        switch (date.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                shiftIndex = scheduleByEmployee.Monday;
                                break;
                            case DayOfWeek.Tuesday:
                                shiftIndex = scheduleByEmployee.Tuesday;
                                break;
                            case DayOfWeek.Wednesday:
                                shiftIndex = scheduleByEmployee.Wednesday;
                                break;
                            case DayOfWeek.Thursday:
                                shiftIndex = scheduleByEmployee.Thursday;
                                break;
                            case DayOfWeek.Friday:
                                shiftIndex = scheduleByEmployee.Friday;
                                break;
                            case DayOfWeek.Saturday:
                                shiftIndex = scheduleByEmployee.Saturday;
                                break;
                            case DayOfWeek.Sunday:
                                shiftIndex = scheduleByEmployee.Sunday;
                                break;
                            default:
                                shiftIndex = 0;
                                break;
                        }
                        continue;
                    }
                    if (employeeInfo != null && employeeInfo.DepartmentIndex.HasValue && employeeInfo.DepartmentIndex.Value > 0)
                    {
                        var scheduleByDepartment = scheduleByDepartments.FirstOrDefault(x => x.DepartmentIndex == employeeInfo.DepartmentIndex.Value
                            && x.FromDate.Date <= date.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= date.Date)));
                        if (scheduleByDepartment != null)
                        {
                            switch (date.DayOfWeek)
                            {
                                case DayOfWeek.Monday:
                                    shiftIndex = scheduleByDepartment.Monday;
                                    break;
                                case DayOfWeek.Tuesday:
                                    shiftIndex = scheduleByDepartment.Tuesday;
                                    break;
                                case DayOfWeek.Wednesday:
                                    shiftIndex = scheduleByDepartment.Wednesday;
                                    break;
                                case DayOfWeek.Thursday:
                                    shiftIndex = scheduleByDepartment.Thursday;
                                    break;
                                case DayOfWeek.Friday:
                                    shiftIndex = scheduleByDepartment.Friday;
                                    break;
                                case DayOfWeek.Saturday:
                                    shiftIndex = scheduleByDepartment.Saturday;
                                    break;
                                case DayOfWeek.Sunday:
                                    shiftIndex = scheduleByDepartment.Sunday;
                                    break;
                                default:
                                    shiftIndex = 0;
                                    break;
                            }
                            continue;
                        }
                    }
                    listShift.Add(new Tuple<DateTime, int>(date, shiftIndex));
                    if (shiftIndex == 0)
                    {
                        if (result.ContainsKey(id))
                        {
                            result[id].Add("EmployeeNotHaveShift");
                        }
                        else
                        {
                            result.Add(id, new HashSet<string> { "EmployeeNotHaveShift" });
                        }
                    }
                }
                if (!dicEmployeeShift.ContainsKey(id))
                {
                    dicEmployeeShift.Add(id, listShift);
                }
                else
                {
                    dicEmployeeShift[id] = listShift;
                }
            }

            var listBusinessRegistered = await _dbContext.TA_BusinessRegistration.Where(x
                => x.Index != param.Index && listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            if (listBusinessRegistered.Any(x => x.BusinessDate.Date >= param.FromDate.Value.Date && x.BusinessDate.Date <= param.ToDate.Value.Date
                && x.Index != param.Index))
            {
                var listBusinessRegisteredDuplicate = listBusinessRegistered.Where(x => x.BusinessDate.Date >= param.FromDate.Value.Date
                    && x.BusinessDate.Date <= param.ToDate.Value.Date && x.Index != param.Index).ToList();
                foreach (var item in listBusinessRegisteredDuplicate)
                {
                    if (result.ContainsKey(item.EmployeeATID))
                    {
                        result[item.EmployeeATID].Add("EmployeeAlreadyHasBusinessRegister");
                    }
                    else
                    {
                        result.Add(item.EmployeeATID, new HashSet<string> { "EmployeeAlreadyHasBusinessRegister" });
                    }
                }
            }

            return result;
        }

        public async Task<List<BusinessRegistrationModel>> ValidationImportBusinessRegistration(List<BusinessRegistrationModel> param, UserInfo user)
        {
            var errorList = new List<BusinessRegistrationModel>();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 50).ToList();
            var checkIsNull = param.Where(e => string.IsNullOrWhiteSpace(e.EmployeeATID)
                || string.IsNullOrWhiteSpace(e.FromDateString)
                || string.IsNullOrWhiteSpace(e.ToDateString)).ToList();

            if (checkMaxLength != null && checkMaxLength.Count() > 0)
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 50) item.ErrorMessage += "Mã ID lớn hơn 50 ký tự" + "\r\n";
                }
            }

            if (checkIsNull != null && checkIsNull.Count() > 0)
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeATID)) item.ErrorMessage += "MCC không được để trống\r\n";
                }
            }

            var format = "dd/MM/yyyy";
            foreach (var item in param)
            {
                item.FromDate = null;
                item.ToDate = null;

                var time = new DateTime();
                if (!string.IsNullOrWhiteSpace(item.FromDateString))
                {
                    if (!DateTime.TryParseExact(item.FromDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out time))
                    {
                        item.ErrorMessage += "Từ ngày không đúng định dạng\r\n";
                    }
                    else
                    {
                        item.FromDate = time;
                    }
                }
                else
                {
                    item.ErrorMessage += "Từ ngày không được để trống\r\n";
                }

                if (!string.IsNullOrWhiteSpace(item.ToDateString))
                {
                    if (!DateTime.TryParseExact(item.ToDateString, format, null, System.Globalization.DateTimeStyles.None,
                    out time))
                    {
                        item.ErrorMessage += "Đến ngày không đúng định dạng\r\n";
                    }
                    else
                    {
                        item.ToDate = time;
                    }
                }
                else
                {
                    item.ErrorMessage += "Đến ngày không được để trống\r\n";
                }

                if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date > item.ToDate.Value.Date)
                {
                    item.ErrorMessage += "Từ ngày không thể lớn hơn đến ngày\r\n";
                }

                if (item.BusinessType == (short)BusinessType.BusinessFromToTime)
                {
                    var timeFormat = "dd/MM/yyyy HH:mm";
                    var isErrorFromToTime = false;
                    if (string.IsNullOrWhiteSpace(item.FromTimeString))
                    {
                        item.ErrorMessage += "Từ giờ không được để trống\r\n";
                        isErrorFromToTime = true;
                    }
                    else
                    {
                        var fullFromTimeString = DateTime.Now.ToddMMyyyy() + " " + item.FromTimeString;
                        if (!DateTime.TryParseExact(fullFromTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                            out time))
                        {
                            item.ErrorMessage += "Từ giờ không đúng định dạng\r\n";
                            isErrorFromToTime = true;
                        }
                        else
                        {
                            item.FromTime = time;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(item.ToTimeString))
                    {
                        item.ErrorMessage += "Đến giờ không được để trống\r\n";
                        isErrorFromToTime = true;
                    }
                    else
                    {
                        var fullToTimeString = DateTime.Now.ToddMMyyyy() + " " + item.ToTimeString;
                        if (!DateTime.TryParseExact(fullToTimeString, timeFormat, null, System.Globalization.DateTimeStyles.None,
                            out time))
                        {
                            item.ErrorMessage += "Đến giờ không đúng định dạng\r\n";
                            isErrorFromToTime = true;
                        }
                        else
                        {
                            item.ToTime = time;
                        }
                    }

                    if (!isErrorFromToTime && item.FromTime > item.ToTime)
                    {
                        item.ErrorMessage += "Từ giờ không được lớn hơn đến giờ\r\n";
                    }
                }
            }

            var minFromDate = param.Min(x => x.FromDate);
            var maxToDate = param.Max(x => x.ToDate);

            var lockAttendanceTimeValidDate = await GetLockAttendanceTimeValidDate(user);

            var listEmployeeATID = param.Select(x => x.EmployeeATID).Distinct().ToList();
            if (listEmployeeATID.Count == 0)
            {
                errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;
            }

            var listBusinessRegistered = await _dbContext.TA_BusinessRegistration.Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            if (minFromDate.HasValue && maxToDate.HasValue)
            {
                var listDate = DateTimeExtension.GetListDate(minFromDate.Value, maxToDate.Value);
                var listYear = listDate.Select(x => x.Year).ToHashSet();

                var listLeaveRegistered = await GetLeaveRegistrationByEmployeeATIDsAndDates(listEmployeeATID, listDate, user.CompanyIndex);

                var employeeInfos = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(listEmployeeATID.ToList(),
                    new DateTime(), user.CompanyIndex);
                var scheduleByDepartments = new List<TA_ScheduleFixedByDepartment>();
                if (employeeInfos != null && employeeInfos.Count > 0
                    && employeeInfos.Any(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0))
                {
                    var listDepartment = employeeInfos.Where(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0).Select(x
                        => x.DepartmentIndex).ToList();
                    scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                        => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                        && minFromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                        && maxToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
                }
                var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && minFromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    && maxToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
                var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && x.Date.Date >= minFromDate.Value.Date && x.Date.Date <= maxToDate.Value.Date).ToListAsync();

                var rule = await _dbContext.TA_Rules_Global.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);

                var listExistParam = new List<BusinessRegistrationModel>();
                foreach (var item in param)
                {
                    var employeeInfo = employeeInfos.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    if (item.FromDate.HasValue && item.ToDate.HasValue)
                    {
                        //if (item.FromDate.Value.Date <= item.ToDate.Value.Date && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                        //    && x.FromDate.Value.Date <= x.ToDate.Value.Date && !(item.ToDate.Value.Date < x.FromDate.Value.Date
                        //    && item.FromDate.Value.Date > x.ToDate.Value.Date) && x.BusinessType == item.BusinessType))
                        //{
                        //    item.ErrorMessage += "Dữ liệu nhân viên đăng ký công tác bị trùng trong tệp tin\r\n";
                        //}
                        //else if (item.FromDate.Value.Date <= item.ToDate.Value.Date && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                        //    && x.FromDate.Value.Date <= x.ToDate.Value.Date && !(item.ToDate.Value.Date < x.FromDate.Value.Date
                        //    && item.FromDate.Value.Date > x.ToDate.Value.Date) && x.BusinessType != item.BusinessType))
                        //{
                        //    item.ErrorMessage += "Nhân viên đăng ký nhiều loại công tác cùng lúc\r\n";
                        //}

                        if (item.FromDate.Value.Date <= item.ToDate.Value.Date && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                            //&& x.BusinessType == item.BusinessType 
                            && DoRangesOverlap(item.FromDate.Value.Date, item.ToDate.Value.Date, x.FromDate.Value.Date, x.ToDate.Value.Date)))
                        {
                            item.ErrorMessage += "Dữ liệu nhân viên đăng ký công tác bị trùng trong tệp tin\r\n";
                        }
                        //else 
                        //if (item.FromDate.Value.Date <= item.ToDate.Value.Date && (listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                        //    && x.BusinessType != item.BusinessType
                        //    && DoRangesOverlap(item.FromDate.Value.Date, item.ToDate.Value.Date, x.FromDate.Value.Date, x.ToDate.Value.Date)) 
                        //    || (item.BusinessType == (short)BusinessType.BusinessAllShift && (!string.IsNullOrWhiteSpace(item.FromTimeString) 
                        //    || !string.IsNullOrWhiteSpace(item.ToTimeString)))))
                        if (item.BusinessType == (short)BusinessType.BusinessAllShift && (!string.IsNullOrWhiteSpace(item.FromTimeString)
                            || !string.IsNullOrWhiteSpace(item.ToTimeString)))
                        {
                            item.ErrorMessage += "Nhân viên đăng ký nhiều loại công tác cùng lúc\r\n";
                        }
                        listExistParam.Add(item);

                        if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date <= item.ToDate.Value.Date)
                        {
                            var itemListDate = DateTimeExtension.GetListDate(item.FromDate.Value, item.ToDate.Value);
                            var itemListYear = itemListDate.Select(x => x.Year).ToHashSet();

                            if (itemListDate.Any(x => x.Date < lockAttendanceTimeValidDate.Date))
                            {
                                item.ErrorMessage += "Đã qua thời gian khóa công hàng tháng\r\n";
                            }

                            foreach (var date in itemListDate)
                            {
                                if (listLeaveRegistered.Any(x => x.EmployeeATID == item.EmployeeATID && x.LeaveDate.Date == date.Date))
                                {
                                    var leaveRegistered = listLeaveRegistered.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                                        && x.LeaveDate.Date == date.Date);
                                    if (leaveRegistered.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift)
                                    {
                                        item.ErrorMessage += "Nhân viên đã đăng ký nghỉ nguyên ngày, không thể đăng ký công tác\r\n";
                                    }
                                    else if (leaveRegistered.LeaveDurationType == (short)LeaveDurationType.LeaveHaftShift
                                        && item.BusinessType == (short)BusinessType.BusinessAllShift)
                                    {
                                        item.ErrorMessage += "Nhân viên đã đăng ký nghỉ, không thể đăng ký công tác nguyên ngày\r\n";
                                    }
                                }

                                var shiftIndex = 0;
                                var employeeShift = employeeShifts.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID && x.Date.Date == date.Date);
                                if (employeeShift != null)
                                {
                                    shiftIndex = employeeShift.ShiftIndex.HasValue ? employeeShift.ShiftIndex.Value : 0;
                                    //continue;
                                }
                                var scheduleByEmployee = scheduleByEmployees.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                                    && x.FromDate.Date <= date.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= date.Date)));
                                if (scheduleByEmployee != null)
                                {
                                    switch (date.DayOfWeek)
                                    {
                                        case DayOfWeek.Monday:
                                            shiftIndex = scheduleByEmployee.Monday;
                                            break;
                                        case DayOfWeek.Tuesday:
                                            shiftIndex = scheduleByEmployee.Tuesday;
                                            break;
                                        case DayOfWeek.Wednesday:
                                            shiftIndex = scheduleByEmployee.Wednesday;
                                            break;
                                        case DayOfWeek.Thursday:
                                            shiftIndex = scheduleByEmployee.Thursday;
                                            break;
                                        case DayOfWeek.Friday:
                                            shiftIndex = scheduleByEmployee.Friday;
                                            break;
                                        case DayOfWeek.Saturday:
                                            shiftIndex = scheduleByEmployee.Saturday;
                                            break;
                                        case DayOfWeek.Sunday:
                                            shiftIndex = scheduleByEmployee.Sunday;
                                            break;
                                        default:
                                            shiftIndex = 0;
                                            break;
                                    }
                                    //continue;
                                }
                                if (employeeInfo != null && employeeInfo.DepartmentIndex.HasValue && employeeInfo.DepartmentIndex.Value > 0)
                                {
                                    var scheduleByDepartment = scheduleByDepartments.FirstOrDefault(x => x.DepartmentIndex == employeeInfo.DepartmentIndex.Value
                                        && x.FromDate.Date <= date.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue || x.ToDate.Value.Date >= date.Date)));
                                    if (scheduleByDepartment != null)
                                    {
                                        switch (date.DayOfWeek)
                                        {
                                            case DayOfWeek.Monday:
                                                shiftIndex = scheduleByDepartment.Monday;
                                                break;
                                            case DayOfWeek.Tuesday:
                                                shiftIndex = scheduleByDepartment.Tuesday;
                                                break;
                                            case DayOfWeek.Wednesday:
                                                shiftIndex = scheduleByDepartment.Wednesday;
                                                break;
                                            case DayOfWeek.Thursday:
                                                shiftIndex = scheduleByDepartment.Thursday;
                                                break;
                                            case DayOfWeek.Friday:
                                                shiftIndex = scheduleByDepartment.Friday;
                                                break;
                                            case DayOfWeek.Saturday:
                                                shiftIndex = scheduleByDepartment.Saturday;
                                                break;
                                            case DayOfWeek.Sunday:
                                                shiftIndex = scheduleByDepartment.Sunday;
                                                break;
                                            default:
                                                shiftIndex = 0;
                                                break;
                                        }
                                        //continue;
                                    }
                                }
                                if (shiftIndex == 0)
                                {
                                    item.ErrorMessage += "Nhân viên không có ca làm việc trong ngày " + date.ToddMMyyyy() + "\r\n";
                                }
                            }
                        }
                    }
                }
            }

            var noErrorParam = param.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
            if (noErrorParam != null && noErrorParam.Count > 0)
            {
                var validMinFromDate = noErrorParam.Min(x => x.FromDate);
                var validMaxToDate = noErrorParam.Max(x => x.ToDate);

                foreach (var item in noErrorParam)
                {
                    var validListDate = DateTimeExtension.GetListDate(item.FromDate.Value, item.ToDate.Value);
                    foreach (var date in validListDate)
                    {
                        var existedRegister = listBusinessRegistered.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                            && x.BusinessDate.Date == date.Date);
                        if (existedRegister != null)
                        {
                            existedRegister.BusinessType = item.BusinessType;
                            existedRegister.WorkPlace = item.WorkPlace;
                            existedRegister.FromTime = item.FromTime;
                            existedRegister.ToTime = item.ToTime;
                            existedRegister.Description = item.Description;
                            existedRegister.UpdatedDate = DateTime.Now;
                            existedRegister.UpdatedUser = user.FullName;

                            DbContext.TA_BusinessRegistration.Update(existedRegister);
                        }
                        else
                        {
                            var newBusinessRegistration = new TA_BusinessRegistration();
                            newBusinessRegistration.EmployeeATID = item.EmployeeATID;
                            newBusinessRegistration.BusinessDate = date;
                            newBusinessRegistration.WorkPlace = item.WorkPlace;
                            newBusinessRegistration.Description = item.Description;
                            newBusinessRegistration.CompanyIndex = user.CompanyIndex;
                            newBusinessRegistration.BusinessType = item.BusinessType;
                            newBusinessRegistration.FromTime = item.FromTime;
                            newBusinessRegistration.ToTime = item.ToTime;
                            newBusinessRegistration.CreatedDate = DateTime.Now;
                            newBusinessRegistration.UpdatedDate = DateTime.Now;
                            newBusinessRegistration.UpdatedUser = user.FullName;
                            await _dbContext.TA_BusinessRegistration.AddAsync(newBusinessRegistration);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
            }

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }

        public bool DoRangesOverlap(DateTime obj1FromTime, DateTime obj1ToTime, DateTime obj2FromTime, DateTime obj2ToTime)
        {
            // Check if one range is fully contained within the other range
            if ((obj1FromTime >= obj2FromTime && obj1FromTime <= obj2ToTime) ||
                (obj1ToTime >= obj2FromTime && obj1ToTime <= obj2ToTime) ||
                (obj2FromTime >= obj1FromTime && obj2FromTime <= obj1ToTime) ||
                (obj2ToTime >= obj1FromTime && obj2ToTime <= obj1ToTime))
            {
                return true; // The ranges overlap
            }

            return false; // The ranges do not overlap
        }
    }
}
