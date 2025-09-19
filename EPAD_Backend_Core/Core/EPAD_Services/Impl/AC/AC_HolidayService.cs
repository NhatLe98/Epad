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
    public class AC_HolidayService : BaseServices<AC_AccHoliday, EPAD_Context>, IAC_HolidayService
    {
        public AC_HolidayService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;

            var holidayList = DbContext.AC_AccHoliday.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.HolidayName.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();

            var doors = (from holidayLst in holidayList
                         join door in DbContext.AC_Door.Where(x => x.CompanyIndex == pCompanyIndex)
                         on holidayLst?.DoorIndex equals door.Index into temp
                         from dummy in temp.DefaultIfEmpty()
                         join timezoneLst in DbContext.AC_TimeZone.Where(x => x.CompanyIndex == pCompanyIndex)
                        on holidayLst?.TimeZone equals timezoneLst.UID into tztemp
                         from timezone in tztemp.DefaultIfEmpty()
                         select new AC_HolidayDTO
                         {
                             UID = holidayLst.UID,
                             DoorIndex = holidayLst.DoorIndex,
                             DoorIndexes = new List<int>() { holidayLst.DoorIndex },
                             DoorName = dummy != null ? dummy.Name : "",
                             HolidayName = holidayLst.HolidayName,
                             StartDate = holidayLst.StartDate,
                             EndDate = holidayLst.EndDate,
                             StartDateString = holidayLst.StartDate.ToString("dd/MM"),
                             EndDateString = holidayLst.EndDate.ToString("dd/MM"),
                             HolidayType = holidayLst.HolidayType,
                             HolidayTypeName = holidayLst.HolidayType != 0 ? "Loại ngày nghỉ " + holidayLst.HolidayType.ToString() : "",
                             Loop = holidayLst.Loop,
                             LoopName = holidayLst.Loop ? "Có" : "Không",
                             TimeZone = holidayLst.TimeZone,
                             TimezoneName = timezone != null ? timezone.Name : ""
                         }).ToList();

            countPage = doors.Count();
            dataGrid = new DataGridClass(countPage, doors);
            if (pPage <= 1)
            {
                var lstHoliday = doors.OrderBy(t => t.HolidayName).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lstHoliday = doors.OrderBy(t => t.HolidayName).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lstHoliday);
            }
            return dataGrid;
        }
    }
}
