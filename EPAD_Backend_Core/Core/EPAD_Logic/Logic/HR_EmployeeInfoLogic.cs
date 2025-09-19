using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using EPAD_Data.Models;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Common.Types;
using Microsoft.Data.SqlClient;
using EPAD_Common.Extensions;
using System.Data;
using Microsoft.Extensions.Configuration;
using EPAD_Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EPAD_Common.Utility;
using AutoMapper;

namespace EPAD_Logic
{
    public class HR_EmployeeInfoLogic : IHR_EmployeeInfoLogic
    {
        ConfigObject _Config;
        IMemoryCache _Cache;
        ezHR_Context ezHR_Context;
        private ILogger _logger;
        private EPAD_Context _dbContext;
        private readonly IMapper _mapper;

        public HR_EmployeeInfoLogic(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, EPAD_Context dbContext, IMapper mapper)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
            _logger = loggerFactory.CreateLogger<HR_EmployeeInfoLogic>();
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<EmployeeInfoResponse>> GetEmployeeInfoByIDs(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var query = from u in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        select new EmployeeInfoResponse { EmployeeID = u.EmployeeATID, FullName = u.FullName };

            return await Task.FromResult(query.ToList());
        }

        public async Task<List<HR_EmployeeInfoResult>> GetAllEmployeeInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true))
                        join e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join w in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals w.EmployeeATID
                        join d in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                        on w.DepartmentIndex equals d.Index
                        join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count > 0 ? empLookup.Contains(x.EmployeeATID) : true) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        select new { User = u, Employee = e, CardInfo = ci, WorkingInfo = w, Department = d };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _mapper.Map<HR_EmployeeInfoResult>(x.User);
                rs = _mapper.Map(x.Employee, rs);
                rs.DepartmentName = x.Department.Name;
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetDataGrid(string pFilter, long[] pDepartmentIndex, int pCompanyIndex, int pPage, int pLimit)
        {
            List<HR_EmployeeInfoResult> dummy = new List<HR_EmployeeInfoResult>();

            if (_Config.IntegrateDBOther)
            {
                var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                             join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on e.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                             on eWorkResult.PositionIndex equals p.Index into pWork
                             from pWorkResult in pWork.DefaultIfEmpty()

                             join t in ezHR_Context.HR_Titles.Where(x => x.CompanyIndex == pCompanyIndex)
                             on eWorkResult.TitlesIndex equals t.Index into tWork
                             from tWorkResult in tWork.DefaultIfEmpty()

                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();
                
                if (string.IsNullOrEmpty(pFilter) == false)
                {
                    query = query.Where(x => x.EmployeeATID.ContainsIgnoreCase(pFilter)
                        || x.Employee.EmployeeCode.ContainsIgnoreCase(pFilter)
                        || x.FullName.ContainsIgnoreCase(pFilter)
                        || x.Employee.CardNumber.ContainsIgnoreCase(pFilter)
                        || x.Department.Name.ContainsIgnoreCase(pFilter));
                }

                if (pDepartmentIndex.Length != 0)
                {
                    query = query.Where(x => pDepartmentIndex.Contains(x.WorkingInfo.DepartmentIndex ?? 0));
                }

                query = query.Where(x => x.WorkingInfo.FromDate.Value.Date <= DateTime.Now.Date && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date));


                if (pPage < 1) pPage = 1;
                var result = query.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
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
                        JoinedDate = x.Employee?.JoinedDate
                    };
                    return rs;
                }).ToList();

                var cardInfo = _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true).ToDictionarySafe(x => x.EmployeeATID);
                dummy = result.Select(x =>
                {
                    var u = _mapper.Map<HR_EmployeeInfoResult>(x);
                    u.CardNumber = cardInfo.ContainsKey(u.EmployeeATID) ? cardInfo[u.EmployeeATID].CardNumber : "";
                    return u;
                }).ToList();
            }
            else
            {
                var query = (from u in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (x.EmployeeType == null || x.EmployeeType == 1))
                             join e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join wi in _dbContext.IC_WorkingInfo
                             on eResult.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in _dbContext.IC_Department
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                             on eResult.EmployeeATID equals c.EmployeeATID into cWork
                             from cResult in cWork.DefaultIfEmpty()

                             select new { User = u, Employee = eResult, WorkingInfo = eWorkResult, Department = dWorkResult, CardInfo = cResult }).AsQueryable();

                if (string.IsNullOrEmpty(pFilter) == false)
                {
                    query = query.Where(x => x.User.EmployeeATID.ContainsIgnoreCase(pFilter)
                        || x.User.EmployeeCode.ContainsIgnoreCase(pFilter)
                        || x.User.FullName.ContainsIgnoreCase(pFilter)
                        || x.CardInfo.CardNumber.ContainsIgnoreCase(pFilter)
                        || x.Department.Name.ContainsIgnoreCase(pFilter));
                }

                if (pDepartmentIndex.Length != 0)
                {
                    query = query.Where(x => pDepartmentIndex.Contains(x.WorkingInfo.DepartmentIndex) && x.Department.IsInactive != true);
                }

                query = query.Where(x => x.WorkingInfo.FromDate.Date <= DateTime.Now.Date 
                    && (!x.WorkingInfo.ToDate.HasValue || x.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date));

                if (pPage < 1) pPage = 1;
                dummy = query.OrderBy(x => x.User.EmployeeATID).Skip((pPage - 1) * pLimit)
                    .Take(pLimit).AsEnumerable().Select(x =>
                {
                    var rs = _mapper.Map<HR_EmployeeInfoResult>(x.User);
                    rs = _mapper.Map(x.Employee, rs);
                    rs = _mapper.Map(x.WorkingInfo, rs);
                    rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                    return rs;
                }).ToList();
            }

            var rs = new DataGridClass(dummy.Count, dummy);
            return await Task.FromResult(rs);
        }

        public async Task<HR_EmployeeInfoResult> GetEmployeeInfo(string pEmployeeATID, int pCompanyIndex, string employeeCode = "")
        {
            try
            {
                var dummy = from u in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID
                            && (employeeCode == "" || x.EmployeeCode == "" || x.EmployeeCode == employeeCode))
                            join e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals e.EmployeeATID

                            join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID into card
                            from ci in card.DefaultIfEmpty()

                            join w in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.Status == (short)TransferStatus.Approve
                            && x.FromDate.Date <= DateTime.Now.Date && (x.ToDate == null || (x.ToDate.HasValue && x.ToDate.Value >= DateTime.Now.Date)))
                            on u.EmployeeATID equals w.EmployeeATID into work
                            from w in work.DefaultIfEmpty()

                            join d in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                            on w.DepartmentIndex equals d.Index into depart
                            from d in depart.DefaultIfEmpty()

                            select new { User = u, Employee = e, CardInfo = ci, Department = d, WorkingInfo = w };

                var result = dummy.AsEnumerable().Select(x =>
                {
                    var rs = _mapper.Map<HR_EmployeeInfoResult>(x.User);
                    rs = _mapper.Map(x.Employee, rs);
                    rs = _mapper.Map(x.Department, rs);
                    rs = _mapper.Map(x.WorkingInfo, rs);
                    if (x.CardInfo != null)
                    {
                        rs = _mapper.Map(x.CardInfo, rs);
                        rs.CardNumber = x?.CardInfo?.CardNumber;
                    }

                    if (rs != null)
                        rs.DepartmentName = x?.Department?.Name;

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

        public async Task<List<HR_EmployeeInfoResult>> GetMany(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;
            return null;
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var resut = new List<HR_EmployeeInfoResult>();
            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var pPage = Convert.ToInt32(pageIndex.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);
            int totalEmployee = 0;

            if (_Config.IntegrateDBOther)
            {
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


                             where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName }).AsQueryable();

                totalEmployee = query.Count();
                resut = query.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
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
                        JoinedDate = x.Employee?.JoinedDate
                    };
                    return rs;
                }).ToList();
            }
            else
            {
                var query = (from u in _dbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (x.EmployeeType == null || x.EmployeeType == (int)EmployeeType.Employee))
                             join e in _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals e.EmployeeATID into eCheck
                             from eResult in eCheck.DefaultIfEmpty()

                             join wi in _dbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                             on eResult.EmployeeATID equals wi.EmployeeATID into eWork
                             from eWorkResult in eWork.DefaultIfEmpty()

                             join d in _dbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && x.IsInactive != true)
                             on eWorkResult.DepartmentIndex equals d.Index into dWork
                             from dWorkResult in dWork.DefaultIfEmpty()

                             join c in _dbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                             on eResult.EmployeeATID equals c.EmployeeATID into cWork
                             from cResult in cWork.DefaultIfEmpty()

                             join us in _dbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex)
                             on u.EmployeeATID equals us.EmployeeATID into usWork
                             from usWorkResult in usWork.DefaultIfEmpty()

                             select new
                             {
                                 User = u,
                                 Employee = eResult,
                                 WorkingInfo = eWorkResult,
                                 Department = dWorkResult,
                                 CardInfo = cResult,
                                 UserMaster = usWorkResult
                             }).AsEnumerable();

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
                                    query = query.Where(u => u.User.EmployeeATID.Contains(filter)
                                    || (u.Department != null && u.Department.Name.Contains(filter))
                                    || u.User.FullName.Contains(filter)
                                    || (u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter))
                                    || (!string.IsNullOrEmpty(u.User.EmployeeCode) && u.User.EmployeeCode.Contains(filter)));
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
                                    query = query.Where(u => (u.WorkingInfo.FromDate.Date <= DateTime.Now.Date
                                        && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo.DepartmentIndex == 0);
                                }
                                break;
                        }
                    }
                }

                if (pPage < 1) pPage = 1;
                totalEmployee = query.Count();
                resut = query.OrderBy(x => x.User.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
                {
                    var rs = _mapper.Map<HR_EmployeeInfoResult>(x.User);
                    if (x.Employee != null)
                        rs = _mapper.Map(x.Employee, rs);
                    if (x.WorkingInfo != null)
                        rs = _mapper.Map(x.WorkingInfo, rs);
                    if (x.Department != null)
                    {
                        rs = _mapper.Map(x.Department, rs);
                        rs.DepartmentName = x.Department.Name;
                    }
                    if (x.UserMaster != null)
                    {
                        rs = _mapper.Map(x.UserMaster, rs);
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
                        rs = _mapper.Map(x.CardInfo, rs);

                    return rs;

                }).ToList();
            }
            var rs = new DataGridClass(totalEmployee, resut);
            return await Task.FromResult(rs);
        }

        public async Task CheckExistedOrCreate(HR_EmployeeInfo hrEmployeeInfo, int pCompanyIndex, bool isCreate)
        {
            if (isCreate)
            {
                await _dbContext.HR_EmployeeInfo.AddAsync(hrEmployeeInfo);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task CheckExistedOrCreateList(List<HR_EmployeeInfo> listHREmployeeInfo, int pCompanyIndex)
        {
            var listEmployeeATID = listHREmployeeInfo.Select(x => x.EmployeeATID).ToHashSet();
            var listHREmpInfoInDB = _dbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)).ToList();
            foreach (var item in listHREmployeeInfo)
            {
                var existed = listHREmpInfoInDB.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                if (existed == null)
                {
                    _dbContext.HR_EmployeeInfo.Add(item);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IHR_EmployeeInfoLogic
    {
        public Task<DataGridClass> GetDataGrid(string pFilter, long[] pDepartmentIndex, int pCompanyIndex, int pPgae, int pLimit);
        Task<List<EmployeeInfoResponse>> GetEmployeeInfoByIDs(string[] pEmployeeATIDs, int pCompanyIndex);
        public Task<List<HR_EmployeeInfoResult>> GetAllEmployeeInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        public Task<HR_EmployeeInfoResult> GetEmployeeInfo(string pEmployeeATID, int pCompanyIndex, string employeeCode = "");
        public Task<DataGridClass> GetPage(List<AddedParam> addedParams, int cCompanyIndex);
        public Task<List<HR_EmployeeInfoResult>> GetMany(List<AddedParam> addedParams, int cCompanyIndex);
        public Task CheckExistedOrCreateList(List<HR_EmployeeInfo> listHREmployeeInfo, int pCompanyIndex);

    }
}
