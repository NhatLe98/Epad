using AutoMapper;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Data.Models.Other;
using EPAD_Logic;
using EPAD_Services.Interface;
using EPAD_Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_UserService : BaseServices<HR_User, EPAD_Context>, IHR_UserService
    {
        ConfigObject _Config;
        IMemoryCache _Cache;
        ezHR_Context ezHR_Context;
        private readonly IHR_EmployeeInfoService _HR_EmployeeInfoService;
        private IIC_ConfigLogic _iC_ConfigLogic;
        private readonly IMapper _mapper;
        private string mLinkEMSApi;
        private IConfiguration _Configuration;
        private string mCommunicateToken;
        private string userNameEMS;
        private string passwordEMS;
        private readonly ILogger _logger;
        public HR_UserService(IServiceProvider serviceProvider, IIC_ConfigLogic iC_ConfigLogic, IMapper mapper, IConfiguration configuration, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _logger = loggerFactory.CreateLogger<HR_UserService>();
            _Configuration = configuration;
            _Config = ConfigObject.GetConfig(_Cache);
            ezHR_Context = serviceProvider.GetService<ezHR_Context>();
            _iC_ConfigLogic = iC_ConfigLogic;
            _HR_EmployeeInfoService = serviceProvider.GetService<IHR_EmployeeInfoService>();
            mCommunicateToken = _Configuration.GetValue<string>("CommunicateToken");
            mLinkEMSApi = _Configuration.GetValue<string>("EMSApi");
            userNameEMS = _Configuration.GetValue<string>("UserNameForEMS");
            passwordEMS = _Configuration.GetValue<string>("PassWordForEMS");
            _mapper = mapper;

        }

        public async Task<List<string>> GetAllEmployeeATID()
        {
            return await DbContext.HR_User.AsNoTracking().Select(x => x.EmployeeATID).ToListAsync();
        }

        public async Task<List<HR_UserResult>> GetAllHR_UserAsync(int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID
                            into cCheck
                            from c in cCheck
                            select new { User = u, CardInfo = c };
                var rs = dummy.AsEnumerable().Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x.User);
                    u.CardNumber = x.CardInfo?.CardNumber ?? "";
                    return u;
                });

                return await Task.FromResult(rs.ToList());
            }
            else
            {
                List<HR_Employee> empDtos = ezHR_Context.HR_Employee.Where(t => t.CompanyIndex == _Config.CompanyIndex).ToList();
                var cardInfo = DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true).ToDictionarySafe(x => x.EmployeeATID);
                var departments = ezHR_Context.HR_Department.Where(t => t.CompanyIndex == _Config.CompanyIndex).ToList();
                var hr2ic = empDtos.Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x);
                    u.CardNumber = !string.IsNullOrEmpty(u.CardNumber) ? u.CardNumber : (cardInfo.ContainsKey(u.EmployeeATID) ? cardInfo[u.EmployeeATID].CardNumber : "");
                    return u;
                });

                var employeeHr = (from u in hr2ic
                                  join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                  on u.EmployeeATID equals wi.EmployeeATID into eWork
                                  from eWorkResult in eWork.DefaultIfEmpty()
                                  select new { User = u, Working = eWorkResult }).ToList();

                var hr = employeeHr.Select(x =>
                {
                    var type = x.Working?.DepartmentIndex ?? 0;
                    var departmentType = GetParent(departments, type);
                    x.User.DepartmentIndexEz = x.Working?.Index;
                    x.User.EmployeeType = Convert.ToInt32(departmentType);
                    return x.User;
                }).ToList();


                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID
                            into cCheck
                            from c in cCheck
                            select new { User = u, CardInfo = c };

                var hrUsers = dummy.AsEnumerable().Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x.User);
                    u.CardNumber = x.CardInfo?.CardNumber ?? "";
                    return u;
                });
                var rs = hr.Concat(hrUsers);

                return await Task.FromResult(rs.ToList());
            }
        }

        public async Task<List<HR_EmployeeInfoResult>> GetEmployeeAndDepartmentLookup(int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var query = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && (!x.EmployeeType.HasValue
                            || x.EmployeeType == 0 || x.EmployeeType == (short)EmployeeType.Employee))
                            join w in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.Status == (short)TransferStatus.Approve
                                && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue || x.ToDate.Value.Date >= DateTime.Now.Date))
                            on u.EmployeeATID equals w.EmployeeATID into wWorking
                            from wResult in wWorking.DefaultIfEmpty()

                            join d in DbContext.IC_Department
                            on wResult.DepartmentIndex equals d.Index into dWorking
                            from dResult in dWorking.DefaultIfEmpty()

                            select new { User = u, Department = dResult };

                var result = (await query.ToListAsync()).Select(x =>
                {
                    var u = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                    u = _Mapper.Map(x.Department, u);
                    return u;
                }).Where(x => x != null).ToList();

                return result;
            }
            else
            {
                var query = from u in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                            join w in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                            on u.EmployeeATID equals w.EmployeeATID into wWorking
                            from wResult in wWorking.DefaultIfEmpty()

                            join d in ezHR_Context.HR_Department
                            on wResult.DepartmentIndex equals d.Index into dWorking
                            from dResult in dWorking.DefaultIfEmpty()

                            select new { User = u, Department = dResult };

                var result = query.AsEnumerable().Select(x =>
                {
                    var u = _Mapper.Map<HR_EmployeeInfoResult>(x.User);
                    //if (x.Department != null)
                    //{
                    //    u = _Mapper.Map(x.Department, u);
                    //}
                    return u;
                });
                //var empDtos = ezHR_Context.HR_Employee.Where(t => t.CompanyIndex == _Config.CompanyIndex).ToList();

                //var hr2ic = empDtos.Select(x => _Mapper.Map<HR_User>(x));

                //var dummy = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex).ToList();

                //var rs = hr2ic.Concat(dummy);

                return await Task.FromResult(result.Where(x => x != null).ToList());
            }
        }

        public async Task<List<HR_User>> GetEmployeeLookup(int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var result = DbContext.HR_User
                    .Where(x => x.CompanyIndex == pCompanyIndex)
                    .ToList();

                return await Task.FromResult(result.ToList());
            }
            else
            {
                //var empDtos = ezHR_Context.HR_Employee
                //    .Where(t => t.CompanyIndex == _Config.CompanyIndex)
                //    .ToList();

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


                             where ((e.MarkForDelete == null || e.MarkForDelete == false))   // loc nhan vien chua nghi viec

                             select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName });
                query = query.Where(u => (u.WorkingInfo.FromDate <= DateTime.Now.Date
                                            && (!u.WorkingInfo.ToDate.HasValue || u.WorkingInfo.ToDate.Value.Date >= DateTime.Now.Date)) || u.WorkingInfo.DepartmentIndex == 0);
                var empDtos = query.Select(e => e.Employee).ToList();

                var hr2ic = empDtos.Select(x => _Mapper.Map<HR_User>(x));

                return await Task.FromResult(hr2ic.ToList());
            }
        }

        public async Task<HR_UserResult> GetHR_UserByIDAsync(string pEmployeeATID, int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var dummy = DbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == pEmployeeATID && x.CompanyIndex == pCompanyIndex);
                var cardInfo = DbContext.HR_CardNumberInfo.FirstOrDefault(x => x.EmployeeATID == pEmployeeATID && x.CompanyIndex == pCompanyIndex && x.IsActive == true);
                if (dummy == null) return null;

                var userInfo = _Mapper.Map<HR_UserResult>(dummy);
                userInfo.CardNumber = cardInfo?.CardNumber ?? "";

                return await Task.FromResult(userInfo);
            }
            else
            {
                HR_UserResult userInfo = null;
                var empDtos = ezHR_Context.HR_Employee.FirstOrDefault(t => t.EmployeeATID == pEmployeeATID && t.CompanyIndex == _Config.CompanyIndex);
                var cardInfo = DbContext.HR_CardNumberInfo.FirstOrDefault(x => x.EmployeeATID == pEmployeeATID && x.CompanyIndex == pCompanyIndex && x.IsActive == true);
                if (empDtos == null)
                {
                    var dummy = DbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == pEmployeeATID && x.CompanyIndex == pCompanyIndex);
                    if (dummy != null)
                    {
                        userInfo = _Mapper.Map<HR_UserResult>(dummy);
                        userInfo.CardNumber = cardInfo?.CardNumber ?? "";
                    }
                }
                else
                {
                    userInfo = _Mapper.Map<HR_UserResult>(empDtos);
                    userInfo.CardNumber = !string.IsNullOrEmpty(empDtos.CardNumber) ? empDtos.CardNumber : (cardInfo?.CardNumber ?? "");
                }
                return await Task.FromResult(userInfo);
            }
        }

        public async Task SaveOrOverwriteList(List<HR_User> listHRUser, int pCompanyIndex)
        {
            var listEmployeeATID = listHRUser.Select(e => e.EmployeeATID).ToHashSet();
            var listHRUserOnDB = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)).ToHashSet();
            foreach (var item in listHRUser)
            {
                var existed = listHRUserOnDB.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);

                if (existed != null)
                {
                    existed = _Mapper.Map<HR_User>(item);
                    await UpdateAsync(existed);
                }
                else
                {
                    existed = new HR_User();
                    existed = _Mapper.Map<HR_User>(item);
                    await InsertAsync(existed);
                }
            }
        }

        public async Task SaveOrUpdateList(List<HR_User> listHRUser, int pCompanyIndex)
        {
            var listEmployeeATID = listHRUser.Select(e => e.EmployeeATID).ToHashSet();
            var listHRUserOnDB = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)).ToHashSet();
            foreach (var item in listHRUser)
            {
                var existed = listHRUserOnDB.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);

                if (existed != null)
                {
                    existed = _Mapper.Map<HR_User>(item);
                    await UpdateAsync(existed);
                }
                else
                {
                    existed = new HR_User();
                    existed = _Mapper.Map<HR_User>(item);
                    await InsertAsync(existed);
                }
            }
        }

        public async Task CheckExistedOrCreate(HR_User hrUser, int pCompanyIndex)
        {
            var existed = DbContext.HR_User.FirstOrDefault(x => x.CompanyIndex == pCompanyIndex && (x.EmployeeATID == hrUser.EmployeeATID || x.EmployeeATID == hrUser.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')
            || x.EmployeeATID == hrUser.EmployeeATID.TrimStart(new Char[] { '0' })));
            if (existed == null || !(existed.EmployeeATID.EndsWith(hrUser.EmployeeATID) && existed.EmployeeATID.Replace(hrUser.EmployeeATID, "0").All(x => x == '0')))
            {
                existed = new HR_User();
                existed = _Mapper.Map<HR_User>(hrUser);
                await InsertAsync(existed);
            }
        }

        public async Task<List<string>> CheckExistedOrCreateList(List<HR_User> listHRUser, int pCompanyIndex)
        {
            var listEmployeeATID = listHRUser.Select(e => e.EmployeeATID).ToList();
            var listPadLeftEmployeeATID = listHRUser.Select(e => e.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
            var listTrimEmployeeATID = listHRUser.Select(e => e.EmployeeATID.TrimStart(new Char[] { '0' })).ToList();
            listEmployeeATID.AddRange(listPadLeftEmployeeATID);
            listEmployeeATID.AddRange(listTrimEmployeeATID);
            var listHRUserOnDB = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && listEmployeeATID.Contains(x.EmployeeATID)).ToHashSet();
            var lstInsert = new List<string>();
            foreach (var item in listHRUser)
            {
                var existed = listHRUserOnDB.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                if (existed == null || !(existed.EmployeeATID.EndsWith(item.EmployeeATID) && existed.EmployeeATID.Replace(item.EmployeeATID, "0").All(x => x == '0')))
                {
                    existed = new HR_User();
                    existed = _Mapper.Map<HR_User>(item);
                    lstInsert.Add(item.EmployeeATID);
                    await InsertAsync(existed);
                }
            }
            return lstInsert; 
        }

        public async Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermission(UserInfo user)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName
                                 });


                queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));

                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                        || (user.ListDepartmentAssigned.Contains(x.DepartmentIndex.Value) || x.DepartmentIndex.Value == 0));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                             on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == user.CompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo
                                on e.EmployeeATID equals c.EmployeeATID into cardinfo
                                from cardif in cardinfo.DefaultIfEmpty()

                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cardif != null ? cardif.CardNumber : "0",
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf != null ? wkinf.DepartmentIndex : 0,
                                    Department = dept != null ? dept.Name : "NoDepartment",
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));


                queryData = queryData.Where(x => !x.ToDate.HasValue
                    || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));

                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                        || (user.ListDepartmentAssigned.Contains(x.DepartmentIndex.Value) || x.DepartmentIndex.Value == 0));
                }


                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByPermissionImprovePerformance(UserInfo user)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == user.CompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName,
                                 });


                queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));

                var result = await queryData.ToListAsync();

                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    result = result.Where(x => !x.DepartmentIndex.HasValue
                        || (user.ListDepartmentAssigned.Contains(x.DepartmentIndex.Value) || x.DepartmentIndex.Value == 0)).ToList();
                }

                return result;
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == user.CompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == user.CompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date
                                    && (!w.ToDate.HasValue
                                    || (w.ToDate.HasValue && w.ToDate.Value.Date >= DateTime.Now.Date)))
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()

                                join cus in DbContext.HR_CustomerInfo
                                on e.EmployeeATID equals cus.EmployeeATID into customerInfo
                                from customer in customerInfo.DefaultIfEmpty()

                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == user.CompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == user.CompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo
                                on e.EmployeeATID equals c.EmployeeATID into cardinfo
                                from cardif in cardinfo.DefaultIfEmpty()

                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cardif != null ? cardif.CardNumber : "0",
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf != null ? wkinf.DepartmentIndex : 0,
                                    Department = dept != null ? dept.Name : "NoDepartment",
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName,
                                    NRIC = !string.IsNullOrEmpty(e.Nric) ? e.Nric : (!string.IsNullOrEmpty(customer.NRIC) ? customer.NRIC : "")
                                };

                var result = await queryData.ToListAsync();

                var listAllDepartmentIndex = DbContext.IC_Department.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                result = result.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value))).ToList();

                if (user.ListDepartmentAssigned != null && user.ListDepartmentAssigned.Count > 0)
                {
                    result = result.Where(x => !x.DepartmentIndex.HasValue
                        || (user.ListDepartmentAssigned.Contains(x.DepartmentIndex.Value) || x.DepartmentIndex.Value == 0)).ToList();
                }

                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetDriverCompactInfoByEmployeeATID(List<string> pEmployeeATIDs, DateTime pDate, int pCompanyIndex)
        {
            var queryData = from e in DbContext.IC_PlanDock.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeATIDs.Contains(x.TripId))

                            select new EmployeeFullInfo()
                            {
                                Avatar = e.Avatar,
                                EmployeeATID = e.TripId,
                                EmployeeCode = null,
                                CardNumber = null,
                                FullName = e.DriverName,
                                Email = "",
                                Phone = e.DriverPhone,
                                EmployeeType = EmployeeType.Driver,
                                CompanyIndex = e.CompanyIndex,

                            };
            return await queryData.ToListAsync();
        }

        public async Task<EmployeeFullInfo> GetDriverCompactInfoByEmployeeATIDOrCardId(string pEmployeeATIDs, string cardID, DateTime pDate, int pCompanyIndex)
        {
            var customerCard = await DbContext.HR_CustomerCard.FirstOrDefaultAsync(x => x.CardID == cardID);
            var dataQuery = from e in DbContext.IC_PlanDock.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeATIDs== x.TripId)
                            join gc in DbContext.GC_TruckDriverLog.Where(x => x.CompanyIndex == pCompanyIndex && !x.IsInactive)
                            on e.TripId equals gc.TripCode
                            select new EmployeeFullInfo()
                            {
                                Avatar = e.Avatar,
                                EmployeeATID = e.TripId,
                                EmployeeCode = null,
                                CardNumber = gc.CardNumber,
                                FullName = e.DriverName,
                                Email = "",
                                Phone = e.DriverPhone,
                                EmployeeType = EmployeeType.Driver,
                                CompanyIndex = e.CompanyIndex,

                            };
            var data = await dataQuery.FirstOrDefaultAsync();
            if (data == null || data != null && data.CardNumber != cardID)
            {
                if (customerCard != null)
                {
                    var dataExtraDriver = await DbContext.GC_TruckExtraDriverLog.FirstOrDefaultAsync(x => x.CardNumber == customerCard.CardNumber
                        && !x.IsInactive);
                    if (dataExtraDriver != null)
                    {
                        data = new EmployeeFullInfo();
                        data.EmployeeATID = dataExtraDriver.TripCode;
                        data.EmployeeCode = null;
                        data.CardNumber = customerCard.CardNumber;
                        data.FullName = dataExtraDriver.ExtraDriverName;
                        data.EmployeeType = EmployeeType.Driver;
                        data.CompanyIndex = customerCard.CompanyIndex;
                        data.IsExtraDriver = true;
                    }
                }
            }
            return data;
        }
        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeATID(List<string> pEmployeeATIDs, DateTime pDate, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeATIDs.Contains(x.EmployeeATID))
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     CardNumber = e.CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeATIDs.Contains(x.EmployeeATID))
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workingInfoGroup
                                from wrk in workingInfoGroup.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into employeeCard
                                from empCard in employeeCard.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = empCard.CardNumber,
                                    FullName = e.FullName,
                                    Email = "",
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wrk.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wrk.FromDate,
                                    ToDate = wrk.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    UserName = e.UserName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
        }


        public async Task<List<EmployeeFullInfo>> GetAllNanny(DateTime pDate, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                                 && dWorkResult.Code == "BM"

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     CardNumber = e.CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workingInfoGroup
                                from wrk in workingInfoGroup.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into employeeCard
                                from empCard in employeeCard.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = empCard.CardNumber,
                                    FullName = e.FullName,
                                    Email = "",
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wrk.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wrk.FromDate,
                                    ToDate = wrk.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    UserName = e.UserName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
        }

        private List<HR_Department> GetChildren(List<HR_Department> foos, long id)
        {
            return foos
                .Where(x => x.ParentIndex == id)
                .Union(foos.Where(x => x.ParentIndex == id)
                    .SelectMany(y => GetChildren(foos, y.Index))
                ).ToList();
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

        public async Task<List<EmployeeFullInfo>> GetAllStudent(DateTime pDate, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var departments = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToListAsync();
                var studentDepartmentParentid = departments.FirstOrDefault(x => x.Code == "AllST")?.Index;
                if (studentDepartmentParentid == null) return new List<EmployeeFullInfo>();

                var studentDepartments = GetChildrenToId(departments, studentDepartmentParentid.Value, "Student").Select(x => x.Index).ToList();
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec
                                 && studentDepartments.Contains(dWorkResult.Index)

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     CardNumber = e.CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workingInfoGroup
                                from wrk in workingInfoGroup.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wrk.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into employeeCard
                                from empCard in employeeCard.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = empCard.CardNumber,
                                    FullName = e.FullName,
                                    Email = "",
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wrk.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wrk.FromDate,
                                    ToDate = wrk.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    UserName = e.UserName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByEmployeeCodes(List<string> pEmployeeCodes, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeCodes.Contains(x.EmployeeCode))
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && pEmployeeCodes.Contains(x.EmployeeCode))
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on w.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on w.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into employeeCard
                                from empCard in employeeCard.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = empCard.CardNumber,
                                    FullName = e.FullName,
                                    Email = "",
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = w.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = w.FromDate,
                                    ToDate = w.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    UserName = e.UserName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfo(int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                             on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo
                                on e.EmployeeATID equals c.EmployeeATID into cardinfo
                                from cardif in cardinfo.DefaultIfEmpty()

                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cardif != null ? cardif.CardNumber : "0",
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                            || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                            && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetAllEmployeeTypeUserCompactInfo(int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                                && (!x.EmployeeType.HasValue || x.EmployeeType == (short)EmployeeType.Employee))
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                             on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo
                                on e.EmployeeATID equals c.EmployeeATID into cardinfo
                                from cardif in cardinfo.DefaultIfEmpty()

                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cardif != null ? cardif.CardNumber : "0",
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                            || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                            && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetFilterEmployeeTypeUserCompactInfo(int pCompanyIndex, List<string> employeeATIDs)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex
                                 && (employeeATIDs.Count <= 0 || employeeATIDs.Contains(x.EmployeeATID)))
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                                && (!x.EmployeeType.HasValue || x.EmployeeType == (short)EmployeeType.Employee)
                                && (employeeATIDs.Count <= 0 || employeeATIDs.Contains(x.EmployeeATID)))
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                             on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo
                                on e.EmployeeATID equals c.EmployeeATID into cardinfo
                                from cardif in cardinfo.DefaultIfEmpty()

                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cardif != null ? cardif.CardNumber : "0",
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    RootDepartment = (dept == null || (dept != null && (!dept.ParentIndex.HasValue
                                        || (dept.ParentIndex.HasValue && (dept.ParentIndex.Value == 0 || dept.ParentIndex.Value == dept.Index))))),
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                            || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                            && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetAllEmployeeCompactInfoByDate(DateTime pDate, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex
                                 && x.FromDate.HasValue && x.FromDate.Value.Date <= pDate.Date
                                 && ((x.ToDate.HasValue && x.ToDate.Value.Date >= pDate.Date) || !x.ToDate.HasValue))
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = 0,
                                     NameFilter = e.EmployeeATID + " - " + e.LastName + " " + e.MidName + " " + e.FirstName
                                 });

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= pDate.Date
                                    && ((w.ToDate.HasValue && w.ToDate.Value.Date >= pDate.Date) || !w.ToDate.HasValue))
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                    NameFilter = e.EmployeeATID + " - " + e.FullName
                                };
                //var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                //listAllDepartmentIndex.Add(0);
                //queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                //    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetAllUserCompactInfo(int pCompanyIndex)
        {
            var addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into eWork
                                from eWorkResult in eWork.DefaultIfEmpty()

                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into hrEmp
                                from heEmpResult in hrEmp.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on eWorkResult.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on eWorkResult.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                    FullName = e.FullName,
                                    Email = heEmpResult.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = eWorkResult.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = eWorkResult.FromDate,
                                    ToDate = eWorkResult.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => (x.EmployeeType.HasValue && x.EmployeeType.Value != EmployeeType.Employee)
                            || ((!x.EmployeeType.HasValue || x.EmployeeType.HasValue && x.EmployeeType.Value == EmployeeType.Employee)
                            && x.FromDate.HasValue && x.FromDate.Value.Date <= DateTime.Now.Date && (!x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                            || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                            && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)))));
                    }
                    else
                    {
                        queryData = queryData.Where(x => (x.EmployeeType.HasValue && x.EmployeeType.Value != EmployeeType.Employee)
                        || ((!x.EmployeeType.HasValue || x.EmployeeType.HasValue && x.EmployeeType.Value == EmployeeType.Employee)
                        && x.FromDate.HasValue && x.FromDate.Value.Date <= DateTime.Now.Date && (!x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date))));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => (x.EmployeeType.HasValue && x.EmployeeType.Value != EmployeeType.Employee)
                        || ((!x.EmployeeType.HasValue || x.EmployeeType.HasValue && x.EmployeeType.Value == EmployeeType.Employee)
                        && x.FromDate.HasValue && x.FromDate.Value.Date <= DateTime.Now.Date && (!x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date))));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID).Select(grp => grp.First()).ToList();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByListEmail(List<string> listEmail, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = 0
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID
                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on w.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on w.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                    FullName = e.FullName,
                                    Email = h.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = w.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = w.FromDate,
                                    ToDate = w.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (listEmail != null && listEmail.Count > 0)
                {
                    queryData = queryData.Where(x => listEmail.Contains(x.Email));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByListEmpATID(List<string> listEmpATID, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (listEmpATID != null && listEmpATID.Count > 0)
                {
                    queryData = queryData.Where(x => listEmpATID.Contains(x.EmployeeATID));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into cWork
                                from cResult in cWork.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cResult == null ? "" : cResult.CardNumber,
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (listEmpATID != null && listEmpATID.Count > 0)
                {
                    queryData = queryData.Where(x => listEmpATID.Contains(x.EmployeeATID));
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeCompactInfoByCardNumber(string cardNumber, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex && x.CardNumber == cardNumber)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                return await queryData.ToListAsync();
            }
            else
            {

                var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into cWork
                                from cResult in cWork.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cResult == null ? "" : cResult.CardNumber,
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (!string.IsNullOrEmpty(cardNumber))
                {
                    queryData = queryData.Where(x => x.CardNumber == cardNumber);
                }

                var result = await queryData.ToListAsync();
                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
            }
            //return await output.ToListAsync();
        }
        private static bool? CastGenderFromNumberToBool(short? gender)
        {
            if (gender == null || gender != 0 || gender != 1) return null;
            return gender != 0;
        }

        public List<EmployeeFullInfo> GetEmployeeCompactInfo
            (int pCompanyIndex, EmployeeType[] employeeTypes, string fields)
        {
            var output = from user in DbContext.HR_User.Where(
                            x => x.CompanyIndex == pCompanyIndex
                            && (employeeTypes.Length == 0 || employeeTypes.Select(et => (int)et).Contains(x.EmployeeType.Value)))
                         join workingInfo in DbContext.IC_WorkingInfo.Where(
                             w => w.CompanyIndex == pCompanyIndex
                             && w.Status == (short)TransferStatus.Approve
                             && w.FromDate.Date <= DateTime.Now.Date
                             && (w.ToDate == null || w.ToDate.Value.Date >= DateTime.Now.Date))
                         on user.EmployeeATID equals workingInfo.EmployeeATID
                         join department in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                         on workingInfo.DepartmentIndex equals department.Index
                         select new EmployeeFullInfo()
                         {
                             Avatar = user.Avatar,
                             EmployeeATID = user.EmployeeATID,
                             EmployeeCode = user.EmployeeCode,
                             CardNumber = null,
                             FullName = user.FullName,
                             Email = null,
                             Phone = null,
                             Gender = CastGenderFromNumberToBool(user.Gender),
                             JoinedDate = null,
                             DepartmentIndex = workingInfo.DepartmentIndex,
                             Department = department.Name,
                             DepartmentCode = department.Code,
                             Position = null,
                             PositionIndex = workingInfo.PositionIndex,
                             Title = null,
                             TitleIndex = null,
                             EmployeeKindIndex = null,
                             EmployeeKind = null,
                             ManagedDepartment = null,
                             ManagedOtherDepartment = null,
                             DirectManager = null,
                             FromDate = workingInfo.FromDate,
                             ToDate = workingInfo.ToDate,
                             DayOfBirth = (short?)user.DayOfBirth,
                             MonthOfBirth = (short?)user.MonthOfBirth,
                             YearOfBirth = (short?)user.YearOfBirth,
                             TaxNumber = null,
                             SocialInsNo = null,
                             CompanyIndex = user.CompanyIndex,
                             EmployeeType = (EmployeeType?)user.EmployeeType,
                         };

            var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
            if (resultShowStoppedEmp.Item1)
            {
                if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                {
                    output = output.Where(x => !x.ToDate.HasValue
                    || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                    || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                    && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                }
                else
                {
                    output = output.Where(x => !x.ToDate.HasValue
                    || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }
            }
            else
            {
                output = output.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
            }

            var rs = output.GetOnDynamicSelector<EmployeeFullInfo>(fields).ToList();
            return rs;
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeByDepartmentIds(List<string> pDepartmentIDs, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     CardNumber = e.CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (pDepartmentIDs.Contains("0"))
                {
                    queryData = queryData.Where(x => (x.DepartmentIndex.HasValue && pDepartmentIDs.Contains(x.DepartmentIndex.Value.ToString()))
                        || !x.DepartmentIndex.HasValue);
                }
                else
                {
                    queryData = queryData.Where(x => x.DepartmentIndex.HasValue && pDepartmentIDs.Contains(x.DepartmentIndex.Value.ToString()));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var output = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                             join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                             && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                             on e.EmployeeATID equals w.EmployeeATID
                             join d in DbContext.IC_Department
                             on w.DepartmentIndex equals d.Index into depGroup
                             from dep in depGroup.DefaultIfEmpty()
                             select new EmployeeFullInfo()
                             {
                                 Avatar = e.Avatar,
                                 EmployeeATID = e.EmployeeATID,
                                 EmployeeCode = e.EmployeeCode,
                                 CardNumber = null,
                                 FullName = e.FullName,
                                 Email = null,
                                 Phone = null,
                                 Gender = CastGenderFromNumberToBool(e.Gender),
                                 JoinedDate = null,
                                 DepartmentIndex = w.DepartmentIndex,
                                 Department = dep.Name,
                                 DepartmentCode = dep.Code,
                                 Position = null,
                                 PositionIndex = w.PositionIndex,
                                 Title = null,
                                 TitleIndex = null,
                                 EmployeeKindIndex = null,
                                 EmployeeKind = null,
                                 ManagedDepartment = null,
                                 ManagedOtherDepartment = null,
                                 DirectManager = null,
                                 FromDate = w.FromDate,
                                 ToDate = w.ToDate,
                                 DayOfBirth = (short?)e.DayOfBirth,
                                 MonthOfBirth = (short?)e.MonthOfBirth,
                                 YearOfBirth = (short?)e.YearOfBirth,
                                 TaxNumber = null,
                                 SocialInsNo = null,
                                 CompanyIndex = e.CompanyIndex,
                                 EmployeeType = (EmployeeType?)e.EmployeeType,
                             };

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        output = output.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        output = output.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    output = output.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (pDepartmentIDs.Contains("0"))
                {
                    output = output.Where(x => (x.DepartmentIndex.HasValue && pDepartmentIDs.Contains(x.DepartmentIndex.Value.ToString()))
                        || !x.DepartmentIndex.HasValue);
                }
                else
                {
                    output = output.Where(x => x.DepartmentIndex.HasValue && pDepartmentIDs.Contains(x.DepartmentIndex.Value.ToString()));
                }
                var result = output.ToList();
                return result;
            }
        }

        public async Task<List<HR_Department>> GetDepartmentList(int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                return DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                         .Select(x => new HR_Department
                         {
                             Index = x.Index,
                             Code = x.Code,
                             Name = x.Name
                         }).OrderBy(x => x.Name).ToList();
            }
            else
            {
                return ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            }
        }

        public async Task<List<HR_Department>> GetDepartmentByIds(List<string> pDepartmentIds, int pCompanyIndex)
        {
            if (!_Config.IntegrateDBOther)
            {
                return DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                    .Where(x => x.CompanyIndex == pCompanyIndex && pDepartmentIds.Contains(x.Index.ToString()))
                    .Select(x => new HR_Department
                    {
                        Index = x.Index,
                        Code = x.Code,
                        Name = x.Name
                    }).OrderBy(x => x.Name).ToList();
            }
            else
            {
                return ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex
                    && pDepartmentIds.Contains(x.Index.ToString())).Select(x => new HR_Department
                    {
                        Index = x.Index,
                        Code = x.Code,
                        Name = x.Name
                    }).OrderBy(x => x.Name).ToList();
            }
        }

        public async Task CheckUserActivedOrCreate(HR_User hrUser, int pCompanyIndex)
        {
            var user = await DbContext.HR_User.FirstOrDefaultAsync(e => e.CompanyIndex == pCompanyIndex && hrUser.EmployeeATID == e.EmployeeATID);

            if (user != null)
            {
                user.FullName = hrUser.FullName;
                user.Avatar = hrUser.Avatar;
                DbContext.HR_User.Update(user);
            }
            else
            {
                DbContext.HR_User.Add(hrUser);
            }
        }

        public async Task<List<HR_UserResult>> GetAllStudent(int pCompanyIndex)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var studentInfoList = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Student)
                                      join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                      on u.EmployeeATID equals c.EmployeeATID
                                      into cCheck
                                      from c in cCheck
                                      select new { User = u, CardInfo = c };
                var rs = studentInfoList.AsEnumerable().Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x.User);
                    u.CardNumber = x.CardInfo?.CardNumber ?? "";
                    return u;
                });

                var studentList = rs.ToList();

                var parentsList = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Parents).ToList();
                var parentsIds = parentsList.Select(x => x.EmployeeATID).ToList();
                var parentsDetailList = DbContext.HR_ParentInfo.Where(x => x.CompanyIndex == pCompanyIndex && parentsIds.Contains(x.EmployeeATID)).ToList();
                foreach (var item in studentList)
                {
                    var parentsDetail = parentsDetailList.Where(x => x.Students.Contains(item.EmployeeATID)).ToList();
                    if (parentsDetail != null && parentsDetail.Any())
                    {
                        var parentsIdList = parentsDetail.Select(x => x.EmployeeATID).ToList();
                        var parents = parentsList.Where(x => parentsIdList.Contains(x.EmployeeATID)).ToList();
                        var parentResult = new List<HR_ParentInfoResult>();
                        if (parents != null && parents.Any())
                            parentResult = _Mapper.Map<List<HR_ParentInfoResult>>(parents);
                        item.HR_ParentInfoResult = parentResult;
                    }
                }

                return await Task.FromResult(studentList);
            }
            else
            {
                List<HR_Employee> empDtos = ezHR_Context.HR_Employee.Where(t => t.CompanyIndex == _Config.CompanyIndex).ToList();
                var cardInfo = DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true).ToDictionarySafe(x => x.EmployeeATID);

                var hr2ic = empDtos.Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x);
                    u.CardNumber = cardInfo.ContainsKey(u.EmployeeATID) ? cardInfo[u.EmployeeATID].CardNumber : "";
                    return u;
                });

                var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                            join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                            on u.EmployeeATID equals c.EmployeeATID
                            into cCheck
                            from c in cCheck
                            select new { User = u, CardInfo = c };

                var hrUsers = dummy.AsEnumerable().Select(x =>
                {
                    var u = _Mapper.Map<HR_UserResult>(x.User);
                    u.CardNumber = x.CardInfo?.CardNumber ?? "";
                    return u;
                });
                var rs = hr2ic.Concat(hrUsers);

                return await Task.FromResult(rs.ToList());
            }
        }

        public async Task<HR_UserResult> GetStudentById(int pCompanyIndex, string pStudentId)
        {
            if (_Config.IntegrateDBOther == false)
            {
                var student = DbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == pStudentId && x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Student);

                if (student == null) return null;

                var parentsInfo = DbContext.HR_ParentInfo.Where(x => x.Students.Contains(student.EmployeeATID)).ToList();
                var parentsIds = parentsInfo.Select(x => x.EmployeeATID).ToList();
                var parents = DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && parentsIds.Contains(x.EmployeeATID)).ToList();
                var cardInfo = DbContext.HR_CardNumberInfo.FirstOrDefault(x => x.EmployeeATID == pStudentId && x.CompanyIndex == pCompanyIndex && x.IsActive == true);

                var parentResult = new List<HR_ParentInfoResult>();
                if (parents != null && parents.Any())
                    parentResult = _Mapper.Map<List<HR_ParentInfoResult>>(parents);

                var studentInfo = _Mapper.Map<HR_UserResult>(student);
                studentInfo.CardNumber = cardInfo?.CardNumber ?? "";
                studentInfo.HR_ParentInfoResult = parentResult;

                return await Task.FromResult(studentInfo);
            }
            else
            {
                HR_UserResult userInfo = null;
                var empDtos = ezHR_Context.HR_Employee.FirstOrDefault(t => t.EmployeeATID == pStudentId && t.CompanyIndex == _Config.CompanyIndex);
                var cardInfo = DbContext.HR_CardNumberInfo.FirstOrDefault(x => x.EmployeeATID == pStudentId && x.CompanyIndex == pCompanyIndex && x.IsActive == true);
                if (empDtos == null)
                {
                    var dummy = DbContext.HR_User.FirstOrDefault(x => x.EmployeeATID == pStudentId && x.CompanyIndex == pCompanyIndex);
                    if (dummy != null)
                    {
                        userInfo = _Mapper.Map<HR_UserResult>(dummy);
                        userInfo.CardNumber = cardInfo?.CardNumber ?? "";
                    }
                }
                else
                {
                    userInfo = _Mapper.Map<HR_UserResult>(empDtos);
                    userInfo.CardNumber = cardInfo?.CardNumber ?? "";
                }
                return await Task.FromResult(userInfo);
            }
        }

        public async Task<HR_UserResult> GetuserByCCCD(string cccd)
        {
            HR_UserResult result = null;

            //var user = await DbContext.HR_User.AsNoTracking().FirstOrDefaultAsync(x => x.Nric == cccd);
            var user = from u in DbContext.HR_User
                       join c in DbContext.HR_CustomerInfo
                       on u.EmployeeATID equals c.EmployeeATID into cInfo
                       from cResult in cInfo.DefaultIfEmpty()
                       join w in DbContext.IC_WorkingInfo.Where(x => x.Status == (short)TransferStatus.Approve
                       && x.FromDate.Date <= DateTime.Now.Date 
                       && (x.ToDate == null || (x.ToDate.HasValue && x.ToDate.Value >= DateTime.Now.Date)))
                       on u.EmployeeATID equals w.EmployeeATID into wInfo
                       from wResult in wInfo.DefaultIfEmpty()
                       join d in DbContext.IC_Department
                       on wResult.DepartmentIndex equals d.Index into dInfo
                       from dResult in dInfo.DefaultIfEmpty()

                       select new { u, cResult, wResult, dResult };
            var listUserData = await user.Where(x => x.u.Nric == cccd).ToListAsync();
            if (listUserData.Count > 0)
            {
                if (listUserData.Any(x => (x.cResult != null && x.cResult.FromTime <= DateTime.Now
                && x.cResult.ToTime > DateTime.Now) || (x.cResult == null && x.wResult != null
                && x.wResult.Status == (short)TransferStatus.Approve
                       && x.wResult.FromDate.Date <= DateTime.Now.Date
                       && (x.wResult.ToDate == null || (x.wResult.ToDate.HasValue && x.wResult.ToDate.Value >= DateTime.Now.Date)))))
                {
                    var userData = listUserData.FirstOrDefault(x => (x.cResult != null && x.cResult.FromTime <= DateTime.Now
                        && x.cResult.ToTime > DateTime.Now) || (x.cResult == null && x.wResult != null
                        && x.wResult.Status == (short)TransferStatus.Approve
                        && x.wResult.FromDate.Date <= DateTime.Now.Date
                        && (x.wResult.ToDate == null || (x.wResult.ToDate.HasValue && x.wResult.ToDate.Value >= DateTime.Now.Date))));
                    result = new HR_UserResult().PopulateWith(userData.u);
                    result.DepartmentIndex = userData.wResult?.DepartmentIndex ?? null;
                    result.Department = userData.dResult?.Name ?? string.Empty;
                    result.IsExpired = false;
                }
                else
                {
                    var userData = listUserData.FirstOrDefault();
                    result = new HR_UserResult().PopulateWith(userData.u);
                    result.DepartmentIndex = userData.wResult?.DepartmentIndex ?? null;
                    result.Department = userData.dResult?.Name ?? string.Empty;
                    result.IsExpired = true;
                }

                if (result != null)
                {
                    var isInBlackList = await DbContext.GC_BlackList.AsNoTracking().AnyAsync(x
                        => x.Nric == cccd && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)));
                    result.IsInBlackList = isInBlackList;
                }
            }

            //var userData = await user.FirstOrDefaultAsync(x => x.u.Nric == cccd);
            //if (userData != null)
            //{
            //    var result = new HR_UserResult().PopulateWith(userData.u);
            //    result.DepartmentIndex = userData.wResult?.DepartmentIndex ?? null;
            //    result.Department = userData.dResult?.Name ?? string.Empty;
            //    if (!userData.u.EmployeeType.HasValue || userData.u.EmployeeType == (short)EmployeeType.Employee 
            //        || userData.u.EmployeeType == (short)EmployeeType.Contractor)
            //    {
            //        var isUserInTime = await DbContext.IC_WorkingInfo.AnyAsync(x => x.EmployeeATID == userData.u.EmployeeATID
            //            && x.FromDate.Date <= DateTime.Now.Date
            //            && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate.Value.Date > DateTime.Now.Date))
            //            );
            //        if (isUserInTime)
            //        {
            //            result.IsExpired = false;
            //        }
            //        else 
            //        {
            //            result.IsExpired = true;
            //        }
            //    }
            //    else
            //    {
            //        var isUserInTime = await DbContext.HR_CustomerInfo.AnyAsync(x => x.EmployeeATID == userData.u.EmployeeATID
            //                && x.FromTime.Date <= DateTime.Now.Date
            //                && x.ToTime.Date > DateTime.Now.Date
            //                );
            //        if (isUserInTime)
            //        {
            //            result.IsExpired = false;
            //        }
            //        else
            //        {
            //            result.IsExpired = true;
            //        }
            //    }

            //    var isInBlackList = await DbContext.GC_BlackList.AsNoTracking().AnyAsync(x
            //        => x.Nric == cccd && x.FromDate.Date <= DateTime.Now.Date && (!x.ToDate.HasValue
            //            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)));
            //    result.IsInBlackList = isInBlackList;

            //    return result;
            //}

            return result;
        }

        private long GetParent(List<HR_Department> foos, long? id)
        {
            long idReturn = 0;

            while (id != null && id != 0)
            {
                var department = foos.FirstOrDefault(x => x.Index == id);
                if (department == null)
                {
                    id = null;
                    idReturn = 0;
                }
                else
                {
                    id = department.ParentIndex;
                    idReturn = department.Index;
                }

            };
            return idReturn;
        }

        public async Task<List<EmployeeFullInfo>> GetEmployeeByUserTypeIds(List<int?> pUserTypes, int pCompanyIndex)
        {
            if (pUserTypes.Contains((int)EmployeeType.Employee))
            {
                pUserTypes.Add(null);
            }
            var output = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && pUserTypes.Contains(x.EmployeeType))
                         join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                         && w.Status == (short)TransferStatus.Approve && w.FromDate.Date <= DateTime.Now.Date && (!w.ToDate.HasValue || w.ToDate.Value.Date >= DateTime.Now.Date))
                         on e.EmployeeATID equals w.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()
                         where (e.EmployeeType != (int)EmployeeType.Employee && e.EmployeeType != null) ||
                         (((e.EmployeeType == (int)EmployeeType.Employee) || e.EmployeeType == null) &&
                                    eWorkResult.Index != 0)
                         select new EmployeeFullInfo()
                         {
                             EmployeeATID = e.EmployeeATID
                         };
            var aaaa = output.Count();
            return output.ToList();


        }

        public Tuple<bool, HashSet<string>> ShowStoppedWorkingEmployeesData()
        {
            var isShow = false;
            var employeeATIDList = new HashSet<string>();

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                if (dbconfig != null)
                {
                    var config = _mapper.Map<IC_ConfigDTO>(dbconfig);
                    if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType != null
                        && config.IntegrateLogParam.ShowStoppedWorkingEmployeesType != (short)ShowStoppedWorkingEmployeesType.NotUse)
                    {
                        isShow = true;
                    }
                    if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Day
                        && config.IntegrateLogParam.ShowStoppedWorkingEmployeesDay.HasValue)
                    {
                        var now = DateTime.Now;
                        var query = from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == 2)
                                    join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                    on e.EmployeeATID equals wi.EmployeeATID into empWi
                                    from empWiResult in empWi.DefaultIfEmpty()
                                    select new
                                    {
                                        Employee = e,
                                        WorkingInfo = empWiResult
                                    };
                        query = query.Where(x => x.WorkingInfo.FromDate.HasValue
                                    && x.WorkingInfo.FromDate.Value.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate.Value.Date <= now.Date
                                    && x.WorkingInfo.ToDate.Value.AddDays(config.IntegrateLogParam.ShowStoppedWorkingEmployeesDay.Value).Date >= now.Date);
                        if (query != null && query.Count() > 0)
                        {
                            employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                        }
                    }
                    else if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Week)
                    {
                        var now = DateTime.Now;
                        if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesWeek.HasValue)
                        {
                            var query = from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == 2)
                                        join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            Employee = e,
                                            WorkingInfo = empWiResult
                                        };
                            query = query.Where(x => x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.FromDate.Value.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate <= now);
                            var nextDayOfWeek = now.GetNextWeekday((DayOfWeek)config.IntegrateLogParam.ShowStoppedWorkingEmployeesWeek);
                            if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.HasValue)
                            {
                                nextDayOfWeek = nextDayOfWeek.ChangeTime(config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Hour,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Minute,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Second, 0);
                            }
                            query = query.Where(x => x.WorkingInfo.ToDate <= nextDayOfWeek && x.WorkingInfo.ToDate.Value.AddDays(6) >= nextDayOfWeek);
                            if (query != null && query.Count() > 0)
                            {
                                employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                            }
                        }
                    }
                    else if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Month)
                    {
                        var now = DateTime.Now;
                        if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesMonth.HasValue)
                        {
                            var query = from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == 2)
                                        join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            Employee = e,
                                            WorkingInfo = empWiResult
                                        };
                            query = query.Where(x => x.WorkingInfo.FromDate.HasValue
                                    && x.WorkingInfo.FromDate.Value.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate <= now);
                            var endShowDate = new DateTime(now.Year, now.Month, config.IntegrateLogParam.ShowStoppedWorkingEmployeesMonth.Value);
                            if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.HasValue)
                            {
                                endShowDate = endShowDate.ChangeTime(config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Hour,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Minute,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Second, 0);
                                query = query.Where(x => (x.WorkingInfo.ToDate.Value.Day < endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.AddMonths(-1).Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day == endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month
                                && x.WorkingInfo.ToDate.Value.TimeOfDay <= endShowDate.TimeOfDay)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month
                                && x.WorkingInfo.ToDate.Value.TimeOfDay <= endShowDate.TimeOfDay));
                            }
                            else
                            {
                                query = query.Where(x => (x.WorkingInfo.ToDate.Value.Day <= endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.AddMonths(-1).Month == endShowDate.Month));
                            }
                            if (query != null && query.Count() > 0)
                            {
                                employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                            }
                        }
                    }
                }
            }
            else
            {
                if (dbconfig != null)
                {
                    var config = _mapper.Map<IC_ConfigDTO>(dbconfig);
                    if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType != null
                        && config.IntegrateLogParam.ShowStoppedWorkingEmployeesType != (short)ShowStoppedWorkingEmployeesType.NotUse)
                    {
                        isShow = true;
                    }
                    if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Day
                        && config.IntegrateLogParam.ShowStoppedWorkingEmployeesDay.HasValue)
                    {
                        var now = DateTime.Now;
                        var query = from e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                    join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                    on e.EmployeeATID equals wi.EmployeeATID into empWi
                                    from empWiResult in empWi.DefaultIfEmpty()
                                    select new
                                    {
                                        WorkingInfo = empWiResult
                                    };
                        query = query.Where(x => x.WorkingInfo.Status == 1
                                    && x.WorkingInfo.FromDate.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate.Value.Date <= now.Date
                                    && x.WorkingInfo.ToDate.Value.AddDays(config.IntegrateLogParam.ShowStoppedWorkingEmployeesDay.Value).Date >= now.Date);
                        if (query != null && query.Count() > 0)
                        {
                            employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                        }
                    }
                    else if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Week)
                    {
                        var now = DateTime.Now;
                        if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesWeek.HasValue)
                        {
                            var query = from e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                        join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            WorkingInfo = empWiResult
                                        };
                            query = query.Where(x => x.WorkingInfo.Status == 1
                                    && x.WorkingInfo.FromDate.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate <= now);
                            var nextDayOfWeek = now.GetNextWeekday((DayOfWeek)config.IntegrateLogParam.ShowStoppedWorkingEmployeesWeek);
                            if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.HasValue)
                            {
                                nextDayOfWeek = nextDayOfWeek.ChangeTime(config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Hour,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Minute,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Second, 0);
                            }
                            query = query.Where(x => x.WorkingInfo.ToDate <= nextDayOfWeek && x.WorkingInfo.ToDate.Value.AddDays(6) >= nextDayOfWeek);
                            if (query != null && query.Count() > 0)
                            {
                                employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                            }
                        }
                    }
                    else if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesType == (short)ShowStoppedWorkingEmployeesType.Month)
                    {
                        var now = DateTime.Now;
                        if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesMonth.HasValue)
                        {
                            var query = from e in DbContext.HR_EmployeeInfo.Where(x => x.CompanyIndex == 2)
                                        join wi in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == 2)
                                        on e.EmployeeATID equals wi.EmployeeATID into empWi
                                        from empWiResult in empWi.DefaultIfEmpty()
                                        select new
                                        {
                                            WorkingInfo = empWiResult
                                        };
                            query = query.Where(x => x.WorkingInfo.Status == 1
                                    && x.WorkingInfo.FromDate.Date <= now.Date
                                    && x.WorkingInfo.ToDate.HasValue
                                    && x.WorkingInfo.ToDate <= now);
                            var endShowDate = new DateTime(now.Year, now.Month, config.IntegrateLogParam.ShowStoppedWorkingEmployeesMonth.Value);
                            if (config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.HasValue)
                            {
                                endShowDate = endShowDate.ChangeTime(config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Hour,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Minute,
                                    config.IntegrateLogParam.ShowStoppedWorkingEmployeesTime.Value.Second, 0);
                                query = query.Where(x => (x.WorkingInfo.ToDate.Value.Day < endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.AddMonths(-1).Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day == endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month
                                && x.WorkingInfo.ToDate.Value.TimeOfDay <= endShowDate.TimeOfDay)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month
                                && x.WorkingInfo.ToDate.Value.TimeOfDay <= endShowDate.TimeOfDay));
                            }
                            else
                            {
                                query = query.Where(x => (x.WorkingInfo.ToDate.Value.Day <= endShowDate.Day && x.WorkingInfo.ToDate.Value.Month == endShowDate.Month)
                                || (x.WorkingInfo.ToDate.Value.Day > endShowDate.Day && x.WorkingInfo.ToDate.Value.AddMonths(-1).Month == endShowDate.Month));
                            }
                            if (query != null && query.Count() > 0)
                            {
                                employeeATIDList = query.Select(x => x.WorkingInfo.EmployeeATID).ToHashSet();
                            }
                        }
                    }
                }
            }
            return new Tuple<bool, HashSet<string>>(isShow, employeeATIDList);
        }

        public async Task<List<UserBasicInfoReponse>> GetUserBasicInfo()
        {
            var result = new List<UserBasicInfoReponse>();
            var departmentLst = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToListAsync();
            var departmentParent = departmentLst.Where(x => x.ParentIndex == 0 || x.ParentIndex == null).ToList();
            var departmentParentLst = new List<HR_DeparmentParent>();
            foreach (var item in departmentParent)
            {
                var departmentChildren = _HR_EmployeeInfoService.GetChildrenToId(departmentLst, item.Index, item.Name);
                if (departmentChildren != null && departmentChildren.Count > 0)
                {
                    departmentParentLst.AddRange(departmentChildren);
                }
                departmentParentLst.Add(new HR_DeparmentParent() { Index = item.Index, DepartmentParentName = item.Name, ParentIndex = item.Index });
            }
            var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToList()
                         join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join d in departmentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join g in departmentParentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals g.Index into deParent
                         from deWorkResult in deParent.DefaultIfEmpty()

                         join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult?.PositionIndex ?? 0 equals p.Index into pWork
                         from pWorkResult in pWork.DefaultIfEmpty()

                         join info in ezHR_Context.HR_EmployeeContactInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals info.EmployeeATID into infoWork
                         from infoResult in infoWork.DefaultIfEmpty()

                         where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                         select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName, DepartmentParent = deWorkResult, Info = infoResult, Positon = pWorkResult }).AsQueryable();

            result = query.ToList().Select(x =>
            {
                var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                var deparmentV3 = teamIndex == 0 ? null : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString()));
                var deparmentV2 = deparmentV3 != null && deparmentV3.ParentIndex != null ? departmentLst.FirstOrDefault(z => z.Index == long.Parse(deparmentV3.ParentIndex.ToString()))?.Name : "";
                var rs = new UserBasicInfoReponse();
                rs.EmployeeATID = x.EmployeeATID;
                rs.EmployeeCode = x.Employee.EmployeeCode;
                rs.FullName = x.FullName;
                rs.PositionIndex = x.Positon?.NameInEng;
                rs.DepartmentNameLV2 = deparmentV2;
                rs.CardNumber = x.Employee?.CardNumber;
                rs.IsActive = x.Employee.Active;
                rs.Email = x.Info.Email;
                rs.ToDate = x.WorkingInfo.ToDate;
                rs.EmployeeType = MappingDepartmentNameWithCharacters(deparmentV2);
                return rs;
            }).ToList();
            return result;
        }

        public async Task<List<UserDetailInfoReponse>> GetUserDetailInfo()
        {
            var result = new List<UserDetailInfoReponse>();
            var departmentLst = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToListAsync();
            var departmentParent = departmentLst.Where(x => x.ParentIndex == 0 || x.ParentIndex == null).ToList();
            var departmentParentLst = new List<HR_DeparmentParent>();
            foreach (var item in departmentParent)
            {
                var departmentChildren = _HR_EmployeeInfoService.GetChildrenToId(departmentLst, item.Index, item.Name);
                if (departmentChildren != null && departmentChildren.Count > 0)
                {
                    departmentParentLst.AddRange(departmentChildren);
                }
                departmentParentLst.Add(new HR_DeparmentParent() { Index = item.Index, DepartmentParentName = item.Name, ParentIndex = item.Index });
            }
            var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToList()
                         join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join c in ezHR_Context.HR_Country.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e?.Nationality ?? 0 equals c.Index into country
                         from countryResult in country.DefaultIfEmpty()

                         join d in departmentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join g in departmentParentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals g.Index into deParent
                         from deWorkResult in deParent.DefaultIfEmpty()

                         join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult?.PositionIndex ?? 0 equals p.Index into pWork
                         from pWorkResult in pWork.DefaultIfEmpty()

                         join info in ezHR_Context.HR_EmployeeContactInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals info.EmployeeATID into infoWork
                         from infoResult in infoWork.DefaultIfEmpty()

                         where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                         select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName, DepartmentParent = deWorkResult, Info = infoResult, Positon = pWorkResult, Country = countryResult }).AsQueryable();
            var teacherInfomation = await GetHomeroomTeacherInfomationEMS(query.Select(x => x.EmployeeATID).ToList());
            result = query.ToList().Select(x =>
            {
                var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                var deparmentV3 = teamIndex == 0 ? null : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString()));
                var deparmentV2 = deparmentV3 != null && deparmentV3.ParentIndex != null ? departmentLst.FirstOrDefault(z => z.Index == long.Parse(deparmentV3.ParentIndex.ToString()))?.Name : "";
                string className = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.mainClassName;
                string formTeacher = string.IsNullOrEmpty(className) ? string.Empty : "Chủ nhiệm";
                var baseLevelForTeacher = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.baseLevelForTeacher;
                var baseLevelNameList = baseLevelForTeacher.Select(x => x.baseLevelName).ToList();
                var rs = new UserDetailInfoReponse();
                rs.EmployeeATID = x.EmployeeATID;
                rs.EmployeeCode = x.Employee.EmployeeCode;
                rs.FullName = x.FullName;
                rs.PositionIndex = x.Positon?.NameInEng;
                rs.DepartmentNameLV2 = deparmentV2;
                rs.CardNumber = x.Employee?.CardNumber;
                rs.IsActive = x.Employee.Active;
                rs.Email = x.Info.Email;
                rs.ToDate = x.WorkingInfo.ToDate;
                rs.CompanyIndex = x.Employee.CompanyIndex;
                rs.Gender = x.Employee?.Gender != null ? x.Employee?.Gender.ToString() : "";
                rs.NickName = x.Employee.NickName;
                rs.DepartmentIndex = (int)x.Department.Index;
                rs.DepartmentName = x.Department.Name;
                rs.PositionName = x.Positon?.Name;
                rs.FormTeacher = formTeacher;
                rs.Class = className;
                rs.GradeName = MappingBaseLevelNameWithCharacters(baseLevelNameList);
                rs.Nationality = MappingNationalityNameWithCharacters(x.Country?.Name);
                rs.EmployeeType = MappingDepartmentNameWithCharacters(deparmentV2);
                return rs;
            }).ToList();
            return result;
        }

        public async Task<List<UserContactInfoReponse>> GetUserContactInfo()
        {
            var result = new List<UserContactInfoReponse>();
            var departmentLst = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToListAsync();
            var departmentParent = departmentLst.Where(x => x.ParentIndex == 0 || x.ParentIndex == null).ToList();
            var departmentParentLst = new List<HR_DeparmentParent>();
            foreach (var item in departmentParent)
            {
                var departmentChildren = _HR_EmployeeInfoService.GetChildrenToId(departmentLst, item.Index, item.Name);
                if (departmentChildren != null && departmentChildren.Count > 0)
                {
                    departmentParentLst.AddRange(departmentChildren);
                }
                departmentParentLst.Add(new HR_DeparmentParent() { Index = item.Index, DepartmentParentName = item.Name, ParentIndex = item.Index });
            }
            var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToList()
                         join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join c in ezHR_Context.HR_Country.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e?.Nationality ?? 0 equals c.Index into country
                         from countryResult in country.DefaultIfEmpty()

                         join d in departmentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join g in departmentParentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals g.Index into deParent
                         from deWorkResult in deParent.DefaultIfEmpty()

                         join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult?.PositionIndex ?? 0 equals p.Index into pWork
                         from pWorkResult in pWork.DefaultIfEmpty()

                         join info in ezHR_Context.HR_EmployeeContactInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals info.EmployeeATID into infoWork
                         from infoResult in infoWork.DefaultIfEmpty()

                         where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                         select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName, DepartmentParent = deWorkResult, Info = infoResult, Positon = pWorkResult, Country = countryResult }).AsQueryable();
            var teacherInfomation = await GetHomeroomTeacherInfomationEMS(query.Select(x => x.EmployeeATID).ToList());
            result = query.ToList().Select(x =>
            {
                var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                var deparmentV3 = teamIndex == 0 ? null : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString()));
                var deparmentV2 = deparmentV3 != null && deparmentV3.ParentIndex != null ? departmentLst.FirstOrDefault(z => z.Index == long.Parse(deparmentV3.ParentIndex.ToString()))?.Name : "";

                string className = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.mainClassName;
                string formTeacher = string.IsNullOrEmpty(className) ? string.Empty : "Chủ nhiệm";
                var baseLevelForTeacher = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.baseLevelForTeacher;
                var baseLevelNameList = baseLevelForTeacher.Select(x => x.baseLevelName).ToList();
                var rs = new UserContactInfoReponse();
                rs.EmployeeATID = x.EmployeeATID;
                rs.EmployeeCode = x.Employee.EmployeeCode;
                rs.FullName = x.FullName;
                rs.PositionIndex = x.Positon?.NameInEng;
                rs.DepartmentNameLV2 = deparmentV2;
                rs.CardNumber = x.Employee?.CardNumber;
                rs.IsActive = x.Employee.Active;
                rs.Email = x.Info.Email;
                rs.ToDate = x.WorkingInfo.ToDate;
                rs.CompanyIndex = x.Employee.CompanyIndex;
                rs.Gender = x.Employee?.Gender != null ? x.Employee?.Gender.ToString() : "";
                rs.NickName = x.Employee.NickName;
                rs.DepartmentIndex = (int)x.Department.Index;
                rs.DepartmentName = x.Department.Name;
                rs.PositionName = x.Positon?.Name;
                rs.FormTeacher = formTeacher;
                rs.Class = className;
                rs.GradeName = MappingBaseLevelNameWithCharacters(baseLevelNameList);
                rs.Nationality = MappingNationalityNameWithCharacters(x.Country?.Name);
                rs.EmployeeType = MappingDepartmentNameWithCharacters(deparmentV2);
                rs.Avatar = x.Employee.Avatar;
                rs.FromDate = x.WorkingInfo.FromDate;
                return rs;
            }).ToList();
            return result;
        }

        public async Task<List<UserPersonalInfoReponse>> GetUserPersonalInfo()
        {
            var result = new List<UserPersonalInfoReponse>();
            var departmentLst = await ezHR_Context.HR_Department.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToListAsync();
            var departmentParent = departmentLst.Where(x => x.ParentIndex == 0 || x.ParentIndex == null).ToList();
            var departmentParentLst = new List<HR_DeparmentParent>();
            foreach (var item in departmentParent)
            {
                var departmentChildren = _HR_EmployeeInfoService.GetChildrenToId(departmentLst, item.Index, item.Name);
                if (departmentChildren != null && departmentChildren.Count > 0)
                {
                    departmentParentLst.AddRange(departmentChildren);
                }
                departmentParentLst.Add(new HR_DeparmentParent() { Index = item.Index, DepartmentParentName = item.Name, ParentIndex = item.Index });
            }
            var query = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == _Config.CompanyIndex).ToList()
                         join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals wi.EmployeeATID into eWork
                         from eWorkResult in eWork.DefaultIfEmpty()

                         join c in ezHR_Context.HR_Country.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e?.Nationality ?? 0 equals c.Index into country
                         from countryResult in country.DefaultIfEmpty()

                         join d in departmentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals d.Index into dWork
                         from dWorkResult in dWork.DefaultIfEmpty()

                         join g in departmentParentLst
                         on eWorkResult?.DepartmentIndex ?? 0 equals g.Index into deParent
                         from deWorkResult in deParent.DefaultIfEmpty()

                         join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on eWorkResult?.PositionIndex ?? 0 equals p.Index into pWork
                         from pWorkResult in pWork.DefaultIfEmpty()

                         join info in ezHR_Context.HR_EmployeeContactInfo.Where(x => x.CompanyIndex == _Config.CompanyIndex)
                         on e.EmployeeATID equals info.EmployeeATID into infoWork
                         from infoResult in infoWork.DefaultIfEmpty()

                         where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                         select new { EmployeeATID = e.EmployeeATID, Employee = e, WorkingInfo = eWorkResult, Department = dWorkResult, FullName = e.LastName + " " + e.MidName + " " + e.FirstName, DepartmentParent = deWorkResult, Info = infoResult, Positon = pWorkResult, Country = countryResult }).ToList();
            var employeeList = query.Select(z => z.Employee.EmployeeATID).ToHashSet();
            var passwordMachine = DbContext.IC_UserMaster.Where(x => employeeList.Contains(x.EmployeeATID)).Select(z => new { Password = z.Password, EmployeeATID = z.EmployeeATID }).ToList();
            var teacherInfomation = await GetHomeroomTeacherInfomationEMS(query.Select(x => x.EmployeeATID).ToList());
            result = query.ToList().Select(x =>
            {
                var teamIndex = x.Department?.ParentIndex != null ? (int)x.Department?.ParentIndex : 0;
                var deparmentV3 = teamIndex == 0 ? null : departmentLst.FirstOrDefault(z => z.Index == long.Parse(teamIndex.ToString()));
                var deparmentV2 = deparmentV3 != null && deparmentV3.ParentIndex != null ? departmentLst.FirstOrDefault(z => z.Index == long.Parse(deparmentV3.ParentIndex.ToString()))?.Name : "";
                string className = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.mainClassName;
                string formTeacher = string.IsNullOrEmpty(className) ? string.Empty : "Chủ nhiệm";
                var baseLevelForTeacher = teacherInfomation.FirstOrDefault(z => z.employeeATID == x.EmployeeATID)?.baseLevelForTeacher;
                var baseLevelNameList = baseLevelForTeacher.Select(x => x.baseLevelName).ToList();
                var info = new UserPersonalInfoReponse();
                info.EmployeeATID = x.EmployeeATID;
                info.EmployeeCode = x.Employee.EmployeeCode;
                info.FullName = x.FullName;
                info.PositionIndex = x.Positon?.NameInEng;
                info.DepartmentNameLV2 = deparmentV2;
                info.CardNumber = x.Employee?.CardNumber;
                info.IsActive = x.Employee.Active;
                info.Email = x.Info.Email;
                info.ToDate = x.WorkingInfo.ToDate;
                info.CompanyIndex = x.Employee.CompanyIndex;
                info.Gender = x.Employee?.Gender != null ? x.Employee?.Gender.ToString() : "";
                info.NickName = x.Employee.NickName;
                info.DepartmentIndex = (int)x.Department.Index;
                info.DepartmentName = x.Department.Name;
                info.PositionName = x.Positon?.Name;
                info.FormTeacher = formTeacher;
                info.Class = className;
                info.GradeName = MappingBaseLevelNameWithCharacters(baseLevelNameList);
                info.Nationality = MappingNationalityNameWithCharacters(x.Country?.Name);
                info.EmployeeType = MappingDepartmentNameWithCharacters(deparmentV2);
                info.Avatar = x.Employee.Avatar;
                info.FromDate = x.WorkingInfo.FromDate;
                info.DayOfBirth = x.Employee?.DayOfBirth ?? 0;
                info.MonthOfBirth = x.Employee?.MonthOfBirth ?? 0;
                info.Password = passwordMachine.FirstOrDefault(z => z.EmployeeATID == x.EmployeeATID)?.Password ?? "";
                return info;
            }).ToList();
            return result;
        }
        private List<RegularDepartmentReponse> RecursiveGetChildrentDepartment(List<IC_Department> lstDept, long pCurrentIndex, decimal pId, int pLevel)
        {
            var lstChild = lstDept.Where(x => x.ParentIndex == pCurrentIndex).ToList();
            var output = new List<RegularDepartmentReponse>();
            for (int i = 0; i < lstChild.Count(); i++)
            {
                var deptTree = new RegularDepartmentReponse()
                {
                    DepartmentIndex = lstChild[i].Index,
                    DepartmentName = lstChild[i].Name
                };
                deptTree.ListChildrent = RecursiveGetChildrentDepartment(lstDept, lstChild[i].Index, deptTree.DepartmentIndex, pLevel + 1);

                output.Add(deptTree);
            }
            return output;
        }

        private string MappingDepartmentNameWithCharacters(string characters)
        {
            string employeeType = string.Empty;
            if (string.IsNullOrEmpty(characters)) return string.Empty;
            switch (characters)
            {
                case DepartmentKeyVstar.OfficeDepartment:
                    employeeType = EmployeeTypeVstar.T.ToString();
                    break;
                case DepartmentKeyVstar.RegularDepartment:
                    employeeType = EmployeeTypeVstar.E.ToString();
                    break;
                case DepartmentKeyVstar.Parents:
                    employeeType = EmployeeTypeVstar.P.ToString();
                    break;
                case DepartmentKeyVstar.HSTH:
                case DepartmentKeyVstar.HSTHCS:
                case DepartmentKeyVstar.HSTHPT:
                    employeeType = EmployeeTypeVstar.S.ToString();
                    break;
                case DepartmentKeyVstar.Visitors:
                    employeeType = EmployeeTypeVstar.V.ToString();
                    break;
            }
            return employeeType;
        }
        private string MappingNationalityNameWithCharacters(string characters)
        {
            string nationality = string.Empty;
            if (string.IsNullOrEmpty(characters)) return null;
            var employeeNationality = characters.RemoveDiacritics();
            nationality = NationalityKeyVstar.NA.ToString();
            switch (employeeNationality)
            {
                case NationalityName.VN:
                    nationality = NationalityKeyVstar.VN.ToString();
                    break;
                case NationalityName.PH:
                    nationality = NationalityKeyVstar.PH.ToString();
                    break;
            }
            return nationality;
        }

        private int MappingBaseLevelNameWithCharacters(List<string> characterList)
        {
            string grande = string.Empty;
            foreach (var item in characterList)
            {
                switch (item)
                {
                    case GradeName.TH:
                        grande += "1";
                        break;
                    case GradeName.THCS:
                        grande += "2";
                        break;
                    case GradeName.THPT:
                        grande += "3";
                        break;
                }
            }
            return int.Parse(grande);
        }

        public async Task<List<TeacherInfomationReponseEMS>> GetHomeroomTeacherInfomationEMS(List<string> employeeATID)
        {
            var emsClient = GetEMSClient();
            try
            {
                var employees = new EmployeesRequest()
                {
                    employees = employeeATID
                };
                var jsonData = JsonConvert.SerializeObject(employees);
                HttpContent inputContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await emsClient.PostAsync("/api/Teacher/GetHomeroomTeacherInfomation", inputContent);
                var result = await response.Content.ReadAsAsync<List<TeacherInfomationReponseEMS>>();
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetHomeroomTeacherInfomationEMS: " + ex.Message);
                return new List<TeacherInfomationReponseEMS>();
            }
        }

        public HttpClient GetEMSClient()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(mLinkEMSApi);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var token = GetEMSTokenAuthentication(client);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEMSClient: " + ex.Message);
                return null;
            }
        }
        public string GetEMSTokenAuthentication(HttpClient client)
        {
            try
            {
                var userLogin = new LoginInfoEMS()
                {
                    username = userNameEMS,
                    password = passwordEMS
                };
                var json = JsonConvert.SerializeObject(userLogin);
                var inputContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PostAsync("/api/accounts/authenticate", inputContent);
                response.Wait();
                var result = response.Result.Content.ReadAsAsync<UserInfoEMS>();
                result.Wait();
                var user = result.Result;
                return user.token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEMSTokenAuthentication: " + ex.Message);
                return "";
            }
        }


        public async Task<List<EmployeeFullInfo>> GetEmployeeInfoAndDepartmentParentByListEmpATID(List<string> listEmpATID, int pCompanyIndex)
        {
            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = 2 });
            addedParams.Add(new AddedParam { Key = "EventType", Value = ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString() });

            var dbconfig = DbContext.IC_Config.FirstOrDefault(x => x.CompanyIndex == 2 && x.EventType == ConfigAuto.MANAGE_STOPPED_WORKING_EMPLOYEES_DATA.ToString());
            if (_Config.IntegrateDBOther)
            {
                var queryData = (from e in ezHR_Context.HR_Employee.Where(x => x.CompanyIndex == pCompanyIndex)
                                 join wi in ezHR_Context.HR_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on e.EmployeeATID equals wi.EmployeeATID into eWork
                                 from eWorkResult in eWork.DefaultIfEmpty()

                                 join d in ezHR_Context.HR_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.DepartmentIndex equals d.Index into dWork
                                 from dWorkResult in dWork.DefaultIfEmpty()

                                 join p in ezHR_Context.HR_Position.Where(x => x.CompanyIndex == pCompanyIndex)
                                 on eWorkResult.PositionIndex equals p.Index into pWork
                                 from pWorkResult in pWork.DefaultIfEmpty()

                                 where (e.MarkForDelete == null || e.MarkForDelete == false)   // loc nhan vien chua nghi viec

                                 select new EmployeeFullInfo()
                                 {
                                     Avatar = e.Image,
                                     EmployeeATID = e.EmployeeATID,
                                     EmployeeCode = e.EmployeeCode,
                                     //CardNumber = x.HR_CardNumbers.FirstOrDefault().CardNumber,
                                     FullName = e.LastName + " " + e.MidName + " " + e.FirstName,
                                     Email = "",
                                     Phone = "",
                                     //Gender = x.Gender ?? false,
                                     JoinedDate = null,
                                     DepartmentIndex = dWorkResult.Index,
                                     Department = dWorkResult.Name,
                                     DepartmentCode = dWorkResult.Code,
                                     Position = pWorkResult.Name,
                                     PositionIndex = pWorkResult.Index,
                                     Title = "",
                                     TitleIndex = 0,
                                     EmployeeKindIndex = 0,
                                     EmployeeKind = "",
                                     ManagedDepartment = 0,
                                     ManagedOtherDepartment = "",
                                     DirectManager = "",
                                     FromDate = eWorkResult.FromDate,
                                     ToDate = eWorkResult.ToDate,
                                     DayOfBirth = 0,
                                     MonthOfBirth = 0,
                                     YearOfBirth = 0,
                                     TaxNumber = "",
                                     SocialInsNo = "",
                                     CompanyIndex = e.CompanyIndex,
                                     EmployeeType = EmployeeType.Employee
                                 });

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (listEmpATID != null && listEmpATID.Count > 0)
                {
                    queryData = queryData.Where(x => listEmpATID.Contains(x.EmployeeATID));
                }

                return await queryData.ToListAsync();
            }
            else
            {
                var queryData = from e in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex)
                                join w in DbContext.IC_WorkingInfo.Where(w => w.CompanyIndex == pCompanyIndex
                                    && w.Status == (short)TransferStatus.Approve
                                    && w.FromDate.Date <= DateTime.Now.Date)
                                on e.EmployeeATID equals w.EmployeeATID into workinginfo
                                from wkinf in workinginfo.DefaultIfEmpty()
                                join h in DbContext.HR_EmployeeInfo
                                on e.EmployeeATID equals h.EmployeeATID into employeeinfo
                                from emif in employeeinfo.DefaultIfEmpty()

                                join p in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.PositionIndex equals p.Index into positionGroup
                                from pst in positionGroup.DefaultIfEmpty()

                                join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                                on wkinf.DepartmentIndex equals d.Index into deptGroup
                                from dept in deptGroup.DefaultIfEmpty()

                                join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                                on e.EmployeeATID equals c.EmployeeATID into cWork
                                from cResult in cWork.DefaultIfEmpty()
                                select new EmployeeFullInfo()
                                {
                                    Avatar = e.Avatar,
                                    EmployeeATID = e.EmployeeATID,
                                    EmployeeCode = e.EmployeeCode,
                                    CardNumber = cResult == null ? "" : cResult.CardNumber,
                                    FullName = e.FullName,
                                    Email = emif.Email,
                                    Phone = "",
                                    //Gender = x.Gender ?? false,
                                    //IsParentDepartment = departmentList.FirstOrDefault(x => x.ParentIndex == wkinf.DepartmentIndex) != null ? true : false,
                                    JoinedDate = null,
                                    DepartmentIndex = wkinf.DepartmentIndex,
                                    Department = dept.Name,
                                    DepartmentCode = dept.Code,
                                    Position = pst.Name,
                                    PositionIndex = pst.Index,
                                    Title = "",
                                    TitleIndex = 0,
                                    EmployeeKindIndex = 0,
                                    EmployeeKind = "",
                                    ManagedDepartment = 0,
                                    ManagedOtherDepartment = "",
                                    DirectManager = "",
                                    FromDate = wkinf.FromDate,
                                    ToDate = wkinf.ToDate,
                                    DayOfBirth = 0,
                                    MonthOfBirth = 0,
                                    YearOfBirth = 0,
                                    TaxNumber = "",
                                    SocialInsNo = "",
                                    CompanyIndex = e.CompanyIndex,
                                    EmployeeType = (EmployeeType?)e.EmployeeType,
                                    UserName = e.UserName,
                                };
                var listAllDepartmentIndex = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).Select(x => (long)x.Index).ToHashSet();
                listAllDepartmentIndex.Add(0);
                queryData = queryData.Where(x => !x.DepartmentIndex.HasValue
                    || (x.DepartmentIndex.HasValue && listAllDepartmentIndex.Contains(x.DepartmentIndex.Value)));

                var resultShowStoppedEmp = ShowStoppedWorkingEmployeesData();
                if (resultShowStoppedEmp.Item1)
                {
                    if (resultShowStoppedEmp.Item2 != null && resultShowStoppedEmp.Item2.Count > 0)
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date)
                        || (x.ToDate.HasValue && x.ToDate.Value.Date < DateTime.Now.Date
                        && resultShowStoppedEmp.Item2.Contains(x.EmployeeATID)));
                    }
                    else
                    {
                        queryData = queryData.Where(x => !x.ToDate.HasValue
                        || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                    }
                }
                else
                {
                    queryData = queryData.Where(x => !x.ToDate.HasValue
                            || (x.ToDate.HasValue && x.ToDate.Value.Date >= DateTime.Now.Date));
                }

                if (listEmpATID != null && listEmpATID.Count > 0)
                {
                    queryData = queryData.Where(x => listEmpATID.Contains(x.EmployeeATID));
                }

                var result = await queryData.ToListAsync();
                var departmentList = DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
                result.ForEach(item => item.IsParentDepartment = departmentList.Any(x => x.ParentIndex == item.DepartmentIndex || (x.ParentIndex == null && item.DepartmentIndex == x.Index)) ? true : false);

                return result.GroupBy(p => p.EmployeeATID)
                           .Select(grp => grp.First()).ToList();
                //return await output.ToListAsync();
            }
        }
    }
}
