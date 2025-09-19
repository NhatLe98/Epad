using EPAD_Common.Services;
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
    public class GC_AreaGroup_GroupDeviceService : BaseServices<GC_AreaGroup_GroupDevice, EPAD_Context>, IGC_AreaGroup_GroupDeviceService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public GC_AreaGroup_GroupDeviceService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_AreaGroup_GroupDeviceService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task DeleteByAreaGroupIndex(int areaGroupIndex, int companyIndex)
        {
            var deleteItems = await _dbContext.GC_AreaGroup_GroupDevice.Where(e 
                => e.AreaGroupIndex == areaGroupIndex && e.CompanyIndex == companyIndex).ToListAsync();
            if (deleteItems != null && deleteItems.Count > 0)
            {
                _dbContext.GC_AreaGroup_GroupDevice.RemoveRange(deleteItems);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<GC_AreaGroup_GroupDevice>> GetDataByAreaGroupIndexAndCompanyIndex(int areaGroupIndex, int companyIndex)
        {
            return await _dbContext.GC_AreaGroup_GroupDevice.Where(x 
                => x.CompanyIndex == companyIndex && x.AreaGroupIndex == areaGroupIndex).ToListAsync();
        }

        public async Task<List<GC_AreaGroup_GroupDevice>> GetDataByListAreaGroupIndexAndCompanyIndex(List<int> areaGroupIndex, int companyIndex)
        {
            return await _dbContext.GC_AreaGroup_GroupDevice.Where(x
                => x.CompanyIndex == companyIndex && areaGroupIndex.Contains(x.AreaGroupIndex)).ToListAsync();
        }
    }
}
