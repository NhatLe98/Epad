using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models;
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
    public class HR_DormRoomService : BaseServices<HR_DormRoom, EPAD_Context>, IHR_DormRoomService
    {
        private ILogger _logger;
        public HR_DormRoomService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<HR_DormRoomService>();
        }

        public async Task<List<HR_FloorLevel>> GetAllFloorlevel(UserInfo user)
        {
            return await DbContext.HR_FloorLevel.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public async Task<List<HR_DormRoom>> GetAllDormRoom(UserInfo user)
        {
            return await DbContext.HR_DormRoom.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex).ToListAsync();
        }

        public async Task<DataGridClass> GetDormRoom(UserInfo user, int page, int limit, string filter)
        {
            var result = new DataGridClass(0, null);
            var queryData = DbContext.HR_DormRoom.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                queryData = queryData.Where(x => x.Name.Contains(filter)
                    || x.Code.Contains(filter));
            }

            result.total = queryData.Count();
            result.data = await queryData.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return result;
        }

        public async Task<HR_DormRoom> GetByIndex(int index)
        {
            return await DbContext.HR_DormRoom.AsNoTracking().FirstOrDefaultAsync(x => x.Index == index);
        }

        public async Task<List<HR_DormRoom>> GetByCodeOrName(UserInfo user, string code, string name)
        { 
            return await DbContext.HR_DormRoom.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex 
                && (code.Contains(x.Code) || name.Contains(x.Name)))
                .ToListAsync();
        }

        public async Task<List<HR_DormRoom>> GetDormRoomUsingByIndexes(List<int> indexes)
        {
            var dormRegisterUsingDormRoom = await DbContext.HR_DormRegister.Where(x => indexes.Contains(x.DormRoomIndex)).ToListAsync();
            if (dormRegisterUsingDormRoom != null && dormRegisterUsingDormRoom.Count > 0)
            {
                var dormRoomUsingIndexes = dormRegisterUsingDormRoom.Select(x => x.DormRoomIndex).ToList();
                return await DbContext.HR_DormRoom.AsNoTracking().Where(x => dormRoomUsingIndexes.Contains(x.Index))
                .ToListAsync();
            }
            return new List<HR_DormRoom>();
        }

        public async Task<bool> AddDormRoom(UserInfo user, HR_DormRoom data)
        {
            var result = true;
            try
            {
                if (data.Index != 0)
                {
                    return false;
                }

                data.CreatedDate = DateTime.Now;
                data.UpdatedDate = DateTime.Now;
                data.CompanyIndex = user.CompanyIndex;
                data.UpdatedUser = user.FullName;

                await DbContext.HR_DormRoom.AddAsync(data);
                await DbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateDormRoom(UserInfo user, HR_DormRoom data)
        {
            var result = true;
            try
            {
                if (data.Index <= 0)
                {
                    return false;
                }

                var existedDormRoom = await DbContext.HR_DormRoom.FirstOrDefaultAsync(x => x.Index == data.Index);
                if (existedDormRoom != null)
                {
                    existedDormRoom.Name = data.Name;
                    existedDormRoom.FloorLevelIndex = data.FloorLevelIndex;
                    existedDormRoom.Description = data.Description;
                    existedDormRoom.UpdatedDate = DateTime.Now;
                    existedDormRoom.UpdatedUser = user.FullName;

                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> DeleteDormRoom(List<int> indexes)
        {
            var result = true;
            try
            {
                var existedDormRoom = await DbContext.HR_DormRoom.Where(x => indexes.Contains(x.Index)).ToListAsync();
                if (existedDormRoom != null)
                {
                    DbContext.HR_DormRoom.RemoveRange(existedDormRoom);
                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
    }
}
