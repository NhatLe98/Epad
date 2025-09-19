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
    public class TA_RulesGlobalService : BaseServices<TA_Rules_Global, EPAD_Context>, ITA_RulesGlobalService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public TA_RulesGlobalService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_RulesGlobalService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<TA_Rules_Global> GetRulesGlobal(UserInfo user)
        {
            return await _dbContext.TA_Rules_Global.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);
        }

        public async Task<bool> UpdateRulesGlobal(TA_Rules_Global param, UserInfo user)
        {
            var result = true;
            try
            {
                param.CompanyIndex = user.CompanyIndex;

                var data = await _dbContext.TA_Rules_Global.FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex);
                if (data != null)
                {
                    data.LockAttendanceTime = param.LockAttendanceTime;
                    data.MaximumAnnualLeaveRegisterByMonth = param.MaximumAnnualLeaveRegisterByMonth;
                    data.OverTimeHoliday = param.OverTimeHoliday;
                    data.OverTimeLeaveDay = param.OverTimeLeaveDay;
                    data.OverTimeNormalDay = param.OverTimeNormalDay;
                    data.NightOverTimeHoliday = param.NightOverTimeHoliday;
                    data.NightOverTimeLeaveDay = param.NightOverTimeLeaveDay;
                    data.NightOverTimeNormalDay = param.NightOverTimeNormalDay;
                    data.NightShiftStartTime = param.NightShiftStartTime;
                    data.NightShiftEndTime = param.NightShiftEndTime;
                    data.NightShiftOvernightEndTime = param.NightShiftOvernightEndTime;
                    data.IsAutoCalculateAttendance = param.IsAutoCalculateAttendance;
                    data.TimePos = param.TimePos;
                    _dbContext.TA_Rules_Global.Update(data);
                }
                else
                {
                    await _dbContext.TA_Rules_Global.AddAsync(param);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
