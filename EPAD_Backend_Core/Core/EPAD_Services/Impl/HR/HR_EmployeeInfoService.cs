using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.Other;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPAD_Common.Enums;

namespace EPAD_Services.Impl
{
    public class HR_EmployeeInfoService : BaseServices<HR_EmployeeInfo, EPAD_Context>, IHR_EmployeeInfoService
    {
        ConfigObject _Config;
        IMemoryCache _Cache;
        ezHR_Context ezHR_Context;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;

        public HR_EmployeeInfoService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
            _logger = loggerFactory.CreateLogger<HR_EmployeeInfoService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<EmployeeInfoResponse>> GetEmployeeInfoByIDs(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var query = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        select new EmployeeInfoResponse { EmployeeID = u.EmployeeATID, FullName = u.FullName };

            return await Task.FromResult(query.ToList());
        }

        public async Task<List<EmployeeInfoResponse>> GetEmployeeInfoByEmployeeATIDs(List<string> pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var query = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && empLookup.Contains(x.EmployeeATID))
                        select new EmployeeInfoResponse { EmployeeID = u.EmployeeATID, FullName = u.FullName };

            return await Task.FromResult(query.ToList());
        }

        public async Task<List<VStarEmployeeInfoResult>> GetAllEmployeeInfoVStar(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var result = new List<VStarEmployeeInfoResult>();
            if (!_Config.IntegrateDBOther)
            {
                var empLookup = pEmployeeATIDs.ToHashSet();
                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                            join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals e.EmployeeATID
                            join w in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals w.EmployeeATID
                            join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                            on w.DepartmentIndex equals d.Index
                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID into card
                            from ci in card.DefaultIfEmpty()
                            select new { User = u, Employee = e, CardInfo = ci, WorkingInfo = w, Department = d };

                result = dummy.ToList().Select(x =>
                {
                    var rs = new VStarEmployeeInfoResult();
                    rs.EmployeeATID = x.User?.EmployeeATID ?? x.Employee?.EmployeeATID ?? "";
                    rs.EmployeeCode = x.User?.EmployeeCode ?? "";
                    rs.FullName = x.User?.FullName ?? "";
                    rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                    rs.DepartmentIndex = (x.Department != null) ? x.Department?.Index ?? 0 : 0;
                    rs.TeamIndex = x.WorkingInfo?.TeamIndex ?? 0;
                    return rs;
                }).ToList();
            }
            else
            {
                var empLookup = pEmployeeATIDs.ToHashSet();
                var departmentLst = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToListAsync();
                var departmentParent = departmentLst.Where(x => x.ParentIndex == 0 || x.ParentIndex == null).ToList();
                var departmentParentLst = new List<HR_DeparmentParent>();
                foreach (var item in departmentParent)
                {
                    var departmentChildren = GetChildrenToId(departmentLst, item.Index, item.Name);
                    if (departmentChildren != null && departmentChildren.Count > 0)
                    {
                        departmentParentLst.AddRange(departmentChildren);
                    }
                    departmentParentLst.Add(new HR_DeparmentParent() { Index = item.Index, DepartmentParentName = item.Name, ParentIndex = item.Index });
                }
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in departmentLst
                             on eWorkResult?.DepartmentIndex ?? 0 equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join g in departmentParentLst
                             on eWorkResult?.DepartmentIndex ?? 0 equals g.Index into deParent
                             from deWorkResult in deParent.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName, DepartmentParent = deWorkResult }).AsQueryable();
                var x = query.ToList();
                result = query.ToList().Select(x =>
                {
                    var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                    var rs = new VStarEmployeeInfoResult();
                    rs.EmployeeATID = x.EmployeeATID;
                    rs.EmployeeCode = x.Employee.EmployeeCode;
                    rs.FullName = x.FullName;
                    rs.CardNumber = x.Employee?.CardNumber;
                    rs.DepartmentIndex = x.Department?.Index;
                    rs.TeamIndex = teamIndex;
                    rs.DepartmentName = x.Department?.Name;
                    rs.TeamName = teamIndex == 0 ? "" : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString())).Name;
                    rs.EmployeeType = x.DepartmentParent?.ParentIndex ?? 0;
                    rs.EmployeeTypeName = x.DepartmentParent?.DepartmentParentName;
                    return rs;
                }).ToList();
            }
            return result;

        }

        public List<HR_DeparmentParent> GetChildrenToId(List<HR_Department> departments, long id, string name)
        {
            return departments
                     .Where(x => x.ParentIndex == id)
                     .Union(departments.Where(x => x.ParentIndex == id)
                         .SelectMany(y => GetChildren(departments, y.Index))
                     ).Select(x => new HR_DeparmentParent
                     {
                         Index = x.Index,
                         ParentIndex = id,
                         DepartmentParentName = name
                     }).ToList();
        }

        public async Task<List<VStarEmployeeInfoResult>> GetAllEmployeeInfoVStarExtend(string[] pEmployeeATIDs, int pCompanyIndex, long type)
        {
            var result = new List<VStarEmployeeInfoResult>();
            if (!_Config.IntegrateDBOther)
            {
                var empLookup = pEmployeeATIDs.ToHashSet();
                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                            join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals e.EmployeeATID
                            join w in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals w.EmployeeATID
                            join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                            on w.DepartmentIndex equals d.Index
                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID into card
                            from ci in card.DefaultIfEmpty()
                            select new { User = u, Employee = e, CardInfo = ci, WorkingInfo = w, Department = d };

                result = dummy.ToList().Select(x =>
                {
                    var rs = new VStarEmployeeInfoResult();
                    rs.EmployeeATID = x.User?.EmployeeATID ?? x.Employee?.EmployeeATID ?? "";
                    rs.EmployeeCode = x.User?.EmployeeCode ?? "";
                    rs.FullName = x.User?.FullName ?? "";
                    rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                    rs.DepartmentIndex = (x.Department != null) ? x.Department?.Index ?? 0 : 0;
                    rs.TeamIndex = x.WorkingInfo?.TeamIndex ?? 0;
                    return rs;
                }).ToList();
            }
            else
            {
                var empLookup = pEmployeeATIDs.ToHashSet();
                var departmentLst = ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToList();
                var typegroups = GetChildren(departmentLst, type);
                var allTypes = typegroups.Select(x => x.Index).ToList();
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true)).ToList()
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()
                             join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.DepartmentIndex ?? 0 equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()
                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                var x = query.ToList();
                result = query.ToList().Select(x =>
                {
                    var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                    var rs = new VStarEmployeeInfoResult();
                    rs.EmployeeATID = x.EmployeeATID;
                    rs.EmployeeCode = x.Employee.EmployeeCode;
                    rs.FullName = x.FullName;
                    rs.CardNumber = x.Employee?.CardNumber;
                    rs.DepartmentIndex = x.Department?.Index;
                    rs.TeamIndex = teamIndex;
                    rs.DepartmentName = x.Department?.Name;
                    rs.TeamName = teamIndex == 0 ? "" : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString())).Name;
                    return rs;
                }).ToList();

                result = result.Where(x => !allTypes.Contains(Convert.ToInt64(x.DepartmentIndex))).ToList();
            }
            return result;
        }

        private List<HR_Department> GetChildren(List<HR_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
        }

        public async Task<List<HR_EmployeeInfoResult>> GetAllEmployeeInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var result = new List<HR_EmployeeInfoResult>();
            if (!_Config.IntegrateDBOther)
            {
                var dummy = (from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex 
                             && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                             join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join wi in DbContext.IC_WorkingInfo
                             on u.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in DbContext.IC_Department
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                                 //join q in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 //on eWorkResult.PositionIndex equals q.Index into qWork
                                 //from qWorkResult in qWork.DefaultIfEmpty()

                                 //join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                                 //on u.EmployeeATID equals c.EmployeeATID into cWork
                                 //from cResult in cWork.DefaultIfEmpty()

                             select new
                             {
                                 User = u,
                                 Employee = eResult,
                                 WorkingInfo = eWorkResult,
                                 Department = dWorkResult
                                 //,CardInfo = cResult, Position = qWorkResult 
                             });
                result = dummy.ToList().Select(x =>
               {
                   var rs = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                   if (x.Employee != null)
                   {
                       rs = _Mapper.Map(x?.Employee, rs);
                   }
                   if (rs != null)
                   {
                       rs.DepartmentIndex = x?.Department?.Index ?? 0;
                       rs.DepartmentName = x?.Department?.Name;
                       //rs.PositionName = x?.Position?.Name;
                       //rs.CardNumber = x?.CardInfo?.CardNumber;
                       rs.Phone = x?.Employee?.Phone;

                   }
                   return rs;
               }).ToList();
            }
            else
            {
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                             from pWorkResult in pWork.DefaultIfEmpty()

                             join t in ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.TitlesIndex equals t.Index into tWork
                             from tWorkResult in tWork.DefaultIfEmpty()

                             join s in ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.ReturnedDate == null
                                && x.StartedDate.Value.Date <= DateTime.Now)
                             on eWorkResult.EmployeeATID equals s.EmployeeATID into sWork
                             from sWorkResult in sWork.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                             && sWorkResult.EmployeeATID == null
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                result = query.AsEnumerable().Select(x =>
                {
                    var rs = new HR_EmployeeInfoResult()
                    {
                        Avatar = (x.Employee.Image != null && x.Employee.Image.Length > 0) ? Convert.ToBase64String(x.Employee.Image) : "",
                        EmployeeATID = x.EmployeeATID,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FullName = x.FullName,
                        Gender = x.Employee.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female,
                        CardNumber = x.Employee.CardNumber,
                        NameOnMachine = "",
                        DepartmentIndex = x.WorkingInfo?.DepartmentIndex ?? 0,
                        DepartmentName = x.Department?.Name ?? "",
                        TitleIndex = x.WorkingInfo?.TitlesIndex ?? 0,
                        PositionIndex = x.WorkingInfo?.PositionIndex ?? 0,
                        DayOfBirth = x.Employee.DayOfBirth,
                        MonthOfBirth = x.Employee.MonthOfBirth,
                        YearOfBirth = x.Employee.YearOfBirth,
                        Phone = "",
                        Email = "",
                        FromDate = x.WorkingInfo?.FromDate,
                        ToDate = x.WorkingInfo?.ToDate,
                        CompanyIndex = x.Employee.CompanyIndex,
                        JoinedDate = x.Employee?.JoinedDate,
                    };

                    if (x.WorkingInfo != null)
                    {
                        rs.WorkingStatus = x.WorkingInfo.FromDate?.Date <= DateTime.Now.Date
                                           && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date) ? "IsWorking" : "StoppedWork";
                        if (rs.WorkingStatus.Contains("StoppedWork"))
                        {
                            rs.Note = x.WorkingInfo.ToDate?.Date.ToddMMyyyyMinus();
                        }
                    }
                    return rs;
                }).ToList();
            }
            return await Task.FromResult(result);
        }

        public async Task<List<HR_EmployeeInfoResult>> GetEmployeeInfoByIds(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var result = new List<HR_EmployeeInfoResult>();
            if (!_Config.IntegrateDBOther)
            {
                var dummy = (from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && empLookup.Contains(x.EmployeeATID))
                             join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join wi in DbContext.IC_WorkingInfo
                             on u.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in DbContext.IC_Department
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                                 //join q in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 //on eWorkResult.PositionIndex equals q.Index into qWork
                                 //from qWorkResult in qWork.DefaultIfEmpty()

                                 //join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                                 //on u.EmployeeATID equals c.EmployeeATID into cWork
                                 //from cResult in cWork.DefaultIfEmpty()

                             select new
                             {
                                 User = u,
                                 Employee = eResult,
                                 WorkingInfo = eWorkResult,
                                 Department = dWorkResult,
                                 //,CardInfo = cResult, Position = qWorkResult
                             }).AsQueryable();
                result = dummy.AsEnumerable().Select(x =>
                {
                    var rs = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                    if (x.Employee != null)
                    {
                        rs = _Mapper.Map(x?.Employee, rs);
                    }
                    if (rs != null)
                    {
                        rs.DepartmentIndex = x?.Department?.Index ?? 0;
                        rs.DepartmentName = x?.Department?.Name;
                        //rs.PositionName = x?.Position?.Name;
                        //rs.CardNumber = x?.CardInfo?.CardNumber;
                        rs.Phone = x?.Employee?.Phone;

                    }
                    return rs;
                }).ToList();
            }
            else
            {
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex && empLookup.Contains(x.EmployeeATID))
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                             from pWorkResult in pWork.DefaultIfEmpty()

                             join t in ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.TitlesIndex equals t.Index into tWork
                             from tWorkResult in tWork.DefaultIfEmpty()

                             join s in ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.ReturnedDate == null
                                && x.StartedDate.Value.Date <= DateTime.Now)
                             on eWorkResult.EmployeeATID equals s.EmployeeATID into sWork
                             from sWorkResult in sWork.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                             && sWorkResult.EmployeeATID == null
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                result = query.AsEnumerable().Select(x =>
                {
                    var rs = new HR_EmployeeInfoResult()
                    {
                        Avatar = (x.Employee.Image != null && x.Employee.Image.Length > 0) ? Convert.ToBase64String(x.Employee.Image) : "",
                        EmployeeATID = x.EmployeeATID,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FullName = x.FullName,
                        Gender = x.Employee.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female,
                        CardNumber = x.Employee.CardNumber,
                        NameOnMachine = "",
                        DepartmentIndex = x.WorkingInfo?.DepartmentIndex ?? 0,
                        DepartmentName = x.Department?.Name ?? "",
                        TitleIndex = x.WorkingInfo?.TitlesIndex ?? 0,
                        PositionIndex = x.WorkingInfo?.PositionIndex ?? 0,
                        DayOfBirth = x.Employee.DayOfBirth,
                        MonthOfBirth = x.Employee.MonthOfBirth,
                        YearOfBirth = x.Employee.YearOfBirth,
                        Phone = "",
                        Email = "",
                        FromDate = x.WorkingInfo?.FromDate,
                        ToDate = x.WorkingInfo?.ToDate,
                        CompanyIndex = x.Employee.CompanyIndex,
                        JoinedDate = x.Employee?.JoinedDate,
                    };

                    if (x.WorkingInfo != null)
                    {
                        rs.WorkingStatus = x.WorkingInfo.FromDate?.Date <= DateTime.Now.Date
                            && (!x.WorkingInfo.ToDate.HasValue 
                            || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date) ? "IsWorking" : "StoppedWork";
                        if (rs.WorkingStatus.Contains("StoppedWork"))
                        {
                            rs.Note = x.WorkingInfo.ToDate?.Date.ToddMMyyyyMinus();
                        }
                    }
                    return rs;
                }).ToList();
            }
            return await Task.FromResult(result);
        }

        public async Task<List<HR_EmployeeInfoResult>> GetEmployeeInfoByDepartment(List<long> pDepartmentIndex, int pCompanyIndex)
        {
            var departmentLookup = pDepartmentIndex.ToHashSet();
            var result = new List<HR_EmployeeInfoResult>();
            if (!_Config.IntegrateDBOther)
            {
                var dummy = (from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex &&
                             (!x.EmployeeType.HasValue || x.EmployeeType == 0 || x.EmployeeType.Value == (short)EmployeeType.Employee))
                             join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join wi in DbContext.IC_WorkingInfo.Where(x => x.FromDate.Date <= DateTime.Now.Date
                             && (!x.ToDate.HasValue || x.ToDate.Value.Date > DateTime.Now.Date)
                             && (departmentLookup.Count > 0 ? departmentLookup.Contains(x.DepartmentIndex) : true))
                             on u.EmployeeATID equals wi.EmployeeATID
                             //into eWork
                             //from eWorkResult in eWork.DefaultIfEmpty()

                             join d in DbContext.IC_Department
                             on wi.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join q in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on wi.PositionIndex equals q.Index into qWork
                             from qWorkResult in qWork.DefaultIfEmpty()

                             join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                             on u.EmployeeATID equals c.EmployeeATID into cWork
                             from cResult in cWork.DefaultIfEmpty()

                             select new { User = u, Employee = eResult, WorkingInfo = wi, Department = dWorkResult, CardInfo = cResult, Position = qWorkResult }).AsQueryable();
                result = dummy.ToList().Select(x =>
                {
                    var rs = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                    if (x.Employee != null)
                    {
                        rs = _Mapper.Map(x?.Employee, rs);
                    }
                    if (rs != null)
                    {
                        rs.DepartmentIndex = x?.Department?.Index ?? 0;
                        rs.DepartmentName = x?.Department?.Name;
                        rs.PositionName = x?.Position?.Name;
                        rs.CardNumber = x?.CardInfo?.CardNumber;
                        rs.Phone = x?.Employee?.Phone;

                    }
                    return rs;
                }).ToList();
            }
            else
            {
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex
                                && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date)
                                && ((x.DepartmentIndex.HasValue && departmentLookup.Count > 0)
                                    ? departmentLookup.Contains(x.DepartmentIndex.Value) : true))
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                             from pWorkResult in pWork.DefaultIfEmpty()

                             join t in ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.TitlesIndex equals t.Index into tWork
                             from tWorkResult in tWork.DefaultIfEmpty()

                             join s in ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.ReturnedDate == null
                                && x.StartedDate.Value.Date <= DateTime.Now)
                             on eWorkResult.EmployeeATID equals s.EmployeeATID into sWork
                             from sWorkResult in sWork.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                             && sWorkResult.EmployeeATID == null
                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                result = query.ToList().Select(x =>
                {
                    var rs = new HR_EmployeeInfoResult()
                    {
                        Avatar = (x.Employee.Image != null && x.Employee.Image.Length > 0) ? Convert.ToBase64String(x.Employee.Image) : "",
                        EmployeeATID = x.EmployeeATID,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FullName = x.FullName,
                        Gender = x.Employee.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female,
                        CardNumber = x.Employee.CardNumber,
                        NameOnMachine = "",
                        DepartmentIndex = x.WorkingInfo?.DepartmentIndex ?? 0,
                        DepartmentName = x.Department?.Name ?? "",
                        TitleIndex = x.WorkingInfo?.TitlesIndex ?? 0,
                        PositionIndex = x.WorkingInfo?.PositionIndex ?? 0,
                        DayOfBirth = x.Employee.DayOfBirth,
                        MonthOfBirth = x.Employee.MonthOfBirth,
                        YearOfBirth = x.Employee.YearOfBirth,
                        Phone = "",
                        Email = "",
                        FromDate = x.WorkingInfo?.FromDate,
                        ToDate = x.WorkingInfo?.ToDate,
                        CompanyIndex = x.Employee.CompanyIndex,
                        JoinedDate = x.Employee?.JoinedDate,
                    };

                    if (x.WorkingInfo != null)
                    {
                        rs.WorkingStatus = x.WorkingInfo.FromDate?.Date <= DateTime.Now.Date
                                           && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date) ? "IsWorking" : "StoppedWork";
                        if (rs.WorkingStatus.Contains("StoppedWork"))
                        {
                            rs.Note = x.WorkingInfo.ToDate?.Date.ToddMMyyyyMinus();
                        }
                    }
                    return rs;
                }).ToList();
            }
            return await Task.FromResult(result);
        }

        public async Task<HR_EmployeeInfoResult> GetEmployeeInfo(string pEmployeeATID, int pCompanyIndex, string employeeCode = "")
        {
            try
            {
                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID
                            && (employeeCode == "" || x.EmployeeCode == "" || x.EmployeeCode == employeeCode))
                            join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals e.EmployeeATID

                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID into card
                            from ci in card.DefaultIfEmpty()

                            join w in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.Status == (short)TransferStatus.Approve
                            && x.FromDate.Date <= DateTime.Now.Date && (x.ToDate == null || (x.ToDate.HasValue && x.ToDate.Value >= DateTime.Now.Date)))
                            on u.EmployeeATID equals w.EmployeeATID into work
                            from w in work.DefaultIfEmpty()

                            join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                            on w.DepartmentIndex equals d.Index into depart
                            from d in depart.DefaultIfEmpty()

                            select new { User = u, Employee = e, CardInfo = ci, Department = d, WorkingInfo = w };

                var result = dummy.AsEnumerable().Select(x =>
                {
                    var rs = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                    rs = _Mapper.Map(x.Employee, rs);
                    rs = _Mapper.Map(x.Department, rs);
                    rs = _Mapper.Map(x.WorkingInfo, rs);
                    if (x.CardInfo != null)
                    {
                        rs = _Mapper.Map(x.CardInfo, rs);
                        rs.CardNumber = x?.CardInfo?.CardNumber;
                    }

                    if (rs != null)
                        rs.DepartmentName = x?.Department?.Name;
                    rs.DepartmentCode = x?.Department?.Code;
                    return rs;
                }).FirstOrDefault();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmployeeInfo: {ex}");
                return null;
            }
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            try
            {
                var result = new List<HR_EmployeeInfoResult>();
                var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
                var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
                var pPage = Convert.ToInt32(pageIndex.Value ?? 1);
                var pLimit = Convert.ToInt32(pageSize.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);
                int totalEmployee = 0;

                if (_Config.IntegrateDBOther)
                {
                    var employeeStopped = ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.StartedDate <= DateTime.Now.Date && x.ReturnedDate == null).Select(x => x.EmployeeATID).ToList();
                    var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 join t in ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                                 on eWorkResult.TitlesIndex equals t.Index into tWork
                                 from tWorkResult in tWork.DefaultIfEmpty()

                                     //join s in ezHR_Context.HR_EmployeeStoppedWorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex && x.ReturnedDate == null
                                     //   && x.StartedDate.Value.Date <= DateTime.Now)
                                     //on eWorkResult.EmployeeATID equals s.EmployeeATID into sWork
                                     //from sWorkResult in sWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                    if (addedParams != null)
                    {
                        foreach (AddedParam param in addedParams)
                        {
                            if (param.Value != null)
                            {
                                switch (param.Key)
                                {
                                    case "Filter":
                                        string filter = param.Value.ToString();
                                        var filterBy = filter.Split(",").ToList();
                                        query = query.Where(u => u.EmployeeATID.Contains(filter)
                                        || (u.Department != null && u.Department.Name.Contains(filter))
                                        || u.FullName.Contains(filter)
                                        || (u.Employee.CardNumber != null && u.Employee.CardNumber.Contains(filter))
                                        || (!string.IsNullOrEmpty(u.Employee.EmployeeCode) && u.Employee.EmployeeCode.Contains(filter))
                                        || ((filterBy.Count > 0 && (filterBy.Contains(u.EmployeeATID) || filterBy.Contains(u.FullName))) || filterBy.Count == 0)
                                        );
                                        break;
                                    case "DepartmentIndex":
                                        int departmentIndex = Convert.ToInt32(param.Value);
                                        query = query.Where(u => (u.WorkingInfo.DepartmentIndex) == departmentIndex);
                                        break;
                                    case "ListDepartment":
                                        IList<long> departments = (IList<long>)param.Value;
                                        query = query.Where(u => (u.WorkingInfo != null && u.WorkingInfo.DepartmentIndex.HasValue && departments.Contains(u.WorkingInfo.DepartmentIndex.Value)) || u.WorkingInfo == null);
                                        break;
                                    case "ListEmployeeATID":
                                        IList<string> listEmployeeID = (IList<string>)param.Value;
                                        query = query.Where(u => listEmployeeID.Contains(u.EmployeeATID));
                                        break;
                                    case "EmployeeATID":
                                        string employeeID = param.Value.ToString();
                                        query = query.Where(u => u.EmployeeATID == employeeID);
                                        break;
                                    case "IsCurrentWorking":
                                        if (param.Value != null)
                                        {
                                            var listWorking = (List<int>)param.Value;
                                            if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                            {
                                                if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                                {
                                                    query = query.Where(u => !employeeStopped.Contains(u.EmployeeATID) && ((u.WorkingInfo.FromDate <= DateTime.Now.Date
                                        && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo == null || u.WorkingInfo.DepartmentIndex == 0));
                                                }
                                                else if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stopped work
                                                {
                                                    query = query.Where(u => employeeStopped.Contains(u.EmployeeATID));

                                                }
                                            }
                                        }
                                        break;
                                    case "FromDate":
                                        var fromDate = (DateTime)param.Value;
                                        query = query.Where(u => u.WorkingInfo.FromDate.HasValue
                                            && u.WorkingInfo.FromDate.Value.Date >= fromDate.Date);
                                        break;
                                    case "ToDate":

                                        var toDate = (DateTime)param.Value;
                                        query = query.Where(u => u.WorkingInfo.FromDate.HasValue
                                            && u.WorkingInfo.FromDate.Value.Date <= toDate.Date);
                                        break;
                                }
                            }
                        }
                    }

                    totalEmployee = query.Count();
                    result = query.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
                    {
                        var rs = new HR_EmployeeInfoResult()
                        {
                            Avatar = (x.Employee.Image != null && x.Employee.Image.Length > 0) ? Convert.ToBase64String(x.Employee.Image) : "",
                            EmployeeATID = x.EmployeeATID,
                            EmployeeCode = x.Employee.EmployeeCode,
                            FullName = x.FullName,
                            Gender = x.Employee.Gender == true ? (short)GenderEnum.Male : (short)GenderEnum.Female,
                            CardNumber = x.Employee.CardNumber,
                            NameOnMachine = "",
                            DepartmentIndex = x.WorkingInfo?.DepartmentIndex ?? 0,
                            DepartmentName = x.Department?.Name ?? "",
                            TitleIndex = x.WorkingInfo?.TitlesIndex ?? 0,
                            PositionIndex = x.WorkingInfo?.PositionIndex ?? 0,
                            DayOfBirth = x.Employee.DayOfBirth,
                            MonthOfBirth = x.Employee.MonthOfBirth,
                            YearOfBirth = x.Employee.YearOfBirth,
                            Phone = "",
                            Email = "",
                            FromDate = x.WorkingInfo?.FromDate,
                            ToDate = x.WorkingInfo?.ToDate,
                            CompanyIndex = x.Employee.CompanyIndex,
                            JoinedDate = x.Employee?.JoinedDate,
                        };

                        if (x.WorkingInfo != null)
                        {
                            rs.WorkingStatus = x.WorkingInfo.FromDate?.Date <= DateTime.Now.Date
                                               && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date) ? "IsWorking" : "StoppedWork";
                            if (rs.WorkingStatus.Contains("StoppedWork"))
                            {
                                rs.Note = x.WorkingInfo.ToDate?.Date.ToddMMyyyyMinus();
                            }
                        }
                        else
                        {
                            rs.WorkingStatus = "IsWorking";
                        }

                        if (employeeStopped.Contains(rs.EmployeeATID))
                        {
                            rs.WorkingStatus = "StoppedWork";
                        }

                        return rs;
                    }).ToList();
                    var userLst = result.Select(x => x.EmployeeATID).ToList();
                    var usermaster = DbContext.IC_UserMaster.Where(x => userLst.Contains(x.EmployeeATID)).ToList();

                    result = result.Select(x =>
                    {
                        var finger = usermaster.FirstOrDefault(y => y.EmployeeATID == x.EmployeeATID);
                        if (finger != null)
                        {

                            x.TotalFingerTemplate = (short)AppUtils.CountIfNotEmpty(
                                finger.FingerData0,
                                finger.FingerData1,
                                finger.FingerData2,
                                finger.FingerData3,
                                finger.FingerData4,
                                finger.FingerData5,
                                finger.FingerData6,
                                finger.FingerData7,
                                finger.FingerData8,
                                finger.FingerData9
                                );
                        }
                        return x;
                    }).ToList();
                }
                else
                {
                    var filter = "";
                    var filterBy = new List<string>();
                    var queryParams = addedParams.FirstOrDefault(x => x.Key == "Filter")?.Value;
                    if (queryParams != null)
                    {
                        filter = queryParams.ToString();
                        filterBy = filter.Split(" ").ToList();
                    }
                    var hrusers = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                    && (x.EmployeeType == null || x.EmployeeType == (int)EmployeeType.Employee || _configClientName == ClientName.MAY.ToString())
                    && (queryParams == null || (x.EmployeeATID.Contains(filter)
                                         || x.FullName.Contains(filter)
                                         || (filterBy.Count > 0 && (filterBy.Contains(x.EmployeeATID) || filterBy.Count == 0))
                                         || (!string.IsNullOrEmpty(x.EmployeeCode) && x.EmployeeCode.Contains(filter)))));
                    var query = (from u in hrusers
                                 join e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on u.EmployeeATID equals e.EmployeeATID into eCheck
                                 from eResult in eCheck.DefaultIfEmpty()

                                 join ut in DbContext.HR_UserType.Where(x => x.CompanyIndex == pCompanyIndex && x.StatusId != (byte)RowStatus.Inactive)
                                 on u.EmployeeType equals ut.UserTypeId into uType
                                 from utResult in uType.DefaultIfEmpty()

                                 join et in DbContext.IC_EmployeeType.Where(x => x.CompanyIndex == pCompanyIndex
                                    && x.IsUsing)
                                 on u.EmployeeTypeIndex equals et.Index into eType
                                 from etResult in eType.DefaultIfEmpty()

                                 join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eResult.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                 on eResult.EmployeeATID equals c.EmployeeATID into cWork
                                 from cResult in cWork.DefaultIfEmpty()

                                 join us in DbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on u.EmployeeATID equals us.EmployeeATID into usWork
                                 from usWorkResult in usWork.DefaultIfEmpty()



                                 select new
                                 {
                                     User = u,
                                     UserType = utResult,
                                     EmployeeType = etResult,
                                     Employee = eResult,
                                     WorkingInfo = eWorkResult,
                                     Department = dWorkResult,
                                     CardInfo = cResult,
                                     UserMaster = usWorkResult
                                 });
                    if (addedParams != null)
                    {
                        foreach (AddedParam param in addedParams)
                        {
                            switch (param.Key)
                            {
                                case "Filter":
                                    if (param.Value != null)
                                    {

                                        //query = query.Where(u =>

                                        //(u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter)));

                                    }
                                    break;
                                case "DepartmentIndex":
                                    if (param.Value != null)
                                    {
                                        int departmentIndex = Convert.ToInt32(param.Value);
                                        query = query.Where(u => (u.WorkingInfo.DepartmentIndex) == departmentIndex);
                                    }
                                    break;
                                case "ListDepartment":
                                    if (param.Value != null)
                                    {
                                        IList<long> departments = (IList<long>)param.Value;
                                        query = query.Where(u => u.WorkingInfo != null && departments.Contains(u.WorkingInfo.DepartmentIndex));
                                    }
                                    break;
                                case "ListEmployeeATID":
                                    if (param.Value != null)
                                    {
                                        IList<string> listEmployeeID = (IList<string>)param.Value;
                                        query = query.Where(u => listEmployeeID.Contains(u.User.EmployeeATID));
                                    }
                                    break;
                                case "EmployeeATID":
                                    if (param.Value != null)
                                    {
                                        string employeeID = param.Value.ToString();
                                        query = query.Where(u => u.User.EmployeeATID == employeeID);
                                    }
                                    break;
                                case "ApproveStatus":
                                    if (param.Value != null)
                                    {
                                        long approveStatus = Convert.ToInt64(param.Value.ToString());
                                        query = query.Where(u => u.WorkingInfo.Status == approveStatus);
                                    }
                                    break;
                                case "IsCurrentWorking":
                                    if (param.Value != null)
                                    {
                                        var listWorking = (List<int>)param.Value;
                                        if (listWorking != null && listWorking.Count > 0) 
                                        {
                                            if (!listWorking.Contains((int)EmployeeStatusType.Working) || !listWorking.Contains((int)EmployeeStatusType.StopWorking))
                                            {
                                                if (listWorking.Contains((int)EmployeeStatusType.Working)) // working
                                                {
                                                    query = query.Where(u => u.WorkingInfo.Status == 1 && (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                                                              && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date > DateTime.Now.Date)));
                                                    //|| u.WorkingInfo.DepartmentIndex == 0); 
                                                }
                                                else if (listWorking.Contains((int)EmployeeStatusType.StopWorking)) // stopped work
                                                {
                                                    query = query.Where(u => u.WorkingInfo.Status == 1 && (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                                                              && (u.WorkingInfo.ToDate.HasValue && u.WorkingInfo.ToDate.Value.Date <= DateTime.Now.Date)));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "UserType":
                                    if (param.Value != null)
                                    {
                                        query = query.Where(u => u.User.EmployeeType != null && u.User.EmployeeType == (int)param.Value);
                                    }
                                    break;
                                case "FromDate":
                                    if (param.Value != null)
                                    {
                                        var fromDate = (DateTime)param.Value;
                                        query = query.Where(u => u.WorkingInfo.FromDate.Date >= fromDate.Date);
                                    }
                                    break;
                                case "ToDate":
                                    if (param.Value != null)
                                    {
                                        var toDate = (DateTime)param.Value;
                                        query = query.Where(u => u.WorkingInfo.FromDate.Date <= toDate.Date);
                                    }
                                    break;
                            }
                        }
                    }

                    if (pPage < 1) pPage = 1;
                    totalEmployee = query.Count();
                    result = query.OrderBy(x => x.User.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
                    {
                        var rs = _Mapper.Map<HR_EmployeeInfoResult>(x.User);

                        rs.IsEmployee = x?.UserType?.IsEmployee ?? false;
                        rs.UserType = x?.UserType?.Name;
                        rs.EmployeeTypeName = x?.EmployeeType?.Name;

                        if (x.Employee != null)
                            rs = _Mapper.Map(x.Employee, rs);


                        if (x.WorkingInfo != null)
                        {
                            rs = _Mapper.Map(x.WorkingInfo, rs);
                            rs.WorkingInfoIndex = x.WorkingInfo.Index;
                            rs.WorkingStatus = x.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                    && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date > DateTime.Now.Date) ? "IsWorking" : "StoppedWork";
                            rs.Note = x.User.Note;
                            if (rs.WorkingStatus.Contains("StoppedWork") && x.WorkingInfo.ToDate.HasValue)
                            {
                                rs.ToDate = x.WorkingInfo.ToDate.Value.Date;
                            }
                        }


                        if (x.Department != null)
                        {
                            rs = _Mapper.Map(x.Department, rs);
                            rs.DepartmentName = x.Department.Name;
                        }
                        if (x.UserMaster != null)
                        {
                            rs = _Mapper.Map(x.UserMaster, rs);
                            rs.TotalFingerTemplate = (short)AppUtils.CountIfNotEmpty(
                                x.UserMaster.FingerData0,
                                x.UserMaster.FingerData1,
                                x.UserMaster.FingerData2,
                                x.UserMaster.FingerData3,
                                x.UserMaster.FingerData4,
                                x.UserMaster.FingerData5,
                                x.UserMaster.FingerData6,
                                x.UserMaster.FingerData7,
                                x.UserMaster.FingerData8,
                                x.UserMaster.FingerData9
                                );
                        }
                        if (x.CardInfo != null)
                            rs = _Mapper.Map(x.CardInfo, rs);

                        return rs;
                    }).ToList();
                }
                var rs = new DataGridClass(totalEmployee, result);
                return await Task.FromResult(rs);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return await Task.FromResult(new DataGridClass(0, null));
            }
        }

        public async Task CheckExistedOrCreateList(List<HR_EmployeeInfo> listHREmployeeInfo, int pCompanyIndex)
        {
            var listEmployeeATID = listHREmployeeInfo.Select(x => x.EmployeeATID).ToList();
            var listPadLeftEmployeeATID = listHREmployeeInfo.Select(x => x.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
            var listTrimEmployeeATID = listHREmployeeInfo.Select(x => x.EmployeeATID.TrimStart(new Char[] { '0' })).ToList();
            listEmployeeATID.AddRange(listPadLeftEmployeeATID);
            listEmployeeATID.AddRange(listTrimEmployeeATID);
            var listHREmpInfoInDB = Where(x => x.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)).ToList();
            foreach (var item in listHREmployeeInfo)
            {
                var existed = listHREmpInfoInDB.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                if (existed == null || !(existed.EmployeeATID.EndsWith(item.EmployeeATID) && existed.EmployeeATID.Replace(item.EmployeeATID, "0").All(x => x == '0')))
                {
                    existed = new HR_EmployeeInfo();
                    existed = _Mapper.Map<HR_EmployeeInfo>(item);
                    await InsertAsync(existed);
                }
            }
        }
    }
}
