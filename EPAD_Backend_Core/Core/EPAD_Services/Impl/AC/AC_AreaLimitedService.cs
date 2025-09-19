using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using EPAD_Common.Types;
using System.Threading.Tasks;
using EPAD_Data.Migrations;
using System.Linq;
using EPAD_Data.Models;
using Microsoft.Extensions.Logging;

namespace EPAD_Services.Impl
{
    public class AC_AreaLimitedService : BaseServices<AC_AreaLimited, EPAD_Context>, IAC_AreaLimitedService
    {
        private readonly ILogger _logger;
        public AC_AreaLimitedService(IServiceProvider serviceProvider, ILogger<AC_AreaLimitedService> logger) : base(serviceProvider)
        {
            _logger = logger;
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            var areaLimitedList = DbContext.AC_AreaLimited.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.Name.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();
            var areaLimitedAndDoor = DbContext.AC_AreaLimitedAndDoor.Where(x => areaLimitedList.Select(z => z.Index).Contains(x.AreaLimited)).ToList();
            var doorInfo = DbContext.AC_Door.Where(x => areaLimitedAndDoor.Select(x => x.DoorIndex).Contains(x.Index)).ToList();

            var areaLimitedInfoList = new List<AC_AreaLimitedDTO>();
            foreach (var areaLimited in areaLimitedList)
            {
                var doorIndexs = areaLimitedAndDoor.Where(x => x.AreaLimited == areaLimited.Index).Select(x => x.DoorIndex).ToList();
                areaLimitedInfoList.Add(new AC_AreaLimitedDTO()
                {
                    Index = areaLimited.Index,
                    Name = areaLimited.Name,
                    DoorIndexes = doorIndexs,
                    DoorName = String.Join(", ", doorInfo.Where(x => doorIndexs.Contains(x.Index)).Select(x => x.Name).ToList()),
                    Description = areaLimited.Description
                });
            }
            var countPage = areaLimitedInfoList.Count();
            var dataGrid = new DataGridClass(countPage, areaLimitedInfoList);
            if (pPage <= 1)
            {
                var lsDevice = areaLimitedInfoList.OrderBy(t => t.Name).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = areaLimitedInfoList.OrderBy(t => t.Name).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

        public bool DeleteAreaLimitedAndDoor(List<int> areaLimitIndex)
        {
            using (var transaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    var areaLimiedAndDoor = DbContext.AC_AreaLimitedAndDoor.Where(x => areaLimitIndex.Contains(x.AreaLimited)).ToList();
                    if (areaLimiedAndDoor != null && areaLimiedAndDoor.Count > 0)
                    {
                        DbContext.RemoveRange(areaLimiedAndDoor);
                    }
                    DbContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteAreaLimitedAndDoor: ", ex);
                    transaction.Rollback();
                    return false;
                }
            }


        }
    }
}
