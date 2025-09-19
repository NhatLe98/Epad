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
    public class GC_ParkingLotService : BaseServices<GC_ParkingLot, EPAD_Context>, IGC_ParkingLotService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public GC_ParkingLotService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_ParkingLotService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<bool> AddParkingLot(GC_ParkingLot param, UserInfo user)
        {
            var result = true;
            try
            {
                var parkingLot = param;
                parkingLot.Index = 0;
                parkingLot.UpdatedDate = DateTime.Now;
                parkingLot.UpdatedUser = user.UserName;
                parkingLot.CompanyIndex = user.CompanyIndex;

                await _dbContext.GC_ParkingLot.AddAsync(parkingLot);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddParkingLot" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateParkingLot(GC_ParkingLot param, UserInfo user)
        {
            var result = true;
            try
            {
                var parkingLot = await _dbContext.GC_ParkingLot.FirstOrDefaultAsync(x => x.Index == param.Index);
                parkingLot.Name = param.Name;
                parkingLot.NameInEng = param.NameInEng;
                parkingLot.Description = param.Description;
                parkingLot.Capacity = param.Capacity;
                parkingLot.UpdatedDate = DateTime.Now;
                parkingLot.UpdatedUser = user.UserName;
                parkingLot.CompanyIndex = user.CompanyIndex;
                _dbContext.GC_ParkingLot.Update(parkingLot);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateParkingLot" + ex);
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteParkingLots(List<int> indexes, UserInfo user)
        {
            var result = true;
            try
            {
                var deleteItems = await _dbContext.GC_ParkingLot.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (deleteItems != null)
                {
                    _dbContext.GC_ParkingLot.RemoveRange(deleteItems);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteParkingLots" + ex);
                result = false;
            }
            return result;
        }

        public async Task<DataGridClass> GetDataByPage(int companyIndex, int page, string filter, int pageSize)
        {
            var dataQuery = _dbContext.GC_ParkingLot.Where(x => x.CompanyIndex == companyIndex);
            if (!string.IsNullOrEmpty(filter))
            {
                dataQuery = dataQuery.Where(x => x.Name.Contains(filter) || x.NameInEng.Contains(filter) || x.Code.Contains(filter));
            }
            var skip = (page - 1) * pageSize;
            if (skip < 0)
            {
                skip = 0;
            }
            int countTotal = dataQuery.Count();
            var output = await dataQuery.Skip(skip).Take(pageSize).ToListAsync();
            var grid = new DataGridClass(countTotal, output);
            return grid;
        }

        public async Task<GC_ParkingLot> GetDataByIndex(int index)
        {
            return await _dbContext.GC_ParkingLot.FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<GC_ParkingLot>> GetDataByCompanyIndex(int companyIndex)
        {
            return await _dbContext.GC_ParkingLot.Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }

        public async Task<GC_ParkingLot> GetDataByNameAndCompanyIndex(string name, int companyIndex)
        {
            return await _dbContext.GC_ParkingLot.FirstOrDefaultAsync(x => x.Name == name && x.CompanyIndex == companyIndex);
        }

        public async Task<GC_ParkingLot> GetDataByCodeAndCompanyIndex(string code, int companyIndex)
        {
            return await _dbContext.GC_ParkingLot.FirstOrDefaultAsync(x => x.Code == code && x.CompanyIndex == companyIndex);
        }

        public async Task<string> TryDeleteParkingLot(List<int> pParkingLotIndexs, int pCompanyIndex)
        {
            var lineIndexLookup = pParkingLotIndexs.ToHashSet();

            _dbContext.Database.BeginTransaction();

            try
            {

                //var allparkinglotdetail = _dbContext.GC_ParkingLotDetail.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.LineIndex));
                //_dbContext.GC_ParkingLotDetail.RemoveRange(allparkinglotdetail);

                var allLine = _dbContext.GC_ParkingLot.Where(x => x.CompanyIndex == pCompanyIndex && lineIndexLookup.Contains(x.Index));
                _dbContext.GC_ParkingLot.RemoveRange(allLine);

                _dbContext.SaveChanges();
                _dbContext.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.Database.RollbackTransaction();
                return await Task.FromResult(ex.Message);
            }

            return await Task.FromResult("");
        }
    }
}
