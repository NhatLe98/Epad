using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Rules_General_AreaGroupService : BaseServices<GC_Rules_General_AreaGroup, EPAD_Context>, IGC_Rules_General_AreaGroupService
    {
        public GC_Rules_General_AreaGroupService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public List<GC_Rules_General_AreaGroup> GetAreaGroupByRulesIndex(List<GC_Rules_General_AreaGroup> rulesAreaGroup, int index, int companyIndex)
        {
            return rulesAreaGroup.Where(e => e.CompanyIndex == companyIndex && e.Rules_GeneralIndex == index).OrderBy(e => e.Priority).ToList();
        }
    }
}
