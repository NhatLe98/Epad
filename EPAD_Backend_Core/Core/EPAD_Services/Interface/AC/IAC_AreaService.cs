using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Common.Types;

namespace EPAD_Services.Interface
{
    public interface IAC_AreaService : IBaseServices<AC_Area, EPAD_Context>
    {
        DataGridClass GetDataGrid(int pCompanyIndex, int pPage, int pLimit, string filter);
    }
}
