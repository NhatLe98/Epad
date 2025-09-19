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
    public class TA_ShiftService : BaseServices<TA_Shift, EPAD_Context>, ITA_ShiftService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public TA_ShiftService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<TA_ShiftService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<TA_Shift>> GetAllShift(int companyIndex)
        {
            var shiftList = await _dbContext.TA_Shift.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();

            shiftList.Add(new TA_Shift { Index = 0, Code = "Không có ca" });
            return shiftList;
        }

        public async Task<TA_Shift> GetShiftByIndex(int index)
        {
            return await _dbContext.TA_Shift.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<TA_Shift>> GetShiftByName(string name, int companyIndex)
        {
            return await _dbContext.TA_Shift.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Name == name).ToListAsync();
        }

        public async Task<List<TA_Shift>> GetShiftByCode(string code, int companyIndex)
        {
            return await _dbContext.TA_Shift.AsNoTracking().Where(x => x.CompanyIndex == companyIndex && x.Code == code).ToListAsync();
        }

        public async Task<bool> AddShift(TA_Shift data)
        {
            var result = true;
            try
            {
                if (data.PaidHolidayStartTime.HasValue)
                {
                    data.PaidHolidayStartTime = new DateTime(data.PaidHolidayStartTime.Value.Year, data.PaidHolidayStartTime.Value.Month,
                        data.PaidHolidayStartTime.Value.Day, data.PaidHolidayStartTime.Value.Hour, data.PaidHolidayStartTime.Value.Minute, 0);
                }
                if (data.PaidHolidayEndTime.HasValue)
                {
                    data.PaidHolidayEndTime = new DateTime(data.PaidHolidayEndTime.Value.Year, data.PaidHolidayEndTime.Value.Month,
                        data.PaidHolidayEndTime.Value.Day, data.PaidHolidayEndTime.Value.Hour, data.PaidHolidayEndTime.Value.Minute, 0);
                }
                if (data.CheckInTime.HasValue)
                {
                    data.CheckInTime = new DateTime(data.CheckInTime.Value.Year, data.CheckInTime.Value.Month,
                        data.CheckInTime.Value.Day, data.CheckInTime.Value.Hour, data.CheckInTime.Value.Minute, 0);
                }
                if (data.CheckOutTime.HasValue)
                {
                    data.CheckOutTime = new DateTime(data.CheckOutTime.Value.Year, data.CheckOutTime.Value.Month,
                        data.CheckOutTime.Value.Day, data.CheckOutTime.Value.Hour, data.CheckOutTime.Value.Minute, 0);
                }
                if (data.BreakStartTime.HasValue)
                {
                    data.BreakStartTime = new DateTime(data.BreakStartTime.Value.Year, data.BreakStartTime.Value.Month,
                        data.BreakStartTime.Value.Day, data.BreakStartTime.Value.Hour, data.BreakStartTime.Value.Minute, 0);
                }
                if (data.BreakEndTime.HasValue)
                {
                    data.BreakEndTime = new DateTime(data.BreakEndTime.Value.Year, data.BreakEndTime.Value.Month,
                        data.BreakEndTime.Value.Day, data.BreakEndTime.Value.Hour, data.BreakEndTime.Value.Minute, 0);
                }
                if (data.OTStartTime.HasValue)
                {
                    data.OTStartTime = new DateTime(data.OTStartTime.Value.Year, data.OTStartTime.Value.Month,
                        data.OTStartTime.Value.Day, data.OTStartTime.Value.Hour, data.OTStartTime.Value.Minute, 0);
                }
                if (data.OTEndTime.HasValue)
                {
                    data.OTEndTime = new DateTime(data.OTEndTime.Value.Year, data.OTEndTime.Value.Month,
                        data.OTEndTime.Value.Day, data.OTEndTime.Value.Hour, data.OTEndTime.Value.Minute, 0);
                }
                await _dbContext.TA_Shift.AddAsync(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateShift(TA_Shift data)
        {
            var result = true;
            try
            {
                if (data.PaidHolidayStartTime.HasValue)
                {
                    data.PaidHolidayStartTime = new DateTime(data.PaidHolidayStartTime.Value.Year, data.PaidHolidayStartTime.Value.Month,
                        data.PaidHolidayStartTime.Value.Day, data.PaidHolidayStartTime.Value.Hour, data.PaidHolidayStartTime.Value.Minute, 0);
                }
                if (data.PaidHolidayEndTime.HasValue)
                {
                    data.PaidHolidayEndTime = new DateTime(data.PaidHolidayEndTime.Value.Year, data.PaidHolidayEndTime.Value.Month,
                        data.PaidHolidayEndTime.Value.Day, data.PaidHolidayEndTime.Value.Hour, data.PaidHolidayEndTime.Value.Minute, 0);
                }
                if (data.CheckInTime.HasValue)
                {
                    data.CheckInTime = new DateTime(data.CheckInTime.Value.Year, data.CheckInTime.Value.Month,
                        data.CheckInTime.Value.Day, data.CheckInTime.Value.Hour, data.CheckInTime.Value.Minute, 0);
                }
                if (data.CheckOutTime.HasValue)
                {
                    data.CheckOutTime = new DateTime(data.CheckOutTime.Value.Year, data.CheckOutTime.Value.Month,
                        data.CheckOutTime.Value.Day, data.CheckOutTime.Value.Hour, data.CheckOutTime.Value.Minute, 0);
                }
                if (data.BreakStartTime.HasValue)
                {
                    data.BreakStartTime = new DateTime(data.BreakStartTime.Value.Year, data.BreakStartTime.Value.Month,
                        data.BreakStartTime.Value.Day, data.BreakStartTime.Value.Hour, data.BreakStartTime.Value.Minute, 0);
                }
                if (data.BreakEndTime.HasValue)
                {
                    data.BreakEndTime = new DateTime(data.BreakEndTime.Value.Year, data.BreakEndTime.Value.Month,
                        data.BreakEndTime.Value.Day, data.BreakEndTime.Value.Hour, data.BreakEndTime.Value.Minute, 0);
                }
                if (data.OTStartTime.HasValue)
                {
                    data.OTStartTime = new DateTime(data.OTStartTime.Value.Year, data.OTStartTime.Value.Month,
                        data.OTStartTime.Value.Day, data.OTStartTime.Value.Hour, data.OTStartTime.Value.Minute, 0);
                }
                if (data.OTEndTime.HasValue)
                {
                    data.OTEndTime = new DateTime(data.OTEndTime.Value.Year, data.OTEndTime.Value.Month,
                        data.OTEndTime.Value.Day, data.OTEndTime.Value.Hour, data.OTEndTime.Value.Minute, 0);
                }
                _dbContext.TA_Shift.Update(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteShift(int index)
        {
            var result = true;
            try
            {
                var data = await _dbContext.TA_Shift.FirstOrDefaultAsync(x => x.Index == index);
                _dbContext.TA_Shift.Remove(data);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> IsShiftUsing(int index)
        {
            return await _dbContext.TA_EmployeeShift.AnyAsync(x => x.ShiftIndex == index)
                || await _dbContext.TA_ScheduleFixedByEmployee.AnyAsync(x
                    => x.Monday == index || x.Tuesday == index || x.Wednesday == index || x.Thursday == index
                    || x.Friday == index || x.Saturday == index || x.Sunday == index)
                || await _dbContext.TA_ScheduleFixedByDepartment.AnyAsync(x
                    => x.Monday == index || x.Tuesday == index || x.Wednesday == index || x.Thursday == index
                    || x.Friday == index || x.Saturday == index || x.Sunday == index);
        }
    }
}
