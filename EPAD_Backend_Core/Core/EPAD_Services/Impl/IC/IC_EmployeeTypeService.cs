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
    public class IC_EmployeeTypeService : BaseServices<IC_EmployeeType, EPAD_Context>, IIC_EmployeeTypeService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public IC_EmployeeTypeService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<IC_EmployeeTypeService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<bool> AddEmployeeType(IC_EmployeeType param, UserInfo user)
        {
            var result = true;
            try
            {
                var employeeType = param;
                employeeType.Index = 0;
                employeeType.UpdatedDate = DateTime.Now;
                employeeType.UpdatedUser = user.UserName;
                employeeType.CompanyIndex = user.CompanyIndex;

                await _dbContext.IC_EmployeeType.AddAsync(employeeType);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddEmployeeType" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateEmployeeType(IC_EmployeeType param, UserInfo user)
        {
            var result = true;
            try
            {
                var employeeType = await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Index == param.Index);
                employeeType.Name = param.Name;
                employeeType.NameInEng = param.NameInEng;
                employeeType.Description = param.Description;
                employeeType.IsUsing = param.IsUsing;
                employeeType.UpdatedDate = DateTime.Now;
                employeeType.UpdatedUser = user.UserName;
                employeeType.CompanyIndex = user.CompanyIndex;
                _dbContext.IC_EmployeeType.Update(employeeType);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateEmployeeType" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteEmployeeTypes(List<int> indexes, UserInfo user)
        {
            var result = true;
            try
            {
                var deleteItems = await _dbContext.IC_EmployeeType.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (deleteItems != null)
                {
                    _dbContext.IC_EmployeeType.RemoveRange(deleteItems);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteEmployeeTypes" + ex);
                result = false;
            }
            return result;
        }

        public async Task<DataGridClass> GetDataByPage(int companyIndex, int page, string filter, int pageSize)
        {
            var dataQuery = _dbContext.IC_EmployeeType.Where(x => x.CompanyIndex == companyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                dataQuery = dataQuery.Where(x => x.Name.Contains(filter) || x.NameInEng.Contains(filter) || x.Code.Contains(filter));
            }
            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = dataQuery.Count();
            var output = await dataQuery.Skip(skip).Take(pageSize).ToListAsync();
            var grid = new DataGridClass(countTotal, output);
            return grid;
        }

        public async Task<IC_EmployeeType> GetDataByIndex(int index)
        {
            return await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<IC_EmployeeType>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _dbContext.IC_EmployeeType.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<List<IC_EmployeeType>> GetUsingEmployeeType(int companyIndex)
        {
            return await _dbContext.IC_EmployeeType.Where(x => x.IsUsing && x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<IC_EmployeeType> GetDataByNameAndCompanyIndex(string name, int companyIndex)
        {
            return await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Name == name && x.CompanyIndex == companyIndex);
        }
        public async Task<List<IC_EmployeeType>> GetDataByListNameAndCompanyIndex(List<string> name, int companyIndex)
        {
            return await _dbContext.IC_EmployeeType.Where(x 
                => name.Contains(x.Name) && x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<IC_EmployeeType> GetDataByCodeAndCompanyIndex(string code, int companyIndex)
        {
            return await _dbContext.IC_EmployeeType.FirstOrDefaultAsync(x => x.Code == code && x.CompanyIndex == companyIndex);
        }
    }
}
