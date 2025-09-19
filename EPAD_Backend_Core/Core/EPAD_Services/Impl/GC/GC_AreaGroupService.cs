using EPAD_Common.Extensions;
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
    public class GC_AreaGroupService : BaseServices<GC_AreaGroup, EPAD_Context>, IGC_AreaGroupService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        private IGC_AreaGroup_GroupDeviceService _GC_AreaGroup_GroupDeviceService;
        public GC_AreaGroupService(IServiceProvider serviceProvider, EPAD_Context context, 
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_AreaGroupService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
            _GC_AreaGroup_GroupDeviceService = serviceProvider.GetService<IGC_AreaGroup_GroupDeviceService>();
        }

        public async Task<Tuple<List<AreaGroupModel>, int>> GetAreaGroup(int page, string filter, int pageSize, UserInfo user)
        {
            var data = new List<AreaGroupModel>();
            var areaGroups = await GetDataByCompanyIndex(user.CompanyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                areaGroups = areaGroups.Where(x => x.Name.Contains(filter) || x.NameInEng.Contains(filter) 
                    || x.Code.Contains(filter)).ToList();
            }
            if (areaGroups != null && areaGroups.Count > 0)
            {
                var listAreaGroupIndex = areaGroups.Select(x => x.Index).ToList();
                var areaGroupDevices = await _GC_AreaGroup_GroupDeviceService.GetDataByListAreaGroupIndexAndCompanyIndex(listAreaGroupIndex,
                        user.CompanyIndex);
                foreach (var areaGroup in areaGroups)
                {
                    var groupDevice = areaGroupDevices.Where(x => x.AreaGroupIndex == areaGroup.Index);

                    var item = new AreaGroupModel().PopulateWith(areaGroup);
                    item.GroupDevice = groupDevice.Select(e => e.DeviceGroupIndex).ToList();

                    var groupDevices = await _dbContext.IC_GroupDevice.AsNoTracking().Where(x => item.GroupDevice.Contains(x.Index)).ToListAsync();
                    if (groupDevices != null && groupDevices.Count > 0)
                    {
                        item.GroupDeviceString = string.Join(", ", groupDevices.Select(x => x.Name).ToList());
                    }

                    item.CreatedDateString = item.CreatedDate.HasValue ? item.CreatedDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : string.Empty;
                    item.UpdatedDateString = item.UpdatedDate.HasValue ? item.UpdatedDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : string.Empty;
                    data.Add(item);
                }

                var skip = (page - 1) * pageSize;
                if (skip < 0)
                {
                    skip = 0;
                }
                int countTotal = data.Count;
                var dataResult = data.Skip(skip).Take(pageSize).ToList();

                return new Tuple<List<AreaGroupModel>, int>(dataResult, countTotal);
            }

            return new Tuple<List<AreaGroupModel>, int>(new List<AreaGroupModel>(), 0);
        }

        public async Task<bool> AddAreaGroup(AreaGroupRequestModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var area = new GC_AreaGroup
                {
                    Name = param.AreaGroup.Name,
                    NameInEng = param.AreaGroup.NameInEng,
                    Code = param.AreaGroup.Code,
                    Description = param.AreaGroup.Description,
                    CompanyIndex = user.CompanyIndex,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UpdatedUser = user.UserName
                };

                await _dbContext.GC_AreaGroup.AddAsync(area);
                await _dbContext.SaveChangesAsync();

                foreach (var groupIndex in param.GroupDevice)
                {
                    var areaGroup_DeviceGroup = new GC_AreaGroup_GroupDevice
                    {
                        AreaGroupIndex = area.Index,
                        DeviceGroupIndex = groupIndex,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName
                    };
                    await _dbContext.GC_AreaGroup_GroupDevice.AddAsync(areaGroup_DeviceGroup);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddAreaGroup" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateAreaGroup(AreaGroupRequestModel param, UserInfo user)
        {
            var result = true;
            try
            {
                var area = await _dbContext.GC_AreaGroup.FirstOrDefaultAsync(x => x.Index == param.AreaGroup.Index);
                area.Name = param.AreaGroup.Name;
                area.NameInEng = param.AreaGroup.NameInEng;
                area.Description = param.AreaGroup.Description;
                area.UpdatedDate = DateTime.Now;
                area.UpdatedUser = user.UserName;
                await _dbContext.SaveChangesAsync();

                await _GC_AreaGroup_GroupDeviceService.DeleteByAreaGroupIndex(area.Index, area.CompanyIndex);

                foreach (var groupIndex in param.GroupDevice)
                {
                    var areaGroup_DeviceGroup = new GC_AreaGroup_GroupDevice
                    {
                        AreaGroupIndex = area.Index,
                        DeviceGroupIndex = groupIndex,
                        CompanyIndex = user.CompanyIndex,
                        UpdatedDate = DateTime.Now,
                        UpdatedUser = user.UserName
                    };
                    await _dbContext.GC_AreaGroup_GroupDevice.AddAsync(areaGroup_DeviceGroup);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddAreaGroup" + ex);
                result = false;
            }
            return result;            
        }

        public async Task<bool> DeleteAreaGroup(List<int> indexes, UserInfo user)
        {
            var result = true;
            try
            {
                foreach (var index in indexes)
                {
                    await _GC_AreaGroup_GroupDeviceService.DeleteByAreaGroupIndex(index, user.CompanyIndex);
                }

                var deleteItems = await _dbContext.GC_AreaGroup.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (deleteItems != null)
                {
                    _dbContext.GC_AreaGroup.RemoveRange(deleteItems);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddAreaGroup" + ex);
                result = false;
            }
            return result;
        }

        public async Task<GC_AreaGroup> GetDataByIndex(int index)
        {
            return await _dbContext.GC_AreaGroup.FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<GC_AreaGroup>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _dbContext.GC_AreaGroup.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }
        public async Task<List<AreaGroupFullInfo>> GetFullDataByCompanyIndex(int companyIndex)
        {
            var listGroup = await _dbContext.GC_AreaGroup.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
            var result = listGroup.Select(e => GetFullData(e)).ToList();
            return result;
        }

        public async Task<GC_AreaGroup> GetDataByCodeAndCompanyIndex(string code, int companyIndex)
        {
            return await _dbContext.GC_AreaGroup.Where(x => x.Code == code && x.CompanyIndex == companyIndex).FirstOrDefaultAsync();
        }

        public async Task<List<GC_AreaGroup_GroupDevice>> GetDeviceByAreaAndCompanyIndex(int areaIndex, int companyIndex)
        {
            return await _dbContext.GC_AreaGroup_GroupDevice.Where(x => x.AreaGroupIndex == areaIndex && x.CompanyIndex == companyIndex).ToListAsync();
        }
        public AreaGroupFullInfo GetFullData(GC_AreaGroup group)
        {
            var item = new AreaGroupFullInfo().PopulateWith(group);
            item.DeviceGroups = _dbContext.GC_AreaGroup_GroupDevice.Where(e => e.AreaGroupIndex == group.Index).Select(e => e.DeviceGroupIndex).ToList();
            return item;
        }
    }
}
