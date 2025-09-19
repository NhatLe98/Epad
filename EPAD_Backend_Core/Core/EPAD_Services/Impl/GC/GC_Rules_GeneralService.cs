using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Rules_GeneralService : BaseServices<GC_Rules_General,EPAD_Context>, IGC_Rules_GeneralService
    {
        public GC_Rules_GeneralService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<GC_Rules_General>> GetRulesGeneralByCompanyIndex(int companyIndex = 0)
        {
            return await DbContext.GC_Rules_General.AsNoTracking().Where(x => x.CompanyIndex == companyIndex).ToListAsync();
        }
        public GC_Rules_General GetRulesGeneralByIndexAndCompanyIndex(int rulesGeneralIndex, int companyIndex = 0)
        {
            return DbContext.GC_Rules_General.FirstOrDefault(x => x.CompanyIndex == companyIndex && x.Index == rulesGeneralIndex);
        }
        public async Task<List<GC_Rules_General_Log>> GetRulesGeneralLog(int rulesGeneralIndex, int companyIndex = 0)
        {
            return await DbContext.GC_Rules_General_Log.Where(x => x.RuleGeneralIndex == rulesGeneralIndex && x.CompanyIndex == companyIndex).OrderBy(e => e.Index).ToListAsync();
        }
        public GC_Rules_General GetRulesGeneralUsing(int companyIndex = 0)
        {
            return DbContext.GC_Rules_General.FirstOrDefault(x => x.CompanyIndex == companyIndex && x.IsUsing == true);
        }
    }
}
