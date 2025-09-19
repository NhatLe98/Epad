using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Google.Apis.Sheets.v4.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using EPAD_Common.Utility;
using EPAD_Logic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EPAD_Services.Impl
{
    public class HR_CustomerInfoService : BaseServices<HR_CustomerInfo, EPAD_Context>, IHR_CustomerInfoService
    {
        private ConfigObject _config;
        private readonly string _configClientName;
        private readonly string _configUrlFileImport;
        private readonly string _applicationName;
        private readonly string _spreadsheetId;
        private readonly string _sheetName;

        IConfiguration _configuration;
        IHR_UserService _userService;
        IIC_DepartmentService _iC_DepartmentService;
        private IMemoryCache _Cache;
        private static List<string> _usingRandomCustomerID = new List<string>();
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly EPAD_Context _DbContext;
        private readonly IIC_AuditLogic _IIC_AuditLogic;
        private readonly IHR_EmployeeLogic _IHR_EmployeeLogic;
        private readonly IIC_CommandService _iC_CommandService;
        private readonly ILogger _logger;
        private readonly IIC_VehicleLogService _IC_VehicleLogService;
        public HR_CustomerInfoService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<HR_CustomerInfoService> logger) : base(serviceProvider)
        {
            _configuration = configuration;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _userService = serviceProvider.GetService<IHR_UserService>();
            _iC_DepartmentService = serviceProvider.GetService<IIC_DepartmentService>();
            _config = ConfigObject.GetConfig(_Cache);
            _configClientName = configuration.GetValue<string>("ClientName").ToUpper();
            _hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
            _DbContext = serviceProvider.GetService<EPAD_Context>();
            _IIC_AuditLogic = serviceProvider.GetService<IIC_AuditLogic>();
            _IHR_EmployeeLogic = serviceProvider.GetService<IHR_EmployeeLogic>();
            _iC_CommandService = serviceProvider.GetService<IIC_CommandService>();
            _configUrlFileImport = configuration.GetValue<string>("ULR_FileGuestError");
            _applicationName = configuration.GetValue<string>("ApplicationName");
            _spreadsheetId = configuration.GetValue<string>("SpreadsheetId");
            _sheetName = configuration.GetValue<string>("SheetName");
            _logger = logger;
            _IC_VehicleLogService = serviceProvider.GetService<IIC_VehicleLogService>();
        }

        public async Task<List<HR_CustomerInfoResult>> GetAllCustomerInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)))
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on e.ContactDepartment equals d.Index.ToString() into cd
                        from cdr in cd.DefaultIfEmpty()
                        join cv in DbContext.GC_CustomerVehicle.Where(x => x.CompanyIndex == pCompanyIndex)
                        on e.EmployeeATID equals cv.EmployeeATID into cvInfo
                        from cvr in cvInfo.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci, Department = cdr, CustomerVehicle = cvr };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                rs.ContactDepartmentName = x.Department != null ? x.Department.Name : "NoDepartment";
                rs.BikePlate = x.CustomerVehicle != null ? x.CustomerVehicle.Plate : rs.BikePlate;
                return rs;
            }).ToList();

            var listContactPersonID = result.Select(x => x.ContactPerson).Distinct().ToList();
            if (listContactPersonID.Count > 0)
            {
                var listContactPerson = await DbContext.HR_User.AsNoTracking().Where(x
                    => listContactPersonID.Contains(x.EmployeeATID)).ToListAsync();
                if (listContactPerson.Count > 0)
                {
                    result.ForEach(x =>
                    {
                        x.ContactPersonName = listContactPerson.FirstOrDefault(y => y.EmployeeATID == x.ContactPerson)?.FullName ?? string.Empty;
                    });
                }
            }

            return await Task.FromResult(result);
        }

        public async Task<List<HR_CustomerInfoResult>> GetNewestActiveCustomerInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)))
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.FromTime.Date <= DateTime.Now.Date
                        && x.ToTime.Date >= DateTime.Now.Date)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on e.ContactDepartment equals d.Index.ToString() into cd
                        from cdr in cd.DefaultIfEmpty()
                            //join cv in DbContext.GC_CustomerVehicle.Where(x => x.CompanyIndex == pCompanyIndex)
                            //on e.EmployeeATID equals cv.EmployeeATID into cvInfo
                            //from cvr in cvInfo.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci, Department = cdr };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardUserIndex = x.CardInfo != null ? x.CardInfo.Index : 0;
                rs.CardNumberUpdatedDate = (x.CardInfo != null && x.CardInfo.UpdatedDate.HasValue)
                    ? x.CardInfo.UpdatedDate.Value : DateTime.MinValue;
                rs.CardNumber = x.CardInfo?.CardNumber;
                rs.ContactDepartmentName = x.Department != null ? x.Department.Name : "NoDepartment";
                return rs;
            }).ToList();

            result = result.OrderByDescending(x => x.CardNumberUpdatedDate).GroupBy(x => x.EmployeeATID)
                .Select(x => x.FirstOrDefault()).Where(x => x != null).ToList();

            var listCustomerID = result.Select(x => x.EmployeeATID).ToList();
            var listCustomerVehicle = new List<GC_CustomerVehicle>();
            if (listCustomerID.Count > 0)
            {
                listCustomerVehicle = await DbContext.GC_CustomerVehicle.AsNoTracking().Where(x
                    => listCustomerID.Contains(x.EmployeeATID)).ToListAsync();
                if (listCustomerVehicle.Count > 0)
                {
                    result.ForEach(x =>
                    {
                        var listBikePlate = listCustomerVehicle.Where(y => y.EmployeeATID == x.EmployeeATID).Select(x => x.Plate).ToList();
                        x.BikePlate = (listBikePlate != null && listBikePlate.Count > 0) ? string.Join(',', listBikePlate) : string.Empty;
                    });
                }
            }

            var listContactPersonID = result.Select(x => x.ContactPerson).Distinct().ToList();
            if (listContactPersonID.Count > 0)
            {
                var listContactPerson = await DbContext.HR_User.AsNoTracking().Where(x
                    => listContactPersonID.Contains(x.EmployeeATID)).ToListAsync();

                result.ForEach(x =>
                {
                    if (!string.IsNullOrWhiteSpace(x.ContactPerson))
                    {
                        var contactPersonInfo = listContactPerson?.FirstOrDefault(z => z.EmployeeATID == x.ContactPerson);
                        x.ContactPersonName = contactPersonInfo != null ? contactPersonInfo?.FullName : x.ContactPerson;
                    }
                    else
                    {
                        x.ContactPersonName = "";
                    }
                    //x.ContactPersonName = listContactPerson.FirstOrDefault(y => y.EmployeeATID == x.ContactPerson)?.FullName ?? string.Empty;
                });

            }

            return await Task.FromResult(result);
        }

        public async Task<List<HR_CustomerInfoResult>> GetNewestCustomerInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)))
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on e.ContactDepartment equals d.Index.ToString() into cd
                        from cdr in cd.DefaultIfEmpty()
                            //join cv in DbContext.GC_CustomerVehicle.Where(x => x.CompanyIndex == pCompanyIndex)
                            //on e.EmployeeATID equals cv.EmployeeATID into cvInfo
                            //from cvr in cvInfo.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci, Department = cdr };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumberUpdatedDate = (x.CardInfo != null && x.CardInfo.UpdatedDate.HasValue)
                    ? x.CardInfo.UpdatedDate.Value : DateTime.MinValue;
                rs.CardNumber = x.CardInfo?.CardNumber;
                rs.ContactDepartmentName = x.Department != null ? x.Department.Name : "NoDepartment";
                return rs;
            }).ToList();

            result = result.OrderByDescending(x => x.CardNumberUpdatedDate).GroupBy(x => x.EmployeeATID)
                .Select(x => x.FirstOrDefault()).Where(x => x != null).ToList();

            var listCustomerID = result.Select(x => x.EmployeeATID).ToList();
            var listCustomerVehicle = new List<GC_CustomerVehicle>();
            if (listCustomerID.Count > 0)
            {
                listCustomerVehicle = await DbContext.GC_CustomerVehicle.AsNoTracking().Where(x
                    => listCustomerID.Contains(x.EmployeeATID)).ToListAsync();
                if (listCustomerVehicle.Count > 0)
                {
                    result.ForEach(x =>
                    {
                        var listBikePlate = listCustomerVehicle.Where(y => y.EmployeeATID == x.EmployeeATID).Select(x => x.Plate).ToList();
                        x.BikePlate = (listBikePlate != null && listBikePlate.Count > 0) ? string.Join(',', listBikePlate) : string.Empty;
                    });
                }
            }

            var listContactPersonID = result.Select(x => x.ContactPerson).Distinct().ToList();
            if (listContactPersonID.Count > 0)
            {
                var listContactPerson = await DbContext.HR_User.AsNoTracking().Where(x
                    => listContactPersonID.Contains(x.EmployeeATID)).ToListAsync();
                if (listContactPerson.Count > 0)
                {
                    result.ForEach(x =>
                    {
                        x.ContactPersonName = listContactPerson.FirstOrDefault(y => y.EmployeeATID == x.ContactPerson)?.FullName ?? string.Empty;
                    });
                }
            }

            return await Task.FromResult(result);
        }

        public async Task<List<HR_CustomerInfoResult>> GetCustomerInfoExcludeExpired(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.EmployeeType == (short)EmployeeType.Guest
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)))
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex
                        && x.FromTime.Date <= DateTime.Now.Date && x.ToTime.Date >= DateTime.Now.Date)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()
                        join d in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex)
                        on e.ContactDepartment equals d.Index.ToString() into cd
                        from cdr in cd.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci, Department = cdr };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                rs.ContactDepartmentName = x.Department != null ? x.Department.Name : "NoDepartment";
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<HR_CustomerInfoResult>> GetAllCustomerAndContractorInfo(string[] pEmployeeATIDs, int pCompanyIndex)
        {
            var empLookup = pEmployeeATIDs.ToHashSet();
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex
                        && (x.EmployeeType == (short)EmployeeType.Guest || x.EmployeeType == (short)EmployeeType.Contractor)
                        && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)))
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && (empLookup.Count <= 0 || empLookup.Contains(x.EmployeeATID)) && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetPage(List<AddedParam> addedParams, int pCompanyIndex)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var employeeAndDepartmentLookup = await _userService.GetEmployeeAndDepartmentLookup(pCompanyIndex);
            var departmentInfoList = await _userService.GetDepartmentList(pCompanyIndex);

            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var employeeType = addedParams.FirstOrDefault(e => e.Key == "EmployeeType");
            var pPage = Convert.ToInt32(pageIndex?.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize?.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);
            var employeeTypee = Convert.ToInt32(employeeType?.Value ?? EmployeeType.Guest);

            var query = (from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == employeeTypee)
                         join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                         on u.EmployeeATID equals e.EmployeeATID into eCheck
                         from eResult in eCheck.DefaultIfEmpty()

                         join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                         on eResult.EmployeeATID equals c.EmployeeATID into cWork
                         from cResult in cWork.DefaultIfEmpty()

                         join us in DbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex)
                         on u.EmployeeATID equals us.EmployeeATID into usWork
                         from usWorkResult in usWork.DefaultIfEmpty()

                         join wk in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor)
                             on u.EmployeeATID equals wk.EmployeeATID into wCheck
                         from wResult in wCheck.DefaultIfEmpty()

                         join de in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor && x.IsInactive != true)
                             on wResult.DepartmentIndex equals de.Index into deCheck
                         from deResult in deCheck.DefaultIfEmpty()

                         join po in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor)
                             on wResult.PositionIndex equals po.Index into poCheck
                         from poResult in poCheck.DefaultIfEmpty()

                         where employeeTypee != (int)EmployeeType.Contractor || (wResult == null || (wResult.ToDate == null))
                         select new
                         {
                             User = u,
                             Employee = eResult,
                             CardInfo = cResult,
                             UserMaster = usWorkResult,
                             Customer = eResult,
                             Working = wResult,
                             Department = deResult,
                             Position = poResult,
                         });


            foreach (AddedParam param in addedParams)
            {
                switch (param.Key)
                {
                    case "Filter":
                        if (param.Value != null)
                        {
                            string filter = param.Value.ToString();
                            var filterBy = filter.Split(" ").ToList();
                            query = query.Where(u => u.User.EmployeeATID.Contains(filter)
                                                     || u.User.FullName.Contains(filter)
                                                     || (u.Customer != null && u.Customer.ContactPerson.Contains(filter))
                                                     || (u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter))
                                                     || (!string.IsNullOrEmpty(u.User.EmployeeCode) && u.User.EmployeeCode.Contains(filter))
                                                     || ((filterBy.Count > 0 && (filterBy.Contains(u.User.EmployeeATID) || filterBy.Contains(u.User.FullName))) || filterBy.Count == 0));
                        }
                        break;
                    case "ListEmployeeATID":
                        if (param.Value != null)
                        {
                            var listEmployeeId = (IList<string>)param.Value;
                            query = query.Where(u => listEmployeeId.Contains(u.User.EmployeeATID));
                        }
                        break;
                    case "ListContactATID":
                        if (param.Value != null)
                        {
                            var listEmployeeId = (IList<string>)param.Value;
                            if (listEmployeeId.Count > 0)
                            {
                                query = query.Where(u => u.Customer != null && listEmployeeId.Contains(u.Customer.ContactPerson));
                            }
                        }
                        break;
                    case "EmployeeATID":
                        if (param.Value != null)
                        {
                            var employeeId = param.Value.ToString();
                            query = query.Where(u => u.User.EmployeeATID == employeeId);
                        }
                        break;

                }
            }

            if (pPage < 1) pPage = 1;
            var dummy = (await query.OrderBy(x => x.User.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).ToListAsync()).Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                if (x.Employee != null)
                {
                    rs = _Mapper.Map(x.Employee, rs);
                    if (x.Employee.ToTime == DateTime.MinValue)
                    {
                        rs.ToTime = null;
                    }
                }

                //if (x.UserMaster != null)
                //    rs = _Mapper.Map(x.UserMaster, rs);
                if (x.CardInfo != null)
                    rs = _Mapper.Map(x.CardInfo, rs);
                if (x.Working != null)
                    rs = _Mapper.Map(x.Working, rs);
                if (x.Position != null)
                    rs.PositionName = x.Position.Name;

                if (x.Department != null)
                {
                    rs = _Mapper.Map(x.Department, rs);
                }
                else
                {
                    rs.DepartmentIndex = 0;
                }

                //rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                return rs;
            }).ToList();

            dummy.ForEach(x =>
            {
                if (x.StudentOfParent != null && x.StudentOfParent.Count > 0)
                {
                    var listStudentOfParentName = employeeAndDepartmentLookup.Where(y => x.StudentOfParent.Contains(y.EmployeeATID)).Select(e
                        => e.FullName).ToList();
                    x.StudentOfParentString = String.Join(", ", listStudentOfParentName);
                }
                if (!string.IsNullOrWhiteSpace(x.ContactPerson))
                {
                    var contactPersonInfo = employeeAndDepartmentLookup.FirstOrDefault(z => z.EmployeeATID == x.ContactPerson);
                    x.ContactPersonGuest = contactPersonInfo != null ? contactPersonInfo?.FullName : x.ContactPerson;
                }
                else
                {
                    x.ContactPersonGuest = "";
                }
                x.ContactDepartmentGuest = !string.IsNullOrWhiteSpace(x.ContactDepartment) ? departmentInfoList.FirstOrDefault(z => z.Index == int.Parse(x.ContactDepartment))?.Name : "";
                //x.ContactPersonGuest = !string.IsNullOrWhiteSpace(x.ContactPerson) ? employeeAndDepartmentLookup.FirstOrDefault(z => z.EmployeeATID == x.ContactPerson)?.FullName : "";
            });

            var rs = new DataGridClass(query.Count(), dummy);
            return rs;
        }

        public async Task<DataGridClass> GetPageAdvance(List<AddedParam> addedParams, int pCompanyIndex, List<string> studentOfParent)
        {
            if (addedParams == null || addedParams.Count == 0)
                return null;

            var employeeAndDepartmentLookup = await _userService.GetEmployeeAndDepartmentLookup(pCompanyIndex);
            var departmentInfoList = await _userService.GetDepartmentList(pCompanyIndex);

            var pageIndex = addedParams.FirstOrDefault(e => e.Key == "PageIndex");
            var pageSize = addedParams.FirstOrDefault(e => e.Key == "PageSize");
            var employeeType = addedParams.FirstOrDefault(e => e.Key == "EmployeeType");
            var pPage = Convert.ToInt32(pageIndex?.Value ?? 1);
            var pLimit = Convert.ToInt32(pageSize?.Value ?? GlobalParams.ROWS_NUMBER_IN_PAGE);
            var employeeTypee = Convert.ToInt32(employeeType?.Value ?? EmployeeType.Guest);

            var query = (from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == employeeTypee)
                         join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                         on u.EmployeeATID equals e.EmployeeATID into eCheck
                         from eResult in eCheck.DefaultIfEmpty()

                         join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                         on eResult.EmployeeATID equals c.EmployeeATID into cWork
                         from cResult in cWork.DefaultIfEmpty()

                         join us in DbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex)
                         on u.EmployeeATID equals us.EmployeeATID into usWork
                         from usWorkResult in usWork.DefaultIfEmpty()

                         join wk in DbContext.IC_WorkingInfo.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor)
                             on u.EmployeeATID equals wk.EmployeeATID into wCheck
                         from wResult in wCheck.DefaultIfEmpty()

                         join de in DbContext.IC_Department.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor && x.IsInactive != true)
                             on wResult.DepartmentIndex equals de.Index into deCheck
                         from deResult in deCheck.DefaultIfEmpty()

                         join po in DbContext.HR_PositionInfo.Where(x => x.CompanyIndex == pCompanyIndex && employeeTypee == (int)EmployeeType.Contractor)
                             on wResult.PositionIndex equals po.Index into poCheck
                         from poResult in poCheck.DefaultIfEmpty()

                         select new
                         {
                             User = u,
                             Employee = eResult,
                             CardInfo = cResult,
                             UserMaster = usWorkResult,
                             Customer = eResult,
                             Working = wResult,
                             Department = deResult,
                             Position = poResult,
                         });


            foreach (AddedParam param in addedParams)
            {
                switch (param.Key)
                {
                    case "Filter":
                        if (param.Value != null)
                        {
                            string filter = param.Value.ToString();
                            var filterBy = filter.Split(" ").ToList();
                            query = query.Where(u => u.User.EmployeeATID.Contains(filter)
                                                     || u.User.FullName.Contains(filter)
                                                     || (u.Customer != null && u.Customer.ContactPerson.Contains(filter))
                                                     || (u.CardInfo != null && u.CardInfo.CardNumber.Contains(filter))
                                                     || (!string.IsNullOrEmpty(u.User.EmployeeCode) && u.User.EmployeeCode.Contains(filter))
                                                     || ((filterBy.Count > 0 && (filterBy.Contains(u.User.EmployeeATID)
                                                     || filterBy.Contains(u.User.FullName))) || filterBy.Count == 0));
                        }
                        break;
                    case "ListEmployeeATID":
                        if (param.Value != null)
                        {
                            var listEmployeeId = (IList<string>)param.Value;
                            query = query.Where(u => listEmployeeId.Contains(u.User.EmployeeATID));
                        }
                        break;
                    case "ListContactATID":
                        if (param.Value != null)
                        {
                            var listEmployeeId = (IList<string>)param.Value;
                            if (listEmployeeId.Count > 0)
                            {
                                query = query.Where(u => u.Customer != null && listEmployeeId.Contains(u.Customer.ContactPerson));
                            }
                        }
                        break;
                    case "EmployeeATID":
                        if (param.Value != null)
                        {
                            var employeeId = param.Value.ToString();
                            query = query.Where(u => u.User.EmployeeATID == employeeId);
                        }
                        break;
                    case "FilterDepartment":
                        if (param.Value != null)
                        {
                            var listDepartmentId = (IList<long>)param.Value;
                            if (listDepartmentId != null && listDepartmentId.Count > 0)
                            {
                                query = query.Where(u => u.Working != null && listDepartmentId.Contains(u.Working.DepartmentIndex));
                            }
                        }
                        break;

                }
            }

            if (pPage < 1) pPage = 1;
            var dummy = query.OrderBy(x => x.User.EmployeeATID).AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                if (x.Employee != null)
                {
                    rs = _Mapper.Map(x.Employee, rs);
                    if (x.Employee.ToTime == DateTime.MinValue)
                    {
                        rs.ToTime = null;
                    }
                }


                if (x.UserMaster != null)
                    rs = _Mapper.Map(x.UserMaster, rs);
                if (x.CardInfo != null)
                    rs = _Mapper.Map(x.CardInfo, rs);
                if (x.Working != null)
                {
                    rs = _Mapper.Map(x.Working, rs);
                    rs.FromTime = x.Working.FromDate;
                    rs.ToTime = x.Working.ToDate;
                }
                if (x.Position != null)
                    rs.PositionName = x.Position.Name;

                if (x.Department != null)
                {
                    rs = _Mapper.Map(x.Department, rs);
                }
                else
                {
                    rs.DepartmentIndex = 0;
                }

                //rs.CardNumber = x.CardInfo?.CardNumber ?? "";
                return rs;
            }).ToList();

            dummy = dummy.OrderByDescending(x => x.FromTime).OrderByDescending(x => !x.ToTime.HasValue)
                .OrderByDescending(x => x.ToTime.HasValue && x.ToTime.Value > DateTime.Now).GroupBy(x => x.EmployeeATID).Select(x => x.FirstOrDefault()).ToList();

            var count = dummy.Count();

            dummy = dummy.Skip((pPage - 1) * pLimit).Take(pLimit).ToList();

            if (studentOfParent != null && studentOfParent.Count > 0)
            {
                dummy = dummy.Where(x => studentOfParent.Any(y => x.StudentOfParent.Contains(y))).ToList();
            }

            dummy.ForEach(x =>
            {
                if (x.StudentOfParent != null && x.StudentOfParent.Count > 0)
                {
                    var listStudentOfParentName = employeeAndDepartmentLookup.Where(y => x.StudentOfParent.Contains(y.EmployeeATID)).Select(e
                        => e.FullName).ToList();
                    x.StudentOfParentString = String.Join(", ", listStudentOfParentName);
                }
                x.ContactDepartmentGuest = !string.IsNullOrWhiteSpace(x.ContactDepartment) ? departmentInfoList.FirstOrDefault(z => z.Index == int.Parse(x.ContactDepartment))?.Name : "";
                x.ContactPersonGuest = !string.IsNullOrWhiteSpace(x.ContactPerson) ? employeeAndDepartmentLookup.FirstOrDefault(z => z.EmployeeATID == x.ContactPerson)?.FullName : "";
            });

            //var rs = new DataGridClass(query.Count(), dummy);
            var rs = new DataGridClass(count, dummy);
            return await Task.FromResult(rs);
        }

        public async Task<HR_CustomerInfoResult> GetCustomerInfo(string pEmployeeATID, int pCompanyIndex)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID)
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID
                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeATID == pEmployeeATID && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        select new { User = u, Employee = e, CardInfo = ci };

            var result = dummy.AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                rs = _Mapper.Map(x.Employee, rs);
                rs.CardNumber = x.CardInfo?.CardNumber;
                return rs;
            }).FirstOrDefault();

            return await Task.FromResult(result);
        }

        public async Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit)
        {
            var dummy = from u in DbContext.HR_User.Where(x => x.CompanyIndex == pCompanyIndex && x.EmployeeType == (int)EmployeeType.Guest)
                        join e in DbContext.HR_CustomerInfo.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals e.EmployeeATID

                        join c in DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == pCompanyIndex && x.IsActive == true)
                        on u.EmployeeATID equals c.EmployeeATID into card
                        from ci in card.DefaultIfEmpty()

                        join us in DbContext.IC_UserMaster.Where(x => x.CompanyIndex == pCompanyIndex)
                        on u.EmployeeATID equals us.EmployeeATID into usermaster
                        from usResult in usermaster.DefaultIfEmpty()

                        select new { EmployeeATID = u.EmployeeATID, User = u, Employee = e, CardInfo = ci, UserMaster = usResult };

            if (pPage < 1) pPage = 1;

            var result = dummy.OrderBy(x => x.EmployeeATID).Skip((pPage - 1) * pLimit).Take(pLimit).AsEnumerable().Select(x =>
            {
                var rs = _Mapper.Map<HR_CustomerInfoResult>(x.User);
                if (x.Employee != null)
                    rs = _Mapper.Map(x.Employee, rs);
                if (x.UserMaster != null)
                    rs = _Mapper.Map(x.UserMaster, rs);
                if (x.CardInfo != null)
                    rs = _Mapper.Map(x.CardInfo, rs);
                return rs;
            }).ToList();

            var gridClass = new DataGridClass(result.Count, result);

            return await Task.FromResult(gridClass);
        }

        public async Task<List<IC_CustomerImportDTO>> ValidationImportCustomer(List<IC_CustomerImportDTO> param, int userType, UserInfo user)
        {
            var listEmployeeLookup = await _userService.GetEmployeeAndDepartmentLookup(user.CompanyIndex);
            var listEmployeATIDDB = DbContext.HR_User.Where(e => e.CompanyIndex == 2).Select(e => e.EmployeeATID).ToHashSet();
            var listCardNumber = DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == 2).ToList();
            var positionNames = param.Select(x => x.PositionName.ToLower()).ToList();
            var positionList = await DbContext.HR_PositionInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && positionNames.Contains(x.Name.ToLower())).ToListAsync();

            var errorList = new List<IC_CustomerImportDTO>();
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateCard = param.Where(x => x.CardNumber != "0" && !string.IsNullOrEmpty(x.CardNumber)).GroupBy(x => x.CardNumber).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20
            || e.PhoneNumber.Length > 50
            || e.Nric.Length > 50
            ).ToList();
            var checkIsNull = param.Where(e => (userType != (short)EmployeeType.Guest && string.IsNullOrWhiteSpace(e.EmployeeATID))
                || (userType == (short)EmployeeType.Parents && string.IsNullOrWhiteSpace(e.StudentOfParent))).ToList();

            var existCard = new List<IC_CustomerImportDTO>();
            foreach (var item in param)
            {
                var card = listCardNumber.FirstOrDefault(x => x.CardNumber == item.CardNumber && item.CardNumber != "0"
                    && !string.IsNullOrEmpty(item.CardNumber) && item.EmployeeATID != x.EmployeeATID && x.IsActive == true);
                if (card != null)
                {
                    item.ErrorMessage += "Đã tồn tại mã thẻ\r\n";
                    existCard.Add(item);
                }
                if (!string.IsNullOrWhiteSpace(item.StudentOfParent))
                {
                    var listStudentOfParent = item.StudentOfParent.Split(',').ToList();
                    if (listStudentOfParent.Any(x => !listEmployeeLookup.Any(y => y.EmployeeATID == x)))
                    {
                        var listNotExistStudent = listStudentOfParent.Where(x => !listEmployeeLookup.Any(y => y.EmployeeATID == x)).ToList();
                        item.ErrorMessage += "Học sinh không tồn tại: " + String.Join(",", listNotExistStudent) + "\r\n";
                    }
                }
                if (userType == (short)EmployeeType.Contractor && !string.IsNullOrWhiteSpace(item.PositionName))
                {
                    if (!positionList.Any(x => x.Name.ToLower() == item.PositionName.ToLower()))
                    {
                        item.ErrorMessage += "Chức vụ không tồn tại\r\n";
                    }
                    else
                    {
                        item.PositionIndex = positionList.FirstOrDefault(x => x.Name.ToLower() == item.PositionName.ToLower()).Index;
                    }
                }
            }
            //errorList.AddRange(existCard);

            var checkExisted = param.Where(e => listEmployeATIDDB.Contains(e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'))).ToList();

            if (checkDuplicate.Any())
            {
                var duplicate = param.Where(e => checkDuplicate.Contains(e.EmployeeATID)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage = "Trùng mã nhân viên\r\n";
                }
                //errorList.AddRange(duplicate);
            }

            if (checkDuplicateCard.Any())
            {
                var duplicate = param.Where(e => checkDuplicateCard.Contains(e.CardNumber)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage += "Trùng mã thẻ trong file excel\r\n";
                }
                //errorList.AddRange(duplicate);
            }

            if (checkMaxLength.Any())
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 100) item.ErrorMessage += "Mã chấm công lớn hơn 50 ký tự" + "\r\n";
                    if (item.EmployeeCode.Length > 50) item.ErrorMessage += "Mã nhân viên lớn hơn 50 ký tự" + "\r\n";
                    if (item.FullName.Length > 200) item.ErrorMessage += "Tên nhân viên lớn hơn 200 ký tự" + "\r\n";
                    if (item.CardNumber.Length > 30) item.ErrorMessage += "Mã thẻ lớn hơn 30 ký tự" + "\r\n";
                    if (item.NameOnMachine.Length > 20) item.ErrorMessage += "Tên trên máy lớn hơn 20 ký tự" + "\r\n";
                    if (item.PhoneNumber.Length > 50) item.ErrorMessage += "Số DT lớn hơn 50 ký tự" + "\r\n";
                    if (item.Nric.Length > 50) item.ErrorMessage += "CMND/CCCD/Passport lớn hơn 50 ký tự" + "\r\n";

                }
                //errorList.AddRange(checkMaxLength);
            }
            if (checkIsNull.Any())
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrWhiteSpace(item.EmployeeATID)) item.ErrorMessage += "Mã chấm không được để trống\r\n";
                    //if (string.IsNullOrEmpty(item.ClassName)) item.ErrorMessage += "Tên lớp không được để trống";
                    if (userType == (short)EmployeeType.Parents && string.IsNullOrWhiteSpace(item.StudentOfParent))
                    {
                        item.ErrorMessage += "Học sinh không được để trống\r\n";
                    }
                }

                //errorList.AddRange(checkIsNull);
            }

            if (checkExisted.Any())
            {
                foreach (var item in checkExisted)
                {
                    item.ErrorMessage += "Mã người dùng đã tồn tại\r\n";
                }
                //errorList.AddRange(checkExisted);
            }

            errorList = param.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

            return await Task.FromResult(errorList);
        }

        public async Task<List<IC_CustomerImportDTO>> ValidationImportCustomer_KinhDo(List<IC_CustomerImportDTO> param, int userType, UserInfo user)
        {
            var listEmployeeLookup = await _userService.GetEmployeeAndDepartmentLookup(user.CompanyIndex);
            var listEmployeATIDDB = DbContext.HR_User.Where(e => e.CompanyIndex == 2).Select(e => e.EmployeeATID).ToHashSet();
            var listCardNumber = DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == 2).ToList();
            var departmentList = _iC_DepartmentService.GetAllDepartment(user.CompanyIndex);

            var positionNames = param.Select(x => x.PositionName.ToLower()).ToList();
            var positionList = await DbContext.HR_PositionInfo.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex
                && positionNames.Contains(x.Name.ToLower())).ToListAsync();
            var listContactPersonID = param.Where(x => !string.IsNullOrWhiteSpace(x.ContactPerson)).Select(x => x.ContactPerson).Distinct().ToList();
            var listContactPersonInfo = await _userService.GetEmployeeCompactInfoByListEmpATID(listContactPersonID, user.CompanyIndex);

            var errorList = new List<IC_CustomerImportDTO>();
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateCard = param.Where(x => x.CardNumber != "0" && !string.IsNullOrEmpty(x.CardNumber)).GroupBy(x => x.CardNumber).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20
            || e.PhoneNumber.Length > 50
            || e.Nric.Length > 50).ToList();
            var checkIsNull = param.Where(e => (userType != (short)EmployeeType.Guest && string.IsNullOrWhiteSpace(e.EmployeeATID))
            || (userType == (short)EmployeeType.Parents && string.IsNullOrWhiteSpace(e.StudentOfParent))).ToList();

            var existCard = new List<IC_CustomerImportDTO>();
            foreach (var item in param)
            {
                var card = listCardNumber.FirstOrDefault(x => x.CardNumber == item.CardNumber && item.CardNumber != "0"
                    && !string.IsNullOrEmpty(item.CardNumber) && item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0') != x.EmployeeATID && x.IsActive == true);
                if (card != null)
                {
                    item.ErrorMessage += "Đã tồn tại mã thẻ\r\n";
                    existCard.Add(item);
                }
                if (!string.IsNullOrWhiteSpace(item.StudentOfParent))
                {
                    var listStudentOfParent = item.StudentOfParent.Split(',').ToList();
                    if (listStudentOfParent.Any(x => !listEmployeeLookup.Any(y => y.EmployeeATID == x)))
                    {
                        var listNotExistStudent = listStudentOfParent.Where(x => !listEmployeeLookup.Any(y => y.EmployeeATID == x)).ToList();
                        item.ErrorMessage += "Học sinh không tồn tại: " + String.Join(",", listNotExistStudent) + "\r\n";
                    }
                }
            }
            //errorList.AddRange(existCard);
            if (_configClientName.ToUpper() != ClientName.MONDELEZ.ToString())
            {
                var checkExisted = param.Where(e => listEmployeATIDDB.Contains(e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'))).ToList();

                if (checkDuplicate.Any())
                {
                    var duplicate = param.Where(e => checkDuplicate.Contains(e.EmployeeATID)).ToList();
                    foreach (var item in duplicate)
                    {
                        item.ErrorMessage = "Trùng mã nhân viên\r\n";
                    }
                    //errorList.AddRange(duplicate);
                }
            }



            if (checkDuplicateCard.Any())
            {
                var duplicate = param.Where(e => checkDuplicateCard.Contains(e.CardNumber)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage += "Trùng mã thẻ trong file excel\r\n";
                }
                //errorList.AddRange(duplicate);
            }

            if (checkMaxLength.Any())
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 100) item.ErrorMessage += "Mã chấm công lớn hơn 50 ký tự" + "\r\n";
                    if (item.EmployeeCode.Length > 50) item.ErrorMessage += "Mã nhân viên lớn hơn 50 ký tự" + "\r\n";
                    if (item.FullName.Length > 200) item.ErrorMessage += "Tên nhân viên lớn hơn 200 ký tự" + "\r\n";
                    if (item.CardNumber.Length > 30) item.ErrorMessage += "Mã thẻ lớn hơn 30 ký tự" + "\r\n";
                    if (item.NameOnMachine.Length > 20) item.ErrorMessage += "Tên trên máy lớn hơn 20 ký tự" + "\r\n";
                    if (item.PhoneNumber.Length > 50) item.ErrorMessage += "Số DT lớn hơn 50 ký tự" + "\r\n";
                    if (item.Nric.Length > 50) item.ErrorMessage += "CMND/CCCD/Passport lớn hơn 50 ký tự" + "\r\n";

                }
                //errorList.AddRange(checkMaxLength);
            }
            if (checkIsNull.Any())
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrEmpty(item.EmployeeATID)) item.ErrorMessage += "Mã chấm công không được để trống\r\n";
                    //if (string.IsNullOrEmpty(item.ClassName)) item.ErrorMessage += "Tên lớp không được để trống";
                    if (userType == (short)EmployeeType.Parents && string.IsNullOrWhiteSpace(item.StudentOfParent))
                    {
                        item.ErrorMessage += "Học sinh không được để trống\r\n";
                    }
                }

                //errorList.AddRange(checkIsNull);
            }

            if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
            {
                var dataCheckBlackList = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID) || !string.IsNullOrWhiteSpace(x.Nric)).ToList();

                var employeeInBlackList = DbContext.GC_BlackList.Where(x => dataCheckBlackList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID) || dataCheckBlackList.Select(z => z.Nric).Contains(x.Nric)).ToList();

                foreach (var item in param)
                {
                    if (userType != (short)EmployeeType.Parents && string.IsNullOrEmpty(item.Nric)) item.ErrorMessage += "CMND/CCCD/Passport không được để trống\r\n";
                    if (!string.IsNullOrEmpty(item.Nric) && char.IsDigit(item.Nric[0]) && item.Nric.Length != 12)
                    {
                        item.ErrorMessage += "CCCD phải đúng 12 ký tự \r\n";
                    }

                    if (string.IsNullOrEmpty(item.FullName)) item.ErrorMessage += "Họ tên không được để trống\r\n";
                    if (userType == 2)
                    {
                        if (string.IsNullOrEmpty(item.ContactDepartment)) item.ErrorMessage += "Phòng ban liên hệ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.ContactPerson)) item.ErrorMessage += "Người liên hệ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.JoinedDate)) item.ErrorMessage += "Từ ngày không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.StoppedDate)) item.ErrorMessage += "Đến ngày không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.StartTime)) item.ErrorMessage += "Từ giờ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.EndTime)) item.ErrorMessage += "Đến giờ không được để trống\r\n";

                        DateTime startTime = new DateTime();
                        DateTime endTime = new DateTime();
                        if (!string.IsNullOrEmpty(item.EndTime))
                        {
                            string[] formats = { "HH:mm:ss" };

                            if (!DateTime.TryParseExact(item.EndTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                            {
                                item.ErrorMessage += "Đến giờ không hợp lệ\r\n";
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(item.StartTime))
                                {
                                    if (!DateTime.TryParseExact(item.StartTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                                    {
                                        item.ErrorMessage += "Từ giờ không hợp lệ\r\n";
                                    }
                                    else if (endTime < startTime)
                                    {
                                        item.ErrorMessage += "Đến giờ phải lớn hơn từ giờ\r\n";
                                    }
                                }
                            }
                        }


                        if (!string.IsNullOrWhiteSpace(item.ContactDepartment))
                        {
                            var listAllDepartmentNameFromImport = new List<string>();
                            if (item.ContactDepartment.Contains("/"))
                            {
                                var listSplitDepartmentName = item.ContactDepartment.Split("/").Distinct().ToList();
                                listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                            }
                            else
                            {
                                listAllDepartmentNameFromImport.Add(item.ContactDepartment);
                            }
                            listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                            var departmentInfo = listAllDepartmentNameFromImport.LastOrDefault();
                            var department = departmentList.FirstOrDefault(y => y.Name == departmentInfo);

                            if (department != null)
                            {

                                if (department.IsContractorDepartment == true || department.IsDriverDepartment == true)
                                {
                                    item.ErrorMessage += "Phòng ban không hợp lệ\r\n";
                                }
                                else
                                {
                                    item.ContactDepartmentIndex = department.Index;
                                }
                                if (!string.IsNullOrWhiteSpace(item.ContactPerson))
                                {
                                    if (!listContactPersonInfo.Any(x => x.EmployeeATID == item.ContactPerson))
                                    {
                                        item.ErrorMessage += "Người liên hệ không tồn tại\r\n";
                                    }
                                    else if (listContactPersonInfo.Any(x => x.EmployeeATID == item.ContactPerson)
                                        && listContactPersonInfo.FirstOrDefault(x
                                            => x.EmployeeATID == item.ContactPerson)?.DepartmentIndex != item.ContactDepartmentIndex)
                                    {
                                        item.ErrorMessage += "Người liên hệ không thuộc phòng ban liên hệ\r\n";
                                    }
                                }
                            }
                            else
                            {
                                item.ErrorMessage += "Phòng ban liên hệ không tồn tại\r\n";
                            }
                        }
                    }
                    if (userType == (short)EmployeeType.Contractor && !string.IsNullOrWhiteSpace(item.PositionName))
                    {
                        if (!positionList.Any(x => x.Name.ToLower() == item.PositionName.ToLower()))
                        {
                            item.ErrorMessage += "Chức vụ không tồn tại\r\n";
                        }
                        else
                        {
                            item.PositionIndex = positionList.FirstOrDefault(x => x.Name.ToLower() == item.PositionName.ToLower()).Index;
                        }
                    }
                    var now = DateTime.Now;
                    if (userType == (int)EmployeeType.Contractor)
                    {
                        if (string.IsNullOrEmpty(item.Department)) item.ErrorMessage += "Phòng ban không được để trống\r\n";

                        if (string.IsNullOrWhiteSpace(item.Department))
                        {
                            item.ErrorMessage += "Phòng ban không được để trống\r\n";
                        }
                        else if (!string.IsNullOrWhiteSpace(item.Department))
                        {
                            var listAllDepartmentNameFromImport = new List<string>();
                            if (item.Department.Contains("/"))
                            {
                                var listSplitDepartmentName = item.Department.Split("/").Distinct().ToList();
                                listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                            }
                            else
                            {
                                listAllDepartmentNameFromImport.Add(item.Department);
                            }
                            listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                            var departmentInfo = listAllDepartmentNameFromImport.LastOrDefault();
                            var department = departmentList.FirstOrDefault(y => y.Name == departmentInfo);

                            if (department != null)
                            {
                                if (department.IsContractorDepartment == false)
                                {
                                    item.ErrorMessage += "Phòng ban không hợp lệ\r\n";
                                }
                                else
                                {
                                    item.DepartmentIndex = department.Index;
                                }

                            }
                            else
                            {
                                item.ErrorMessage += "Phòng ban không tồn tại\r\n";
                            }
                        }

                    }
                    //Check Birday 
                    if (string.IsNullOrEmpty(item.DateOfBirth))
                    {
                        if (userType != (short)EmployeeType.Parents)
                        {
                            item.ErrorMessage += "Ngày sinh không được để trống\r\n";
                        }
                    }
                    else
                    {
                        string[] formats = { "dd/MM/yyyy" };
                        var birthDay = new DateTime();

                        var convertFromDate = DateTime.TryParseExact(item.DateOfBirth, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out birthDay);
                        if (!convertFromDate)
                        {
                            item.ErrorMessage += "Ngày sinh không hợp lệ\r\n";
                        }
                        else
                        {
                            if (birthDay.AddYears(18).Date >= now.Date)
                            {
                                item.ErrorMessage += "Nhân viên chưa đủ 18 tuổi\r\n";
                            }
                        }
                    }


                    //Check stopped day > start day
                    DateTime stoppedDate = new DateTime();
                    DateTime joinedDate = new DateTime();
                    if (!string.IsNullOrEmpty(item.StoppedDate))
                    {
                        string[] formats = { "dd/MM/yyyy" };

                        if (!DateTime.TryParseExact(item.StoppedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out stoppedDate))
                        {
                            item.ErrorMessage += "Ngày nghỉ không hợp lệ\r\n";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.JoinedDate))
                            {
                                if (stoppedDate.Date <= now.Date)
                                {
                                    item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày hiện tại\r\n";
                                }
                            }
                            else if (!DateTime.TryParseExact(item.JoinedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinedDate))
                            {
                                item.ErrorMessage += "Ngày vào không hợp lệ\r\n";
                            }
                            else if (stoppedDate.Date < joinedDate.Date)
                            {
                                item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày vào\r\n";
                            }
                        }
                    }

                    var checkEmployeeInBlackList = employeeInBlackList.Any(x => ((!string.IsNullOrWhiteSpace(item.EmployeeATID) && item.EmployeeATID == x.EmployeeATID) || (!string.IsNullOrEmpty(item.Nric) && x.Nric == item.Nric))
                                                                                                            && x.FromDate.Date <= now.Date
                                                                                                            && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
                    if (checkEmployeeInBlackList)
                    {
                        item.ErrorMessage += "Người dùng thuộc danh sách đen\r\n";
                    }

                    if (!string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        //errorList.Add(item);
                    }
                }

            }



            //if (checkExisted.Any())
            //{
            //    foreach (var item in checkExisted)
            //    {
            //        item.ErrorMessage += "Mã người dùng đã tồn tại\r\n";
            //    }
            //    //errorList.AddRange(checkExisted);
            //}

            errorList = param.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

            return await Task.FromResult(errorList);
        }

        public async Task<List<IC_CustomerImportDTO>> ValidationImportCustomerFromGoogleSheet(List<IC_CustomerImportDTO> param, int userType)
        {
            var companyIndex = 2;
            var listEmployeeLookup = await _userService.GetEmployeeAndDepartmentLookup(companyIndex);
            var listEmployeATIDDB = DbContext.HR_User.Where(e => e.CompanyIndex == companyIndex).Select(e => e.EmployeeATID).ToHashSet();
            var listCardNumber = DbContext.HR_CardNumberInfo.Where(x => x.CompanyIndex == companyIndex).ToList();
            var departmentList = _iC_DepartmentService.GetAllDepartment(companyIndex);

            var positionNames = param.Select(x => x.PositionName.ToLower()).ToList();
            var positionList = await DbContext.HR_PositionInfo.AsNoTracking().Where(x => x.CompanyIndex == companyIndex
                && positionNames.Contains(x.Name.ToLower())).ToListAsync();
            var listContactPersonID = param.Where(x => !string.IsNullOrWhiteSpace(x.ContactPerson)).Select(x => x.ContactPerson).Distinct().ToList();
            var listContactPersonInfo = await _userService.GetEmployeeCompactInfoByListEmpATID(listContactPersonID, companyIndex);

            var errorList = new List<IC_CustomerImportDTO>();
            var checkDuplicate = param.GroupBy(x => x.EmployeeATID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkDuplicateCard = param.Where(x => x.CardNumber != "0" && !string.IsNullOrEmpty(x.CardNumber)).GroupBy(x => x.CardNumber).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            var checkMaxLength = param.Where(e => e.EmployeeATID.Length > 100
            || e.EmployeeCode.Length > 50
            || e.FullName.Length > 200
            || e.CardNumber.Length > 30
            || e.NameOnMachine.Length > 20
            || e.PhoneNumber.Length > 50
            || e.Nric.Length > 50).ToList();
            var checkIsNull = param.Where(e => (userType != (short)EmployeeType.Guest && string.IsNullOrWhiteSpace(e.EmployeeATID))
            || (userType == (short)EmployeeType.Parents && string.IsNullOrWhiteSpace(e.StudentOfParent))).ToList();

            var existCard = new List<IC_CustomerImportDTO>();
            foreach (var item in param)
            {
                var card = listCardNumber.FirstOrDefault(x => x.CardNumber == item.CardNumber && item.CardNumber != "0"
                    && !string.IsNullOrEmpty(item.CardNumber) && item.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0') != x.EmployeeATID && x.IsActive == true);
                if (card != null)
                {
                    item.ErrorMessage += "Đã tồn tại mã thẻ\r\n";
                    existCard.Add(item);
                }
            }
            //errorList.AddRange(existCard);
            if (_configClientName.ToUpper() != ClientName.MONDELEZ.ToString())
            {
                var checkExisted = param.Where(e => listEmployeATIDDB.Contains(e.EmployeeATID.PadLeft(_config.MaxLenghtEmployeeATID, '0'))).ToList();

                if (checkDuplicate.Any())
                {
                    var duplicate = param.Where(e => checkDuplicate.Contains(e.EmployeeATID)).ToList();
                    foreach (var item in duplicate)
                    {
                        item.ErrorMessage = "Trùng mã nhân viên\r\n";
                    }
                    //errorList.AddRange(duplicate);
                }
            }



            if (checkDuplicateCard.Any())
            {
                var duplicate = param.Where(e => checkDuplicateCard.Contains(e.CardNumber)).ToList();
                foreach (var item in duplicate)
                {
                    item.ErrorMessage += "Trùng mã thẻ trong file excel\r\n";
                }
                //errorList.AddRange(duplicate);
            }

            if (checkMaxLength.Any())
            {
                foreach (var item in checkMaxLength)
                {
                    if (item.EmployeeATID.Length > 100) item.ErrorMessage += "Mã chấm công lớn hơn 50 ký tự" + "\r\n";
                    if (item.EmployeeCode.Length > 50) item.ErrorMessage += "Mã nhân viên lớn hơn 50 ký tự" + "\r\n";
                    if (item.FullName.Length > 200) item.ErrorMessage += "Tên nhân viên lớn hơn 200 ký tự" + "\r\n";
                    if (item.CardNumber.Length > 30) item.ErrorMessage += "Mã thẻ lớn hơn 30 ký tự" + "\r\n";
                    if (item.NameOnMachine.Length > 20) item.ErrorMessage += "Tên trên máy lớn hơn 20 ký tự" + "\r\n";
                    if (item.PhoneNumber.Length > 50) item.ErrorMessage += "Số DT lớn hơn 50 ký tự" + "\r\n";
                    if (item.Nric.Length > 50) item.ErrorMessage += "CMND/CCCD/Passport lớn hơn 50 ký tự" + "\r\n";

                }
                //errorList.AddRange(checkMaxLength);
            }
            if (checkIsNull.Any())
            {
                foreach (var item in checkIsNull)
                {
                    if (string.IsNullOrEmpty(item.EmployeeATID)) item.ErrorMessage += "Mã chấm công không được để trống\r\n";
                    //if (string.IsNullOrEmpty(item.ClassName)) item.ErrorMessage += "Tên lớp không được để trống";
                }

                //errorList.AddRange(checkIsNull);
            }

            if (_configClientName.ToUpper() == ClientName.MONDELEZ.ToString())
            {
                var dataCheckBlackList = param.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeATID) || !string.IsNullOrWhiteSpace(x.Nric)).ToList();

                var employeeInBlackList = DbContext.GC_BlackList.Where(x => dataCheckBlackList.Select(z => z.EmployeeATID).Contains(x.EmployeeATID) || dataCheckBlackList.Select(z => z.Nric).Contains(x.Nric)).ToList();

                foreach (var item in param)
                {
                    if (userType != (short)EmployeeType.Parents && string.IsNullOrEmpty(item.Nric)) item.ErrorMessage += "CMND/CCCD/Passport không được để trống\r\n";
                    if (!string.IsNullOrEmpty(item.Nric) && char.IsDigit(item.Nric[0]) && item.Nric.Length != 12)
                    {
                        item.ErrorMessage += "CCCD phải đúng 12 ký tự \r\n";
                    }
                    if (string.IsNullOrEmpty(item.FullName)) item.ErrorMessage += "Họ tên không được để trống\r\n";
                    if (userType == 2)
                    {
                        if (string.IsNullOrEmpty(item.ContactDepartment)) item.ErrorMessage += "Phòng ban liên hệ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.ContactPerson)) item.ErrorMessage += "Người liên hệ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.JoinedDate)) item.ErrorMessage += "Từ ngày không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.StoppedDate)) item.ErrorMessage += "Đến ngày không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.StartTime)) item.ErrorMessage += "Từ giờ không được để trống\r\n";
                        if (string.IsNullOrEmpty(item.EndTime)) item.ErrorMessage += "Đến giờ không được để trống\r\n";

                        DateTime startTime = new DateTime();
                        DateTime endTime = new DateTime();
                        if (!string.IsNullOrEmpty(item.EndTime))
                        {
                            string[] formats = { "HH:mm:ss" };

                            if (!DateTime.TryParseExact(item.EndTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                            {
                                item.ErrorMessage += "Đến giờ không hợp lệ\r\n";
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(item.StartTime))
                                {
                                    if (!DateTime.TryParseExact(item.StartTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                                    {
                                        item.ErrorMessage += "Từ giờ không hợp lệ\r\n";
                                    }
                                    else if (endTime < startTime)
                                    {
                                        item.ErrorMessage += "Đến giờ phải lớn hơn từ giờ\r\n";
                                    }
                                }
                            }
                        }


                        if (!string.IsNullOrWhiteSpace(item.ContactDepartment))
                        {
                            var listAllDepartmentNameFromImport = new List<string>();
                            if (item.ContactDepartment.Contains("/"))
                            {
                                var listSplitDepartmentName = item.ContactDepartment.Split("/").Distinct().ToList();
                                listAllDepartmentNameFromImport.AddRange(listSplitDepartmentName);
                            }
                            else
                            {
                                listAllDepartmentNameFromImport.Add(item.ContactDepartment);
                            }
                            listAllDepartmentNameFromImport = listAllDepartmentNameFromImport.Distinct().ToList();
                            var departmentInfo = listAllDepartmentNameFromImport.LastOrDefault();
                            var department = departmentList.FirstOrDefault(y => y.Name == departmentInfo);

                            if (department != null)
                            {

                                if (department.IsContractorDepartment == true || department.IsDriverDepartment == true)
                                {
                                    item.ErrorMessage += "Phòng ban không hợp lệ\r\n";
                                }
                                else
                                {
                                    item.ContactDepartmentIndex = department.Index;
                                }

                            }
                            else
                            {
                                item.ErrorMessage += "Phòng ban liên hệ không tồn tại\r\n";
                            }
                        }
                    }

                    var now = DateTime.Now;
                    //Check Birday 
                    if (string.IsNullOrEmpty(item.DateOfBirth))
                    {
                        if (userType != (short)EmployeeType.Parents)
                        {
                            item.ErrorMessage += "Ngày sinh không được để trống\r\n";
                        }
                    }
                    else
                    {
                        string[] formats = { "dd/MM/yyyy" };
                        var birthDay = new DateTime();

                        var convertFromDate = DateTime.TryParseExact(item.DateOfBirth, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out birthDay);
                        if (!convertFromDate)
                        {
                            item.ErrorMessage += "Ngày sinh không hợp lệ\r\n";
                        }
                        else
                        {
                            if (birthDay.AddYears(18).Date >= now.Date)
                            {
                                item.ErrorMessage += "Nhân viên chưa đủ 18 tuổi\r\n";
                            }
                        }
                    }

                    //Check stopped day > start day
                    DateTime stoppedDate = new DateTime();
                    DateTime joinedDate = new DateTime();
                    if (!string.IsNullOrEmpty(item.StoppedDate))
                    {
                        string[] formats = { "dd/MM/yyyy" };

                        if (!DateTime.TryParseExact(item.StoppedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out stoppedDate))
                        {
                            item.ErrorMessage += "Ngày nghỉ không hợp lệ\r\n";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.JoinedDate))
                            {
                                if (stoppedDate.Date <= now.Date)
                                {
                                    item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày hiện tại\r\n";
                                }
                            }
                            else if (!DateTime.TryParseExact(item.JoinedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out joinedDate))
                            {
                                item.ErrorMessage += "Ngày vào không hợp lệ\r\n";
                            }
                            else if (stoppedDate.Date < joinedDate.Date)
                            {
                                item.ErrorMessage += "Ngày nghỉ phải lớn hơn ngày vào\r\n";
                            }
                        }
                    }

                    var checkEmployeeInBlackList = employeeInBlackList.Any(x => ((!string.IsNullOrWhiteSpace(item.EmployeeATID) && item.EmployeeATID == x.EmployeeATID) || (!string.IsNullOrEmpty(item.Nric) && x.Nric == item.Nric))
                                                                                                            && x.FromDate.Date <= now.Date
                                                                                                            && (x.ToDate == null || (x.ToDate != null && now.Date <= x.ToDate.Value.Date)));
                    if (checkEmployeeInBlackList)
                    {
                        item.ErrorMessage += "Người dùng thuộc danh sách đen\r\n";
                    }

                    if (!string.IsNullOrWhiteSpace(item.ErrorMessage))
                    {
                        //errorList.Add(item);
                    }
                }

            }



            //if (checkExisted.Any())
            //{
            //    foreach (var item in checkExisted)
            //    {
            //        item.ErrorMessage += "Mã người dùng đã tồn tại\r\n";
            //    }
            //    //errorList.AddRange(checkExisted);
            //}

            errorList = param.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).ToList();

            return await Task.FromResult(errorList);
        }
        public async Task CheckCustomerInfoActivedOrCreate(HR_CustomerInfo hrCustomerInfo, int pCompanyIndex)
        {
            var rs = await DbContext.HR_CustomerInfo.FirstOrDefaultAsync(e => e.CompanyIndex == pCompanyIndex && hrCustomerInfo.EmployeeATID == e.EmployeeATID);

            if (rs != null)
            {
                rs.AccompanyingPersonList = hrCustomerInfo.AccompanyingPersonList;
                rs.Address = hrCustomerInfo.Address;
                rs.BikeDescription = hrCustomerInfo.BikeDescription;
                rs.BikeModel = hrCustomerInfo.BikeModel;
                rs.BikePlate = hrCustomerInfo.BikePlate;
                rs.Phone = hrCustomerInfo.Phone;
                rs.BikeType = hrCustomerInfo.BikeType;
                rs.Company = hrCustomerInfo.Company;
                rs.ContactPerson = hrCustomerInfo.ContactPerson;
                rs.ContactPersonATIDs = hrCustomerInfo.ContactPersonATIDs;
                rs.CustomerID = hrCustomerInfo.CustomerID;
                rs.DataStorageTime = hrCustomerInfo.DataStorageTime;
                rs.Email = hrCustomerInfo.Email;
                rs.ExtensionTime = hrCustomerInfo.ExtensionTime;
                rs.FromTime = hrCustomerInfo.FromTime;
                rs.GoInSystem = hrCustomerInfo.GoInSystem;
                rs.IsVIP = hrCustomerInfo.IsVIP;
                rs.IdentityImage = hrCustomerInfo.IdentityImage;
                rs.LicensePlateBackImage = hrCustomerInfo.LicensePlateBackImage;
                rs.LicensePlateFrontImage = hrCustomerInfo.LicensePlateFrontImage;
                rs.NRIC = hrCustomerInfo.NRIC;
                rs.NRICBackImage = hrCustomerInfo.NRICBackImage;
                rs.NRICFrontImage = hrCustomerInfo.NRICFrontImage;
                rs.NumberOfContactPerson = hrCustomerInfo.NumberOfContactPerson;
                rs.RegisterCode = hrCustomerInfo.RegisterCode;
                rs.RegisterTime = hrCustomerInfo.RegisterTime;
                rs.RulesCustomerIndex = hrCustomerInfo.RulesCustomerIndex;
                rs.ToTime = hrCustomerInfo.ToTime;
                rs.WorkContent = hrCustomerInfo.WorkContent;
                rs.UpdatedUser = hrCustomerInfo.UpdatedUser;
                rs.UpdatedDate = hrCustomerInfo.UpdatedDate;
                DbContext.HR_CustomerInfo.Update(rs);
            }
            else
            {
                DbContext.HR_CustomerInfo.Add(hrCustomerInfo);
            }
        }

        public List<string> GenerateUniqueNumberStrings(int listLength, int lengthString, string prefix, List<string> existingList)
        {
            List<string> result = new List<string>();
            HashSet<string> uniqueStrings = new HashSet<string>(existingList);

            long maxValue = listLength;
            long minValue = 0;

            for (long i = minValue; i < maxValue; i++)
            {
                string str = prefix + i.ToString().PadLeft(lengthString - prefix.Length, '0');
                if (!uniqueStrings.Contains(str))
                {
                    result.Add(str);
                    uniqueStrings.Add(str);
                }
                else
                {
                    var j = 0;
                    while (uniqueStrings.Contains(str))
                    {
                        str = prefix + (i + j).ToString().PadLeft(lengthString - prefix.Length, '0');
                        if (!uniqueStrings.Contains(str))
                        {
                            result.Add(str);
                            uniqueStrings.Add(str);
                            break;
                        }
                        j++;
                    }
                }
            }

            return result;
        }

        public List<string> RemoveWhiteSpaceAndToUpper(string text)
        {
            if (text == null)
            {
                return new List<string>();
            }
            return text.Split("|").ToList();
        }

        public async Task<bool> ImportDataFromGoogleSheet(int companyIndex)
        {
            var randomIds = new List<string>();
            try
            {
                if (string.IsNullOrWhiteSpace(_applicationName) || string.IsNullOrWhiteSpace(_spreadsheetId) || string.IsNullOrWhiteSpace(_sheetName))
                {
                    return false;
                }

                var listEmailAllow = new List<string>();
                var listEmailDeclareGuest = _DbContext.HR_EmailDeclareGuest.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).Select(x => x.EmailAddress).ToList();
                foreach (var email in listEmailDeclareGuest)
                {
                    var emailAllowList = email.Split(',').ToList();
                    listEmailAllow.AddRange(emailAllowList);
                }

                //var icConfig = _DbContext.IC_Config.FirstOrDefault(t => t.CompanyIndex == companyIndex && t.EventType == ConfigAuto.CONFIG_EMAIL_ALLOW_IMPORT_GGSHEET.ToString());
                //if (icConfig != null)
                //{
                //    if (icConfig.CustomField != null && icConfig.CustomField != "")
                //    {
                //        var emailField = JsonConvert.DeserializeObject<IntegrateLogParam>(icConfig.CustomField);
                //        if (!string.IsNullOrWhiteSpace(emailField.EmailAllowImportGoogleSheet) && emailField.EmailAllowImportGoogleSheet.Trim() != "")
                //        {
                //            listEmailAllow = emailField.EmailAllowImportGoogleSheet.Split(';').ToList();
                //        }
                //        else
                //        {
                //            _logger.LogError("ImportDataFromGoogleSheet_DontAllowMail");
                //            return false;
                //        }
                //    }
                //}
                //else
                //{
                //    _logger.LogError("ImportDataFromGoogleSheet_DontAllowMail");
                //    return false;
                //}

                ConfigObject config = ConfigObject.GetConfig(_Cache);
                string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
                string applicationName = _applicationName;
                GoogleCredential credential;
                using (var stream =
                    new FileStream(@"Files/GoogleSheet/credentials.json", FileMode.Open, System.IO.FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(scopes);
                }

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName,
                });

                // Define request parameters.
                String spreadsheetId = _spreadsheetId;
                String range = _sheetName + "!A1:W";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(spreadsheetId, range);

                // Fetch data.
                ValueRange response = request.Execute();
                var values = response.Values;

                var listDateImport = values.Skip(1).ToList();


                var param = new List<IC_CustomerImportDTO>();

                foreach (var item in listDateImport)
                {
                    //Check email allow add data
                    var emailCheck = item.ElementAtOrDefault(22)?.ToString() ?? "";
                    if (!listEmailAllow.Contains(emailCheck))
                    {
                        continue;
                    }
                    DateTime timesTamp = new DateTime();
                    string[] formats = { "dd/MM/yyyy H:mm:ss" };

                    var employeeATIDText = item.ElementAtOrDefault(1)?.ToString() ?? "";
                    var employeeATIDList = RemoveWhiteSpaceAndToUpper(employeeATIDText);

                    var fullNameText = item.ElementAtOrDefault(2)?.ToString() ?? "";
                    var listNameGuest = RemoveWhiteSpaceAndToUpper(fullNameText);

                    var nameOnMachineText = item.ElementAtOrDefault(3)?.ToString() ?? "";
                    var nameOnMachineList = RemoveWhiteSpaceAndToUpper(nameOnMachineText);

                    var genderText = item.ElementAtOrDefault(4)?.ToString()?.ToString() ?? "";
                    var genderList = RemoveWhiteSpaceAndToUpper(genderText);

                    var dateOfBirthText = item.ElementAtOrDefault(5)?.ToString() ?? "";
                    var dateOfBirthList = RemoveWhiteSpaceAndToUpper(dateOfBirthText);

                    var nricText = item.ElementAtOrDefault(6)?.ToString() ?? "";
                    var nricList = RemoveWhiteSpaceAndToUpper(nricText);

                    //var companyNameText = item.ElementAtOrDefault(7)?.ToString() ?? "";
                    //var companyNameList = RemoveWhiteSpaceAndToUpper(companyNameText);

                    var phoneNumberText = item.ElementAtOrDefault(8)?.ToString() ?? "";
                    var phoneNumberList = RemoveWhiteSpaceAndToUpper(phoneNumberText);

                    var emailText = item.ElementAtOrDefault(9)?.ToString() ?? "";
                    var emailList = RemoveWhiteSpaceAndToUpper(emailText);

                    var addressText = item.ElementAtOrDefault(10)?.ToString() ?? "";
                    var addressList = RemoveWhiteSpaceAndToUpper(addressText);

                    //var isAllowPhoneText = item.ElementAtOrDefault(17)?.ToString() ?? "";
                    //var isAllowPhoneList = RemoveWhiteSpaceAndToUpper(isAllowPhoneText);

                    //var workingContentText = item.ElementAtOrDefault(18)?.ToString() ?? "";
                    //var workingContentList = RemoveWhiteSpaceAndToUpper(workingContentText);

                    //var noteText = item.ElementAtOrDefault(19)?.ToString() ?? "";
                    //var noteList = RemoveWhiteSpaceAndToUpper(noteText);

                    if (!DateTime.TryParseExact(item.ElementAtOrDefault(0).ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out timesTamp))
                    {
                        _logger.LogError("ImportDataFromGoogleSheet_FormatException" + item.ElementAtOrDefault(0).ToString());
                    };

                    for (int i = 0; i < listNameGuest.Count; i++)
                    {
                        var gender = 2;
                        if (genderList.ElementAtOrDefault(i)?.ToString().Replace(" ", "").ToUpper() == "NAM")
                        {
                            gender = 1;
                        }
                        else if (genderList.ElementAtOrDefault(i)?.ToString().Replace(" ", "").ToUpper() == "NỮ")
                        {
                            gender = 0;
                        }
                        var allowPhone = 0;
                        if (item.ElementAtOrDefault(18)?.ToString().Replace(" ", "").ToUpper() == "CÓ")
                        {
                            allowPhone = 1;
                        }
                        var dataGuest = new IC_CustomerImportDTO()
                        {
                            Timestamp = timesTamp,
                            EmployeeATID = employeeATIDList.ElementAtOrDefault(i)?.ToString() ?? "",
                            EmployeeCode = "",
                            CardNumber = "",
                            PositionName = "",
                            FullName = listNameGuest.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            NameOnMachine = nameOnMachineList.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            Gender = gender,
                            DateOfBirth = dateOfBirthList.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            Nric = nricList.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            Company = item.ElementAtOrDefault(7)?.ToString().Trim() ?? "",
                            PhoneNumber = phoneNumberList.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            Email = emailList.ElementAtOrDefault(i)?.ToString().Trim() ?? "",
                            Address = item.ElementAtOrDefault(10)?.ToString().Trim() ?? "",

                            ContactDepartment = item.ElementAtOrDefault(11)?.ToString() ?? "",
                            ContactPerson = item.ElementAtOrDefault(12)?.ToString() ?? "",
                            ContactPersonPhoneNumber = item.ElementAtOrDefault(13)?.ToString() ?? "",
                            JoinedDate = item.ElementAtOrDefault(14)?.ToString() ?? "",
                            StoppedDate = item.ElementAtOrDefault(15)?.ToString() ?? "",
                            StartTime = item.ElementAtOrDefault(16)?.ToString() ?? "",
                            EndTime = item.ElementAtOrDefault(17)?.ToString() ?? "",

                            IsAllowPhone = allowPhone,
                            WorkingContent = item.ElementAtOrDefault(19)?.ToString().Trim() ?? "",
                            Note = item.ElementAtOrDefault(20)?.ToString().Trim() ?? "",
                            UpdateUser = item.ElementAtOrDefault(22)?.ToString() ?? "",
                        };

                        //dataGuest.FullName = listGuest[i];
                        //dataGuest.Nric = nricList[i];
                        param.Add(dataGuest);
                    }

                    //if (!DateTime.TryParseExact(item.ElementAtOrDefault(0).ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out timesTamp))
                    //{
                    //    _logger.LogError("ImportDataFromGoogleSheet_FormatException" + item.ElementAtOrDefault(0).ToString());
                    //};
                    //var gender = 2;
                    //if (item.ElementAtOrDefault(4)?.ToString() == "Nam")
                    //{
                    //    gender = 1;
                    //}
                    //else if (item.ElementAtOrDefault(4)?.ToString() == "Nữ")
                    //{
                    //    gender = 0;
                    //}
                    //var allowPhone = 0;
                    //if (item.ElementAtOrDefault(17)?.ToString() == "Có")
                    //{
                    //    allowPhone = 1;
                    //}

                    //var dto = new IC_CustomerImportDTO
                    //{
                    //    Timestamp = timesTamp,
                    //    EmployeeATID = item.ElementAtOrDefault(1)?.ToString() ?? "",
                    //    EmployeeCode = "",
                    //    CardNumber = "",
                    //    PositionName = "",
                    //    FullName = item.ElementAtOrDefault(2)?.ToString() ?? "",
                    //    NameOnMachine = item.ElementAtOrDefault(3)?.ToString() ?? "",
                    //    Gender = gender,
                    //    DateOfBirth = item.ElementAtOrDefault(5)?.ToString() ?? "",
                    //    Nric = item.ElementAtOrDefault(6)?.ToString() ?? "",
                    //    Company = item.ElementAtOrDefault(7)?.ToString() ?? "",
                    //    PhoneNumber = item.ElementAtOrDefault(8)?.ToString() ?? "",
                    //    Email = item.ElementAtOrDefault(9)?.ToString() ?? "",
                    //    Address = item.ElementAtOrDefault(10)?.ToString() ?? "",
                    //    ContactDepartment = item.ElementAtOrDefault(11)?.ToString() ?? "",
                    //    ContactPerson = item.ElementAtOrDefault(12)?.ToString() ?? "",
                    //    JoinedDate = item.ElementAtOrDefault(13)?.ToString() ?? "",
                    //    StoppedDate = item.ElementAtOrDefault(14)?.ToString() ?? "",
                    //    StartTime = item.ElementAtOrDefault(15)?.ToString() ?? "",
                    //    EndTime = item.ElementAtOrDefault(16)?.ToString() ?? "",
                    //    IsAllowPhone = allowPhone,
                    //    WorkingContent = item.ElementAtOrDefault(18)?.ToString() ?? "",
                    //    Note = item.ElementAtOrDefault(19)?.ToString() ?? "",
                    //    UpdateUser = item.ElementAtOrDefault(21)?.ToString() ?? "",
                    //};

                    //param.Add(dto);
                }

                var timestamp = _DbContext.HR_CustomerInfo.Select(x => x.Timestamp).Max();
                if (timestamp != null)
                {
                    //string formattedTimestamp = timestamp.Value.ToString("MM/dd/yyyy HH:mm:ss");
                    param = param.Where(x => x.Timestamp > timestamp.Value).ToList();
                }

                // validation data
                List<IC_CustomerImportDTO> listError = new List<IC_CustomerImportDTO>();
                if (param != null && param.Count > 0)
                {
                    var countNotHaveID = param.Count(x => string.IsNullOrWhiteSpace(x.EmployeeATID));
                    if (countNotHaveID > 0)
                    {
                        var customerIdExisted = await _userService.GetAllEmployeeATID();
                        if (_usingRandomCustomerID != null && _usingRandomCustomerID.Count > 0)
                        {
                            customerIdExisted.AddRange(_usingRandomCustomerID);
                        }
                        randomIds = GenerateUniqueNumberStrings(countNotHaveID, _config.MaxLenghtEmployeeATID,
                            _config.AutoGenerateCustomerIDPrefix, customerIdExisted);
                        _usingRandomCustomerID.AddRange(randomIds);
                        var notHaveIdParam = param.Where(x => string.IsNullOrWhiteSpace(x.EmployeeATID)).ToList();
                        for (var i = 0; i < countNotHaveID; i++)
                        {
                            notHaveIdParam[i].EmployeeATID = randomIds[i];
                        }
                    }
                }
                var userType = 2;
                listError = await ValidationImportCustomerFromGoogleSheet(param, userType);
                var message = "";
                string sWebRootFolder = _hostingEnvironment.ContentRootPath;
                FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, @"Files/ImportCustomerByGoogleSheetError.xlsx"));

                if (listError != null && listError.Count() > 0)
                {
                    var listEmployeeIDError = listError.Select(e => e.EmployeeATID).ToList();
                    param = param.Where(e => !listEmployeeIDError.Contains(e.EmployeeATID)).ToList();
                    message = listError.Count().ToString();

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("CustomerError");
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Mã người dùng";
                        worksheet.Cell(currentRow, 2).Value = "Họ tên (*)";
                        worksheet.Cell(currentRow, 3).Value = "Tên trên máy";
                        worksheet.Cell(currentRow, 4).Value = "Giới tính";
                        worksheet.Cell(currentRow, 5).Value = "Ngày sinh (ngày/tháng/năm) (*)";
                        worksheet.Cell(currentRow, 6).Value = "CMND/CCCD/Passport (*)";
                        worksheet.Cell(currentRow, 7).Value = "Tên công ty";
                        worksheet.Cell(currentRow, 8).Value = "Số điện thoại";
                        worksheet.Cell(currentRow, 9).Value = "Email";
                        worksheet.Cell(currentRow, 10).Value = "Địa chỉ";
                        worksheet.Cell(currentRow, 11).Value = "Phòng ban liên hệ (*)";
                        worksheet.Cell(currentRow, 12).Value = "Người liên hệ (*)";
                        worksheet.Cell(currentRow, 13).Value = "Từ ngày (*)";
                        worksheet.Cell(currentRow, 14).Value = "Đến ngày (*)";
                        worksheet.Cell(currentRow, 15).Value = "Từ giờ (*)";
                        worksheet.Cell(currentRow, 16).Value = "Đến giờ (*)";
                        worksheet.Cell(currentRow, 17).Value = "Sử dụng điện thoại";
                        worksheet.Cell(currentRow, 18).Value = "Nội dung làm việc";
                        worksheet.Cell(currentRow, 19).Value = "Ghi chú";
                        worksheet.Cell(currentRow, 20).Value = "Lỗi";



                        for (int i = 1; i < 21; i++)
                        {
                            worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.Yellow;
                            worksheet.Cell(1, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Column(i).Width = 20;
                        }

                        foreach (var users in listError)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = users.EmployeeATID;
                            worksheet.Cell(currentRow, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "0".PadLeft(users.EmployeeATID.Length, '0');


                            worksheet.Cell(currentRow, 2).Value = users.FullName;
                            worksheet.Cell(currentRow, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 3).Value = users.NameOnMachine;
                            worksheet.Cell(currentRow, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = users.Gender == 1 ? "Nam" : users.Gender == 0 ? "Nữ" : users.Gender == 2 ? "Khác" : "";
                            worksheet.Cell(currentRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 5).Value = users.DateOfBirth;
                            worksheet.Cell(currentRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 6).Value = users.Nric;
                            worksheet.Cell(currentRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 7).Value = users.Company;
                            worksheet.Cell(currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 8).Value = users.PhoneNumber;
                            worksheet.Cell(currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 9).Value = users.Email;
                            worksheet.Cell(currentRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 10).Value = users.Address;
                            worksheet.Cell(currentRow, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 11).Value = users.ContactDepartment;
                            worksheet.Cell(currentRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 12).Value = "'" + users.ContactPerson;
                            worksheet.Cell(currentRow, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 13).Value = users.JoinedDate;
                            worksheet.Cell(currentRow, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 14).Value = users.StoppedDate;
                            worksheet.Cell(currentRow, 14).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 15).Value = users.StartTime;
                            worksheet.Cell(currentRow, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 16).Value = users.EndTime;
                            worksheet.Cell(currentRow, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 17).Value = users.IsAllowPhone == 1 ? "x" : "";
                            worksheet.Cell(currentRow, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 18).Value = users.WorkingContent;
                            worksheet.Cell(currentRow, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 19).Value = users.Note;
                            worksheet.Cell(currentRow, 19).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            worksheet.Cell(currentRow, 20).Value = "Lỗi";
                            worksheet.Cell(currentRow, 20).Value = users.ErrorMessage;
                            worksheet.Cell(currentRow, 20).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        }

                        workbook.SaveAs(file.FullName);
                    }
                }

                var listEmployeeID = param.Select(e => e.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0')).ToList();
                var listEmployeeDB = new List<HR_User>();
                var listCustomerInfoDB = new List<HR_CustomerInfo>();
                var listUserMasterDB = new List<IC_UserMaster>();
                var listCardNumberDB = new List<HR_CardNumberInfo>();
                var listWorkingInfoDB = new List<IC_WorkingInfo>();

                if (listEmployeeID.Count > 5000)
                {
                    var listSplitEmployeeID = CommonUtils.SplitList(listEmployeeID, 5000);
                    foreach (var listEmployeeSplit in listSplitEmployeeID)
                    {
                        var resultEmployee = _DbContext.HR_User.Where(e => e.CompanyIndex == companyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCustomerInfo = _DbContext.HR_CustomerInfo.Where(e => e.CompanyIndex == companyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultUserMaster = _DbContext.IC_UserMaster.Where(e => e.CompanyIndex == companyIndex && listEmployeeSplit.Contains(e.EmployeeATID)).ToList();
                        var resultCardNumber = _DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == companyIndex && e.IsActive == true).ToList();

                        listEmployeeDB.AddRange(resultEmployee);
                        listCustomerInfoDB.AddRange(resultCustomerInfo);
                        listUserMasterDB.AddRange(resultUserMaster);
                        listCardNumberDB.AddRange(resultCardNumber);
                        if (userType == (int)EmployeeType.Contractor)
                        {
                            var listWorkingInfo = _DbContext.IC_WorkingInfo.Where(e => e.CompanyIndex == companyIndex && listEmployeeID.Contains(e.EmployeeATID)
                                && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date
                                && (e.ToDate == null || (e.ToDate.HasValue && e.ToDate.Value.Date > DateTime.Now.Date))).OrderByDescending(e => e.FromDate).ToList();
                            listWorkingInfoDB.AddRange(listWorkingInfo);
                        }

                    }
                }
                else
                {
                    listEmployeeDB = _DbContext.HR_User.Where(e => e.CompanyIndex == companyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCustomerInfoDB = _DbContext.HR_CustomerInfo.Where(e => e.CompanyIndex == companyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listUserMasterDB = _DbContext.IC_UserMaster.Where(e => e.CompanyIndex == companyIndex && listEmployeeID.Contains(e.EmployeeATID)).ToList();
                    listCardNumberDB = _DbContext.HR_CardNumberInfo.Where(e => e.CompanyIndex == companyIndex && e.IsActive == true).ToList();
                    if (userType == (int)EmployeeType.Contractor)
                    {
                        listWorkingInfoDB = _DbContext.IC_WorkingInfo.Where(e => e.CompanyIndex == companyIndex && listEmployeeID.Contains(e.EmployeeATID)
                            && e.Status == (short)TransferStatus.Approve && e.FromDate.Date <= DateTime.Now.Date
                            && (e.ToDate == null || (e.ToDate.HasValue && e.ToDate.Value.Date > DateTime.Now.Date))).OrderByDescending(e => e.FromDate).ToList();
                    }
                }

                List<HR_User> listEmployee = new List<HR_User>();
                List<IC_UserMasterDTO> listUserMaster = new List<IC_UserMasterDTO>();

                var departmentContactList = _DbContext.IC_Department.Where(e => param.Select(x => x.ContactDepartment).Contains(e.Name)).ToList();
                foreach (var item in param)
                {
                    try
                    {
                        item.EmployeeATID = item.EmployeeATID.PadLeft(config.MaxLenghtEmployeeATID, '0');

                        var existedEmployee = listEmployeeDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedEmployee != null)
                        {
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.EmployeeType = userType;
                            existedEmployee.UpdatedUser = item.UpdateUser;
                            existedEmployee.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedEmployee.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedEmployee.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            existedEmployee.EmployeeType = userType;
                            _DbContext.HR_User.Update(existedEmployee);
                        }
                        else
                        {
                            existedEmployee = new HR_User();
                            existedEmployee.CompanyIndex = companyIndex;
                            existedEmployee.EmployeeATID = item.EmployeeATID;
                            existedEmployee.EmployeeCode = item.EmployeeCode;
                            existedEmployee.FullName = item.FullName;
                            existedEmployee.Gender = (short)item.Gender;
                            existedEmployee.EmployeeType = userType;
                            existedEmployee.UpdatedDate = DateTime.Now;
                            existedEmployee.UpdatedUser = item.UpdateUser;
                            existedEmployee.CreatedDate = DateTime.Now;
                            existedEmployee.DayOfBirth = StringHelper.GetDateOfBirthEmployee("Day", item.DateOfBirth);
                            existedEmployee.MonthOfBirth = StringHelper.GetDateOfBirthEmployee("Month", item.DateOfBirth);
                            existedEmployee.YearOfBirth = StringHelper.GetDateOfBirthEmployee("Year", item.DateOfBirth);
                            _DbContext.HR_User.Add(existedEmployee);
                        }

                        string[] formats = { "dd/MM/yyyy" };
                        var joinedDate = new DateTime();
                        var endDate = new DateTime();
                        var now = DateTime.Now;

                        var convertFromDate = DateTime.TryParseExact(item.JoinedDate, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out joinedDate);

                        var convertEndDate = DateTime.TryParseExact(item.StoppedDate, formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out endDate);

                        var startTime = new DateTime();
                        var endTime = new DateTime();
                        var convertStartTime = DateTime.TryParse(item.StartTime, out startTime);
                        var convertEndTime = DateTime.TryParse(item.EndTime, out endTime);

                        var student = listCustomerInfoDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (student != null)
                        {
                            student.Company = item.Company;
                            student.WorkingContent = item.WorkingContent;
                            student.CompanyIndex = companyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = item.UpdateUser;
                            student.NRIC = item.Nric;
                            student.Address = item.Address;
                            student.Email = item.Email;
                            student.Phone = item.PhoneNumber;
                            student.ContactPerson = item.ContactPerson;
                            student.ContactDepartment = item.ContactDepartmentIndex.ToString();
                            student.IsAllowPhone = item.IsAllowPhone == 1 ? true : false;
                            student.Timestamp = item.Timestamp;
                            student.ContactPersonPhoneNumber = item.ContactPersonPhoneNumber;
                            _DbContext.HR_CustomerInfo.Update(student);
                        }
                        else
                        {

                            student = new HR_CustomerInfo();
                            student.EmployeeATID = item.EmployeeATID;
                            student.NRIC = item.Nric;
                            student.Address = item.Address;
                            student.Email = item.Email;
                            student.Phone = item.PhoneNumber;
                            student.ContactPerson = item.ContactPerson;
                            student.ContactDepartment = item.ContactDepartmentIndex.ToString();
                            student.IsAllowPhone = item.IsAllowPhone == 1 ? true : false;
                            student.Note = item.Note;
                            student.CompanyIndex = companyIndex;
                            student.UpdatedDate = DateTime.Now;
                            student.UpdatedUser = item.UpdateUser;
                            student.FromTime = new DateTime(joinedDate.Year, joinedDate.Month, joinedDate.Day, startTime.Hour, startTime.Minute, startTime.Second);
                            student.ToTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endTime.Hour, endTime.Minute, endTime.Second);
                            student.StudentOfParent = item.StudentOfParent;
                            student.Company = item.Company;
                            student.WorkingContent = item.WorkingContent;
                            student.Timestamp = item.Timestamp;
                            student.ContactPersonPhoneNumber = item.ContactPersonPhoneNumber;

                            _DbContext.HR_CustomerInfo.Add(student);
                        }

                        var listOtherCardNumberByEmpID = listCardNumberDB.Where(x => x.EmployeeATID == item.EmployeeATID
                            && x.CardNumber != item.CardNumber).ToList();
                        if (listOtherCardNumberByEmpID.Count > 0)
                        {
                            foreach (var card in listOtherCardNumberByEmpID)
                            {
                                card.IsActive = false;
                                card.UpdatedDate = DateTime.Now;
                                _DbContext.HR_CardNumberInfo.Update(card);
                            }
                        }

                        var existedCardNumber = listCardNumberDB.FirstOrDefault(e => e.CardNumber == item.CardNumber);
                        if (existedCardNumber != null)
                        {
                            if (existedEmployee.EmployeeATID == item.EmployeeATID)
                            {

                            }
                            else
                            {

                            }

                        }
                        else
                        {
                            existedCardNumber = new HR_CardNumberInfo();
                            existedCardNumber.EmployeeATID = existedEmployee.EmployeeATID;
                            existedCardNumber.CompanyIndex = existedEmployee.CompanyIndex;
                            existedCardNumber.CardNumber = item.CardNumber;
                            existedCardNumber.IsActive = true;
                            existedCardNumber.CreatedDate = DateTime.Now;
                            existedCardNumber.UpdatedDate = existedEmployee.UpdatedDate;
                            existedCardNumber.UpdatedUser = existedEmployee.UpdatedUser;
                            _DbContext.HR_CardNumberInfo.Add(existedCardNumber);
                        }


                        var existedUserMaster = listUserMasterDB.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                        if (existedUserMaster == null)
                        {
                            existedUserMaster = new IC_UserMaster();
                            existedUserMaster.EmployeeATID = existedEmployee.EmployeeATID;
                            existedUserMaster.CompanyIndex = existedEmployee.CompanyIndex;
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.AuthenMode = AuthenMode.FullAccessRight.ToString();
                            existedUserMaster.Privilege = GlobalParams.DevicePrivilege.SDKStandardRole;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.CreatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = item.UpdateUser;
                            _DbContext.IC_UserMaster.Add(existedUserMaster);
                        }
                        else
                        {
                            existedUserMaster.NameOnMachine = item.NameOnMachine;
                            existedUserMaster.CardNumber = item.CardNumber;
                            existedUserMaster.UpdatedDate = DateTime.Now;
                            existedUserMaster.UpdatedUser = item.UpdateUser;
                            _DbContext.IC_UserMaster.Update(existedUserMaster);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    try
                    {
                        _DbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                    }



                    // Add audit log
                    IC_AuditEntryDTO audit = new IC_AuditEntryDTO(null);
                    audit.TableName = "HR_CustomerInfo";
                    audit.UserName = item.UpdateUser;
                    audit.CompanyIndex = companyIndex;
                    audit.State = AuditType.Added;
                    //audit.Description = AuditType.Added.ToString() + " Import From Excel " + listEmployeeID.Count().ToString() + " HR_CustomerInfo";
                    audit.Description = AuditType.Added.ToString() + "CustomerFromExcel:/:" + listEmployeeID.Count().ToString();
                    audit.DateTime = DateTime.Now;
                    _IIC_AuditLogic.Create(audit);

                }

                var employeeATIDs = param.Select(x => x.EmployeeATID).ToList();
                if(employeeATIDs != null && employeeATIDs.Count > 0)
                {
                    await _IC_VehicleLogService.IntegrateEmployeeToLovad(employeeATIDs);
                    await _IHR_EmployeeLogic.IntegrateUserToOfflineCustomer(employeeATIDs);

                    //Add employee in department AC
                    var employeeInfoList = await _userService.GetEmployeeCompactInfoByEmployeeATID(employeeATIDs, DateTime.Now, companyIndex);
                    var employeeInDepartment = _DbContext.AC_DepartmentAccessedGroup.Where(x => employeeInfoList.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                    if (employeeInDepartment != null && employeeInDepartment.Count > 0)
                    {
                        string userName = "SYSTEM_AUTO";
                        //var employeeAccessGr = employeeInfoList.Where(x => employeeInDepartment.Select(z => z.DepartmentIndex).Contains(x.DepartmentIndex)).ToList();
                        var user = new UserInfo(userName)
                        { CompanyIndex = companyIndex, UserName = userName, FullName = "" };
                        foreach (var departmentAcc in employeeInDepartment)
                        {
                            var listUserAcc = employeeInfoList.Where(x => x.DepartmentIndex == departmentAcc.DepartmentIndex).Select(x => x.EmployeeATID).ToList();

                            await _iC_CommandService.UploadTimeZone(departmentAcc.GroupIndex, user);
                            await _iC_CommandService.UploadUsers(departmentAcc.GroupIndex, listUserAcc, user);
                            await _iC_CommandService.UploadACUsers(departmentAcc.GroupIndex, listUserAcc, user);
                        }
                    }
                }
            
               

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ImportDataFromGoogleSheet" + ex.ToString());
                return false;
            }
            finally
            {
                if (randomIds != null && randomIds.Count > 0 && _usingRandomCustomerID.Any(y
                    => randomIds.Contains(y)))
                {
                    _usingRandomCustomerID.RemoveAll(x => randomIds.Contains(x));
                }
            }

        }
    }
}
