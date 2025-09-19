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
    public class TA_HolidayService : BaseServices<TA_Holiday, EPAD_Context>, ITA_HolidayService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public TA_HolidayService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_HolidayService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            IEnumerable<object> dep;

            List<TA_Holiday> areaList = null;
            areaList = await DbContext.TA_Holiday.AsNoTracking().Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrWhiteSpace(filter) && (t.Name.Contains(filter) 
             || t.Code.Contains(filter)) 
             || string.IsNullOrWhiteSpace(filter))).ToListAsync();
            var data = areaList.Select(x => new TA_HolidayDTO
            { 
                Index = x.Index,
                Name = x.Name,
                Code = x.Code,
                HolidayDate = x.HolidayDate,
                HolidayDateString = x.HolidayDate.ToddMMyyyy(),
                IsPaidWhenNotWorking = x.IsPaidWhenNotWorking,
                IsRepeatAnnually = x.IsRepeatAnnually,
                CompanyIndex = x.CompanyIndex,
                UpdatedDate = x.UpdatedDate,
                UpdatedUser = x.UpdatedUser
            }).ToList();
            dep = from groupdevice in data
                  orderby groupdevice.Name
                  select new
                  {
                      value = groupdevice.Index.ToString(),
                      label = groupdevice.Name
                  };
            countPage = data.Count();
            dataGrid = new DataGridClass(countPage, data);
            if (pPage <= 1)
            {
                var lsDevice = data.Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = data.Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

        public async Task<List<TA_Holiday>> GetAllHoliday(int companyIndex)
        {
            return await _dbContext.TA_Holiday.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<TA_Holiday> GetHolidayByIndex(int index)
        {
            return await _dbContext.TA_Holiday.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<TA_Holiday>> GetHolidayByName(string name, int companyIndex)
        {
            return await _dbContext.TA_Holiday.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Name == name).ToListAsync();
        }

        public async Task<List<TA_Holiday>> GetHolidayByCode(string code, int companyIndex)
        {
            return await _dbContext.TA_Holiday.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Code == code).ToListAsync();
        }

        public async Task<bool> AddHoliday(TA_Holiday data)
        {
            var result = true;
            try
            {
                await _dbContext.TA_Holiday.AddAsync(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateHoliday(TA_Holiday data)
        {
            var result = true;
            try
            {
                _dbContext.TA_Holiday.Update(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteHoliday(List<int> index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_Holiday.Where(x => index.Contains(x.Index)).ToListAsync();
                if (data.Count > 0)
                {
                    _dbContext.TA_Holiday.RemoveRange(data);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
