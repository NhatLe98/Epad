using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPAD_Services.Impl
{
    public class GC_Rules_GeneralAccess_GatesService : BaseServices<GC_Rules_GeneralAccess_Gates, EPAD_Context>, IGC_Rules_GeneralAccess_GatesService
    {
        public GC_Rules_GeneralAccess_GatesService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public List<GC_Rules_GeneralAccess_Gates> GetListGateByRulesIndex(List<GC_Rules_GeneralAccess_Gates> rulesGates, int index, int companyIndex)
        {
            return rulesGates.Where(e => e.CompanyIndex == companyIndex && e.RulesGeneralIndex == index).ToList();
        }
    }
}
