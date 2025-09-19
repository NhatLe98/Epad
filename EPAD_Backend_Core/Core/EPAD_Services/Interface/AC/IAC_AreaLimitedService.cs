using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Common.Types;
using System.Collections.Generic;

namespace EPAD_Services.Interface
{
    public interface IAC_AreaLimitedService : IBaseServices<AC_AreaLimited, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        bool DeleteAreaLimitedAndDoor(List<int> areaLimitIndex);
    }
}
