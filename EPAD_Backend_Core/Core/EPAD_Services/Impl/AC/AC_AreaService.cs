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

namespace EPAD_Services.Impl
{
    public class AC_AreaService : BaseServices<AC_Area, EPAD_Context>, IAC_AreaService
    {
        public AC_AreaService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter)
        {
            DataGridClass dataGrid = null;
            int countPage = 0;
            IEnumerable<object> dep;

            List<AC_Area> areaList = null;
            areaList = DbContext.AC_Area.Where(t => t.CompanyIndex == pCompanyIndex
             && (!string.IsNullOrEmpty(filter) && t.Name.Contains(filter) || string.IsNullOrEmpty(filter))).ToList();
            dep = from groupdevice in areaList
                  orderby groupdevice.Name
                  select new
                  {
                      value = groupdevice.Index.ToString(),
                      label = groupdevice.Name
                  };
            countPage = areaList.Count();
            dataGrid = new DataGridClass(countPage, areaList);
            if (pPage <= 1)
            {
                var lsDevice = areaList.OrderBy(t => t.Name).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            else
            {
                int fromRow = pLimit * (pPage - 1);
                var lsDevice = areaList.OrderBy(t => t.Name).Skip(fromRow).Take(pLimit).ToList();
                dataGrid = new DataGridClass(countPage, lsDevice);
            }
            return dataGrid;
        }

    }
}
