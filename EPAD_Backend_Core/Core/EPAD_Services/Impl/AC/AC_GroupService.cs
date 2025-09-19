using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Services.Impl
{
    public class AC_GroupService : BaseServices<AC_AccGroup, EPAD_Context>, IAC_GroupService
    {
        public AC_GroupService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            List<AC_AccGroup> areaList = null;
            areaList = DbContext.AC_AccGroup.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.Name.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();

            var doors = (from areaLst in areaList
                         join timezone in DbContext.AC_TimeZone.Where(x => x.CompanyIndex == pCompanyIndex)
                        on areaLst?.Timezone equals timezone.UID into temp1
                         from dummy1 in temp1.DefaultIfEmpty()
                         join doorLst in DbContext.AC_Door.Where(x => x.CompanyIndex == pCompanyIndex)
                         on areaLst?.DoorIndex equals doorLst.Index into temp2
                         from dummy2 in temp2.DefaultIfEmpty()
                         select new AC_GroupDTO
                         {
                             Name = areaLst.Name,
                             Verify = areaLst.Verify,
                             ValidHoliday = areaLst.ValidHoliday,
                             TimeZoneString = areaLst.TimeZoneString,
                             Timezone = areaLst.Timezone,
                             TimezoneName = dummy1 != null ? dummy1.Name : "",
                             UID = areaLst.UID,
                             DoorIndex = areaLst.DoorIndex,
                             DoorName = dummy2 != null ? dummy2.Name : ""
                         }).ToList();


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

    }



}
