using EPAD_Common.FileProvider;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Rules_WarningService : BaseServices<GC_Rules_Warning,EPAD_Context>, IGC_Rules_WarningService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;

        private readonly IStoreFileProvider _FileHandler;
        public GC_Rules_WarningService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();

            _FileHandler = serviceProvider.GetService<IStoreFileProvider>();
        }

        public async Task<GC_Rules_Warning> GetDataByIndex(int index)
        {
            return await DbContext.GC_Rules_Warning.FirstOrDefaultAsync(x => x.Index == index);
        }
        public async Task<List<GC_Rules_WarningGroup>> GetRulesWarningGroupsByCompanyIndex(int companyIndex = 0)
        {
            return await DbContext.GC_Rules_WarningGroup.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }
        public async Task<List<GC_Rules_Warning>> GetRulesWarningByCompanyIndex(int companyIndex = 0)
        {
            return await DbContext.GC_Rules_Warning.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }
        public GC_Rules_Warning GetRulesWarningByCompanyIndexAndCode(string code, int companyIndex = 0)
        {
            var group = DbContext.GC_Rules_WarningGroup.FirstOrDefault(e => e.Code == code);
            if (group == null)
            {
                return null;
            }
            return DbContext.GC_Rules_Warning.FirstOrDefault(x => x.CompanyIndex == companyIndex && x.RulesWarningGroupIndex == group.Index);
        }
        public async Task<List<GC_Rules_Warning_EmailSchedule>> GetRulesWarningEmailSchedule(int rulesWarningIndex, int companyIndex = 0)
        {
            return await DbContext.GC_Rules_Warning_EmailSchedules.Where(x => x.RulesWarningIndex == rulesWarningIndex && x.CompanyIndex == companyIndex).OrderBy(e => e.Index).ToListAsync();
        }
        public async Task<List<GC_Rules_Warning_ControllerChannel>> GetRulesWarningControllerChannel(int rulesWarningIndex, int companyIndex = 0)
        {
            return await DbContext.GC_Rules_Warning_ControllerChannels.Where(x => x.RulesWarningIndex == rulesWarningIndex && x.CompanyIndex == companyIndex).OrderBy(e => e.Index).ToListAsync();
        }

        public GC_Rules_Warning GetRulesWarningByGroupIndex(int groupIndex, int companyIndex = 0)
        {
            return DbContext.GC_Rules_Warning.FirstOrDefault(x => x.RulesWarningGroupIndex == groupIndex && x.CompanyIndex == companyIndex);
        }

        public GC_Rules_WarningGroup GetRulesWarningGroupsByGroupIndex(int groupIndex)
        {
            return DbContext.GC_Rules_WarningGroup.FirstOrDefault(x => x.Index == groupIndex);
        }

        public async Task<int> AddRulesWarning(GC_Rules_Warning data, UserInfo user) 
        {
            var result = 0;
            try
            {
                var newData = new GC_Rules_Warning()
                {
                    CompanyIndex = user.CompanyIndex,
                    UpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    UpdatedUser = user.UserName,

                    UseChangeColor = data.UseChangeColor,
                    UseComputerSound = data.UseComputerSound,
                    UseEmail = data.UseEmail,
                    UseLed = data.UseLed,
                    UseSpeaker = data.UseSpeaker,
                    RulesWarningGroupIndex = data.RulesWarningGroupIndex
                };
                if (data.UseSpeaker ?? false)
                {
                    newData.UseSpeakerFocus = data.UseSpeakerFocus;
                    newData.UseSpeakerInPlace = data.UseSpeakerInPlace;
                }
                if (data.UseLed ?? false)
                {
                    newData.UseLedFocus = data.UseLedFocus;
                    newData.UseLedInPlace = data.UseLedInPlace;
                }
                if (data.UseEmail ?? false)
                {
                    newData.Email = data.Email;
                    newData.EmailSendType = data.EmailSendType;
                }
                if (data.UseComputerSound ?? false)
                {
                    newData.ComputerSoundPath = data.ComputerSoundPath;
                }

                await DbContext.GC_Rules_Warning.AddAsync(newData);
                await DbContext.SaveChangesAsync();
                result = newData.Index;
            }
            catch (Exception ex)
            {
                result = -1;
                _logger.LogError("AddRulesWarning " + ex);
            }
            return result;
        }

        public async Task<int> UpdateRulesWarning(GC_Rules_Warning data, UserInfo user)
        {
            var result = 0;
            try
            {
                var oldData = await GetDataByIndex(data.Index);

                oldData.UpdatedDate = DateTime.Now;
                oldData.UpdatedUser = user.UserName;
                oldData.UseChangeColor = data.UseChangeColor;
                oldData.UseComputerSound = data.UseComputerSound;
                oldData.UseEmail = data.UseEmail;
                oldData.UseLed = data.UseLed;
                oldData.UseSpeaker = data.UseSpeaker;
                oldData.UseSpeakerFocus = data.UseSpeakerFocus;
                oldData.UseSpeakerInPlace = data.UseSpeakerInPlace;
                oldData.UseLedFocus = data.UseLedFocus;
                oldData.UseLedInPlace = data.UseLedInPlace;
                oldData.Email = data.Email;
                oldData.EmailSendType = data.EmailSendType;
                if (data.UseComputerSound == false)
                {
                    oldData.ComputerSoundPath = data.ComputerSoundPath;
                }
                DbContext.GC_Rules_Warning.Update(oldData);
                await DbContext.SaveChangesAsync();
                result = oldData.Index;
            }
            catch (Exception ex)
            {
                result = -1;
                _logger.LogError("UpdateRulesWarning " + ex);
            }
            return result;
        }

        public async Task<bool> DeleteRulesWarning(int index, UserInfo user)
        {
            var result = true;
            try
            {
                var oldData = await GetDataByIndex(index);

                var listSchedule = await GetRulesWarningEmailSchedule(oldData.Index, user.CompanyIndex);
                if (listSchedule != null && listSchedule.Any())
                {
                    DbContext.GC_Rules_Warning_EmailSchedules.RemoveRange(listSchedule);
                }

                var listControllerChannel = await GetRulesWarningControllerChannel(oldData.Index, user.CompanyIndex);
                if (listControllerChannel != null && listControllerChannel.Any())
                {
                    DbContext.GC_Rules_Warning_ControllerChannels.RemoveRange(listControllerChannel);
                }

                DbContext.GC_Rules_Warning.Remove(oldData);

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
                _logger.LogError("DeleteRulesWarning " + ex);
            }
            return result;
        }

        public async Task<bool> AddRulesWarningEmailSchedule(List<EmailScheduleRequestModel> data, UserInfo user)
        {
            var result = true;
            try
            {
                var firstSchedule = data.FirstOrDefault();
                var listOldDate = await GetRulesWarningEmailSchedule(firstSchedule.RulesWarningIndex, user.CompanyIndex);
                if (listOldDate != null && listOldDate.Any())
                {
                    DbContext.GC_Rules_Warning_EmailSchedules.RemoveRange(listOldDate);
                }

                foreach (var schedule in data)
                {
                    var time = new TimeSpan(schedule.Time.Hour, schedule.Time.Minute, 0);
                    var newData = new GC_Rules_Warning_EmailSchedule()
                    {
                        CompanyIndex = user.CompanyIndex,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,

                        Time = time,
                        DayOfWeekIndex = schedule.DayOfWeekIndex,
                        RulesWarningIndex = schedule.RulesWarningIndex
                    };
                    await DbContext.GC_Rules_Warning_EmailSchedules.AddAsync(newData);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
                _logger.LogError("AddRulesWarningEmailSchedule " + ex);
            }
            return result;
        }

        public async Task<bool> AddRulesWarningControllerChannels(List<GC_Rules_Warning_ControllerChannel> data, UserInfo user)
        {
            var result = true;
            try
            {
                var listOld = await GetRulesWarningControllerChannel(data.FirstOrDefault().RulesWarningIndex, user.CompanyIndex);
                if (listOld != null && listOld.Any())
                {
                    DbContext.GC_Rules_Warning_ControllerChannels.RemoveRange(listOld);
                }

                foreach (var item in data)
                {
                    var newData = new GC_Rules_Warning_ControllerChannel()
                    {
                        CompanyIndex = user.CompanyIndex,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName,

                        ControllerIndex = item.ControllerIndex,
                        ChannelIndex = item.ChannelIndex,
                        LineIndex = item.LineIndex,
                        GateIndex = item.GateIndex,
                        SerialNumber = item.SerialNumber,
                        RulesWarningIndex = item.RulesWarningIndex,
                        Type = item.Type
                    };
                    await DbContext.GC_Rules_Warning_ControllerChannels.AddAsync(newData);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
                _logger.LogError("AddRulesWarningControllerChannels " + ex);
            }
            return result;
        }

        public async Task<bool> AddEzFileRulesWarning(EzFileRequestSimple data, UserInfo user)
        {
            var result = true;
            try
            {
                var oldData = await GetDataByIndex(data.Index);

                var listEzFiles = new List<EzFile>();
                foreach (var ezfile in data.Attachments)
                {
                    if (ezfile.Url.StartsWith("data:") && ezfile.Url.Length > 5)
                    {
                        var path = "RulesWarning/RulesWarning_" + data.Index + "/" + DateTime.Now.ToFileTime() + "/";
                        _FileHandler.uploadAndUpdateUrl(ezfile, path);
                        var file = new EzFile
                        {
                            Name = ezfile.Name,
                            Url = path
                        };
                        listEzFiles.Add(file);

                        oldData.ComputerSoundPath = string.Concat(JsonConvert.SerializeObject(listEzFiles));
                        DbContext.GC_Rules_Warning.Update(oldData);
                    }
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
                _logger.LogError("AddRulesWarningControllerChannels " + ex);
            }
            return result;
        }

        public async Task SendReloadWarningRuleToClientAsync(int companyIndex, string logContent)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri(_Config.RealTimeServerLink);
            var json = JsonConvert.SerializeObject(new Tuple<int, string> (companyIndex, logContent));
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = await client.PostAsync("/api/PushAttendanceLog/SendReloadWarningRule", content);
                request.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _logger.LogError($"SendData: {ex}");
            }
        }
    }
}
