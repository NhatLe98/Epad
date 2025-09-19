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
    public class TA_LeaveRegistrationService : BaseServices<TA_LeaveRegistration, EPAD_Context>, ITA_LeaveRegistrationService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        IHR_UserService _HR_UserService;
        public TA_LeaveRegistrationService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_LeaveRegistrationService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
        }

        public async Task<DataGridClass> GetLeaveRegistration(LeaveRegistrationModel param, UserInfo user)
        {
            var filterBy = new List<string>();
            if (!string.IsNullOrWhiteSpace(param.Filter) && param.Filter.Contains(" "))
            {
                filterBy = param.Filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            var query = from ea in DbContext.TA_LeaveRegistration.Where(x => x.CompanyIndex == user.CompanyIndex)
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
                        join ldt in DbContext.TA_LeaveDateType
                        on ea.LeaveDateType equals ldt.Index into ldtInfo
                        from ldtResult in ldtInfo.DefaultIfEmpty()
                        select new LeaveRegistrationModel
                        {
                            Index = ea.Index,
                            EmployeeATID = ea.EmployeeATID,
                            EmployeeCode = u.EmployeeCode,
                            FullName = u.FullName,
                            LeaveDate = ea.LeaveDate,
                            LeaveDateString = ea.LeaveDate.ToddMMyyyy(),
                            LeaveDateType = ea.LeaveDateType,
                            LeaveDateTypeName = ldtResult != null ? ldtResult.Name : string.Empty,
                            LeaveDurationType = ea.LeaveDurationType,
                            LeaveDurationTypeName = ea.LeaveDurationType > 0 ? ((LeaveDurationType)ea.LeaveDurationType).ToString() : string.Empty,
                            HaftLeaveType = ea.HaftLeaveType,
                            HaftLeaveTypeName = (ea.HaftLeaveType.HasValue && ea.HaftLeaveType.Value > 0)
                                ? ((HaftLeaveType)ea.HaftLeaveType.Value).ToString() : string.Empty,
                            TotalWork = (float)(ea.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5),
                            Description = ea.Description,
                            DepartmentIndex = depResult != null ? depResult.Index : 0,
                            DepartmentName = depResult != null ? depResult.Name : string.Empty,
                            CreatedDate = ea.CreatedDate,
                            UpdatedDate = ea.UpdatedDate,
                            UpdatedUser = ea.UpdatedUser
                        };

            if(param.FromDate.HasValue)
            {
                query = query.Where(x => x.LeaveDate.Date >= param.FromDate.Value.Date);
            }
            if (param.ToDate.HasValue)
            {
                query = query.Where(x => x.LeaveDate.Date <= param.ToDate.Value.Date);
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

            var result = new DataGridClass(countTotal, dataResult);

            return result;
        }

        public async Task<List<TA_LeaveRegistration>> GetLeaveRegistrationByListIndex(List<int> index)
        {
            return await _dbContext.TA_LeaveRegistration.AsNoTracking().Where(x => index.Contains(x.Index)).ToListAsync();
        }

        public async Task<List<TA_BusinessRegistration>> GetBusinessRegistrationByEmployeeATIDsAndDates(List<string> employeeATIDs,
            List<DateTime> dates, int companyIndex)
        {
            var minDate = dates.Min(x => x);
            var maxDate = dates.Max(x => x);
            return await _dbContext.TA_BusinessRegistration.AsNoTracking().Where(x
                => employeeATIDs.Contains(x.EmployeeATID) && x.BusinessDate.Date >= minDate.Date && x.BusinessDate.Date <= maxDate.Date)
                .ToListAsync();
        }

        public async Task<bool> AddLeaveRegistration(LeaveRegistrationModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var listDate = DateTimeExtension.GetListDate(param.FromDate.Value, param.ToDate.Value);
                foreach (var id in param.ListEmployeeATID)
                {
                    foreach (var date in listDate)
                    {
                        await _dbContext.TA_LeaveRegistration.AddAsync(new TA_LeaveRegistration
                        {
                            EmployeeATID = id,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = user.FullName,
                            Description = param.Description,
                            LeaveDate = date,
                            LeaveDateType = param.LeaveDateType,
                            LeaveDurationType = param.LeaveDurationType,
                            HaftLeaveType = param.HaftLeaveType
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

        public async Task<bool> UpdateLeaveRegistration(LeaveRegistrationModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var existData = await _dbContext.TA_LeaveRegistration.FirstOrDefaultAsync(x => x.Index == param.Index);
                if (existData != null)
                {
                    existData.Description = param.Description;
                    existData.LeaveDate = param.LeaveDate;
                    existData.LeaveDateType = param.LeaveDateType;
                    existData.LeaveDurationType = param.LeaveDurationType;
                    existData.HaftLeaveType = param.HaftLeaveType;

                    _dbContext.TA_LeaveRegistration.Update(existData);
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

        public async Task<bool> DeleteLeaveRegistration(List<int> indexex)
        {
            var result = true;
            try
            {
                var existData = await _dbContext.TA_LeaveRegistration.Where(x => indexex.Contains(x.Index)).ToListAsync();
                if (existData != null && existData.Count > 0)
                {
                    _dbContext.TA_LeaveRegistration.RemoveRange(existData);
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
                result =  new DateTime(now.Year, now.Month, rule.LockAttendanceTime).AddMonths(-1).AddDays(1);
            }
            else
            {
                result =  new DateTime(now.Year, now.Month, rule.LockAttendanceTime).AddDays(1);
            }
            return result;
        }

        public async Task<bool> IsPaidLeave(int index)
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().AnyAsync(x => x.Index == index && x.IsPaidLeave);
        }

        public async Task<Dictionary<string, HashSet<string>>> CheckRuleLeaveRegister(LeaveRegistrationModel param, UserInfo user)
        {
            var result = new Dictionary<string, HashSet<string>>();
            var isPaidLeave = await IsPaidLeave(param.LeaveDateType);
            var dicEmployeeShift = new Dictionary<string, List<Tuple<DateTime, int>>>();

            var listDate = DateTimeExtension.GetListDate(param.FromDate.Value, param.ToDate.Value);
            var listYear = listDate.Select(x => x.Year).ToHashSet();

            var minFromDate = new DateTime(listYear.Min(), 1, 1);
            var maxToDate = new DateTime(listYear.Max(), 12, 31);
            var listFullDate = DateTimeExtension.GetListDate(minFromDate, maxToDate);

            var listEmployeeATID = param.ListEmployeeATID.ToHashSet();
            var employeeInfos = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(param.ListEmployeeATID, new DateTime(), user.CompanyIndex);
            
            var scheduleByDepartments = new List<TA_ScheduleFixedByDepartment>();
            if (employeeInfos != null && employeeInfos.Count > 0 && employeeInfos.Any(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0))
            {
                var listDepartment = employeeInfos.Where(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0).Select(x
                    => x.DepartmentIndex).ToList();
                //scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                //    => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                //    && param.FromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                //    && param.ToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
                scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                    => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                    && minFromDate.Date <= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    && maxToDate.Date >= x.ToDate.Value.Date))).ToListAsync();
            }

            //var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
            //    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
            //    && param.FromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
            //    && param.ToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
            var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                && minFromDate.Date <= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                && maxToDate.Date >= x.ToDate.Value.Date))).ToListAsync();

            //var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
            //    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
            //    && x.Date.Date >= param.FromDate.Value.Date && x.Date.Date <= param.ToDate.Value.Date).ToListAsync();
            var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                && x.Date.Date >= minFromDate.Date && x.Date.Date <= maxToDate.Date).ToListAsync();

            var listShiftIndex = employeeShifts.Select(x => x?.ShiftIndex ?? 0).ToList();

            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Monday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Tuesday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Wednesday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Thursday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Friday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Saturday).ToList());
            listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Sunday).ToList());

            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Monday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Tuesday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Wednesday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Thursday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Friday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Saturday).ToList());
            listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Sunday).ToList());

            listShiftIndex = listShiftIndex.Distinct().ToList();

            var listShift = await _dbContext.TA_Shift.Where(x
                => listShiftIndex.Contains(x.Index)).ToListAsync();

            //var listBusinessRegistered = await GetBusinessRegistrationByEmployeeATIDsAndDates(listEmployeeATID.ToList(), listDate, user.CompanyIndex);
            var listBusinessRegistered = await GetBusinessRegistrationByEmployeeATIDsAndDates(listEmployeeATID.ToList(), listFullDate, user.CompanyIndex);

            foreach (var id in listEmployeeATID)
            {
                var employeeInfo = employeeInfos.FirstOrDefault(x => x.EmployeeATID == id);
                var listEmployeeShift = new List<Tuple<DateTime, int>>();
                //foreach (var date in listDate)
                foreach (var date in listFullDate)
                {
                    if (listDate.Any(y => y.Date == date.Date) 
                        && listBusinessRegistered.Any(x => x.EmployeeATID == id && x.BusinessDate.Date == date.Date))
                    {
                        var businessRegistered = listBusinessRegistered.FirstOrDefault(x 
                            => x.EmployeeATID == id && x.BusinessDate.Date == date.Date);
                        if (businessRegistered.BusinessType == (short)BusinessType.BusinessAllShift)
                        {
                            if (result.ContainsKey(id))
                            {
                                result[id].Add("EmployeeHaveAllDayBusinessRegistered");
                            }
                            else
                            {
                                result.Add(id, new HashSet<string> { "EmployeeHaveAllDayBusinessRegistered" });
                            }
                        }
                        else if (businessRegistered.BusinessType == (short)BusinessType.BusinessFromToTime
                            && param.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift)
                        {
                            if (result.ContainsKey(id))
                            {
                                result[id].Add("EmployeeHaveBusinessRegistered");
                            }
                            else
                            {
                                result.Add(id, new HashSet<string> { "EmployeeHaveBusinessRegistered" });
                            }
                        }
                    }

                    var shiftIndex = 0;
                    var employeeShift = employeeShifts.FirstOrDefault(x => x.EmployeeATID == id && x.Date.Date == date.Date);
                    if (employeeShift != null)
                    {
                        shiftIndex = employeeShift.ShiftIndex.HasValue ? employeeShift.ShiftIndex.Value : 0;
                        //continue;
                    }

                    if (shiftIndex == 0)
                    {
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
                            //continue;
                        }
                    }

                    if (shiftIndex == 0)
                    {
                        if (employeeInfo != null && employeeInfo.DepartmentIndex.HasValue && employeeInfo.DepartmentIndex.Value > 0)
                        {
                            var scheduleByDepartment = scheduleByDepartments.FirstOrDefault(x
                                => x.DepartmentIndex == employeeInfo.DepartmentIndex.Value
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
                    }

                    listEmployeeShift.Add(new Tuple<DateTime, int>(date, shiftIndex));
                    if (listDate.Any(y => y.Date == date.Date) && shiftIndex == 0)
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
                    dicEmployeeShift.Add(id, listEmployeeShift);
                }
                else
                {
                    dicEmployeeShift[id] = listEmployeeShift;
                }
            }

            var listLeaveRegistered = await _dbContext.TA_LeaveRegistration.Where(x
                => x.Index != param.Index && listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            if (listLeaveRegistered.Any(x => x.LeaveDate.Date >= param.FromDate.Value.Date && x.LeaveDate.Date <= param.ToDate.Value.Date
                && x.Index != param.Index))
            {
                var listLeaveRegisteredDuplicate = listLeaveRegistered.Where(x => x.LeaveDate.Date >= param.FromDate.Value.Date 
                    && x.LeaveDate.Date <= param.ToDate.Value.Date && x.Index != param.Index).ToList();
                foreach (var item in listLeaveRegisteredDuplicate)
                {
                    if (result.ContainsKey(item.EmployeeATID))
                    {
                        result[item.EmployeeATID].Add("EmployeeAlreadyHasLeaveRegister");
                    }
                    else
                    {
                        result.Add(item.EmployeeATID, new HashSet<string> { "EmployeeAlreadyHasLeaveRegister" });
                    }
                }
            }

            var listAnnualLeave = await _dbContext.TA_AnnualLeave.Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            var rule = await _dbContext.TA_Rules_Global.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);

            foreach (var id in listEmployeeATID)
            {
                if (isPaidLeave)
                {
                    var employeeListShift = dicEmployeeShift[id];

                    var annualLeave = listAnnualLeave.FirstOrDefault(x => x.EmployeeATID == id);
                    if (annualLeave != null)
                    {
                        foreach (var year in listYear)
                        {
                            var employeeLeaveRegistered = listLeaveRegistered.Where(x 
                                => x.EmployeeATID == id && x.LeaveDate.Year == year).ToList();
                            var yearLeaveRegistered = employeeLeaveRegistered.Sum(x =>
                            {
                                var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.LeaveDate.Date)?.Item2 ?? 0;
                                if (employeeShiftIndex != 0)
                                {
                                    var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                    if (shift != null)
                                    {
                                        return (shift.TheoryWorkedTimeByShift *
                                            (x.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                    }
                                    return 0;
                                }
                                return 0;
                            });

                            var employeeNewLeaveRegisterDate = listDate.Where(x => x.Year == year).ToList();
                            var yearNewLeaveRegister = employeeNewLeaveRegisterDate.Sum(x =>
                            {
                                var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.Date)?.Item2 ?? 0;
                                if (employeeShiftIndex != 0)
                                {
                                    var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                    if (shift != null)
                                    {
                                        return (shift.TheoryWorkedTimeByShift * 
                                            (param.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                    }
                                    return 0;
                                }
                                return 0;
                            });

                            //if (employeeLeaveRegistered.Count >= annualLeave.AnnualLeave
                            //    || (employeeLeaveRegistered.Count + employeeNewLeaveRegisterDate.Count) > annualLeave.AnnualLeave)
                            if (yearLeaveRegistered >= annualLeave.AnnualLeave
                                || (yearLeaveRegistered + yearNewLeaveRegister) > annualLeave.AnnualLeave)
                            {
                                if (result.ContainsKey(id))
                                {
                                    result[id].Add("AnnualLeaveNotEnough");
                                }
                                else
                                {
                                    result.Add(id, new HashSet<string> { "AnnualLeaveNotEnough" });
                                }
                            }
                        }
                    }
                    foreach (var date in listDate)
                    {
                        var employeeLeaveRegistered = listLeaveRegistered.Where(x => x.EmployeeATID == id
                            && x.LeaveDate.Year == date.Year && x.LeaveDate.Month == date.Month).ToList();
                        var monthLeaveRegistered = employeeLeaveRegistered.Sum(x =>
                        {
                            var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.LeaveDate.Date)?.Item2 ?? 0;
                            if (employeeShiftIndex != 0)
                            {
                                var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                if (shift != null)
                                {
                                    return (shift.TheoryWorkedTimeByShift *
                                            (x.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                }
                                return 0;
                            }
                            return 0;
                        });

                        var employeeNewLeaveRegisterDate = listDate.Where(x => x.Year == date.Year && x.Month == date.Month).ToList();
                        var monthNewLeaveRegister = employeeNewLeaveRegisterDate.Sum(x =>
                        {
                            var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.Date)?.Item2 ?? 0;
                            if (employeeShiftIndex != 0)
                            {
                                var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                if (shift != null)
                                {
                                    return (shift.TheoryWorkedTimeByShift *
                                            (param.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                }
                                return 0;
                            }
                            return 0;
                        });

                        //if (employeeLeaveRegistered.Count >= rule.MaximumAnnualLeaveRegisterByMonth
                        //    || (employeeLeaveRegistered.Count + employeeNewLeaveRegisterDate.Count) > rule.MaximumAnnualLeaveRegisterByMonth)
                        if (monthLeaveRegistered >= rule.MaximumAnnualLeaveRegisterByMonth
                            || (monthLeaveRegistered + monthNewLeaveRegister) > rule.MaximumAnnualLeaveRegisterByMonth)
                        {
                            if (result.ContainsKey(id))
                            {
                                result[id].Add("AnnualPerMonthLeaveNotEnough");
                            }
                            else
                            {
                                result.Add(id, new HashSet<string> { "AnnualPerMonthLeaveNotEnough" });
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool ExportTemplateLeaveRegister(string folderDetails)
        {
            try
            {
                var employeeTypeList = _dbContext.TA_LeaveDateType.AsNoTracking().Select(x => x.Name).OrderByDescending(x => x).ToList();

                using (var workbook = new XLWorkbook(folderDetails))
                {
                    var worksheet = workbook.Worksheets;
                    IXLWorksheet worksheet1;
                    IXLWorksheet worksheet5;

                    var w1 = worksheet.TryGetWorksheet("LeaveDateTypeData", out worksheet1);
                    worksheet1.Cells().Clear();
                    string startCanteenCell = "A1";
                    string endCanteenCell = string.Empty;
                    for (int i = 0; i < employeeTypeList.Count; i++)
                    {
                        if (i == (employeeTypeList.Count - 1))
                        {
                            endCanteenCell = "A" + (i + 1);
                        }
                        worksheet1.Cell("A" + (i + 1)).Value = employeeTypeList[i];
                    }

                    var w = worksheet.TryGetWorksheet("Data", out worksheet5);
                    if (!string.IsNullOrWhiteSpace(startCanteenCell) && !string.IsNullOrWhiteSpace(endCanteenCell))
                        worksheet5.Range("F3:F10003").SetDataValidation().List(worksheet1.Range(startCanteenCell
                            + ":" + endCanteenCell), true);

                    workbook.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ExportTemplateLeaveRegister: " + ex.ToString());
                return false;
            }
        }

        public async Task<List<LeaveRegistrationModel>> ValidationImportLeaveRegistration(List<LeaveRegistrationModel> param, UserInfo user)
        {
            var errorList = new List<LeaveRegistrationModel>();
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

            var listLeaveDateTypeName = param.Select(x => x.LeaveDateTypeName).ToList();
            var listLeaveDateType = await _dbContext.TA_LeaveDateType.Where(x 
                => listLeaveDateTypeName.Contains(x.Name) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

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

                if (!string.IsNullOrWhiteSpace(item.LeaveDateTypeName))
                {
                    if (!listLeaveDateType.Any(x => x.Name == item.LeaveDateTypeName))
                    {
                        item.ErrorMessage += "Loại ngày nghỉ không tồn tại\r\n";
                    }
                    else
                    {
                        item.LeaveDateType = listLeaveDateType.FirstOrDefault(x => x.Name == item.LeaveDateTypeName).Index;
                    }
                }
                else
                {
                    item.ErrorMessage += "Loại ngày nghỉ không được để trống\r\n";
                }

                if (item.LeaveDurationType == (short)LeaveDurationType.LeaveHaftShift)
                {
                    if (!item.FirstHaftLeave && !item.LastHaftLeave)
                    {
                        item.ErrorMessage += "Nhân viên chưa chọn đầu ca hoặc cuối ca khi nghỉ nửa ngày\r\n";
                    }
                    else if (item.FirstHaftLeave && item.LastHaftLeave)
                    {
                        item.ErrorMessage += "Chỉ có thể chọn đầu ca hoặc cuối ca khi nghỉ nửa ngày\r\n";
                    }
                    else if (item.FirstHaftLeave && !item.LastHaftLeave)
                    {
                        item.HaftLeaveType = (short)HaftLeaveType.LeaveFirstHaftShift;
                    }
                    else if (!item.FirstHaftLeave && item.LastHaftLeave)
                    {
                        item.HaftLeaveType = (short)HaftLeaveType.LeaveLastHaftShift;
                    }
                }
                else if (item.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift 
                    && (item.FirstHaftLeave || item.LastHaftLeave))
                {
                    item.ErrorMessage += "Nhân viên đăng ký nhiều loại nghỉ cùng lúc\r\n";
                }
            }

            var minFromDate = param.Min(x => x.FromDate);
            var maxToDate = param.Max(x => x.ToDate);

            var lockAttendanceTimeValidDate = await GetLockAttendanceTimeValidDate(user);

            var dicEmployeeShift = new Dictionary<string, List<Tuple<DateTime, int>>>();
            var listEmployeeATID = param.Select(x => x.EmployeeATID).Distinct().ToList();
            if (listEmployeeATID.Count == 0)
            {
                errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                return errorList;
            }

            var listLeaveRegistered = await _dbContext.TA_LeaveRegistration.Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            var listAnnualLeave = await _dbContext.TA_AnnualLeave.AsNoTracking().Where(x
                => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex).ToListAsync();

            if (minFromDate.HasValue && maxToDate.HasValue)
            {
                var listDate = DateTimeExtension.GetListDate(minFromDate.Value, maxToDate.Value);
                var listYear = listDate.Select(x => x.Year).ToHashSet();

                var minYearFromDate = new DateTime(listYear.Min(), 1, 1);
                var maxYearToDate = new DateTime(listYear.Max(), 12, 31);
                var listFullDate = DateTimeExtension.GetListDate(minYearFromDate, maxYearToDate);

                //var listBusinessRegistered = await GetBusinessRegistrationByEmployeeATIDsAndDates(listEmployeeATID, listDate, user.CompanyIndex);
                var listBusinessRegistered = await GetBusinessRegistrationByEmployeeATIDsAndDates(listEmployeeATID, listFullDate, user.CompanyIndex);

                var employeeInfos = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(listEmployeeATID, 
                    new DateTime(), user.CompanyIndex);
                var scheduleByDepartments = new List<TA_ScheduleFixedByDepartment>();
                if (employeeInfos != null && employeeInfos.Count > 0 
                    && employeeInfos.Any(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0))
                {
                    var listDepartment = employeeInfos.Where(x => x.DepartmentIndex.HasValue && x.DepartmentIndex.Value > 0).Select(x
                        => x.DepartmentIndex).ToList();
                    //scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                    //    => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                    //    && minFromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    //    && maxToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
                    scheduleByDepartments = await _dbContext.TA_ScheduleFixedByDepartment.AsNoTracking().Where(x
                        => listDepartment.Contains(x.DepartmentIndex) && x.CompanyIndex == user.CompanyIndex
                        && minYearFromDate.Date <= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                        && maxYearToDate.Date >= x.ToDate.Value.Date))).ToListAsync();
                }

                //var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                //    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                //    && minFromDate.Value.Date >= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                //    && maxToDate.Value.Date <= x.ToDate.Value.Date))).ToListAsync();
                var scheduleByEmployees = await _dbContext.TA_ScheduleFixedByEmployee.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && minYearFromDate.Date <= x.FromDate.Date && (!x.ToDate.HasValue || (x.ToDate.HasValue
                    && maxYearToDate.Date >= x.ToDate.Value.Date))).ToListAsync();

                //var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                //    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                //    && x.Date.Date >= minFromDate.Value.Date && x.Date.Date <= maxToDate.Value.Date).ToListAsync();
                var employeeShifts = await _dbContext.TA_EmployeeShift.AsNoTracking().Where(x
                    => listEmployeeATID.Contains(x.EmployeeATID) && x.CompanyIndex == user.CompanyIndex
                    && x.Date.Date >= minYearFromDate.Date && x.Date.Date <= maxToDate.Value.Date).ToListAsync();

                var listShiftIndex = employeeShifts.Select(x => x?.ShiftIndex ?? 0).ToList();

                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Monday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Tuesday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Wednesday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Thursday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Friday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Saturday).ToList());
                listShiftIndex.AddRange(scheduleByEmployees.Select(x => x.Sunday).ToList());

                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Monday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Tuesday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Wednesday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Thursday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Friday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Saturday).ToList());
                listShiftIndex.AddRange(scheduleByDepartments.Select(x => x.Sunday).ToList());

                listShiftIndex = listShiftIndex.Distinct().ToList();

                var listShift = await _dbContext.TA_Shift.Where(x
                    => listShiftIndex.Contains(x.Index)).ToListAsync();

                foreach (var id in listEmployeeATID)
                {
                    var employeeInfo = employeeInfos.FirstOrDefault(x => x.EmployeeATID == id);
                    var listEmployeeShift = new List<Tuple<DateTime, int>>();
                    //foreach (var date in listDate)
                    foreach (var date in listFullDate)
                    {
                        var shiftIndex = 0;
                        var employeeShift = employeeShifts.FirstOrDefault(x => x.EmployeeATID == id && x.Date.Date == date.Date);
                        if (employeeShift != null)
                        {
                            shiftIndex = employeeShift.ShiftIndex.HasValue ? employeeShift.ShiftIndex.Value : 0;
                            //continue;
                        }

                        if (shiftIndex == 0)
                        {
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
                                //continue;
                            }
                        }

                        if (shiftIndex == 0)
                        {
                            if (employeeInfo != null && employeeInfo.DepartmentIndex.HasValue && employeeInfo.DepartmentIndex.Value > 0)
                            {
                                var scheduleByDepartment = scheduleByDepartments.FirstOrDefault(x
                                    => x.DepartmentIndex == employeeInfo.DepartmentIndex.Value
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
                        }

                        listEmployeeShift.Add(new Tuple<DateTime, int>(date, shiftIndex));

                        //if (listDate.Any(y => y.Date == date.Date) && shiftIndex == 0)
                        //{
                        //    if (result.ContainsKey(id))
                        //    {
                        //        result[id].Add("EmployeeNotHaveShift");
                        //    }
                        //    else
                        //    {
                        //        result.Add(id, new HashSet<string> { "EmployeeNotHaveShift" });
                        //    }
                        //}
                    }
                    if (!dicEmployeeShift.ContainsKey(id))
                    {
                        dicEmployeeShift.Add(id, listEmployeeShift);
                    }
                    else
                    {
                        dicEmployeeShift[id] = listEmployeeShift;
                    }
                }

                var rule = await _dbContext.TA_Rules_Global.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);

                var listExistParam = new List<LeaveRegistrationModel>();
                foreach (var item in param)
                {
                    var employeeListShift = dicEmployeeShift[item.EmployeeATID];
                    var employeeInfo = employeeInfos.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    if (item.FromDate.HasValue && item.ToDate.HasValue && item.LeaveDateType > 0)
                    {
                        //if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date <= item.ToDate.Value.Date
                        //    && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                        //    && x.FromDate.Value.Date <= x.ToDate.Value.Date && !(item.ToDate.Value.Date < x.FromDate.Value.Date
                        //    && item.FromDate.Value.Date > x.ToDate.Value.Date) && x.LeaveDateType == item.LeaveDateType))
                        //{
                        //    item.ErrorMessage += "Dữ liệu nhân viên đăng ký nghỉ bị trùng trong tệp tin\r\n";
                        //}
                        //else if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date <= item.ToDate.Value.Date
                        //    && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                        //    && x.FromDate.Value.Date <= x.ToDate.Value.Date && !(item.ToDate.Value.Date < x.FromDate.Value.Date
                        //    && item.FromDate.Value.Date > x.ToDate.Value.Date) && x.LeaveDateType != item.LeaveDateType))
                        //{
                        //    item.ErrorMessage += "Nhân viên đăng ký nhiều loại ngày nghỉ cùng lúc\r\n";
                        //}

                        if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date <= item.ToDate.Value.Date
                            && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                            && x.LeaveDateType == item.LeaveDateType
                            && DoRangesOverlap(item.FromDate.Value.Date, item.ToDate.Value.Date, x.FromDate.Value.Date, x.ToDate.Value.Date)))
                        {
                            item.ErrorMessage += "Dữ liệu nhân viên đăng ký nghỉ bị trùng trong tệp tin\r\n";
                        }
                        else if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value.Date <= item.ToDate.Value.Date
                            && listExistParam.Any(x => x.EmployeeATID == item.EmployeeATID
                            && x.LeaveDateType != item.LeaveDateType
                            && DoRangesOverlap(item.FromDate.Value.Date, item.ToDate.Value.Date, x.FromDate.Value.Date, x.ToDate.Value.Date)))
                        {
                            item.ErrorMessage += "Nhân viên đăng ký nhiều loại ngày nghỉ cùng lúc\r\n";
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
                                if (listBusinessRegistered.Any(x => x.EmployeeATID == item.EmployeeATID && x.BusinessDate.Date == date.Date))
                                {
                                    var businessRegistered = listBusinessRegistered.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                                        && x.BusinessDate.Date == date.Date);
                                    if (businessRegistered.BusinessType == (short)BusinessType.BusinessAllShift)
                                    {
                                        item.ErrorMessage += "Nhân viên đã đăng ký công tác nguyên ngày, không thể đăng ký nghỉ\r\n";
                                    }
                                    else if (businessRegistered.BusinessType == (short)BusinessType.BusinessFromToTime
                                        && item.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift)
                                    {
                                        item.ErrorMessage += "Nhân viên đã đăng ký công tác, không thể đăng ký nghỉ nguyên ngày\r\n";
                                    }
                                }

                                var shiftIndex = 0;

                                var employeeDateShift = employeeListShift.FirstOrDefault(x => x.Item1.Date == date.Date);
                                if (employeeDateShift != null)
                                {
                                    shiftIndex = employeeDateShift.Item2;
                                }

                                if (shiftIndex == 0)
                                {
                                    item.ErrorMessage += "Nhân viên không có ca làm việc trong ngày " + date.ToddMMyyyy() + "\r\n";
                                }
                            }

                            var itemLeaveDateType = listLeaveDateType.FirstOrDefault(x => x.Index == item.LeaveDateType);
                            if (itemLeaveDateType != null && itemLeaveDateType.IsPaidLeave)
                            {
                                var annualLeave = listAnnualLeave.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                                if (annualLeave != null)
                                {
                                    foreach (var year in listYear)
                                    {
                                        var employeeLeaveRegistered = listLeaveRegistered.Where(x => x.EmployeeATID == item.EmployeeATID
                                            && x.LeaveDate.Year == year).ToList();
                                        var yearLeaveRegistered = employeeLeaveRegistered.Sum(x =>
                                        {
                                            var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.LeaveDate.Date)?.Item2 ?? 0;
                                            if (employeeShiftIndex != 0)
                                            {
                                                var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                                if (shift != null)
                                                {
                                                    return (shift.TheoryWorkedTimeByShift *
                                                        (x.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                                }
                                                return 0;
                                            }
                                            return 0;
                                        });

                                        var employeeNewLeaveRegisterDate = itemListDate.Where(x => x.Year == year).ToList();
                                        var yearNewLeaveRegister = employeeNewLeaveRegisterDate.Sum(x =>
                                        {
                                            var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.Date)?.Item2 ?? 0;
                                            if (employeeShiftIndex != 0)
                                            {
                                                var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                                if (shift != null)
                                                {
                                                    return (shift.TheoryWorkedTimeByShift *
                                                        (item.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                                }
                                                return 0;
                                            }
                                            return 0;
                                        });

                                        //if (employeeLeaveRegistered.Count >= annualLeave.AnnualLeave
                                        //    || (employeeLeaveRegistered.Count + employeeNewLeaveRegisterDate.Count) > annualLeave.AnnualLeave)
                                        if (yearLeaveRegistered >= annualLeave.AnnualLeave
                                            || (yearLeaveRegistered + yearNewLeaveRegister) > annualLeave.AnnualLeave)
                                        {
                                            item.ErrorMessage += "Không đủ ngày phép năm đăng ký\r\n";
                                        }
                                    }
                                }
                                foreach (var date in itemListDate)
                                {
                                    var employeeLeaveRegistered = listLeaveRegistered.Where(x => x.EmployeeATID == item.EmployeeATID
                                        && x.LeaveDate.Year == date.Year && x.LeaveDate.Month == date.Month).ToList();
                                    var monthLeaveRegistered = employeeLeaveRegistered.Sum(x =>
                                    {
                                        var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.LeaveDate.Date)?.Item2 ?? 0;
                                        if (employeeShiftIndex != 0)
                                        {
                                            var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                            if (shift != null)
                                            {
                                                return (shift.TheoryWorkedTimeByShift *
                                                        (x.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                            }
                                            return 0;
                                        }
                                        return 0;
                                    });

                                    var employeeNewLeaveRegisterDate = itemListDate.Where(x => x.Year == date.Year && x.Month == date.Month).ToList();
                                    var monthNewLeaveRegister = employeeNewLeaveRegisterDate.Sum(x =>
                                    {
                                        var employeeShiftIndex = employeeListShift.FirstOrDefault(y => y.Item1.Date == x.Date)?.Item2 ?? 0;
                                        if (employeeShiftIndex != 0)
                                        {
                                            var shift = listShift.FirstOrDefault(y => y.Index == employeeShiftIndex);
                                            if (shift != null)
                                            {
                                                return (shift.TheoryWorkedTimeByShift *
                                                        (item.LeaveDurationType == (short)LeaveDurationType.LeaveAllShift ? 1 : 0.5));
                                            }
                                            return 0;
                                        }
                                        return 0;
                                    });

                                    //if (employeeLeaveRegistered.Count >= rule.MaximumAnnualLeaveRegisterByMonth
                                    //    || (employeeLeaveRegistered.Count + employeeNewLeaveRegisterDate.Count) > rule.MaximumAnnualLeaveRegisterByMonth)
                                    if (monthLeaveRegistered >= rule.MaximumAnnualLeaveRegisterByMonth
                                        || (monthLeaveRegistered + monthNewLeaveRegister) > rule.MaximumAnnualLeaveRegisterByMonth)
                                    {
                                        item.ErrorMessage += "Không đủ ngày phép năm đăng ký trong tháng theo quy định\r\n";
                                    }
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
                        var existedRegister = listLeaveRegistered.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID
                            && x.LeaveDate.Date == date.Date);
                        if (existedRegister != null)
                        {
                            existedRegister.LeaveDateType = item.LeaveDateType;
                            existedRegister.LeaveDurationType = item.LeaveDurationType;
                            existedRegister.HaftLeaveType = item.HaftLeaveType;
                            existedRegister.Description = item.Description;
                            existedRegister.UpdatedDate = DateTime.Now;
                            existedRegister.UpdatedUser = user.FullName;

                            DbContext.TA_LeaveRegistration.Update(existedRegister);
                        }
                        else
                        {
                            var newLeaveRegistration = new TA_LeaveRegistration();
                            newLeaveRegistration.EmployeeATID = item.EmployeeATID;
                            newLeaveRegistration.LeaveDate = date;
                            newLeaveRegistration.Description = item.Description;
                            newLeaveRegistration.CompanyIndex = user.CompanyIndex;
                            newLeaveRegistration.LeaveDateType = item.LeaveDateType;
                            newLeaveRegistration.LeaveDurationType = item.LeaveDurationType;
                            newLeaveRegistration.HaftLeaveType = item.HaftLeaveType;
                            newLeaveRegistration.CreatedDate = DateTime.Now;
                            newLeaveRegistration.UpdatedDate = DateTime.Now;
                            newLeaveRegistration.UpdatedUser = user.FullName;
                            await _dbContext.TA_LeaveRegistration.AddAsync(newLeaveRegistration);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
            }

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            return errorList;
        }

        public async Task<List<TA_LeaveDateType>> GetAllLeaveDateType()
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().ToListAsync();
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
