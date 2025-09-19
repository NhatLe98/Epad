using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Rules_General_AreaGroupService : IBaseServices<GC_Rules_General_AreaGroup, EPAD_Context>
    {
        List<GC_Rules_General_AreaGroup> GetAreaGroupByRulesIndex(List<GC_Rules_General_AreaGroup> rulesAreaGroup, int index, int companyIndex);
    }
}
