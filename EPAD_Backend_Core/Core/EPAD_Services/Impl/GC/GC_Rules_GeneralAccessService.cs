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
    public class GC_Rules_GeneralAccessService : BaseServices<GC_Rules_GeneralAccess, EPAD_Context>, IGC_Rules_GeneralAccessService
    {
        public GC_Rules_GeneralAccessService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
