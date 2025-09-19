using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.VariantTypes;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class TA_ListLocationService : BaseServices<TA_ListLocation, EPAD_Context>, ITA_ListLocationService
    {
        public EPAD_Context _dbContext;
        private ILogger _logger;
        public TA_ListLocationService(IServiceProvider serviceProvider, EPAD_Context context, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _dbContext = context;
            _logger = loggerFactory.CreateLogger<TA_ListLocation>();
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var listData = _dbContext.TA_ListLocation.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                string lowerFilter = filter.ToLower();
                listData = listData.Where(x => x.LocationName.ToLower().Contains(lowerFilter));
            }
            countPage = listData.Count();
            var data = listData
                .OrderBy(t => t.LocationName)
                .Skip((pPage - 1) * pLimit)
                .Take(pLimit)
                .Select(item => new TA_ListLocationDTO()
                {
                    LocationIndex = item.Index,
                    LocationName = item.LocationName,
                    Address = item.Address,
                    Coordinates = item.Coordinates,
                    Radius = item.Radius,
                    Description = item.Description,
                })
                .ToList();
            dataGrid = new DataGridClass(countPage, data);
            return dataGrid;
        }
        public async Task<string> AddLocation(TA_ListLocation data, UserInfo user)
        {
            try
            {
                var normalizedLocationName = data.LocationName.ToLower().Replace(" ", string.Empty);
                var existingLocation = await _dbContext.TA_ListLocation
                    .AnyAsync(x => x.LocationName.ToLower().Replace(" ", string.Empty) == normalizedLocationName);
                if (existingLocation)
                {
                    return "LocationExisted";
                }
                else
                {
                    var location = new TA_ListLocation();
                    location.LocationName = data.LocationName;
                    location.Address = data.Address;
                    location.Coordinates = data.Coordinates;
                    location.Radius = data.Radius;
                    location.Description = data.Description;
                    location.CompanyIndex = user.CompanyIndex;
                    location.UpdatedDate = DateTime.Now;
                    location.UpdatedUser = user.UserName;
                    location.CreatedDate = DateTime.Now;
                    await _dbContext.TA_ListLocation.AddAsync(location);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddLocationFail: {ex}");
            }
            return string.Empty;
        }

        public async Task<bool> UpdateLocation(TA_ListLocationDTO data, UserInfo user, TA_ListLocation existLocation)
        {
            var result = true;
            try
            {
                var normalizedLocationName = data.LocationName.ToLower().Replace(" ", string.Empty);
                var existingLocation = await _dbContext.TA_ListLocation
                    .AnyAsync(x => x.LocationName.ToLower().Replace(" ", string.Empty) == normalizedLocationName && x.Index != existLocation.Index);
                if (existingLocation)
                {
                    return false;
                }
                else
                {
                    existLocation.LocationName = data.LocationName;
                    existLocation.Address = data.Address;
                    existLocation.Coordinates = data.Coordinates;
                    existLocation.Radius = data.Radius;
                    existLocation.Description = data.Description;
                    existLocation.UpdatedDate = DateTime.Now;
                    existLocation.UpdatedUser = user.UserName;
                    _dbContext.TA_ListLocation.Update(existLocation);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateLocationFail: {ex}");
                result = false;
            }
            return result;
        }
        public string DeleteLocation(List<int> listIndex)
        {
            using (var transaction = DbContext.Database.BeginTransaction())
            {
                string message = string.Empty;
                try
                {
                    var locationUsedInDepartment = _dbContext.TA_LocationByDepartment.Where(lD => listIndex.Contains(lD.LocationIndex)).Select(lD => lD.LocationIndex).ToList();
                    var locationUsedInEmployee = _dbContext.TA_LocationByEmployee.Where(lE => listIndex.Contains(lE.LocationIndex)).Select(lD => lD.LocationIndex).ToList();
                    if (locationUsedInDepartment.Any() || locationUsedInEmployee.Any())
                    {
                        var locationNamesInUse = _dbContext.TA_ListLocation.Where(l => locationUsedInDepartment.Contains(l.Index) || locationUsedInEmployee.Contains(l.Index)).Select(l => l.LocationName).ToList();
                        foreach (var locationName in locationNamesInUse)
                        {
                            message += "<p>  - " + locationName + "</p>" + "<p class=\"\" style=\"margin: 4px;\"></p>";
                            continue;
                        }
                    }
                    else
                    {
                        var listLocation = _dbContext.TA_ListLocation.Where(x => listIndex.Contains(x.Index)).ToList();
                        if (listLocation != null && listLocation.Count > 0)
                        {
                            _dbContext.RemoveRange(listLocation);
                            _dbContext.SaveChanges();
                        }
                    }
                    transaction.Commit();
                    return message;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteListLocationFail: {ex}");
                    transaction.Rollback();
                    return string.Empty;
                }
            }
        }
        public async Task<TA_ListLocation> GetLocationByIndex(int locationIndex)
        {
            return await _dbContext.TA_ListLocation.FirstOrDefaultAsync(x => x.Index == locationIndex);
        }
    }
}