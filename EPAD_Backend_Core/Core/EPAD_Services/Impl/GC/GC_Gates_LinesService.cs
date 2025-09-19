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
    public class GC_Gates_LinesService : BaseServices<GC_Gates_Lines, EPAD_Context>, IGC_Gates_LinesService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public GC_Gates_LinesService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_Gates_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<GC_Gates_Lines>> GetDataByGateIndexAndCompanyIndex(int gateIndex, int companyIndex)
        {
            return await _dbContext.GC_Gates_Lines.Where(x => x.CompanyIndex == companyIndex && x.GateIndex == gateIndex).ToListAsync();
        }

        public async Task<List<GC_Gates_Lines>> GetDataByListGateIndexAndCompanyIndex(List<int> gateIndex, int companyIndex)
        {
            return await _dbContext.GC_Gates_Lines.Where(x 
                => x.CompanyIndex == companyIndex && gateIndex.Contains(x.GateIndex)).ToListAsync();
        }

        public async Task<List<GC_Gates>> GetAllGates(int companyIndex)
        {
            return await _dbContext.GC_Gates.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<List<GC_Gates_Lines>> GetAllGatesLines(int companyIndex)
        {
            return await _dbContext.GC_Gates_Lines.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<bool> UpdateGateLine(GateLineParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var gateLineData = await _dbContext.GC_Gates_Lines.Where(x
                    => x.CompanyIndex == user.CompanyIndex && x.GateIndex == param.GateIndex).ToListAsync();
                if (gateLineData != null && gateLineData.Count > 0)
                {
                    _dbContext.GC_Gates_Lines.RemoveRange(gateLineData);
                }
                var now = DateTime.Now;
                for (int i = 0; i < param.ListLineIndex.Count; i++)
                {
                    var gateLine = new GC_Gates_Lines();
                    gateLine.GateIndex = param.GateIndex;
                    gateLine.LineIndex = param.ListLineIndex[i];
                    gateLine.CompanyIndex = user.CompanyIndex;
                    gateLine.UpdatedDate = now;
                    gateLine.UpdatedUser = user.FullName;

                    await _dbContext.GC_Gates_Lines.AddAsync(gateLine);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateGateLine" + ex);
                result = false;
            }
            return result;
        }
    }
}
