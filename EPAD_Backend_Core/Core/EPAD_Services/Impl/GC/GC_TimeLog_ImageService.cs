using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_TimeLog_ImageService : BaseServices<GC_TimeLog_Image, EPAD_Context>, IGC_TimeLog_ImageService
    {
        public GC_TimeLog_ImageService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task AddTimeLogImage(GC_TimeLog_Image timeLog)
        {
            await DbContext.AddAsync(timeLog);
            await DbContext.SaveChangesAsync();
        }
    }
}
