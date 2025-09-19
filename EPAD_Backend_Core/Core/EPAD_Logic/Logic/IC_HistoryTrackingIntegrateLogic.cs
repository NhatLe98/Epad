using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EPAD_Logic
{
    public class IC_HistoryTrackingIntegrateLogic : IIC_HistoryTrackingIntegrateLogic
    {
        public EPAD_Context _dbContext;
        private readonly ILogger _logger;
        private static IMemoryCache _cache;
        private ConfigObject _config;

        public IC_HistoryTrackingIntegrateLogic(EPAD_Context context, IMemoryCache cache)
        {
            _dbContext = context;
            _cache = cache;
            _config = ConfigObject.GetConfig(_cache);
        }

        public async Task AddHistoryTrackingIntegrate(IC_HistoryTrackingIntegrate iC_HistoryTrackingIntegrate)
        {
            try
            {
                var historyData = new IC_HistoryTrackingIntegrate()
                {
                    JobName = iC_HistoryTrackingIntegrate.JobName,
                    RunTime = DateTime.Now,
                    DataNew = iC_HistoryTrackingIntegrate.DataNew,
                    DataUpdate = iC_HistoryTrackingIntegrate.DataUpdate,
                    DataDelete = iC_HistoryTrackingIntegrate.DataDelete,
                    CompanyIndex = _config.CompanyIndex
                };
                await _dbContext.IC_HistoryTrackingIntegrate.AddAsync(historyData);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
            }
        }
    }
    public interface IIC_HistoryTrackingIntegrateLogic
    {
        Task AddHistoryTrackingIntegrate(IC_HistoryTrackingIntegrate iC_HistoryTrackingIntegrate);
    }
}

