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
using Microsoft.CodeAnalysis;

namespace EPAD_Services.Impl
{
    public class TA_LocationByDepartmentService : BaseServices<TA_LocationByDepartment, EPAD_Context>, ITA_LocationByDepartmentService
    {
        public EPAD_Context _dbContext;
        private ILogger _logger;
        public TA_LocationByDepartmentService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _logger = loggerFactory.CreateLogger<TA_LocationByDepartment>();
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
            var listData = (from location in _dbContext.TA_ListLocation
                            join deptLoc in _dbContext.TA_LocationByDepartment
                            on location.Index equals deptLoc.LocationIndex
                            join department in _dbContext.IC_Department
                            on deptLoc.DepartmentIndex equals department.Index
                            select new TA_LocationByDepartmentDTO
                            {
                                DepartmentList = new List<int> { deptLoc.DepartmentIndex },
                                DepartmentIndex = department.Index,
                                LocationIndex = deptLoc.LocationIndex,
                                Index = deptLoc.Index,
                                DepartmentIndexDTO = deptLoc.Index,
                                DepartmentName = department.Name,
                                LocationName = location.LocationName,
                                Address = location.Address,
                            }).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                listData = listData.Where(x => filterBy.Contains(x.DepartmentName) || filter.Contains(x.DepartmentName)).ToList();
            }

            countPage = listData.Count();
            dataGrid = new DataGridClass(countPage, listData);

            if (pPage <= 1)
            {
                var lstDepartment = listData.OrderBy(t => t.DepartmentName).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstDepartment);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstDepartment = listData.OrderBy(t => t.DepartmentName).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstDepartment);
            }
            return dataGrid;
        }
        public async Task<string> AddListLocationByDepartment(TA_LocationByDepartmentDTO data, UserInfo user)
        {
            string message = string.Empty;
            var departments = await _dbContext.IC_Department.Where(d => data.DepartmentList.Contains(d.Index)).ToListAsync();
            var location = await _dbContext.TA_ListLocation.FirstOrDefaultAsync(l => l.Index == data.LocationIndex);
            var existingDepartments = await _dbContext.TA_LocationByDepartment.Where(dD => data.DepartmentList.Contains(dD.DepartmentIndex)).ToListAsync();
            var newDepartments = new List<TA_LocationByDepartment>();
            foreach (var departmentIndex in data.DepartmentList)
            {
                var department = departments.FirstOrDefault(d => d.Index == departmentIndex);
                var duplicateDepartments = existingDepartments.Any(dD => dD.DepartmentIndex == departmentIndex);
                if (duplicateDepartments)
                {
                    message += "<p>  - " + department.Name + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                    continue;
                }
                if (location != null)
                {
                    newDepartments.Add(new TA_LocationByDepartment
                    {
                        DepartmentIndex = department.Index,
                        LocationIndex = location.Index,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                        CreatedDate = DateTime.Now
                    });
                }
            }
            await _dbContext.TA_LocationByDepartment.AddRangeAsync(newDepartments);
            await _dbContext.SaveChangesAsync();
            return message;
        }
        public async Task<bool> UpdateLocationByDepartment(TA_LocationByDepartmentDTO data, UserInfo user)
        {
            try
            {
                var existingDepartment = await _dbContext.TA_LocationByDepartment.FirstOrDefaultAsync(d => d.DepartmentIndex == data.DepartmentIndex);
                if (existingDepartment == null)
                {
                    return false;
                }
                else
                {
                    _dbContext.TA_LocationByDepartment.RemoveRange(existingDepartment);
                    var newLocationByDepartment = new TA_LocationByDepartment
                    {
                        DepartmentIndex = data.DepartmentIndex,
                        LocationIndex = data.LocationIndex,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedUser = user.UserName,
                        UpdatedDate = DateTime.Now,
                    };
                    _dbContext.TA_LocationByDepartment.Add(newLocationByDepartment);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateDepartmentFail: {ex}");
                return false;
            }
        }
        public bool DeleteListLocationByDepartment(List<int> listIndex)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var deleteDepartments = _dbContext.TA_LocationByDepartment.Where(x => listIndex.Contains(x.Index)).ToList();
                    if (deleteDepartments.Any())
                    {
                        _dbContext.TA_LocationByDepartment.RemoveRange(deleteDepartments);
                        _dbContext.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteDepartmentFail: ", ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public async Task<TA_LocationByDepartment> GetLocationByDepartmentByIndex(int index)
        {
            return await _dbContext.TA_LocationByDepartment.FirstOrDefaultAsync(x => x.Index == index);
        }
    }
}
