using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Logic
{
    public class HR_PositionInfoLogic : IHR_PositionInfoLogic
    {
        private EPAD_Context _dbContext;
        private readonly ILogger _logger;

        public HR_PositionInfoLogic(EPAD_Context dbContext, ILogger<HR_PositionInfoLogic> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<long> CheckExistedOrCreate(HR_PositionInfoDTO item) 
        {
            try
            {
                var companyIndex = item.CompanyIndex == 0 ? 2 : item.CompanyIndex;
                var entity = await _dbContext.HR_PositionInfo
                    .FirstOrDefaultAsync(e => e.Name.ToLower() == item.Name.ToLower() && e.CompanyIndex == companyIndex);
                if (entity == null)
                {
                    entity = new HR_PositionInfo 
                    { 
                        Name = item.Name, 
                        CompanyIndex = companyIndex, 
                        UpdatedDate = DateTime.Now 
                    };
                    await _dbContext.HR_PositionInfo.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                }
                return entity.Index;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return 0;
            }
        }

        public async Task<bool?> CheckExistedOrCreateAVN(HR_PositionInfoDTO item)
        {
            try
            {
                var companyIndex = item.CompanyIndex == 0 ? 2 : item.CompanyIndex;
                var entity = await _dbContext.HR_PositionInfo
                    .FirstOrDefaultAsync(e => e.Code == item.Code);
                if (entity == null)
                {
                    entity = new HR_PositionInfo
                    {
                        Name = item.Name,
                        CompanyIndex = companyIndex,
                        UpdatedDate = DateTime.Now,
                        Code = item.Code,
                        NameInEng = item.NameInEng
                    };
                    await _dbContext.HR_PositionInfo.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    entity.Name = item.Name;
                    entity.NameInEng = item.NameInEng;
                    entity.CompanyIndex = companyIndex;

                    _dbContext.Update(entity);
                    await _dbContext.SaveChangesAsync();
                    return false;
                }
              

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                return null ;
            }
        }
    }

    public interface IHR_PositionInfoLogic
    {
        Task<long> CheckExistedOrCreate(HR_PositionInfoDTO param);
        Task<bool?> CheckExistedOrCreateAVN(HR_PositionInfoDTO param);
    }
}
