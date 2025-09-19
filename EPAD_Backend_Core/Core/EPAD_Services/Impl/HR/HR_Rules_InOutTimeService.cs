using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_Rules_InOutTimeService : BaseServices<HR_Rules_InOutTime, EPAD_Context>, IHR_Rules_InOutTimeService
    {
        private ILogger _logger;
        public HR_Rules_InOutTimeService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_Rules_InOutTimeService>();
        }

        public List<Rules_InOutTimeParam> GetAllRules(UserInfo user)
        {
            var listRules = DbContext.HR_Rules_InOutTime.Where(x => x.CompanyIndex == user.CompanyIndex)
            .Select(x => new Rules_InOutTimeParam()
            {
                Index = x.Index,
                FromDate = x.FromDate.ToString("dd/MM/yyyy"),
                Description = x.Description,
                CheckInTime = x.CheckInTime,
                MaxEarlyCheckInMinute = x.MaxEarlyCheckInMinute,
                MaxLateCheckInMinute = x.MaxLateCheckInMinute,
                CheckOutTime = x.CheckOutTime,
                MaxEarlyCheckOutMinute = x.MaxEarlyCheckOutMinute,
                MaxLateCheckOutMinute = x.MaxLateCheckOutMinute,
            }).ToList();

            return listRules;
        }

        public async Task<HR_Rules_InOutTime> GetByDate(DateTime date, UserInfo user)
        {
            return await DbContext.HR_Rules_InOutTime.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyIndex == user.CompanyIndex
                && x.FromDate.Date == date.Date);
        }

        public async Task<HR_Rules_InOutTime> GetByIndex(int index)
        {
            return await DbContext.HR_Rules_InOutTime.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<bool> AddRuleInOutTime(Rules_InOutTimeParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var format = "dd-MM-yyyy";
                DateTime.TryParseExact(param.FromDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime date);

                var ruleData = new HR_Rules_InOutTime();

                ruleData.FromDate = date;
                ruleData.Description = param.Description;

                ruleData.CheckInTime = param.CheckInTime;
                ruleData.MaxEarlyCheckInMinute = param.MaxEarlyCheckInMinute;
                ruleData.MaxLateCheckInMinute = param.MaxLateCheckInMinute;
                ruleData.CheckOutTime = param.CheckOutTime;
                ruleData.MaxEarlyCheckOutMinute = param.MaxEarlyCheckOutMinute;
                ruleData.MaxLateCheckOutMinute = param.MaxLateCheckOutMinute;

                ruleData.CompanyIndex = user.CompanyIndex;
                ruleData.CreatedDate = DateTime.Now;

                ruleData.UpdatedDate = DateTime.Now;
                ruleData.UpdatedUser = user.UserName;

                DbContext.HR_Rules_InOutTime.Add(ruleData);

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateRuleInOutTime(Rules_InOutTimeParam param, UserInfo user)
        {
            var result = true;
            try
            {
                var format = "dd-MM-yyyy";
                DateTime.TryParseExact(param.FromDate, format, null, System.Globalization.DateTimeStyles.None, out DateTime date);

                var updateData = await DbContext.HR_Rules_InOutTime.FirstOrDefaultAsync(x => x.Index == param.Index);

                updateData.FromDate = date;
                updateData.Description = param.Description;

                updateData.CheckInTime = param.CheckInTime != null ? (new DateTime(2000, 01, 01,
                    param.CheckInTime.Value.Hour, param.CheckInTime.Value.Minute, 0)) : param.CheckInTime;

                updateData.MaxEarlyCheckInMinute = param.MaxEarlyCheckInMinute;
                updateData.MaxLateCheckInMinute = param.MaxLateCheckInMinute;
                updateData.CheckOutTime = param.CheckOutTime != null ? (new DateTime(2000, 01, 01,
                    param.CheckOutTime.Value.Hour, param.CheckOutTime.Value.Minute, 0)) : param.CheckOutTime;

                updateData.MaxEarlyCheckOutMinute = param.MaxEarlyCheckOutMinute;
                updateData.MaxLateCheckOutMinute = param.MaxLateCheckOutMinute;

                updateData.UpdatedDate = DateTime.Now;
                updateData.UpdatedUser = user.UserName;

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteRuleInOutTime(int index)
        {
            var result = true;
            try
            {

                var deleteData = await DbContext.HR_Rules_InOutTime.FirstOrDefaultAsync(x => x.Index == index);
                if (deleteData != null)
                { 
                    DbContext.HR_Rules_InOutTime.Remove(deleteData);
                }

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
