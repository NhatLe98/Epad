using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
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
    public class GC_Department_AccessedGroupService : BaseServices<GC_Department_AccessedGroup, EPAD_Context>, IGC_Department_AccessedGroupService
    {
        private readonly IHR_UserService _HR_UserService;
        private readonly IHR_CustomerInfoService _HR_CustomerInfoService;
        public EPAD_Context _dbContext;
        private ILogger _logger;
        public GC_Department_AccessedGroupService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _HR_UserService = serviceProvider.GetService<IHR_UserService>();
            _HR_CustomerInfoService = serviceProvider.GetService<IHR_CustomerInfoService>();
            _logger = loggerFactory.CreateLogger<GC_Department_AccessedGroupService>();
        }

        public List<GC_Department_AccessedGroup> GetByListDepartmentAndFromToDate(List<int> departmentIndexList, DateTime pFromDate, DateTime? pToDate, UserInfo user)
        {
            if (pToDate == null) pToDate = DateTime.MaxValue;
            var department_AccessedGroups = _dbContext.GC_Department_AccessedGroup.Where(x => x.CompanyIndex == user.CompanyIndex
            && departmentIndexList.Contains(x.DepartmentIndex)
            && x.FromDate.Date <= pToDate.Value.Date
            && (x.ToDate == null || x.ToDate >= pFromDate)).OrderByDescending(x => x.FromDate).ToList();
            return department_AccessedGroups;
        }

        public async Task<GC_Department_AccessedGroup> GetByDepartmentAndFromToDate(int pDepartmentIndex, DateTime pFromDate, DateTime? pToDate, UserInfo user)
        {
            if (pToDate == null) pToDate = DateTime.MaxValue;
            var dummy = Where(x => x.CompanyIndex == user.CompanyIndex
            && x.DepartmentIndex == pDepartmentIndex
            && x.FromDate.Date <= pToDate.Value.Date
            && (x.ToDate == null || x.ToDate >= pFromDate)).OrderByDescending(x => x.FromDate).FirstOrDefault();
            return await Task.FromResult(dummy);
        }

        public async Task<DataGridClass> GetDepartmentAccessedGroup(int page, int pageSize, string filter, UserInfo user)
        {
            var data = new List<DepartmentAccessedGroupModel>();
            var departmentAccessedGroupList = await GetDataByCompanyIndex(user.CompanyIndex);

            var accessedGroup = DbContext.GC_AccessedGroup.Where(x => departmentAccessedGroupList.Select(x => x.AccessedGroupIndex).Contains(x.Index)).ToList();
            var departmentList = departmentAccessedGroupList.Select(x => x.DepartmentIndex.ToString()).ToList();

            var departmentInfo = await _HR_UserService.GetDepartmentByIds(departmentList, user.CompanyIndex);

            foreach (var departmentAccessedGroup in departmentAccessedGroupList)
            {
                var item = new DepartmentAccessedGroupModel().PopulateWith(accessedGroup);
                item.Index = departmentAccessedGroup.Index;

                item.DepartmentIndex = departmentInfo.FirstOrDefault(x => x.Index == departmentAccessedGroup.DepartmentIndex).Index;
                item.DepartmentName = departmentInfo.FirstOrDefault(x => x.Index == departmentAccessedGroup.DepartmentIndex)?.Name ?? string.Empty;
                item.CompanyIndex = user.CompanyIndex;
                item.FromDate = departmentAccessedGroup.FromDate;
                item.ToDate = departmentAccessedGroup.ToDate;
                item.FromDateFormat = departmentAccessedGroup.FromDate.ToddMMyyyy();
                item.ToDateFormat = departmentAccessedGroup.ToDate.HasValue ? departmentAccessedGroup.ToDate.Value.ToddMMyyyy() : "";
                item.AccessedGroupName = accessedGroup.FirstOrDefault(x => x.Index == departmentAccessedGroup.AccessedGroupIndex)?.Name;
                item.AccessedGroupIndex = departmentAccessedGroup.AccessedGroupIndex;
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

        public async Task<DataGridClass> GetDepartmentAccessedGroupByFilter(int page, int pageSize, string filter,
            DateTime? fromDate, DateTime? toDate, List<long> listDepartment, UserInfo user)
        {
            var data = new List<DepartmentAccessedGroupModel>();
            var departmentAccessedGroupList = await GetDataByCompanyIndex(user.CompanyIndex);

            if (listDepartment != null && listDepartment.Count > 0)
            {
                departmentAccessedGroupList = departmentAccessedGroupList.Where(x => listDepartment.Contains(x.DepartmentIndex)).ToList();
            }
            var accessedGroup = await DbContext.GC_AccessedGroup.Where(x
                => departmentAccessedGroupList.Select(x => x.AccessedGroupIndex).Contains(x.Index)).ToListAsync();
            var departmentList = departmentAccessedGroupList.Select(x => x.DepartmentIndex.ToString()).ToList();


            var departmentInfo = await _HR_UserService.GetDepartmentByIds(departmentList, user.CompanyIndex);

            foreach (var departmentAccessedGroup in departmentAccessedGroupList)
            {
                var item = new DepartmentAccessedGroupModel().PopulateWith(accessedGroup);
                item.Index = departmentAccessedGroup.Index;
                item.DepartmentIndex = departmentInfo.FirstOrDefault(x => x.Index == departmentAccessedGroup.DepartmentIndex)?.Index;

                item.DepartmentIDs = item.DepartmentIndex;
                item.DepartmentName = departmentInfo.FirstOrDefault(x => x.Index == departmentAccessedGroup.DepartmentIndex)?.Name ?? string.Empty;
                item.CompanyIndex = user.CompanyIndex;
                item.FromDate = departmentAccessedGroup.FromDate;
                item.ToDate = departmentAccessedGroup.ToDate;
                item.FromDateFormat = departmentAccessedGroup.FromDate.ToddMMyyyy();
                item.ToDateFormat = departmentAccessedGroup.ToDate.HasValue ? departmentAccessedGroup.ToDate.Value.ToddMMyyyy() : "";
                item.AccessedGroupName = accessedGroup.FirstOrDefault(x => x.Index == departmentAccessedGroup.AccessedGroupIndex)?.Name;
                item.AccessedGroupIndex = departmentAccessedGroup.AccessedGroupIndex;
                data.Add(item);
            }

            if (data != null && data.Count > 0 && listDepartment != null && listDepartment.Count > 0
                && (listDepartment == null || listDepartment.Count == 0))
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

            if (!string.IsNullOrEmpty(filter))
            {
                data = data.Where(x => (x.DepartmentName != null && x.DepartmentName.ToLower().Contains(filter.ToLower())) || (x.AccessedGroupName != null && x.AccessedGroupName.ToLower().Contains(filter.ToLower()))).ToList();
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

        public async Task<List<GC_Department_AccessedGroup>> GetDataByCompanyIndex(int companyIndex)
        {
            return await DbContext.GC_Department_AccessedGroup.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public EmployeeAccessRule GetInfoDepartmentAccessedGroup(string pEmployeeATID, DateTime pDate, int pCompanyIndex)
        {
            var employeeInfo = _HR_UserService.GetEmployeeCompactInfoByListEmpATID(new List<string>() { pEmployeeATID }, pCompanyIndex).Result;
            var departmentIndex = employeeInfo.FirstOrDefault()?.DepartmentIndex;

            var rulesAreaGroup = DbContext.GC_Rules_General_AreaGroup.Where(e => e.CompanyIndex == pCompanyIndex);

            var query = from employeeAccess in DbContext.GC_Department_AccessedGroup
                            .Where(t => t.CompanyIndex == pCompanyIndex && t.DepartmentIndex == departmentIndex && t.FromDate.Date <= pDate.Date && (t.ToDate == null || t.ToDate.Value.Date >= pDate.Date))
                        from accessGroup in DbContext.GC_AccessedGroup.Where(accessGroup => employeeAccess.AccessedGroupIndex == accessGroup.Index).DefaultIfEmpty()
                        from rule in DbContext.GC_Rules_GeneralAccess.Where(rule => accessGroup.GeneralAccessRuleIndex == rule.Index).DefaultIfEmpty()
                        from parkingRule in DbContext.GC_Rules_ParkingLot.Where(parkingRule => accessGroup.ParkingLotRuleIndex == parkingRule.Index).DefaultIfEmpty()
                            //from areaGroupRule in _DBContext.GC_Rules_General_AreaGroup.Where(areaGroupRule => areaGroupRule.CompanyIndex == pCompanyIndex && areaGroupRule.Rules_GeneralIndex == rule.Index).OrderBy(e => e.Priority).DefaultIfEmpty()
                        select new EmployeeAccessRule()
                        {
                            EmployeeATID = pEmployeeATID,
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


        public async Task<List<DepartmentAccessedGroupModel>> ImportDepartmentAccessedGroup(List<DepartmentAccessedGroupModel> param, UserInfo user)
        {
            var result = param;
            try
            {
                var departmentList = await _HR_UserService.GetDepartmentList(user.CompanyIndex);
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
                        var accessedGroup = accessedGroupList.FirstOrDefault(y => y.Name.ToLower().RemoveAccents() == x.AccessedGroupName.ToLower().RemoveAccents());
                        if (accessedGroup != null)
                        {
                            x.AccessedGroupIndex = accessedGroup.Index;
                        }
                        else
                        {
                            x.ErrorMessage += "Nhóm truy cập không hợp lệ\r\n";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(x.DepartmentName))
                    {
                        x.ErrorMessage += "Phòng ban không được để trống\r\n";
                    }
                    else if (!string.IsNullOrWhiteSpace(x.DepartmentName))
                    {
                        var listAllDepartmentNameFromImport = new List<string>();
                        if (x.DepartmentName.Contains("/"))
                        {
                            var listSplitDepartmentName = x.DepartmentName.Split("/").Distinct().ToList();
                            listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                        }
                        else
                        {
                            listAllDepartmentNameFromImport.Add(x.DepartmentName);
                        }
                        listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                        var departmentInfo = listAllDepartmentNameFromImport.LastOrDefault();
                        var department = departmentList.FirstOrDefault(y => y.Name == departmentInfo);

                        if (department != null)
                        {
                            x.DepartmentIndex = department.Index;
                        }
                        else
                        {
                            x.ErrorMessage += "Phòng ban không tồn tại\r\n";
                        }
                    }
                });


                var dataImportValid = result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();
                var listDepartmentIndex = dataImportValid.Select(x => x.DepartmentIndex).ToHashSet();
                var existedDepartmentAccessedGroup = await _dbContext.GC_Department_AccessedGroup.Where(x => listDepartmentIndex.Contains(x.DepartmentIndex)).ToListAsync();
                var allEmployee = await _HR_CustomerInfoService.GetAllCustomerInfo(new string[0], user.CompanyIndex);
                foreach (var dataImport in result.Where(x => string.IsNullOrWhiteSpace(x.ErrorMessage)))
                {
                    var existedDepartment = CheckDepartmentAccessedGroupExist(existedDepartmentAccessedGroup, (int)dataImport.DepartmentIndex, dataImport.FromDate, dataImport.ToDate);
                    if (existedDepartment != null)
                    {
                        dataImport.ErrorMessage += "Phòng ban đã khai báo nhóm truy cập trong cùng khoảng thời gian\r\n";
                        continue;
                    }
                    //if (existedDepartment != null && existedDepartment.FromDate.Date == dataImport.FromDate.Date)
                    //{
                    //    existedDepartment.AccessedGroupIndex = dataImport.AccessedGroupIndex;
                    //    existedDepartment.ToDate = dataImport.ToDate;
                    //    existedDepartment.UpdatedDate = DateTime.Now;
                    //    existedDepartment.UpdatedUser = user.UserName;
                    //    existedDepartment.CompanyIndex = user.CompanyIndex;
                    //    _dbContext.GC_Department_AccessedGroup.Update(existedDepartment);
                    //}
                    //else
                    //{
                        
                    //}
                    var employee_AccessedGroup = new GC_Department_AccessedGroup();
                    employee_AccessedGroup.DepartmentIndex = (int)dataImport.DepartmentIndex;
                    employee_AccessedGroup.FromDate = dataImport.FromDate;
                    employee_AccessedGroup.AccessedGroupIndex = dataImport.AccessedGroupIndex;
                    if (dataImport.ToDate.HasValue)
                    {
                        employee_AccessedGroup.ToDate = dataImport.ToDate;
                    }
                    employee_AccessedGroup.UpdatedDate = DateTime.Now;
                    employee_AccessedGroup.UpdatedUser = user.UserName;
                    employee_AccessedGroup.CompanyIndex = user.CompanyIndex;
                    _dbContext.GC_Department_AccessedGroup.Add(employee_AccessedGroup);
                }
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError("ImportEmployeeAccessedGroup: " + ex);
            }
            return result;
        }
        private GC_Department_AccessedGroup CheckDepartmentAccessedGroupExist(List<GC_Department_AccessedGroup> existedDepartmentAccessedGroup, int department, DateTime fromDate, DateTime? toDate)
        {
            var existedDepartment = new GC_Department_AccessedGroup();
            if (toDate == null)
            {
                existedDepartment = existedDepartmentAccessedGroup
               .FirstOrDefault(x => x.DepartmentIndex == department &&
                           (x.FromDate == fromDate || x.ToDate == fromDate
                             || x.FromDate >= fromDate
                           || (x.ToDate == null)
                           || (x.FromDate > fromDate && x.ToDate < fromDate)
                           || (x.FromDate <= fromDate && x.ToDate >= fromDate)));
            }
            else
            {
                existedDepartment = existedDepartmentAccessedGroup
               .FirstOrDefault(x => x.DepartmentIndex == department &&
                           (x.FromDate == fromDate || x.ToDate == fromDate
                           || ((x.FromDate >= fromDate && x.FromDate <= toDate) && x.ToDate == null)
                           || ((x.FromDate <= fromDate || x.FromDate <= toDate) && x.ToDate == null)
                           || ((x.FromDate <= fromDate && x.ToDate >= fromDate) || ((x.FromDate <= toDate && x.ToDate >= toDate)))));
            }
            return existedDepartment;
        }

        private List<GC_Rules_General_AreaGroup> GetAreaGroupByRulesIndex(IEnumerable<GC_Rules_General_AreaGroup> rulesAreaGroup, int index, int companyIndex)
        {
            return rulesAreaGroup.Where(e => e.CompanyIndex == companyIndex && e.Rules_GeneralIndex == index).OrderBy(e => e.Priority).ToList();
        }
    }
}
