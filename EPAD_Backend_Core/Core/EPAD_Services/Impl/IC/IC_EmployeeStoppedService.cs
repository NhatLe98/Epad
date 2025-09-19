using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_EmployeeStoppedService : BaseServices<IC_EmployeeStopped, EPAD_Context>, IIC_EmployeeStoppedService
    {
        public EPAD_Context _dbContext;
        private readonly ILogger _logger;
        public IC_EmployeeStoppedService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _logger = loggerFactory.CreateLogger<IC_EmployeeStoppedService>();
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            var query = (from emp in _dbContext.IC_EmployeeStopped
                         join user in _dbContext.HR_User on emp.EmployeeATID equals user.EmployeeATID
                         join work in _dbContext.IC_WorkingInfo on user.EmployeeATID equals work.EmployeeATID
                         join dept in _dbContext.IC_Department on work.DepartmentIndex equals dept.Index
                         where(emp.CompanyIndex == pCompanyIndex)
                         select new IC_EmployeeStoppedDTO
                         {
                             Index = emp.Index,
                             EmployeeATID = emp.EmployeeATID,
                             StoppedDate = emp.StoppedDate,
                             Reason = emp.Reason,
                             DepartmentName = dept.Name,
                             FullName = user.FullName,
                         }).ToList();
          
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            countPage = query.Count();
            dataGrid = new DataGridClass(countPage, query);
            if (pPage <= 1)
            {
                var lsEmployee = query.OrderBy(t => t.EmployeeATID).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsEmployee);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsEmployee = query.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsEmployee);
            }
            return dataGrid;
        }

        public async Task<string> AddEmployeeStopped(IC_EmployeeStoppedDTO ic_EmployeeStopped, UserInfo user)
        {
            var employeeATIDList = ic_EmployeeStopped.EmployeeATIDs;
            var employeeList = await _dbContext.IC_EmployeeStopped.Where(x => employeeATIDList.Contains(x.EmployeeATID)).ToListAsync();
            var userInfo = DbContext.HR_User.Where(u => u.EmployeeATID.Contains(u.EmployeeATID)).ToList();
            string message = string.Empty;
            var employeeStoppedList = new List<IC_EmployeeStopped>();
            foreach (var employeeATID in employeeATIDList)
            {
                DateTime stoppedDate;
                string[] formats = { "dd-MM-yyyy" };
                bool isValidStoppedDate = DateTime.TryParseExact(ic_EmployeeStopped.StoppedDateString, formats, null, System.Globalization.DateTimeStyles.None, out stoppedDate);
                var employeeExist = employeeList.Any(x => x.EmployeeATID == employeeATID);
                if (employeeExist)
                {
                    message += "<p>  - " + employeeATID + " " + userInfo.FirstOrDefault(u => u.EmployeeATID == employeeATID)?.FullName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    continue;
                }
                else
                {
                    var employeeStopped = new IC_EmployeeStopped();
                    employeeStopped.EmployeeATID = employeeATID;
                    employeeStopped.StoppedDate = stoppedDate;
                    employeeStopped.Reason = ic_EmployeeStopped.Reason;
                    employeeStopped.CompanyIndex = user.CompanyIndex;
                    employeeStopped.CreateDate = DateTime.Now;
                    employeeStopped.UpdatedUser = user.UserName;
                    employeeStopped.UpdatedDate = DateTime.Now;
                    employeeStoppedList.Add(employeeStopped);
                }
            }
            await _dbContext.IC_EmployeeStopped.AddRangeAsync(employeeStoppedList);
            await _dbContext.SaveChangesAsync();
            return message;
        }
        public async Task<bool> UpdateEmployeeStopped(IC_EmployeeStoppedDTO param, UserInfo user, IC_EmployeeStopped existingEmployee)
        {
            var result = true;
            try
            {
                DateTime stoppedDate;
                string[] formats = { "dd-MM-yyyy" };
                bool isDate = DateTime.TryParseExact(param.StoppedDateString, formats, null, System.Globalization.DateTimeStyles.None, out stoppedDate);
                if (isDate)
                {
                    existingEmployee.StoppedDate = stoppedDate;
                    existingEmployee.Reason = param.Reason;
                    _dbContext.IC_EmployeeStopped.Update(existingEmployee);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateFailed");
            }
            return result;
        }

        public async Task<IC_EmployeeStopped> GetEmployeeStoppedByIndex(int index, int companyIndex)
        {
            return await _dbContext.IC_EmployeeStopped.FirstOrDefaultAsync(x => x.Index == index && x.CompanyIndex == companyIndex);
        }
        public async Task<bool> DeleteEmployeeStopped(List<int> listIndex)
        {
            var result = true;
            try
            {
                var data = await _dbContext.IC_EmployeeStopped.Where(x => listIndex.Contains(x.Index)).ToListAsync();
                if (data.Count > 0)
                {
                    _dbContext.IC_EmployeeStopped.RemoveRange(data);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteFailed");
                return false;
            }
            return result;
        }
        public List<IC_EmployeeStoppedImportDTO> ValidationImportEmployeeStopped(List<IC_EmployeeStoppedImportDTO> param, UserInfo user)
        {
            var errorList = new List<IC_EmployeeStoppedImportDTO>();

            var lstEmployeeATIDs = param.Select(x => x.EmployeeATID).ToList();

            var employeeATIDs = _dbContext.HR_User
                .Where(x => lstEmployeeATIDs.Contains(x.EmployeeATID))
                .Select(x => x.EmployeeATID)
                .ToList();
            var employeeATIDNotExist = param
                .Where(x => !employeeATIDs.Contains(x.EmployeeATID))
                .ToList();
            if (employeeATIDNotExist != null && employeeATIDNotExist.Count > 0)
            {
                foreach (var item in employeeATIDNotExist)
                {
                    item.ErrorMessage += "- Mã chấm công không tồn tại\r\n";
                }
            }
            var duplicateEmployeeATIs = param
                .GroupBy(x => x.EmployeeATID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateEmployeeATIs.Any())
            {
                foreach (var item in param.Where(e => duplicateEmployeeATIs.Contains(e.EmployeeATID)))
                {
                    item.ErrorMessage += "- Trùng mã chấm công\r\n";
                    if (!errorList.Contains(item))
                    {
                        errorList.Add(item);
                    }
                }
            }
            foreach (var item in param)
            {
                if (item.StoppedDate == null)
                {
                    item.ErrorMessage += "- Ngày nghỉ không được để trống\r\n";
                }
                else
                {
                    DateTime stoppedDate;
                    string[] formats = { "dd/MM/yyyy" };
                    bool isValidDate = DateTime.TryParseExact(item.StoppedDate.ToString(), formats, null, System.Globalization.DateTimeStyles.None, out stoppedDate);
                    if (!isValidDate)
                    {
                        item.ErrorMessage += "- Ngày nghỉ không hợp lệ\r\n";
                    }
                }
                if (string.IsNullOrEmpty(item.Reason))
                {
                    item.ErrorMessage += "- Lý do không được để trống\r\n";
                }
            }

            errorList = param.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var dataSave = param.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var employeeATIDsSave = param.Select(x => x.EmployeeATID).ToList();
            if (dataSave != null && dataSave.Count() > 0)
            {
                var lstEmployeeStopped = _dbContext.IC_EmployeeStopped.Where(x => employeeATIDsSave.Contains(x.EmployeeATID)).ToList();
                foreach (var item in dataSave)
                {
                    DateTime stoppedDate;
                    string[] formats = { "dd/MM/yyyy" };
                    bool isDateParse = DateTime.TryParseExact(item.StoppedDate.ToString(), formats, null, System.Globalization.DateTimeStyles.None, out stoppedDate);

                    var existEmployeeStopped = lstEmployeeStopped.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    if (existEmployeeStopped != null)
                    {
                        existEmployeeStopped.EmployeeATID = item.EmployeeATID;
                        existEmployeeStopped.Reason = item.Reason;
                        existEmployeeStopped.StoppedDate = stoppedDate;
                        existEmployeeStopped.UpdatedDate = DateTime.Now;
                        existEmployeeStopped.UpdatedUser = user.FullName;
                        _dbContext.IC_EmployeeStopped.Update(existEmployeeStopped);
                    }
                    else
                    {
                        existEmployeeStopped = new IC_EmployeeStopped()
                        {
                            CompanyIndex = user.CompanyIndex,
                            EmployeeATID = item.EmployeeATID,
                            StoppedDate = stoppedDate,
                            Reason = item.Reason,
                            CreateDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        };

                        _dbContext.IC_EmployeeStopped.Add(existEmployeeStopped);
                    }
                }
                _dbContext.SaveChanges();
            }
            return errorList;
        }
    }
}
