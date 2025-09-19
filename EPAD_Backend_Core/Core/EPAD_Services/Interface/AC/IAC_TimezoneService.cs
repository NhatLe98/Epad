using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static EPAD_Services.Impl.AC_TimezoneService;

namespace EPAD_Services.Interface
{
    public interface IAC_TimezoneService : IBaseServices<AC_TimeZone, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
        AC_TimeZone GetTimezoneByID(int UID);
        List<AC_TimeZone> GetAllDataTimezoneReturn(int pCompanyIndex);
    }
}
