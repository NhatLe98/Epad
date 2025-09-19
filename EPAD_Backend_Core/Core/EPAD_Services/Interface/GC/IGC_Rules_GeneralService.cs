using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Rules_GeneralService : IBaseServices<GC_Rules_General, EPAD_Context>
    {
        Task<List<GC_Rules_General>> GetRulesGeneralByCompanyIndex(int companyIndex = 0);
        GC_Rules_General GetRulesGeneralByIndexAndCompanyIndex(int rulesGeneralIndex, int companyIndex = 0);
        Task<List<GC_Rules_General_Log>> GetRulesGeneralLog(int rulesGeneralIndex, int companyIndex = 0);
        GC_Rules_General GetRulesGeneralUsing(int companyIndex = 0);
    }
}
