using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPAD_Data.Models;
using EPAD_Common.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace EPAD_Services.Impl
{

    public class TA_LocationByEmployeeService : BaseServices<TA_LocationByEmployee, EPAD_Context>, ITA_LocationByEmployeeService
    {
        public EPAD_Context _dbContext;
        private ILogger _logger;
        public TA_LocationByEmployeeService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _logger = loggerFactory.CreateLogger<TA_LocationByEmployee>();
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage;
            var filterBy = new List<string>();
            if (filter != null)
            {
                filterBy = filter.Split(" ").Select(x => x.ToLower()).ToList();
            }
            var listDataEmployee = (from emp in _dbContext.TA_LocationByEmployee
                                    join location in _dbContext.TA_ListLocation on emp.LocationIndex equals location.Index
                                    join hrUser in _dbContext.HR_User on emp.EmployeeATID equals hrUser.EmployeeATID
                                    join working in _dbContext.IC_WorkingInfo on hrUser.EmployeeATID equals working.EmployeeATID
                                    join department in _dbContext.IC_Department on working.DepartmentIndex equals department.Index
                                    select new TA_LocationByEmployeeDTO
                                    {
                                        EmployeeATIDs = new List<string> { emp.EmployeeATID },
                                        DepartmentList = new List<int> { department.Index },
                                        EmployeeATID = emp.EmployeeATID,
                                        Index = emp.Index,
                                        EmployeeIndexDTO = emp.Index,
                                        LocationIndex = emp.LocationIndex,
                                        DepartmentIndex = department.Index,
                                        DepartmentName = department.Name,
                                        LocationName = location.LocationName,
                                        FullName = hrUser.FullName,
                                        Address = location.Address,
                                    }).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                listDataEmployee = listDataEmployee.Where(x => filterBy.Contains(x.EmployeeATID) || filter.Contains(x.EmployeeATID)
                || (!string.IsNullOrWhiteSpace(x.FullName) && (filter.ToLower().Contains(x.FullName.ToLower())
                || filter.ToLower() == x.FullName.ToLower() || x.FullName.ToLower().Contains(filter.ToLower())))).ToList();
            }

            countPage = listDataEmployee.Count();
            dataGrid = new DataGridClass(countPage, listDataEmployee);

            if (pPage <= 1)
            {
                var lstEmployee = listDataEmployee.OrderBy(t => t.EmployeeATID).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstEmployee);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstEmployee = listDataEmployee.OrderBy(t => t.EmployeeATID).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstEmployee);
            }
            return dataGrid;
        }
        public async Task<string> AddListLocationByEmployee(TA_LocationByEmployeeDTO data, UserInfo user)
        {
            string message = string.Empty;
            var existingEmployees = await _dbContext.TA_LocationByEmployee.Where(x => data.EmployeeATIDs.Contains(x.EmployeeATID)).ToListAsync();
            var departments = await _dbContext.IC_Department.Where(d => data.DepartmentList.Contains(d.Index)).ToDictionaryAsync(d => d.Index);
            var location = await _dbContext.TA_ListLocation.FirstOrDefaultAsync(l => l.Index == data.LocationIndex);
            var employeeInfo = await _dbContext.HR_User.Where(u => data.EmployeeATIDs.Contains(u.EmployeeATID)).ToDictionaryAsync(u => u.EmployeeATID);
            var newEmployeeLocations = new List<TA_LocationByEmployee>();

            foreach (var employeeATID in data.EmployeeATIDs)
            {
                if (existingEmployees.Any(e => e.EmployeeATID == employeeATID))
                {
                    var employee = employeeInfo.ContainsKey(employeeATID) ? employeeInfo[employeeATID] : null;
                    message += $"<p>  - {employeeATID} {employee?.FullName}</p><p class=\"\" style=\"margin: 4px;\"></p>";
                    continue;
                }
                else
                {
                    newEmployeeLocations.Add(new TA_LocationByEmployee
                    {
                        EmployeeATID = employeeATID,
                        LocationIndex = location.Index,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                        CreatedDate = DateTime.Now,
                    });
                }
            }
            await _dbContext.TA_LocationByEmployee.AddRangeAsync(newEmployeeLocations);
            await _dbContext.SaveChangesAsync();
            return message;
        }
        public async Task<bool> UpdateLocationByEmployee(TA_LocationByEmployeeDTO data, UserInfo user)
        {
            try
            {
                var existingLocationByEmployee = await _dbContext.TA_LocationByEmployee.FirstOrDefaultAsync(x => x.EmployeeATID == data.EmployeeATID);
                if (existingLocationByEmployee == null)
                {
                    return false;
                }
                else
                {
                    _dbContext.TA_LocationByEmployee.RemoveRange(existingLocationByEmployee);
                    var newLocationByEmployee = new TA_LocationByEmployee
                    {
                        EmployeeATID = data.EmployeeATID,
                        LocationIndex = data.LocationIndex,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                        CreatedDate = DateTime.Now
                    };
                    _dbContext.TA_LocationByEmployee.Add(newLocationByEmployee);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateEmployeeFail: {ex}");
                return false;
            }
        }
        public bool DeleteListLocationByEmployee(List<int> listIndex)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var deleteEmployees = _dbContext.TA_LocationByEmployee.Where(x => listIndex.Contains(x.Index)).ToList();
                    if (deleteEmployees.Any())
                    {
                        _dbContext.TA_LocationByEmployee.RemoveRange(deleteEmployees);
                        _dbContext.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteEmployeeFail: {ex}");
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public async Task<TA_LocationByEmployee> GetLocationByEmployeeByIndex(int index)
        {
            return await _dbContext.TA_LocationByEmployee.FirstOrDefaultAsync(x => x.Index == index);
        }

        public List<TA_LocationByEmployeeImportExcel> ValidationImportLocationByEmployee(List<TA_LocationByEmployeeImportExcel> data, UserInfo user)
        {
            var errorList = new List<TA_LocationByEmployeeImportExcel>();
            var lstEmployeeATIDs = data.Select(x => x.EmployeeATID).ToList();
            var employeeData = _dbContext.HR_User.Where(x => lstEmployeeATIDs.Contains(x.EmployeeATID)).Select(x => new { x.EmployeeATID, x.EmployeeCode }).ToList();
            var employeeATIDs = employeeData.Select(x => x.EmployeeATID).ToList();
            var employeeATIDNotExist = data.Where(x => !employeeATIDs.Contains(x.EmployeeATID)).ToList();
            foreach (var item in employeeATIDNotExist)
            {
                item.ErrorMessage += "Mã chấm công không tồn tại\r\n";
            }
            var checkDuplicateEmployee = data.GroupBy(x => x.EmployeeATID).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            var duplicate = data.Where(e => checkDuplicateEmployee.Contains(e.EmployeeATID)).ToList();
            foreach (var item in duplicate)
            {
                item.ErrorMessage += "Trùng mã chấm công\r\n";
            }
            foreach (var item in data)
            {
                item.EmployeeCode = employeeData.FirstOrDefault(e => e.EmployeeATID == item.EmployeeATID)?.EmployeeCode;

                if (string.IsNullOrEmpty(item.LocationName))
                {
                    item.ErrorMessage += "- Địa điểm không được để trống\r\n";
                }
            }
            var duplicateLocationPerEmployee = data.GroupBy(x => new { x.EmployeeATID, x.LocationName }).Where(g => g.Count() > 1).Select(g => new { g.Key.EmployeeATID, g.Key.LocationName }).ToList();
            foreach (var item in data)
            {
                if (duplicateLocationPerEmployee.Any(dl => dl.EmployeeATID == item.EmployeeATID && dl.LocationName == item.LocationName))
                {
                    item.ErrorMessage += "- Trùng địa điểm\r\n";
                }
            }

            var locationNames = data.Select(x => x.LocationName).ToList();
            var locations = _dbContext.TA_ListLocation.Where(x => locationNames.Contains(x.LocationName)).Select(x => new { x.LocationName, x.Index }).ToList();
            foreach (var item in data)
            {
                var location = locations.FirstOrDefault(x => x.LocationName == item.LocationName);
                if (location != null)
                {
                    item.LocationIndex = location.Index;
                }
                else
                {
                    item.ErrorMessage += "- Địa điểm không tồn tại\r\n";
                }
            }

            errorList = data.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var dataSave = data.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();
            var employeeATIDsSave = dataSave.Select(x => x.EmployeeATID).ToList();

            if (dataSave.Any())
            {
                var lstLocationByEmployee = _dbContext.TA_LocationByEmployee
                    .Where(x => employeeATIDsSave.Contains(x.EmployeeATID))
                    .ToList();

                foreach (var item in dataSave)
                {
                    var existEmployee = lstLocationByEmployee.FirstOrDefault(x => x.EmployeeATID == item.EmployeeATID);
                    if (existEmployee != null)
                    {
                        existEmployee.EmployeeATID = item.EmployeeATID;
                        existEmployee.LocationIndex = item.LocationIndex;
                        existEmployee.UpdatedDate = DateTime.Now;
                        existEmployee.UpdatedUser = user.FullName;
                        _dbContext.TA_LocationByEmployee.Update(existEmployee);
                    }
                    else
                    {
                        var newEmployee = new TA_LocationByEmployee()
                        {
                            EmployeeATID = item.EmployeeATID,
                            LocationIndex = item.LocationIndex,
                            CompanyIndex = user.CompanyIndex,
                            CreatedDate = DateTime.Now,
                            UpdatedUser = user.FullName
                        };
                        _dbContext.TA_LocationByEmployee.Add(newEmployee);
                    }
                }
                _dbContext.SaveChanges();
            }
            return errorList;
        }
    }
}
