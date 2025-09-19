using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Collections.Generic;

namespace EPAD_Services.Interface
{
    public interface IGC_Rules_GeneralAccess_GatesService : IBaseServices<GC_Rules_GeneralAccess_Gates, EPAD_Context>
    {
        List<GC_Rules_GeneralAccess_Gates> GetListGateByRulesIndex(List<GC_Rules_GeneralAccess_Gates> rulesGates, int index, int companyIndex);
    }
}
