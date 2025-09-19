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
    public class GC_Employee_AccessedGroupService : BaseServices<GC_Employee_AccessedGroup, EPAD_Context>, IGC_Employee_AccessedGroupService
    {
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        public EPAD_Context _dbContext;
        private ILogger _logger;
        public GC_Employee_AccessedGroupService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_CustomerInfoService = serviceProvider.GetService<IHR_CustomerInfoService>();
            _logger = loggerFactory.CreateLogger<GC_Employee_AccessedGroupService>();
        }

        public async Task<List<GC_Employee_AccessedGroup>> GetByEmployeeAndFromToDate(List<string> employeeATIDsList, DateTime pFromDate, DateTime? pToDate, UserInfo user)
        {
            if (pToDate == null) pToDate = DateTime.MaxValue;
            var dummy = Where(x => x.CompanyIndex == user.CompanyIndex
            && employeeATIDsList.Contains(x.EmployeeATID)
            && x.FromDate.Date <= pToDate.Value.Date
            && (x.ToDate == null || x.ToDate >= pFromDate)).OrderByDescending(x => x.FromDate).ToList();
            return await Task.FromResult(dummy);
        }

        public async Task<DataGridClass> GetEmployeeAccessedGroup(int page, int pageSize, string filter, UserInfo user)
        {
            var data = new List<EmployeeAccessedGroupModel>();
            var employeeAccessedGroupList = await GetDataByCompanyIndex(user.CompanyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                employeeAccessedGroupList = employeeAccessedGroupList.Where(x => x.EmployeeATID.Contains(filter)).ToList();
            }
            var accessedGroup = DbContext.GC_AccessedGroup.Where(x => employeeAccessedGroupList.Select(x => x.AccessedGroupIndex).Contains(x.Index)).ToList();
            var employeeATIDs = employeeAccessedGroupList.Select(x => x.EmployeeATID).ToHashSet();
            //var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex && employeeATIDs.Contains(x.EmployeeATID))
            //                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
            //                && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
            //                on e.EmployeeATID equals w.EmployeeATID into workingInfoGroup
            //                from wrk in workingInfoGroup.DefaultIfEmpty()
            //                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
            //                on wrk.DepartmentIndex equals d.Index into deptGroup
            //                from dept in deptGroup.DefaultIfEmpty()
            //                select new EmployeeFullInfo()
            //                {
            //                    EmployeeATID = e.EmployeeATID,
            //                    FullName = e.FullName,
            //                    JoinedDate = null,
            //                    DepartmentIndex = wrk.DepartmentIndex,
            //                    Department = dept.Name,
            //                    DepartmentCode = dept.Code,
            //                    FromDate = wrk.FromDate,
            //                    ToDate = wrk.ToDate,
            //                    CompanyIndex = e.CompanyIndex,
            //                    UserName = e.UserName
            //                };
            //var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex).Select(x => (long)x.Index).ToHashSet();
            //listAllDepartmentIndex.Add(0);
            //queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
            //    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

            //var resultShowStoppedEmp = _HR_UserService.ShowStoppedWorkingEmployeesData();
            //if (resultShowStoppedEmp.Item1)
            //{
            //    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
            //    {
            //        queryData = queryData.Where(x => !x.ToDate.HasValue
            //        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
            //        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
            //        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
            //    }
            //    else
            //    {
            //        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
            //    }
            //}
            //else
            //{
            //    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
            //}
            //var employeeInfoList = queryData.ToList();
            var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);
            foreach (var employeeAccessedGroup in employeeAccessedGroupList)
            {
                var item = new EmployeeAccessedGroupModel().PopulateWith(accessedGroup);
                item.Index = employeeAccessedGroup.Index;
                item.EmployeeATID = employeeAccessedGroup.EmployeeATID;
                item.EmployeeCode = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.EmployeeCode;
                item.EmployeeName = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.FullName;
                item.DepartmentIndex = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.DepartmentIndex;
                item.DepartmentName = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.Department ?? string.Empty;
                item.CompanyIndex = user.CompanyIndex;
                item.FromDate = employeeAccessedGroup.FromDate;
                item.ToDate = employeeAccessedGroup.ToDate;
                item.FromDateFormat = employeeAccessedGroup.FromDate.ToddMMyyyy();
                item.ToDateFormat = employeeAccessedGroup.ToDate.HasValue ? employeeAccessedGroup.ToDate.Value.ToddMMyyyy() : "";
                item.AccessedGroupName = accessedGroup.FirstOrDefault(x => x.Index == employeeAccessedGroup.AccessedGroupIndex)?.Name;
                item.AccessedGroupIndex = employeeAccessedGroup.AccessedGroupIndex;
                data.Add(item);
            }

            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = data.Count;
            var dataResult = data.Skip(skip).Take(pageSize).ToList();

            var grid = new DataGridClass(countTotal, dataResult);
            return grid;
        }

        public async Task<DataGridClass> GetEmployeeAccessedGroupByFilter(int page, int pageSize, string filter,
            DateTime? fromDate, DateTime? toDate, List<long> listDepartment, List<string> listEmployeeATID, UserInfo user)
        {
            var data = new List<EmployeeAccessedGroupModel>();
            var employeeAccessedGroupList = await GetDataByCompanyIndex(user.CompanyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                employeeAccessedGroupList = employeeAccessedGroupList.Where(x => x.EmployeeATID.Contains(filter)).ToList();
            }
            if (listEmployeeATID != null && listEmployeeATID.Count > 0)
            {
                employeeAccessedGroupList = employeeAccessedGroupList.Where(x => listEmployeeATID.Contains(x.EmployeeATID)).ToList();
            }
            var accessedGroup = await DbContext.GC_AccessedGroup.Where(x
                => employeeAccessedGroupList.Select(x => x.AccessedGroupIndex).Contains(x.Index)).ToListAsync();
            var employeeATIDs = employeeAccessedGroupList.Select(x => x.EmployeeATID).ToHashSet();
            var employeeInfoList = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);
            foreach (var employeeAccessedGroup in employeeAccessedGroupList)
            {
                var item = new EmployeeAccessedGroupModel().PopulateWith(accessedGroup);
                item.Index = employeeAccessedGroup.Index;
                item.EmployeeATID = employeeAccessedGroup.EmployeeATID;
                item.EmployeeCode = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.EmployeeCode;
                item.EmployeeName = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.FullName;
                item.DepartmentIndex = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.DepartmentIndex;
                item.DepartmentName = employeeInfoList.FirstOrDefault(x => x.EmployeeATID == employeeAccessedGroup.EmployeeATID)?.Department ?? string.Empty;
                item.CompanyIndex = user.CompanyIndex;
                item.FromDate = employeeAccessedGroup.FromDate;
                item.ToDate = employeeAccessedGroup.ToDate;
                item.FromDateFormat = employeeAccessedGroup.FromDate.ToddMMyyyy();
                item.ToDateFormat = employeeAccessedGroup.ToDate.HasValue ? employeeAccessedGroup.ToDate.Value.ToddMMyyyy() : "";
                item.AccessedGroupName = accessedGroup.FirstOrDefault(x => x.Index == employeeAccessedGroup.AccessedGroupIndex)?.Name;
                item.AccessedGroupIndex = employeeAccessedGroup.AccessedGroupIndex;
                data.Add(item);
            }

            if (data != null && data.Count > 0 && listDepartment != null && listDepartment.Count > 0
                && (listEmployeeATID == null || listEmployeeATID.Count == 0))
            {
                if (listDepartment.Contains(0))
                {
                    data = data.Where(x => (x.DepartmentIndex.HasValue && listDepartment.Contains(x.DepartmentIndex.Value))
                        || !x.DepartmentIndex.HasValue).ToList();
                }
                else
                {
                    data = data.Where(x => x.DepartmentIndex.HasValue && listDepartment.Contains(x.DepartmentIndex.Value)).ToList();
                }
            }

            if (fromDate.HasValue)
            {
                data = data.Where(x => x.FromDate.Date >= fromDate.Value.Date).ToList();
            }

            if (toDate.HasValue)
            {
                data = data.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date <= toDate.Value.Date)).ToList();
            }

            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = data.Count;
            var dataResult = data.Skip(skip).Take(pageSize).ToList();

            var grid = new DataGridClass(countTotal, dataResult);
            return grid;
        }

        public async Task<List<GC_Employee_AccessedGroup>> GetDataByCompanyIndex(int companyIndex)
        {
            return await DbContext.GC_Employee_AccessedGroup.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public EmployeeAccessRule GetInfoByEmpATIDAndFromDate(string pEmployeeATID, DateTime pDate, int pCompanyIndex)
        {
            var rulesAreaGroup = DbContext.GC_Rules_General_AreaGroup.Where(e => e.CompanyIndex == pCompanyIndex);
            var query = from employeeAccess in DbContext.GC_Employee_AccessedGroup
                            .Where(t => t.CompanyIndex == pCompanyIndex && t.EmployeeATID == pEmployeeATID && t.FromDate.Date <= pDate.Date && (t.ToDate == null || t.ToDate.Value.Date >= pDate.Date))
                        from accessGroup in DbContext.GC_AccessedGroup.Where(accessGroup => employeeAccess.AccessedGroupIndex == accessGroup.Index).DefaultIfEmpty()
                        from rule in DbContext.GC_Rules_GeneralAccess.Where(rule => accessGroup.GeneralAccessRuleIndex == rule.Index).DefaultIfEmpty()
                        from parkingRule in DbContext.GC_Rules_ParkingLot.Where(parkingRule => accessGroup.ParkingLotRuleIndex == parkingRule.Index).DefaultIfEmpty()
                            //from areaGroupRule in _DBContext.GC_Rules_General_AreaGroup.Where(areaGroupRule => areaGroupRule.CompanyIndex == pCompanyIndex && areaGroupRule.Rules_GeneralIndex == rule.Index).OrderBy(e => e.Priority).DefaultIfEmpty()
                        select new EmployeeAccessRule()
                        {
                            EmployeeATID = employeeAccess.EmployeeATID,
                            AccessedGroupIndex = accessGroup.Index,
                            FromDate = employeeAccess.FromDate,
                            ToDate = employeeAccess.ToDate,
                            GeneralAccessRuleIndex = rule.Index,
                            ParkingLotIndex = parkingRule.Index,
                            /*  các qui định về thời gian vào ra cho phép  */
                            CheckInByShift = rule.CheckInByShift,
                            CheckInTime = rule.CheckInTime,
                            MaxEarlyCheckInMinute = rule.MaxEarlyCheckInMinute,
                            MaxLateCheckInMinute = rule.MaxLateCheckInMinute,
                            CheckOutByShift = rule.CheckOutByShift,
                            CheckOutTime = rule.CheckOutTime,
                            MaxEarlyCheckOutMinute = rule.MaxEarlyCheckOutMinute,
                            MaxLateCheckOutMinute = rule.MaxLateCheckOutMinute,
                            AllowEarlyOutLateInMission = rule.AllowEarlyOutLateInMission,
                            AllowFreeInAndOutInTimeRange = rule.AllowFreeInAndOutInTimeRange,
                            MissionMaxEarlyCheckOutMinute = rule.MissionMaxEarlyCheckOutMinute,
                            MissionMaxLateCheckInMinute = rule.MissionMaxLateCheckInMinute,
                            AdjustByLateInEarlyOut = rule.AdjustByLateInEarlyOut,
                            BeginLastHaftTime = rule.BeginLastHaftTime,
                            EndFirstHaftTime = rule.EndFirstHaftTime,
                            /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
                            AllowInLeaveDay = rule.AllowInLeaveDay,
                            AllowInMission = rule.AllowInMission,
                            AllowInBreakTime = rule.AllowInBreakTime,
                            /*  các qui định liên quan đến ra giữa giờ không đăng ký */
                            AllowCheckOutInWorkingTime = rule.AllowCheckOutInWorkingTime,
                            AllowCheckOutInWorkingTimeRange = rule.AllowCheckOutInWorkingTimeRange,
                            MaxMinuteAllowOutsideInWorkingTime = rule.MaxMinuteAllowOutsideInWorkingTime,
                            /*  các qui định cấm vào ra  */
                            DenyInLeaveWholeDay = rule.DenyInLeaveWholeDay,
                            DenyInMissionWholeDay = rule.DenyInMissionWholeDay,
                            DenyInStoppedWorkingInfo = rule.DenyInStoppedWorkingInfo,
                            /*  các quy định nhà xe*/
                            UseTimeLimitParking = parkingRule.UseTimeLimitParking ?? false,
                            LimitDayNumber = parkingRule.LimitDayNumber ?? 0,
                            UseCardDependent = parkingRule.UseCardDependent ?? false,
                            UseRequiredParkingLotAccessed = parkingRule.UseRequiredParkingLotAccessed ?? false,
                            UseRequiredEmployeeVehicle = parkingRule.UseRequiredEmployeeVehicle ?? false,

                            CheckLogByShift = rule.CheckLogByShift,
                            CheckLogByAreaGroup = rule.CheckLogByAreaGroup,
                        };

            var data = query.FirstOrDefault();
            if (data != null)
            {
                data.AreaGroups = GetAreaGroupByRulesIndex(rulesAreaGroup, data.GeneralAccessRuleIndex, pCompanyIndex);
            }
            return data;

        }

        public EmployeeAccessRule GetInfoByDriver(int pCompanyIndex, string employeeATID)
        {
            try
            {
                var rulesAreaGroup = DbContext.GC_Rules_General_AreaGroup.Where(e => e.CompanyIndex == pCompanyIndex);
                var query = from accessGroup in DbContext.GC_AccessedGroup.Where(accessGroup => accessGroup.IsDriverDefaultGroup).DefaultIfEmpty()
                            from rule in DbContext.GC_Rules_GeneralAccess.Where(rule => accessGroup.GeneralAccessRuleIndex == rule.Index).DefaultIfEmpty()
                            from parkingRule in DbContext.GC_Rules_ParkingLot.Where(parkingRule => accessGroup.ParkingLotRuleIndex == parkingRule.Index).DefaultIfEmpty()
                                //from areaGroupRule in _DBContext.GC_Rules_General_AreaGroup.Where(areaGroupRule => areaGroupRule.CompanyIndex == pCompanyIndex && areaGroupRule.Rules_GeneralIndex == rule.Index).OrderBy(e => e.Priority).DefaultIfEmpty()
                            select new EmployeeAccessRule()
                            {
                                EmployeeATID = employeeATID,
                                AccessedGroupIndex = accessGroup.Index,
                                GeneralAccessRuleIndex = rule.Index,
                                ParkingLotIndex = parkingRule.Index,
                                /*  các qui định về thời gian vào ra cho phép  */
                                CheckInByShift = rule.CheckInByShift,
                                CheckInTime = rule.CheckInTime,
                                MaxEarlyCheckInMinute = rule.MaxEarlyCheckInMinute,
                                MaxLateCheckInMinute = rule.MaxLateCheckInMinute,
                                CheckOutByShift = rule.CheckOutByShift,
                                CheckOutTime = rule.CheckOutTime,
                                MaxEarlyCheckOutMinute = rule.MaxEarlyCheckOutMinute,
                                MaxLateCheckOutMinute = rule.MaxLateCheckOutMinute,
                                AllowFreeInAndOutInTimeRange = rule.AllowFreeInAndOutInTimeRange,
                                AllowEarlyOutLateInMission = rule.AllowEarlyOutLateInMission,
                                MissionMaxEarlyCheckOutMinute = rule.MissionMaxEarlyCheckOutMinute,
                                MissionMaxLateCheckInMinute = rule.MissionMaxLateCheckInMinute,
                                AdjustByLateInEarlyOut = rule.AdjustByLateInEarlyOut,
                                BeginLastHaftTime = rule.BeginLastHaftTime,
                                EndFirstHaftTime = rule.EndFirstHaftTime,
                                /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
                                AllowInLeaveDay = rule.AllowInLeaveDay,
                                AllowInMission = rule.AllowInMission,
                                AllowInBreakTime = rule.AllowInBreakTime,
                                /*  các qui định liên quan đến ra giữa giờ không đăng ký */
                                AllowCheckOutInWorkingTime = rule.AllowCheckOutInWorkingTime,
                                AllowCheckOutInWorkingTimeRange = rule.AllowCheckOutInWorkingTimeRange,
                                MaxMinuteAllowOutsideInWorkingTime = rule.MaxMinuteAllowOutsideInWorkingTime,
                                /*  các qui định cấm vào ra  */
                                DenyInLeaveWholeDay = rule.DenyInLeaveWholeDay,
                                DenyInMissionWholeDay = rule.DenyInMissionWholeDay,
                                DenyInStoppedWorkingInfo = rule.DenyInStoppedWorkingInfo,
                                /*  các quy định nhà xe*/
                                UseTimeLimitParking = parkingRule.UseTimeLimitParking ?? false,
                                LimitDayNumber = parkingRule.LimitDayNumber ?? 0,
                                UseCardDependent = parkingRule.UseCardDependent ?? false,
                                UseRequiredParkingLotAccessed = parkingRule.UseRequiredParkingLotAccessed ?? false,
                                UseRequiredEmployeeVehicle = parkingRule.UseRequiredEmployeeVehicle ?? false,

                                CheckLogByShift = rule.CheckLogByShift,
                                CheckLogByAreaGroup = rule.CheckLogByAreaGroup,
                            };

                var data = query.FirstOrDefault();
                if (data != null)
                {
                    data.AreaGroups = GetAreaGroupByRulesIndex(rulesAreaGroup, data.GeneralAccessRuleIndex, pCompanyIndex);
                }
                return data;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public EmployeeAccessRule GetInfoByGuest(int pCompanyIndex, string employeeATID)
        {
            try
            {
                var rulesAreaGroup = DbContext.GC_Rules_General_AreaGroup.Where(e => e.CompanyIndex == pCompanyIndex);
                var query = from accessGroup in DbContext.GC_AccessedGroup.Where(accessGroup => accessGroup.IsGuestDefaultGroup).DefaultIfEmpty()
                            from rule in DbContext.GC_Rules_GeneralAccess.Where(rule => accessGroup.GeneralAccessRuleIndex == rule.Index).DefaultIfEmpty()
                            from parkingRule in DbContext.GC_Rules_ParkingLot.Where(parkingRule => accessGroup.ParkingLotRuleIndex == parkingRule.Index).DefaultIfEmpty()
                                //from areaGroupRule in _DBContext.GC_Rules_General_AreaGroup.Where(areaGroupRule => areaGroupRule.CompanyIndex == pCompanyIndex && areaGroupRule.Rules_GeneralIndex == rule.Index).OrderBy(e => e.Priority).DefaultIfEmpty()
                            select new EmployeeAccessRule()
                            {
                                EmployeeATID = employeeATID,
                                AccessedGroupIndex = accessGroup.Index,
                                GeneralAccessRuleIndex = rule.Index,
                                ParkingLotIndex = parkingRule.Index,
                                /*  các qui định về thời gian vào ra cho phép  */
                                CheckInByShift = rule.CheckInByShift,
                                CheckInTime = rule.CheckInTime,
                                MaxEarlyCheckInMinute = rule.MaxEarlyCheckInMinute,
                                MaxLateCheckInMinute = rule.MaxLateCheckInMinute,
                                CheckOutByShift = rule.CheckOutByShift,
                                CheckOutTime = rule.CheckOutTime,
                                MaxEarlyCheckOutMinute = rule.MaxEarlyCheckOutMinute,
                                MaxLateCheckOutMinute = rule.MaxLateCheckOutMinute,
                                AllowFreeInAndOutInTimeRange = rule.AllowFreeInAndOutInTimeRange,
                                AllowEarlyOutLateInMission = rule.AllowEarlyOutLateInMission,
                                MissionMaxEarlyCheckOutMinute = rule.MissionMaxEarlyCheckOutMinute,
                                MissionMaxLateCheckInMinute = rule.MissionMaxLateCheckInMinute,
                                AdjustByLateInEarlyOut = rule.AdjustByLateInEarlyOut,
                                BeginLastHaftTime = rule.BeginLastHaftTime,
                                EndFirstHaftTime = rule.EndFirstHaftTime,
                                /*  các qui định liên quan đến ra giữa giờ có đăng ký  */
                                AllowInLeaveDay = rule.AllowInLeaveDay,
                                AllowInMission = rule.AllowInMission,
                                AllowInBreakTime = rule.AllowInBreakTime,
                                /*  các qui định liên quan đến ra giữa giờ không đăng ký */
                                AllowCheckOutInWorkingTime = rule.AllowCheckOutInWorkingTime,
                                AllowCheckOutInWorkingTimeRange = rule.AllowCheckOutInWorkingTimeRange,
                                MaxMinuteAllowOutsideInWorkingTime = rule.MaxMinuteAllowOutsideInWorkingTime,
                                /*  các qui định cấm vào ra  */
                                DenyInLeaveWholeDay = rule.DenyInLeaveWholeDay,
                                DenyInMissionWholeDay = rule.DenyInMissionWholeDay,
                                DenyInStoppedWorkingInfo = rule.DenyInStoppedWorkingInfo,
                                /*  các quy định nhà xe*/
                                UseTimeLimitParking = parkingRule.UseTimeLimitParking ?? false,
                                LimitDayNumber = parkingRule.LimitDayNumber ?? 0,
                                UseCardDependent = parkingRule.UseCardDependent ?? false,
                                UseRequiredParkingLotAccessed = parkingRule.UseRequiredParkingLotAccessed ?? false,
                                UseRequiredEmployeeVehicle = parkingRule.UseRequiredEmployeeVehicle ?? false,

                                CheckLogByShift = rule.CheckLogByShift,
                                CheckLogByAreaGroup = rule.CheckLogByAreaGroup,
                            };

                var data = query.FirstOrDefault();
                if (data != null)
                {
                    data.AreaGroups = GetAreaGroupByRulesIndex(rulesAreaGroup, data.GeneralAccessRuleIndex, pCompanyIndex);
                }
                var customer = _dbContext.HR_CustomerInfo.FirstOrDefault(x => x.EmployeeATID == employeeATID);
                data.CheckInTime = customer.FromTime;
                data.CheckOutTime = customer.ToTime;

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<EmployeeAccessedGroupModel>> ImportEmployeeAccessedGroup(List<EmployeeAccessedGroupModel> param, UserInfo user)
        {
            var result = param;
            try
            {
                var employeeATIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                var employees = await _HR_UserService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs.ToList(), DateTime.Now, user.CompanyIndex);

                var accessedGroupName = result.Select(x => x.AccessedGroupName).ToHashSet();
                var accessedGroupList = await _dbContext.GC_AccessedGroup.Where(x => accessedGroupName.Contains(x.Name)).ToListAsync();

                string[] formats = { "dd/MM/yyyy" };

                result.ForEach(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.FromDateFormat))
                    {
                        x.ErrorMessage += "Từ ngày không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.FromDateFormat))
                    {
                        var fromDate = new DateTime();
                        var convertFromDate = DateTime.TryParseExact(x.FromDateFormat, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out fromDate);
                        if (!convertFromDate)
                        {
                            x.ErrorMessage += "Từ ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            x.FromDate = fromDate;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(x.ToDateFormat))
                    {
                        var toDate = new DateTime();
                        var convertToDate = DateTime.TryParseExact(x.ToDateFormat, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out toDate);
                        if (!convertToDate)
                        {
                            x.ErrorMessage += "Đến ngày không hợp lệ\r\n";
                        }
                        else
                        {
                            x.ToDate = toDate;
                        }
                    }

                    if (x.ToDate.HasValue && x.ToDate.Value.Date < x.FromDate.Date)
                    {
                        x.ErrorMessage += "Từ ngày không được lớn hơn đến ngày\r\n";
                    }

                    if (string.IsNullOrWhiteSpace(x.AccessedGroupName))
                    {
                        x.ErrorMessage += "Nhóm truy cập không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.AccessedGroupName))
                    {
                        var accessedGroup = accessedGroupList.FirstOrDefault(y => y.Name == x.AccessedGroupName);
                        if (accessedGroup != null)
                        {
                            x.AccessedGroupIndex = accessedGroup.Index;
                        }
                        else
                        {
                            x.ErrorMessage += "Nhóm truy cập không hợp lệ\r\n";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(x.EmployeeATID))
                    {
                        x.ErrorMessage += "Nhân viên không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.EmployeeATID))
                    {
                        var employee = employees.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID);
                        if (employee != null)
                        {
                            x.EmployeeName = employee.FullName;
                        }
                        else
                        {
                            x.ErrorMessage += "Nhân viên không tồn tại\r\n";
                        }
                    }
                });

                if (result.Any(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)))
                {
                    return result;
                }
                else
                {
                    var listEmpIDs = result.Select(x => x.EmployeeATID).ToHashSet();
                    var existedEmployeeAccessedGroup = await _dbContext.GC_Employee_AccessedGroup.Where(x => listEmpIDs.Contains(x.EmployeeATID)).ToListAsync();
                    var allEmployee = await _HR_CustomerInfoService.GetAllCustomerInfo(new string[0], user.CompanyIndex);

                    result.ForEach(x =>
                    {
                        var existedEmployee = CheckEmployeeAccessedGroupExist(existedEmployeeAccessedGroup, x.EmployeeATID, x.FromDate, x.ToDate);
                        if (existedEmployee != null)
                        {
                            existedEmployee.AccessedGroupIndex = x.AccessedGroupIndex;
                            if (x.ToDate.HasValue)
                            {
                                existedEmployee.ToDate = x.ToDate;
                            }
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = user.UserName;
                            existedEmployee.CompanyIndex = user.CompanyIndex;
                            _dbContext.GC_Employee_AccessedGroup.Update(existedEmployee);
                        }
                        else
                        {
                            var employee_AccessedGroup = new GC_Employee_AccessedGroup();
                            employee_AccessedGroup.EmployeeATID = x.EmployeeATID;
                            employee_AccessedGroup.FromDate = x.FromDate;
                            employee_AccessedGroup.AccessedGroupIndex = x.AccessedGroupIndex;
                            if (x.ToDate.HasValue)
                            {
                                employee_AccessedGroup.ToDate = x.ToDate;
                            }
                            employee_AccessedGroup.UpdatedDate = DateTime.Now;
                            employee_AccessedGroup.UpdatedUser = user.UserName;
                            employee_AccessedGroup.CompanyIndex = user.CompanyIndex;
                            _dbContext.GC_Employee_AccessedGroup.Add(employee_AccessedGroup);
                        }
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ImportEmployeeAccessedGroup: " + ex);
            }
            return result;
        }

        private GC_Employee_AccessedGroup CheckEmployeeAccessedGroupExist(List<GC_Employee_AccessedGroup> existedEmployeeAccessedGroup, string employeeATID, DateTime fromDate, DateTime? toDate)
        {
            if (toDate == null) toDate = DateTime.MaxValue;
            var existedEmployee = existedEmployeeAccessedGroup.FirstOrDefault(_ => _.EmployeeATID == employeeATID && _.FromDate.Date <= toDate.Value.Date && (_.ToDate == null || _.ToDate >= fromDate));
            return existedEmployee;
        }
        private List<GC_Rules_General_AreaGroup> GetAreaGroupByRulesIndex(IEnumerable<GC_Rules_General_AreaGroup> rulesAreaGroup, int index, int companyIndex)
        {
            return rulesAreaGroup.Where(e => e.CompanyIndex == companyIndex && e.Rules_GeneralIndex == index).OrderBy(e => e.Priority).ToList();
        }
    }
}
