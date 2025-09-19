using EPAD_Common.Enums;
using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using EPAD_Services.Plugins;
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
using static EPAD_Common.Utility.AppUtils;

namespace EPAD_Services.Impl
{
    public class TA_LeaveDateTypeService : BaseServices<TA_LeaveDateType, EPAD_Context>, ITA_LeaveDateTypeService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public TA_LeaveDateTypeService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_LeaveDateTypeService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            IEnumerable<object> dep;

            List<TA_LeaveDateType> areaList = null;
            areaList = await DbContext.TA_LeaveDateType.AsNoTracking().Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrWhiteSpace(filter) && (t.Name.Contains(filter) || t.Code.Contains(filter)) || string.IsNullOrWhiteSpace(filter))).ToListAsync();
            dep = from groupdevice in areaList
                  orderby groupdevice.Name
                  select new
                  {
                      value = groupdevice.Index.ToString(),
                      label = groupdevice.Name
                  };
            countPage = areaList.Count();
            dataGrid = new DataGridClass(countPage, areaList);
            if (pPage <= 1)
            {
                var lsDevice = areaList.Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = areaList.Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

        public async Task<List<TA_LeaveDateType>> GetAllLeaveDateType(int companyIndex)
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<TA_LeaveDateType> GetLeaveDateTypeByIndex(int index)
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<TA_LeaveDateType>> GetLeaveDateTypeByName(string name, int companyIndex)
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Name == name).ToListAsync();
        }

        public async Task<List<TA_LeaveDateType>> GetLeaveDateTypeByCode(string code, int companyIndex)
        {
            return await _dbContext.TA_LeaveDateType.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Code == code).ToListAsync();
        }

        public async Task<bool> AddLeaveDateType(TA_LeaveDateType data)
        {
            var result = true;
            try
            {
                await _dbContext.TA_LeaveDateType.AddAsync(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateLeaveDateType(TA_LeaveDateType data)
        {
            var result = true;
            try
            {
                _dbContext.TA_LeaveDateType.Update(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteLeaveDateType(List<int> index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_LeaveDateType.Where(x => index.Contains(x.Index)).ToListAsync();
                if (data.Count > 0)
                {
                    _dbContext.TA_LeaveDateType.RemoveRange(data);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> IsLeaveDateTypeUsing(List<int> index)
        {
            return await _dbContext.TA_LeaveRegistration.AnyAsync(x => index.Contains(x.LeaveDateType));
        }
    }
}
