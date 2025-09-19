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
    public class TA_RulesShiftService : BaseServices<TA_Rules_Shift, EPAD_Context>, ITA_RulesShiftService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public TA_RulesShiftService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_RulesShiftService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<TA_Rules_ShiftDTO>> GetAllRulesShift(int companyIndex)
        {
            var ruleShiftData = await _dbContext.TA_Rules_Shift.AsNoTracking().Where(x 
                => x.CompanyIndex == companyIndex).ToListAsync();
            var ruleShiftIndexData = ruleShiftData.Select(x => x.Index).ToList();
            var ruleShiftInOutData = await _dbContext.TA_Rules_Shift_InOut.AsNoTracking().Where(x 
                => ruleShiftIndexData.Contains(x.RuleShiftIndex)).ToListAsync();
            var result = ruleShiftData.Select(x =>
            {
                var data = new TA_Rules_ShiftDTO().PopulateWith(x);
                var dataRuleInOut = ruleShiftInOutData.Where(y => y.RuleShiftIndex == x.Index).Select(y 
                    => new TA_Rules_Shift_InOutDTO().PopulateWith(y)).ToList();
                data.RuleInOutTime = dataRuleInOut;
                return data;
            }).ToList();
            return result;
        }

        public async Task<TA_Rules_Shift> GetRulesShiftByIndex(int index) 
        { 
            return await _dbContext.TA_Rules_Shift.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<TA_Rules_Shift>> GetRulesShiftByName(string name, int companyIndex)
        {
            return await _dbContext.TA_Rules_Shift.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Name == name).ToListAsync();
        }

        public async Task<bool> AddRulesShift(TA_Rules_ShiftDTO data)
        {
            var result = true;
            try
            {
                await _dbContext.TA_Rules_Shift.AddAsync(data);
                await _dbContext.SaveChangesAsync();

                if (data.RuleInOutTime != null && data.RuleInOutTime.Count > 0)
                {
                    foreach (var item in data.RuleInOutTime)
                    {
                        await _dbContext.TA_Rules_Shift_InOut.AddAsync(new TA_Rules_Shift_InOut
                        {
                            RuleShiftIndex = data.Index,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = data.UpdatedUser,
                            CompanyIndex = data.CompanyIndex,
                            FromTime = item.FromTime,
                            ToTime = item.ToTime,
                            FromOvernightTime = item.FromOvernightTime,
                            ToOvernightTime = item.ToOvernightTime,
                            TimeMode = item.TimeMode
                        });
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateRulesShift(TA_Rules_ShiftDTO data)
        {
            var result = true;
            try
            {
                _dbContext.TA_Rules_Shift.Update(data);
                await _dbContext.SaveChangesAsync();

                if (data.RuleInOutTime != null && data.RuleInOutTime.Count > 0)
                {
                    var oldRuleShiftInOut = await _dbContext.TA_Rules_Shift_InOut.Where(x => x.RuleShiftIndex == data.Index).ToListAsync();
                    if (oldRuleShiftInOut != null && oldRuleShiftInOut.Count > 0)
                    {
                        _dbContext.TA_Rules_Shift_InOut.RemoveRange(oldRuleShiftInOut);
                    }
                    foreach (var item in data.RuleInOutTime)
                    {
                        await _dbContext.TA_Rules_Shift_InOut.AddAsync(new TA_Rules_Shift_InOut
                        {
                            RuleShiftIndex = data.Index,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            UpdatedUser = data.UpdatedUser,
                            CompanyIndex = data.CompanyIndex,
                            FromTime = item.FromTime,
                            ToTime = item.ToTime,
                            FromOvernightTime = item.FromOvernightTime,
                            ToOvernightTime = item.ToOvernightTime,
                            TimeMode = item.TimeMode
                        });
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteRulesShift(int index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_Rules_Shift.FirstOrDefaultAsync(x => x.Index == index);
                _dbContext.TA_Rules_Shift.Remove(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> IsRuleUsing(int index) 
        {
            return await _dbContext.TA_Shift.AnyAsync(x => x.RulesShiftIndex == index); 
        } 
    }
}
