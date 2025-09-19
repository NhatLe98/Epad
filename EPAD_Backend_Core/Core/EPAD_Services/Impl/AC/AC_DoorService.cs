using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class AC_DoorService : BaseServices<AC_Door, EPAD_Context>, IAC_DoorService
    {
        public AC_DoorService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            var lstDevice = DbContext.AC_DoorAndDevice.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var lstDevices = DbContext.IC_Device.Where(x => x.CompanyIndex == pCompanyIndex).ToList();
            var doorList = DbContext.AC_Door.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.Name.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();

            var doors = (from doorLst in doorList
                         join areaDoor in DbContext.AC_AreaAndDoor.Where(x => x.CompanyIndex == pCompanyIndex)
                         on doorLst?.Index equals areaDoor.DoorIndex into temp
                         from dummy in temp.DefaultIfEmpty()

                         join timezone in DbContext.AC_TimeZone.Where(x => x.CompanyIndex == pCompanyIndex)
                        on doorLst?.Timezone equals timezone.UID into temp1
                         from dummy1 in temp1.DefaultIfEmpty()

                         join area in DbContext.AC_Area.Where(x => x.CompanyIndex == pCompanyIndex)
                       on dummy?.AreaIndex equals area.Index into temp2
                         from dummy2 in temp2.DefaultIfEmpty()
                         select new AC_DoorDTO
                         {
                             Index = doorLst.Index,
                             AreaIndex = dummy != null ? dummy.AreaIndex : 0,
                             DoorOpenTimezoneUID = doorLst.DoorOpenTimezoneUID,
                             Name = doorLst.Name,
                             SerialNumberLst = lstDevice.Where(x => x.DoorIndex == doorLst.Index).Select(x => x.SerialNumber).ToList(),
                             AreaName = dummy2 != null ? dummy2.Name : "",
                             Timezone = doorLst.Timezone,
                             TimezoneName = dummy1 != null ? dummy1.Name : "",
                             DoorOpenTimezoneUIDString = doorLst.DoorOpenTimezoneUID != 0 ? "Mốc thời gian " 
                                + doorLst.DoorOpenTimezoneUID.ToString() : "",
                             DoorSettingDescription = doorLst.DoorSettingDescription,
                             Description = doorLst.DoorSettingDescription,
                         }).ToList();
            doors = doors.Select(x => { x.NameDeviceLst = lstDevices.Where(y => x.SerialNumberLst.Contains(y.SerialNumber)).Select(z => string.IsNullOrWhiteSpace(z.AliasName) ? z.SerialNumber : z.AliasName).ToList(); return x; }).ToList();
            doors = doors.Select(x => { x.DeviceListName = x.NameDeviceLst != null ? string.Join(',', x.NameDeviceLst) : ""; return x; }).ToList();

            countPage = doors.Count();
            dataGrid = new DataGridClass(countPage, doors);
            if (pPage <= 1)
            {
                var lsDevice = doors.OrderBy(t => t.Name).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = doors.OrderBy(t => t.Name).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

        public async Task<List<AC_Door>> GetExistedDoorSetting(UserInfo user, List<int> param) 
        { 
            return await DbContext.AC_Door.AsNoTracking().Where(x => x.CompanyIndex == user.CompanyIndex && param.Contains(x.Index)).ToListAsync();
        }

        public async Task<AC_Door> GetByIndex(int param)
        {
            return await DbContext.AC_Door.AsNoTracking().FirstOrDefaultAsync(x => param == x.Index);
        }

        public async Task UpdateDoorSettings(UserInfo user, AC_DoorDTO param)
        {
            var existedDoor = await GetExistedDoorSetting(user, param.DoorIndexes);
            if (existedDoor != null && existedDoor.Count > 0)
            {
                foreach (var item in existedDoor)
                { 
                    item.Timezone = param.Timezone;
                    item.DoorSettingDescription = param.DoorSettingDescription;
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedUser = user.FullName;
                }
                DbContext.UpdateRange(existedDoor);
                await DbContext.SaveChangesAsync();
            }
        }
        public async Task UpdateDoorSetting(UserInfo user, AC_DoorDTO param)
        {
            var existedDoor = await GetByIndex(param.Index);
            if (existedDoor != null)
            {
                existedDoor.Timezone = param.Timezone;
                existedDoor.DoorSettingDescription = param.DoorSettingDescription;
                existedDoor.UpdatedDate = DateTime.Now;
                existedDoor.UpdatedUser = user.FullName;
                DbContext.Update(existedDoor);
                await DbContext.SaveChangesAsync();
            }
        }
        public async Task DeleteDoorSettings(UserInfo user, List<int> param)
        {
            var existedDoor = await GetExistedDoorSetting(user, param);
            if (existedDoor != null && existedDoor.Count > 0)
            {
                foreach (var item in existedDoor)
                {
                    item.Timezone = 0;
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedUser = user.FullName;
                }
                DbContext.UpdateRange(existedDoor);
                await DbContext.SaveChangesAsync();
            }
        }
    }
}
