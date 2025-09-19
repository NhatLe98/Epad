using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EPAD_Data.Models;
using static EPAD_Data.Models.CommonUtils;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Common.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EPAD_Logic
{
    public class IC_WorkingInfoLogic : IIC_WorkingInfoLogic
    {
        private EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private IHR_PositionInfoLogic _hR_PositionInfoLogic;
        private readonly ILogger _logger;

        public IC_WorkingInfoLogic(EPAD_Context dbContext, IMemoryCache pCache, IHR_PositionInfoLogic hR_PositionInfoLogic, ILogger<IC_WorkingInfoLogic> logger)
        {
            _dbContext = dbContext;
            _Cache = pCache;
            _Config = ConfigObject.GetConfig(_Cache);
            _hR_PositionInfoLogic = hR_PositionInfoLogic;
            _logger = logger;
        }
        public ListDTOModel<IC_WorkingInfoDTO> GetPage(List<AddedParam> addedParams)
        {
            var query = (from wi in _dbContext.IC_WorkingInfo
                         join emp in _dbContext.HR_User
                          on wi.EmployeeATID equals emp.EmployeeATID
                         join dpNew in _dbContext.IC_Department
                            on wi.DepartmentIndex equals dpNew.Index
                         select new IC_WorkingInfoDTO
                         {
                             CompanyIndex = wi.CompanyIndex,
                             EmployeeATID = wi.EmployeeATID,
                             FromDate = wi.FromDate,
                             ToDate = wi.ToDate,
                             FullName = emp.FullName,
                             NewDepartmentName = dpNew.Name,
                             Status = wi.Status
                         }).AsQueryable();
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
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string filter = p.Value.ToString();
                                query = query.Where(u => u.FullName.Contains(filter)
                                        || u.NewDepartmentName.Contains(filter)
                                        || u.EmployeeATID.Contains(filter));
                            }
                            break;
                        case "Status":
                            if (p.Value != null)
                            {
                                short status = Convert.ToInt16(p.Value);
                                query = query.Where(u => u.Status == status);
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromDate.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.ToDate.HasValue ? u.ToDate.Value.Date <= toDate.Date : u.ToDate == null);
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);

            ListDTOModel<IC_WorkingInfoDTO> mv = new ListDTOModel<IC_WorkingInfoDTO>();
            mv.TotalCount = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var data = query.ToList();

            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }
        public ListDTOModel<IC_EmployeeTransferDTO> GetPageEmpTransfer(List<AddedParam> addedParams)
        {
            var query = (from empw in _dbContext.IC_WorkingInfo
                         join emp in _dbContext.HR_User
                          on empw.EmployeeATID equals emp.EmployeeATID
                         join dpm in _dbContext.IC_Department
                            on empw.DepartmentIndex equals dpm.Index into departmentJoin
                         from dpm in departmentJoin.DefaultIfEmpty()
                         where (empw.DepartmentIndex == 0 && dpm == null) || (empw.DepartmentIndex != 0 && dpm.Index != 0)
                         select new IC_EmployeeTransferDTO
                         {
                             WorkingInfoIndex = empw.Index,
                             CompanyIndex = empw.CompanyIndex,
                             EmployeeATID = empw.EmployeeATID,
                             FullName = emp.FullName,
                             IsFromTime = empw.FromDate.ToString("dd/MM/yyyy"),
                             IsToTime = empw.ToDate.HasValue ? empw.ToDate.Value.ToString("dd/MM/yyyy") : "",
                             FromTime = empw.FromDate,
                             ToTime = empw.ToDate,
                             Description = string.Empty,
                             NewDepartment = empw.DepartmentIndex,
                             NewDepartmentName = dpm.Name,
                             OldDepartment = null,
                             OldDepartmentName = string.Empty,
                             RemoveFromOldDepartmentName = string.Empty,
                             RemoveFromOldDepartment = null,
                             AddOnNewDepartment = null,
                             AddOnNewDepartmentName = null,
                             TypeTemporaryTransfer = "Lâu dài",
                             TransferApprovedDate = empw.ApprovedDate.HasValue ? empw.ApprovedDate.Value.ToString("dd/MM/yyyy") : "",
                             TransferApprovedUser = empw.ApprovedUser,
                             Status = empw.Status,
                             TransferApproveStatus = empw.Status == 0 ? "Chờ duyệt" : empw.Status == 1 ? "Đã duyệt" : "Từ chối",
                             TemporaryTransfer = false
                         });
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
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string searchcode = p.Value.ToString();
                                query = query.Where(u => u.Description.Contains(searchcode)
                                || u.EmployeeATID.Contains(searchcode)
                                || u.FullName.Contains(searchcode)
                                || u.NewDepartmentName.Contains(searchcode)
                                || (u.AddOnNewDepartment == true ? "Có" : "Không").Contains(searchcode)
                                || (u.RemoveFromOldDepartment == true ? "Có" : "Không").Contains(searchcode));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex.Equals(companyIndex));
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromTime.HasValue && u.FromTime.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromTime.HasValue && u.FromTime.Value.Date <= toDate.Date);
                            }
                            break;
                        case "IsPenddingApprove":
                            if (p.Value != null)
                            {
                                bool isPenddingApprove = Convert.ToBoolean(p.Value);
                                query = query.Where(u => isPenddingApprove == true ? u.Status == (short)TransferStatus.Pendding : u.Status != (short)TransferStatus.Pendding);
                            }
                            break;
                        case "ListDepartment":
                            if (p.Value != null)
                            {
                                IList<long> departments = (IList<long>)p.Value;
                                query = query.Where(u => departments.Contains(u.NewDepartment));
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);

            ListDTOModel<IC_EmployeeTransferDTO> mv = new ListDTOModel<IC_EmployeeTransferDTO>();
            mv.TotalCount = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var data = query.ToList();

            mv.PageIndex = pageIndex;
            mv.Data = data;
            return mv;
        }

        public async Task<Dictionary<string, Tuple<object, object, object>>> GetTransferLast7Days(UserInfo user)
        {
            var listEmployeeQuery = from u in _dbContext.HR_User.Where(x => !x.EmployeeType.HasValue || x.EmployeeType == 0
                                               || x.EmployeeType == (short)EmployeeType.Employee)
                                    join w in _dbContext.IC_WorkingInfo.AsNoTracking()
                                    .Where(x => x.Status == (short)TransferStatus.Approve
                                        && x.FromDate.Date <= DateTime.Now.Date)
                                    on u.EmployeeATID equals w.EmployeeATID
                                    //into userInfo
                                    //from userInfoResult in userInfo.DefaultIfEmpty()
                                    join d in _dbContext.IC_Department
                                    on w.DepartmentIndex equals d.Index into userDepInfo
                                    from userDepInfoResult in userDepInfo.DefaultIfEmpty()
                                    select new
                                    {
                                        EmployeeATID = u.EmployeeATID,
                                        EmployeeCode = u.EmployeeCode,
                                        FullName = u.FullName,
                                        FromDate = w != null ? (DateTime?)w.FromDate : null,
                                        ToDate = w != null ? w.ToDate : null,
                                        DepartmentIndex = w != null ? (long?)w.DepartmentIndex : null,
                                        DepartmentName = userDepInfoResult != null ? userDepInfoResult.Name : string.Empty,
                                        Date = w != null ? w.FromDate.Date.ToString("dd/MM/yyyy") : string.Empty,
                                        WorkingInfoIndex = w != null ? (int?)w.Index : null,
                                    };
            listEmployeeQuery = listEmployeeQuery.Where(x => !x.DepartmentIndex.HasValue || x.DepartmentIndex == 0
                || user.ListDepartmentAssigned.Contains(x.DepartmentIndex.Value));
            var listEmployee = listEmployeeQuery.ToList();

            var groupWorkingInfo = listEmployee.GroupBy(x => x.EmployeeATID).ToList();
            var listFirstWorkingInfoIndex = new List<int>();
            groupWorkingInfo.ForEach(x =>
            {
                var minIndexWorkingInfo = x.ToList().Where(x => x.WorkingInfoIndex.HasValue).OrderBy(x => x.FromDate).ToList();
                if (minIndexWorkingInfo != null && minIndexWorkingInfo.Count > 0)
                {
                    listFirstWorkingInfoIndex.Add(minIndexWorkingInfo.FirstOrDefault().WorkingInfoIndex.Value);
                }
            });

            var listEmployeePernamentTransfer = from wi in _dbContext.IC_WorkingInfo.AsNoTracking()
                                                .Where(x => x.Status == (short)TransferStatus.Approve
                                                && x.FromDate.Date <= DateTime.Now.Date && x.FromDate.Date >= DateTime.Now.Date.AddDays(-6)
                                                && !listFirstWorkingInfoIndex.Contains(x.Index))
                                                join u in _dbContext.HR_User.Where(x => !x.EmployeeType.HasValue || x.EmployeeType == 0
                                                || x.EmployeeType == (short)EmployeeType.Employee)
                                                on wi.EmployeeATID equals u.EmployeeATID
                                                //into userInfo
                                                //from userInfoResult in userInfo.DefaultIfEmpty()
                                                join d in _dbContext.IC_Department
                                                on wi.DepartmentIndex equals d.Index into userDep
                                                from userDepResult in userDep.DefaultIfEmpty()
                                                select new
                                                {
                                                    EmployeeATID = wi.EmployeeATID,
                                                    EmployeeCode = u.EmployeeCode,
                                                    FullName = u.FullName,
                                                    FromDate = wi.FromDate,
                                                    ToDate = wi.ToDate,
                                                    FromDateString = wi.FromDate.ToString("dd/MM/yyyy"),
                                                    ToDateString = wi.ToDate.HasValue ? wi.ToDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                                                    DepartmentIndex = wi.DepartmentIndex,
                                                    DepartmentTransfer = userDepResult.Name,
                                                    Date = wi.FromDate.Date.ToString("dd/MM"),
                                                    TransferType = "Pernament"
                                                };
            listEmployeePernamentTransfer = listEmployeePernamentTransfer.Where(x => x.DepartmentIndex == 0
                || user.ListDepartmentAssigned.Contains(x.DepartmentIndex));

            var listEmployeeTempTransfer = from wi in _dbContext.IC_EmployeeTransfer
                                                .Where(x => x.Status == (short)TransferStatus.Approve
                                                && x.FromTime.Date <= DateTime.Now.Date && x.FromTime.Date >= DateTime.Now.Date.AddDays(-6))
                                           join u in _dbContext.HR_User.Where(x => !x.EmployeeType.HasValue || x.EmployeeType == 0
                                           || x.EmployeeType == (short)EmployeeType.Employee)
                                           on wi.EmployeeATID equals u.EmployeeATID
                                           //into userInfo
                                           //from userInfoResult in userInfo.DefaultIfEmpty()
                                           join d in _dbContext.IC_Department
                                           on wi.NewDepartment equals d.Index into userDep
                                           from userDepResult in userDep.DefaultIfEmpty()
                                           select new
                                           {
                                               EmployeeATID = wi.EmployeeATID,
                                               EmployeeCode = u.EmployeeCode,
                                               FullName = u.FullName,
                                               FromDate = wi.FromTime,
                                               ToDate = wi.ToTime,
                                               FromDateString = wi.FromTime.ToString("dd/MM/yyyy"),
                                               ToDateString = wi.ToTime.ToString("dd/MM/yyyy"),
                                               DepartmentIndex = wi.NewDepartment,
                                               DepartmentTransfer = userDepResult.Name,
                                               Date = wi.FromTime.Date.ToString("dd/MM"),
                                               TransferType = "Temporary"
                                           };
            listEmployeeTempTransfer = listEmployeeTempTransfer.Where(x => x.DepartmentIndex == 0
                || user.ListDepartmentAssigned.Contains(x.DepartmentIndex));

            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-6);
            var list7Days = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                        .Select(offset => startDate.AddDays(offset)).ToList();

            var result = new Dictionary<string, Tuple<object, object, object>>();
            foreach (var date in list7Days)
            {
                var listEmployeeOfDate = listEmployee.Where(x => (!x.FromDate.HasValue
                    || (x.FromDate.HasValue && x.FromDate.Value.Date <= date.Date)) && (!x.ToDate.HasValue
                    || (x.ToDate.HasValue && x.ToDate.Value.Date > date.Date))).GroupBy(x => x.EmployeeATID).Select(x => x.FirstOrDefault()).ToList();

                var listTransfer = new List<object>();
                var listPernamentTransferOfDate = listEmployeePernamentTransfer.Where(x => x.FromDate.Date == date.Date).ToList();
                var listTemporaryTransferOfDate = listEmployeeTempTransfer.Where(x => x.FromDate.Date == date.Date).ToList();
                listTransfer.AddRange(listPernamentTransferOfDate);
                listTransfer.AddRange(listTemporaryTransferOfDate.Where(x
                    => !listPernamentTransferOfDate.Any(y => y.EmployeeATID == x.EmployeeATID)));

                var listNoTransfer = listEmployeeOfDate.Where(x => !listPernamentTransferOfDate.Any(y => y.EmployeeATID == x.EmployeeATID)
                    && !listTemporaryTransferOfDate.Any(y => y.EmployeeATID == x.EmployeeATID)).ToList();

                if (!result.ContainsKey(date.ToString("dd/MM/yyyy")))
                {
                    result.Add(date.ToString("dd/MM/yyyy"), new Tuple<object, object, object>(listEmployeeOfDate, listTransfer, listNoTransfer));
                }
            }

            return await Task.FromResult(result);
        }

        public List<IC_EmployeeTransferDTO> GetManyEmpTransfer(List<AddedParam> addedParams)
        {
            var query = (from empw in _dbContext.IC_WorkingInfo
                         join emp in _dbContext.HR_User
                          on empw.EmployeeATID equals emp.EmployeeATID
                         join dpm in _dbContext.IC_Department
                            on empw.DepartmentIndex equals dpm.Index
                         select new IC_EmployeeTransferDTO
                         {
                             WorkingInfoIndex = empw.Index,
                             CompanyIndex = empw.CompanyIndex,
                             EmployeeATID = empw.EmployeeATID,
                             FullName = emp.FullName,
                             IsFromTime = empw.FromDate.ToString("dd/MM/yyyy"),
                             IsToTime = empw.ToDate.HasValue ? empw.ToDate.Value.ToString("dd/MM/yyyy") : "",
                             FromTime = empw.FromDate,
                             ToTime = empw.ToDate,
                             Description = string.Empty,
                             NewDepartment = empw.DepartmentIndex,
                             NewDepartmentName = dpm.Name,
                             OldDepartment = null,
                             OldDepartmentName = string.Empty,
                             RemoveFromOldDepartmentName = string.Empty,
                             RemoveFromOldDepartment = null,
                             AddOnNewDepartment = null,
                             AddOnNewDepartmentName = null,
                             TypeTemporaryTransfer = "Lâu dài",
                             TransferApprovedDate = empw.ApprovedDate.HasValue ? empw.ApprovedDate.Value.ToString("dd/MM/yyyy") : "",
                             TransferApprovedUser = empw.ApprovedUser,
                             Status = empw.Status,
                             TransferApproveStatus = empw.Status == 0 ? "Chờ duyệt" : empw.Status == 1 ? "Đã duyệt" : "Từ chối",
                             TemporaryTransfer = false
                         });

            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                                string searchcode = p.Value.ToString();
                                query = query.Where(u => u.Description.Contains(searchcode)
                                || u.FullName.Contains(searchcode)
                                || u.NewDepartmentName.Contains(searchcode)
                                || (u.AddOnNewDepartment == true ? "Có" : "Không").Contains(searchcode)
                                || (u.RemoveFromOldDepartment == true ? "Có" : "Không").Contains(searchcode));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex.Equals(companyIndex));
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromTime.HasValue && u.FromTime.Value.Date >= fromDate.Date);
                            }
                            break;
                        case "ToDate":
                            if (p.Value != null)
                            {
                                DateTime toDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromTime.HasValue && u.FromTime.Value.Date <= toDate.Date);
                            }
                            break;
                        case "IsPenddingApprove":
                            if (p.Value != null)
                            {
                                bool isPenddingApprove = Convert.ToBoolean(p.Value);
                                query = query.Where(u => isPenddingApprove == true ? u.Status == (short)TransferStatus.Pendding : u.Status != (short)TransferStatus.Pendding);
                            }
                            break;
                        case "ListDepartment":
                            if (p.Value != null)
                            {
                                IList<long> departments = (IList<long>)p.Value;
                                query = query.Where(u => departments.Contains(u.NewDepartment));
                            }
                            break;
                        case "WorkingInfoIndexList":
                            if (p.Value != null)
                            {
                                var workingInfoIndexList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(p.Value.ToString());
                                if (workingInfoIndexList != null && workingInfoIndexList.Count > 0)
                                {
                                    query = query.Where(u => u.WorkingInfoIndex.HasValue && workingInfoIndexList.Contains(u.WorkingInfoIndex.Value));
                                }
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.FromTime);
            return query.ToList();
        }
        public IEnumerable<IC_WorkingInfoDTO> GetMany(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                            }
                            break;
                        case "TransferStatus":
                            if (p.Value != null)
                            {
                                int status = Convert.ToInt16(p.Value);
                                query = query.Where(u => u.Status == status);
                            }
                            break;
                        case "EmployeeATID":
                            if (p.Value != null)
                            {
                                string employeeATID = p.Value.ToString();
                                query = query.Where(u => u.EmployeeATID.Equals(employeeATID));
                            }
                            break;
                        case "DepartmentIndex":
                            if (p.Value != null)
                            {
                                int department = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.DepartmentIndex == department);
                            }
                            break;
                        case "ListEmployeeATID":
                            if (p.Value != null)
                            {
                                IList<string> listEmployeeATID = (IList<string>)p.Value;
                                query = query.Where(u => listEmployeeATID.Contains(u.EmployeeATID));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromDate.Date >= fromDate.Date);
                            }
                            break;
                        case "CheckExistFromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => (fromDate.Date <= u.FromDate.Date || fromDate.Date <= u.ToDate.Value.Date || u.DepartmentIndex == 0) && (u.Status == (short)TransferStatus.Approve || u.Status == (short)TransferStatus.Pendding));
                            }
                            break;
                        case "IsApprovedOrPending":
                            if (p.Value != null)
                            {
                                var isApproveOrPending = Convert.ToBoolean(p.Value);
                                if (isApproveOrPending)
                                {
                                    query = query.Where(u => u.Status == (short)TransferStatus.Approve || u.Status == (short)TransferStatus.Pendding);
                                }
                            }
                            break;
                        case "IsCurrentWorking":
                            if (p.Value != null)
                            {
                                query = query.Where(e => e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                && e.Status == (short)TransferStatus.Approve);
                            }
                            break;
                        case "IsCurrentWorkingAndNoDepartment":
                            if (p.Value != null)
                            {
                                query = query.Where(e => (e.FromDate.Date <= DateTime.Now.Date && (!e.ToDate.HasValue || e.ToDate.Value.Date >= DateTime.Now.Date)
                                && e.Status == (short)TransferStatus.Approve) || e.DepartmentIndex == 0);
                            }
                            break;

                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);
            var count = query.Count();
            var data = query.Select(u => new IC_WorkingInfoDTO
            {
                Index = u.Index,
                CompanyIndex = u.CompanyIndex,
                EmployeeATID = u.EmployeeATID,
                DepartmentIndex = u.DepartmentIndex,
                FromDate = u.FromDate,
                ToDate = u.ToDate,
                IsSync = u.IsSync,
                IsManager = u.IsManager,
                UpdatedDate = u.UpdatedDate,
                UpdatedUser = u.UpdatedUser,
                Status = u.Status,
                ApprovedDate = u.ApprovedDate,
                ApprovedUser = u.ApprovedUser,
            }).ToList();
            return data;
        }

        public IEnumerable<IC_WorkingInfo> GetList(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                        case "Filter":
                            if (p.Value != null)
                            {
                            }
                            break;
                        case "TransferStatus":
                            if (p.Value != null)
                            {
                                int status = Convert.ToInt16(p.Value);
                                query = query.Where(u => u.Status == status);
                            }
                            break;
                        case "EmployeeATID":
                            if (p.Value != null)
                            {
                                string employeeATID = p.Value.ToString();
                                query = query.Where(u => u.EmployeeATID.Equals(employeeATID));
                            }
                            break;
                        case "DepartmentIndex":
                            if (p.Value != null)
                            {
                                int department = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.DepartmentIndex == department);
                            }
                            break;
                        case "ListEmployeeATID":
                            if (p.Value != null)
                            {
                                IList<string> listEmployeeATID = (IList<string>)p.Value;
                                query = query.Where(u => listEmployeeATID.Contains(u.EmployeeATID));
                            }
                            break;
                        case "CompanyIndex":
                            if (p.Value != null)
                            {
                                int companyIndex = Convert.ToInt32(p.Value);
                                query = query.Where(u => u.CompanyIndex == companyIndex);
                            }
                            break;
                        case "FromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => u.FromDate.Date >= fromDate.Date);
                            }
                            break;
                        case "CheckExistFromDate":
                            if (p.Value != null)
                            {
                                DateTime fromDate = Convert.ToDateTime(p.Value);
                                query = query.Where(u => (fromDate.Date <= u.FromDate.Date || fromDate.Date <= u.ToDate.Value.Date || u.DepartmentIndex == 0)
                                    && (u.Status == (short)TransferStatus.Approve || u.Status == (short)TransferStatus.Pendding));
                            }
                            break;
                        case "IsCurrentWorking":
                            if (p.Value != null)
                            {
                                query = query.Where(u => u.FromDate.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                    && u.Status == (short)TransferStatus.Approve);
                            }
                            break;
                        case "IsCurrentWorkingAndNoDepartment":
                            if (p.Value != null)
                            {
                                query = query.Where(u => (u.FromDate.Date <= DateTime.Now.Date && (!u.ToDate.HasValue || u.ToDate.Value.Date >= DateTime.Now.Date)
                                    && u.Status == (short)TransferStatus.Approve) || u.DepartmentIndex == 0);
                            }
                            break;
                    }
                }
            }
            query = query.OrderBy(u => u.EmployeeATID);
            var data = query.ToList();
            return data;
        }

        public IC_WorkingInfoDTO Get(int id)
        {
            var query = _dbContext.IC_WorkingInfo.Where(u => u.Index == id);

            var data = query.FirstOrDefault();
            var result = (IC_WorkingInfoDTO)ConvertObject(data, new IC_WorkingInfoDTO());
            return result;
        }

        public IC_WorkingInfoDTO Get(List<AddedParam> addedParams)
        {
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            if (addedParams != null)
            {
                foreach (AddedParam p in addedParams)
                {
                    switch (p.Key)
                    {
                    }
                }
            }
            var data = query.FirstOrDefault();
            var result = (IC_WorkingInfoDTO)ConvertObject(data, new IC_WorkingInfoDTO());
            return result;
        }

        public IC_WorkingInfoDTO Create(IC_WorkingInfoDTO workingInfo)
        {
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            query = query.Where(u => u.CompanyIndex == workingInfo.CompanyIndex && u.EmployeeATID == workingInfo.EmployeeATID && u.Status == (short)TransferStatus.Approve);
            var data = query.FirstOrDefault();
            if (data == null)
            {
                data = new IC_WorkingInfo();
                data.EmployeeATID = workingInfo.EmployeeATID;
                data.CompanyIndex = workingInfo.CompanyIndex;
                data.Status = workingInfo.Status;
                data.FromDate = workingInfo.FromDate;
                data.ToDate = workingInfo.ToDate;
                data.IsManager = workingInfo.IsManager;
                data.IsSync = workingInfo.IsSync;
                _dbContext.Add(data);
                _dbContext.SaveChanges();
                return workingInfo;
            }

            return null;
        }
        public void CreateUnsaved(IC_WorkingInfo workingInfo)
        {
            _dbContext.IC_WorkingInfo.Add(workingInfo);
        }

        public void UpdateUnsaved(IC_WorkingInfo workingInfo)
        {
            _dbContext.IC_WorkingInfo.Update(workingInfo);
        }

        public IC_WorkingInfo CreateSave(IC_WorkingInfo workingInfo)
        {
            _dbContext.IC_WorkingInfo.Add(workingInfo);
            _dbContext.SaveChanges();
            return workingInfo;
        }

        public IC_WorkingInfo UpdateSave(IC_WorkingInfo workingInfo)
        {
            _dbContext.IC_WorkingInfo.Update(workingInfo);
            _dbContext.SaveChanges();
            return workingInfo;
        }

        public void CreateSave(List<IC_WorkingInfo> listWorkingInfo)
        {
            foreach (var workingInfo in listWorkingInfo)
            {
                _dbContext.IC_WorkingInfo.Add(workingInfo);
            }
            _dbContext.SaveChanges();
        }

        public void UpdateSave(List<IC_WorkingInfo> listWorkingInfo)
        {
            foreach (var workingInfo in listWorkingInfo)
            {
                _dbContext.IC_WorkingInfo.Update(workingInfo);
            }
            _dbContext.SaveChanges();
        }

        public IC_WorkingInfoDTO Update(List<AddedParam> addParams)
        {
            AddedParam addParam = addParams.FirstOrDefault(a => a.Key == "WorkingInfoIndex");
            if (addParam == null) return null;
            addParams.Remove(addParam);
            int workingInfoIndex = Convert.ToInt32(addParam.Value);
            IC_WorkingInfo dataItem = _dbContext.IC_WorkingInfo.FirstOrDefault(u => u.Index == workingInfoIndex);
            if (dataItem == null) return null;
            //update field
            foreach (AddedParam p in addParams)
            {
                switch (p.Key)
                {
                    case "IsSync":
                        bool isSync = false;
                        if (p.Value != null)
                        {
                            isSync = Convert.ToBoolean(p.Value);
                        }
                        dataItem.IsSync = isSync;
                        break;
                }
            }
            _dbContext.IC_WorkingInfo.Update(dataItem);
            _dbContext.SaveChanges();

            var item = (IC_WorkingInfoDTO)ConvertObject(dataItem, new IC_WorkingInfoDTO());
            return item;
        }

        public int Update(IC_WorkingInfoDTO objectInfo)
        {
            return 0;
        }

        public List<IC_WorkingInfoDTO> SaveList(IC_WorkingInfoDTO info)
        {
            return new List<IC_WorkingInfoDTO>();
        }

        public List<IC_WorkingInfo> UpdateList(List<IC_WorkingInfoDTO> listItem, List<AddedParam> addParams)
        {
            if (listItem != null && addParams != null)
            {
                var listWorkingIndex = listItem.Select(e => e.Index).ToHashSet();
                var listUpdate = _dbContext.IC_WorkingInfo.Where(u => listWorkingIndex.Contains(u.Index)).ToList();

                foreach (AddedParam p in addParams)
                {
                    switch (p.Key)
                    {
                        case "IsSync":
                            bool updateIsSync = Convert.ToBoolean(p.Value);
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.IsSync = updateIsSync;
                                }
                            }
                            break;
                        case "UpdatedDate":
                            DateTime updatedDate = Convert.ToDateTime(p.Value);
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.UpdatedDate = updatedDate;
                                }
                            }
                            break;
                        case "UpdatedUser":
                            string updateUser = p.Value.ToString();
                            foreach (var item in listItem)
                            {
                                var updateItem = listUpdate.FirstOrDefault(u => u.Index == item.Index);
                                if (updateItem != null)
                                {
                                    updateItem.UpdatedUser = updateUser;
                                }
                            }
                            break;
                    }
                }

                _dbContext.UpdateRange(listUpdate);
                _dbContext.SaveChanges();
                return listUpdate;
            }
            return null;
        }

        public int Delete(int id)
        {
            return 0;
        }

        public void ReBaseWorkingInfo(string employeeID, int companyID)
        {
            var listWorkingInfo = new List<IC_WorkingInfo>();
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            var data = query.Where(u => u.CompanyIndex == companyID && u.EmployeeATID == employeeID && u.Status == (short)TransferStatus.Approve)
                .OrderBy(u => u.FromDate).ToList();
            if (data != null)
            {
                //ReOrderWorkingInfo(listWorkingInfo);
                for (int i = 0; i < data.Count - 1; i++)
                {
                    if (data[i].ToDate < data[i + 1].FromDate.AddDays(-1))
                    {
                        data[i].ToDate = data[i].FromDate;
                    }
                    else {
                        data[i].ToDate = data[i + 1].FromDate.AddDays(-1);
                    }
                     
                    _dbContext.IC_WorkingInfo.Update(data[i]);
                }
            }
        }

        public void SaveChange()
        {
            _dbContext.SaveChanges();
        }

        public void ReBaseListWorkingInfo(List<IC_WorkingInfoDTO> listRebase)
        {
            var listEmployeeDTID = listRebase.Select(e => e.EmployeeATID).ToList();
            List<IC_WorkingInfo> listWorkingInfo = new List<IC_WorkingInfo>();
            var query = _dbContext.IC_WorkingInfo.AsQueryable();
            var data = query.Where(u => u.CompanyIndex == listRebase.First().CompanyIndex && listEmployeeDTID.Contains(u.EmployeeATID) && u.Status == (short)TransferStatus.Approve).OrderBy(u => u.FromDate).ToList();
            if (data != null)
            {
                foreach (var item in listRebase)
                {
                    var listWorking = data.Where(e => e.EmployeeATID == item.EmployeeATID).ToList();
                    if (listWorking != null && listWorking.Count() > 1)
                    {
                        for (int i = 0; i < listWorking.Count - 1; i++)
                        {
                            listWorking[i].ToDate = listWorking[i + 1].FromDate.AddDays(-1);
                            _dbContext.IC_WorkingInfo.Update(listWorking[i]);
                        }
                    }
                }
            }

            _dbContext.SaveChanges();
        }

        private void ReOrderWorkingInfo(List<IC_WorkingInfo> listWorkingInfo)
        {
            if (listWorkingInfo != null)
            {
                listWorkingInfo = listWorkingInfo.OrderBy(u => u.FromDate).ToList();
                for (int i = 0; i < listWorkingInfo.Count - 1; i++)
                {
                    var item = listWorkingInfo[i + 1];
                    listWorkingInfo[i].ToDate = item.FromDate.AddDays(-1);

                }
            }
        }

        public IC_WorkingInfoDTO CheckUpdateOrInsert(IC_WorkingInfoDTO request)
        {
            try
            {
                if (request == null)
                    return null;

                var addedParams = new List<AddedParam>
                {
                    new AddedParam { Key = "CompanyIndex", Value = request.CompanyIndex },
                    new AddedParam { Key = "IsCurrentWorking", Value = true },
                    new AddedParam { Key = "EmployeeATID", Value = request.EmployeeATID }
                };

                if (request.ToDate != null && request.ToDate < request.FromDate)
                {
                    request.FromDate = request.ToDate.Value;
                }

                if (request.ToDate != null)
                {
                    addedParams = new List<AddedParam>
                    {
                        new AddedParam { Key = "CompanyIndex", Value = request.CompanyIndex },
                        new AddedParam { Key = "EmployeeATID", Value = request.EmployeeATID }
                    };

                }
                var listWorkingInfo = GetList(addedParams).OrderByDescending(u => u.Index).ToList();
                if (listWorkingInfo.Any() && listWorkingInfo.Count() > 2)
                {
                    var row = 1;
                    foreach (var item in listWorkingInfo)
                    {
                        if (row > 2)
                        {
                            _dbContext.IC_WorkingInfo.Remove(item);
                        }
                        row++;
                    }
                    //Just keep 2 latest working info in each employee
                    _dbContext.SaveChanges();
                }
                var existed = listWorkingInfo.OrderByDescending(x => x.FromDate).FirstOrDefault();
                if (listWorkingInfo.Count > 0)
                {
                    var existedDepartment = listWorkingInfo.FirstOrDefault(x => x.DepartmentIndex == request.DepartmentIndex);
                    if (existedDepartment != null)
                    {
                        existed = existedDepartment;
                    }
                }

                if (existed == null)
                {
                    existed = new IC_WorkingInfo
                    {
                        EmployeeATID = request.EmployeeATID,
                        CompanyIndex = request.CompanyIndex,
                        DepartmentIndex = request.DepartmentIndex,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        IsManager = request.IsManager,
                        UpdatedUser = request.UpdatedUser,
                        Status = (short)TransferStatus.Approve,
                        IsSync = request.IsSync
                    };

                    var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(request.PositionName) ? "Nhân Viên" : request.PositionName };
                    existed.PositionIndex = _hR_PositionInfoLogic.CheckExistedOrCreate(employeePosition).Result;
                    _dbContext.IC_WorkingInfo.Add(existed);
                    request.IsInsert = true;
                }
                else if (existed.DepartmentIndex > 0 && existed.FromDate.Date < DateTime.Today.Date && existed.DepartmentIndex != request.DepartmentIndex)
                {
                    if (existed.FromDate == request.FromDate)
                    {
                        request.FromDate = DateTime.Now.Date;
                    }
                    existed = new IC_WorkingInfo
                    {
                        EmployeeATID = request.EmployeeATID,
                        CompanyIndex = request.CompanyIndex,
                        DepartmentIndex = request.DepartmentIndex,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        IsManager = request.IsManager,
                        UpdatedUser = request.UpdatedUser,
                        Status = request.Status
                    };

                    var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(request.PositionName) ? "Nhân Viên" : request.PositionName };
                    existed.PositionIndex = _hR_PositionInfoLogic.CheckExistedOrCreate(employeePosition).Result;
                    _dbContext.IC_WorkingInfo.Add(existed);
                    request.IsInsert = true;
                }
                else if (existed.DepartmentIndex == 0 && existed.DepartmentIndex != request.DepartmentIndex)
                {
                    existed.FromDate = DateTime.Today;
                    existed.ToDate = request.ToDate;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.DepartmentIndex = request.DepartmentIndex;
                    existed.IsManager = request.IsManager;
                    existed.ApprovedDate = DateTime.Today;
                    existed.ApprovedUser = request.ApprovedUser;
                    existed.UpdatedUser = request.UpdatedUser;
                    existed.UpdatedDate = DateTime.Today;

                    var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(request.PositionName) ? "Nhân Viên" : request.PositionName };
                    existed.PositionIndex = _hR_PositionInfoLogic.CheckExistedOrCreate(employeePosition).Result;
                    _dbContext.IC_WorkingInfo.Update(existed);
                    request.IsUpdate = true;
                }
                else if (existed.ToDate == null && request.ToDate != null)
                {
                    existed.ToDate = request.ToDate;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.UpdatedUser = request.UpdatedUser;
                    existed.UpdatedDate = DateTime.Today;

                    var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(request.PositionName) ? "Nhân Viên" : request.PositionName };
                    existed.PositionIndex = _hR_PositionInfoLogic.CheckExistedOrCreate(employeePosition).Result;
                    _dbContext.IC_WorkingInfo.Update(existed);
                    request.IsUpdate = true;
                }
                else
                {
                    var employeePosition = new HR_PositionInfoDTO { Name = string.IsNullOrEmpty(request.PositionName) ? "Nhân Viên" : request.PositionName };
                    existed.PositionIndex = _hR_PositionInfoLogic.CheckExistedOrCreate(employeePosition).Result;
                    _dbContext.IC_WorkingInfo.Update(existed);
                    request.IsUpdate = true;
                    request.IsNotChange = true;
                }
               
                if (request.IsInsert == true)
                {
                    _dbContext.SaveChanges();
                    ReBaseWorkingInfo(request.EmployeeATID, request.CompanyIndex);
                }
                _dbContext.SaveChanges();
                var returnResult = ConvertToDTO(existed);
                returnResult.IsUpdate = request.IsUpdate;
                returnResult.IsInsert = request.IsInsert;
                returnResult.IsNotChange = request.IsNotChange;
                return returnResult;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
            return null;
        }


        public IC_WorkingInfoDTO CheckUpdateOrInsertDto(IC_WorkingInfoDTO currentWorkingInfo, List<IC_WorkingInfo> listWorkingInfo, EPAD_Context db)
        {
            if (currentWorkingInfo == null)
                return null;

            if (currentWorkingInfo.ToDate != null)
            {
                var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID).ToList();
                if (listExisted.Count(x => x.ToDate == null) > 0)
                {
                    foreach (var item in listExisted)
                    {
                        item.ToDate = currentWorkingInfo.ToDate;
                        db.IC_WorkingInfo.Update(item);
                    }
                }
                else if (listExisted.Count(x => x.ToDate != null) == 0)
                {
                    var existed = new IC_WorkingInfo();
                    existed.EmployeeATID = currentWorkingInfo.EmployeeATID;
                    existed.CompanyIndex = currentWorkingInfo.CompanyIndex;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = currentWorkingInfo.ToDate;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.IsSync = currentWorkingInfo.IsSync;
                    db.IC_WorkingInfo.Add(existed);
                }
                return currentWorkingInfo;
            }
            else
            {
                var existed = listWorkingInfo.FirstOrDefault(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && (currentWorkingInfo.DepartmentIndex == x.DepartmentIndex || x.DepartmentIndex == 0));
                if (existed == null)
                {
                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }

                    existed = new IC_WorkingInfo();
                    existed.EmployeeATID = currentWorkingInfo.EmployeeATID;
                    existed.CompanyIndex = currentWorkingInfo.CompanyIndex;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = currentWorkingInfo.ToDate;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.IsSync = currentWorkingInfo.IsSync;
                    db.IC_WorkingInfo.Add(existed);
                }
                else if (existed.DepartmentIndex == 0 && existed.DepartmentIndex != currentWorkingInfo.DepartmentIndex)
                {
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = null;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.ApprovedDate = DateTime.Today;
                    existed.ApprovedUser = currentWorkingInfo.ApprovedUser;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.UpdatedDate = DateTime.Today;
                    db.IC_WorkingInfo.Update(existed);

                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && x.Index != existed.Index);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }
                }
                else
                {
                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && x.Index != existed.Index);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }

                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.UpdatedDate = DateTime.Today;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    db.IC_WorkingInfo.Update(existed);
                }
                return ConvertToDTO(existed);
            }
        }


        public IC_WorkingInfoDTO CheckUpdateOrInsertDtoAVN(IC_WorkingInfoDTO currentWorkingInfo, List<IC_WorkingInfo> listWorkingInfo, EPAD_Context db)
        {
            if (currentWorkingInfo == null)
                return null;

            if (currentWorkingInfo.ToDate != null)
            {
                var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID).ToList();
                if (listExisted.Count(x => x.ToDate == null) > 0)
                {
                    foreach (var item in listExisted)
                    {
                        item.ToDate = currentWorkingInfo.ToDate;
                        db.IC_WorkingInfo.Update(item);
                    }
                }
                else if (listExisted.Count(x => x.ToDate != null) == 0)
                {
                    var existed = new IC_WorkingInfo();
                    existed.EmployeeATID = currentWorkingInfo.EmployeeATID;
                    existed.CompanyIndex = currentWorkingInfo.CompanyIndex;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = currentWorkingInfo.ToDate;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.IsSync = currentWorkingInfo.IsSync;
                    db.IC_WorkingInfo.Add(existed);
                }
                return currentWorkingInfo;
            }
            else
            {
                var existed = listWorkingInfo.FirstOrDefault(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && (currentWorkingInfo.DepartmentIndex == x.DepartmentIndex || x.DepartmentIndex == 0));
                if (existed == null)
                {
                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }

                    existed = new IC_WorkingInfo();
                    existed.EmployeeATID = currentWorkingInfo.EmployeeATID;
                    existed.CompanyIndex = currentWorkingInfo.CompanyIndex;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = currentWorkingInfo.ToDate;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.IsSync = currentWorkingInfo.IsSync;
                    db.IC_WorkingInfo.Add(existed);
                }
                else if (existed.DepartmentIndex == 0 && existed.DepartmentIndex != currentWorkingInfo.DepartmentIndex)
                {
                    existed.FromDate = currentWorkingInfo.FromDate;
                    existed.ToDate = null;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.DepartmentIndex = currentWorkingInfo.DepartmentIndex;
                    existed.IsManager = currentWorkingInfo.IsManager;
                    existed.ApprovedDate = DateTime.Today;
                    existed.ApprovedUser = currentWorkingInfo.ApprovedUser;
                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    existed.UpdatedDate = DateTime.Today;
                    db.IC_WorkingInfo.Update(existed);

                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && x.Index != existed.Index);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }
                }
                else
                {
                    var listExisted = listWorkingInfo.Where(x => x.EmployeeATID == currentWorkingInfo.EmployeeATID && x.ToDate == null && x.Index != existed.Index);
                    foreach (var item in listExisted)
                    {
                        item.ToDate = DateTime.Now;
                        db.IC_WorkingInfo.Update(item);
                    }

                    existed.UpdatedUser = currentWorkingInfo.UpdatedUser;
                    existed.UpdatedDate = DateTime.Today;
                    existed.PositionIndex = currentWorkingInfo.PositionIndex;
                    db.IC_WorkingInfo.Update(existed);
                }
                return ConvertToDTO(existed);
            }
        }

        public List<IC_WorkingInfoDTO> CheckUpdateOrInsertList(List<IC_WorkingInfoDTO> currentWorkingInfo)
        {

            if (currentWorkingInfo == null || currentWorkingInfo.Count() == 0)
                return null;

            List<AddedParam> addedParams = new List<AddedParam>();
            addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = currentWorkingInfo.First().CompanyIndex });
            addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            addedParams.Add(new AddedParam { Key = "ListEmployeeATID", Value = currentWorkingInfo.Select(e => e.EmployeeATID).ToList() });

            List<IC_WorkingInfo> listWorkingInfo = GetList(addedParams).OrderByDescending(u => u.FromDate).ToList();

            foreach (var item in currentWorkingInfo)
            {
                var existed = listWorkingInfo.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);

                if (existed == null)
                {
                    existed = new IC_WorkingInfo();
                    existed.EmployeeATID = item.EmployeeATID;
                    existed.DepartmentIndex = item.DepartmentIndex;
                    existed.CompanyIndex = item.CompanyIndex;
                    existed.FromDate = item.FromDate;
                    existed.IsManager = item.IsManager;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.UpdatedDate = item.UpdatedDate;
                    existed.UpdatedUser = item.UpdatedUser;
                    existed.ApprovedDate = item.ApprovedDate;
                    existed.ApprovedUser = item.ApprovedUser;
                    _dbContext.IC_WorkingInfo.Add(existed);
                }
                else if (existed.DepartmentIndex == 0 && existed.DepartmentIndex != item.DepartmentIndex)
                {
                    existed.ToDate = null;
                    existed.DepartmentIndex = item.DepartmentIndex;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.UpdatedDate = item.UpdatedDate;
                    existed.UpdatedUser = item.UpdatedUser;
                    _dbContext.IC_WorkingInfo.Update(existed);
                }
                else if (existed.DepartmentIndex > 0 && existed.FromDate.Date < DateTime.Now.Date && existed.DepartmentIndex != item.DepartmentIndex)
                {
                    existed = new IC_WorkingInfo();
                    existed.EmployeeATID = item.EmployeeATID;
                    existed.DepartmentIndex = item.DepartmentIndex;
                    existed.CompanyIndex = item.CompanyIndex;
                    existed.FromDate = item.FromDate;
                    existed.IsManager = item.IsManager;
                    existed.Status = (short)TransferStatus.Approve;
                    existed.UpdatedDate = item.UpdatedDate;
                    existed.UpdatedUser = item.UpdatedUser;
                    existed.ApprovedDate = item.ApprovedDate;
                    existed.ApprovedUser = item.ApprovedUser;
                    _dbContext.IC_WorkingInfo.Add(existed);
                }
            }
            _dbContext.SaveChanges();
            ReBaseListWorkingInfo(currentWorkingInfo);

            return currentWorkingInfo;
        }
        public IC_WorkingInfoDTO CheckExistedOrCreate(IC_WorkingInfoDTO workingInfo)
        {
            //List<AddedParam> addedParams = new List<AddedParam>();
            //addedParams.Add(new AddedParam { Key = "CompanyIndex", Value = currentWorkingInfo.CompanyIndex });
            //addedParams.Add(new AddedParam { Key = "IsCurrentWorking", Value = true });
            //addedParams.Add(new AddedParam { Key = "EmployeeATID", Value = currentWorkingInfo.EmployeeATID });
            //List<IC_WorkingInfoDTO> listWorkingInfo = GetMany(addedParams).OrderByDescending(u => u.FromDate).ToList();

            var existed = _dbContext.IC_WorkingInfo.FirstOrDefault(e => (e.EmployeeATID == workingInfo.EmployeeATID || e.EmployeeATID == workingInfo.EmployeeATID.TrimStart(new Char[] { '0' })
                || e.EmployeeATID == workingInfo.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')) && e.CompanyIndex == workingInfo.CompanyIndex && e.Status != (short)TransferStatus.Reject);
            if (existed == null || !(existed.EmployeeATID.EndsWith(workingInfo.EmployeeATID) && existed.EmployeeATID.Replace(workingInfo.EmployeeATID, "0").All(x => x == '0')))
            {
                existed = new IC_WorkingInfo();
                existed.EmployeeATID = workingInfo.EmployeeATID;
                existed.CompanyIndex = workingInfo.CompanyIndex;
                existed.DepartmentIndex = workingInfo.DepartmentIndex;
                existed.FromDate = DateTime.Today;
                existed.IsManager = workingInfo.IsManager;
                existed.ApprovedDate = DateTime.Now;
                existed.ApprovedUser = workingInfo.UpdatedUser;
                existed.UpdatedDate = DateTime.Now;
                existed.UpdatedUser = workingInfo.UpdatedUser;
                existed.Status = (short)TransferStatus.Approve;
                _dbContext.IC_WorkingInfo.Add(existed);
            }
            _dbContext.SaveChanges();
            //ReBaseWorkingInfo(existed.EmployeeATID,existed.CompanyIndex);
            workingInfo = ConvertToDTO(existed);
            return workingInfo;
        }

        public async Task<List<IC_WorkingInfoDTO>> CheckExistedOrCreateList(List<IC_WorkingInfoDTO> listWorkingInfo)
        {
            if (listWorkingInfo == null || listWorkingInfo.Count() == 0)
                return null;

            List<IC_WorkingInfoDTO> listNoExistedWorkingInfo = new List<IC_WorkingInfoDTO>();
            var listEmployeeID = listWorkingInfo.Select(e => e.EmployeeATID).ToList();
            var listPadLeftEmployeeID = listWorkingInfo.Select(e => e.EmployeeATID.PadLeft(_Config.MaxLenghtEmployeeATID, '0')).ToList();
            var listTrimEmployeeID = listWorkingInfo.Select(e => e.EmployeeATID.TrimStart(new Char[] { '0' })).ToList();
            listEmployeeID.AddRange(listPadLeftEmployeeID);
            listEmployeeID.AddRange(listTrimEmployeeID);
            var listexisted = await _dbContext.IC_WorkingInfo.Where(e => listEmployeeID.Contains(e.EmployeeATID) && e.CompanyIndex == listWorkingInfo.First().CompanyIndex && e.Status != (short)TransferStatus.Reject).ToListAsync();
            foreach (var item in listWorkingInfo)
            {
                var existed = listexisted.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID);
                if (existed == null || !(existed.EmployeeATID.EndsWith(item.EmployeeATID) && existed.EmployeeATID.Replace(item.EmployeeATID, "0").All(x => x == '0')))
                {
                    existed = new IC_WorkingInfo();
                    existed.EmployeeATID = item.EmployeeATID;
                    existed.CompanyIndex = item.CompanyIndex;
                    existed.DepartmentIndex = item.DepartmentIndex;
                    existed.IsManager = item.IsManager;
                    existed.IsSync = item.IsSync;
                    existed.FromDate = DateTime.Today;
                    existed.ApprovedDate = DateTime.Now;
                    existed.ApprovedUser = item.UpdatedUser;
                    existed.UpdatedDate = DateTime.Now;
                    existed.UpdatedUser = item.UpdatedUser;
                    existed.Status = (short)TransferStatus.Approve;
                    await _dbContext.IC_WorkingInfo.AddAsync(existed);
                    listNoExistedWorkingInfo.Add(ConvertToDTO(existed));

                }
            }
            await _dbContext.SaveChangesAsync();
            return listNoExistedWorkingInfo;
        }

        private void ConvertToData(IC_WorkingInfoDTO dto, IC_WorkingInfo data)
        {
            data.Index = dto.Index;
            data.EmployeeATID = dto.EmployeeATID;
            data.CompanyIndex = dto.CompanyIndex;
            data.DepartmentIndex = dto.DepartmentIndex;
            data.FromDate = dto.FromDate;
            data.ToDate = dto.ToDate;
            data.IsManager = dto.IsManager;
            data.IsSync = dto.IsSync;
            data.Status = dto.Status;

            data.ApprovedDate = dto.ApprovedDate;
            data.ApprovedUser = dto.ApprovedUser;
            data.UpdatedDate = dto.UpdatedDate;
            data.UpdatedUser = dto.UpdatedUser;
        }

        private IC_WorkingInfoDTO ConvertToDTO(IC_WorkingInfo data)
        {
            var dto = new IC_WorkingInfoDTO
            {
                Index = data.Index,
                EmployeeATID = data.EmployeeATID,
                CompanyIndex = data.CompanyIndex,
                DepartmentIndex = data.DepartmentIndex,
                FromDate = data.FromDate,
                ToDate = data.ToDate,
                IsManager = data.IsManager,
                IsSync = data.IsSync,
                Status = data.Status,
                ApprovedDate = data.ApprovedDate,
                ApprovedUser = data.ApprovedUser,
                UpdatedDate = data.UpdatedDate,
                UpdatedUser = data.UpdatedUser
            };
            return dto;
        }

    }
    public interface IIC_WorkingInfoLogic
    {
        Task<List<IC_WorkingInfoDTO>> CheckExistedOrCreateList(List<IC_WorkingInfoDTO> listWorkingInfo);
        IC_WorkingInfoDTO CheckExistedOrCreate(IC_WorkingInfoDTO workingInfo);
        IC_WorkingInfoDTO CheckUpdateOrInsert(IC_WorkingInfoDTO currentWorkingInfo);
        List<IC_WorkingInfoDTO> CheckUpdateOrInsertList(List<IC_WorkingInfoDTO> currentWorkingInfo);
        List<IC_WorkingInfo> UpdateList(List<IC_WorkingInfoDTO> listWorkingInfo, List<AddedParam> addParams);
        void ReBaseWorkingInfo(string employeeID, int companyID);
        ListDTOModel<IC_WorkingInfoDTO> GetPage(List<AddedParam> addedParams);
        ListDTOModel<IC_EmployeeTransferDTO> GetPageEmpTransfer(List<AddedParam> addedParams);
        Task<Dictionary<string, Tuple<object, object, object>>> GetTransferLast7Days(UserInfo user);
        List<IC_EmployeeTransferDTO> GetManyEmpTransfer(List<AddedParam> addedParams);
        IEnumerable<IC_WorkingInfoDTO> GetMany(List<AddedParam> addedParams);
        IC_WorkingInfoDTO Get(int id);
        IC_WorkingInfoDTO Get(List<AddedParam> addedParams);
        IC_WorkingInfoDTO Update(List<AddedParam> addParams);
        int Update(IC_WorkingInfoDTO objectInfo);
        IC_WorkingInfoDTO Create(IC_WorkingInfoDTO objectInfo);
        List<IC_WorkingInfoDTO> SaveList(IC_WorkingInfoDTO info);
        int Delete(int id);
        IC_WorkingInfoDTO CheckUpdateOrInsertDto(IC_WorkingInfoDTO currentWorkingInfo, List<IC_WorkingInfo> listWorkingInfo, EPAD_Context db);
        IEnumerable<IC_WorkingInfo> GetList(List<AddedParam> addedParams);
        void SaveChange();
        IC_WorkingInfoDTO CheckUpdateOrInsertDtoAVN(IC_WorkingInfoDTO currentWorkingInfo, List<IC_WorkingInfo> listWorkingInfo, EPAD_Context db);
        void CreateUnsaved(IC_WorkingInfo workingInfo);
        void UpdateUnsaved(IC_WorkingInfo workingInfo);
        IC_WorkingInfo CreateSave(IC_WorkingInfo workingInfo);
        IC_WorkingInfo UpdateSave(IC_WorkingInfo workingInfo);
        void CreateSave(List<IC_WorkingInfo> listWorkingInfo);
        void UpdateSave(List<IC_WorkingInfo> listWorkingInfo);
    }
}
