using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_AreaGroup_GroupDeviceService : IBaseServices<GC_AreaGroup_GroupDevice, EPAD_Context>
    {
        Task DeleteByAreaGroupIndex(int areaGroupIndex, int companyIndex);
        Task<List<GC_AreaGroup_GroupDevice>> GetDataByAreaGroupIndexAndCompanyIndex(int areaGroupIndex, int companyIndex);
        Task<List<GC_AreaGroup_GroupDevice>> GetDataByListAreaGroupIndexAndCompanyIndex(List<int> areaGroupIndex, int companyIndex);
    }
}
