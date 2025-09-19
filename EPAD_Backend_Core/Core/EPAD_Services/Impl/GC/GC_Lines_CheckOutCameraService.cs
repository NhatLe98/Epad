using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class GC_Lines_CheckOutCameraService : BaseServices<GC_Lines_CheckOutCamera, EPAD_Context>, IGC_Lines_CheckOutCameraService
    {
        public GC_Lines_CheckOutCameraService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<GC_Lines_CheckOutCamera>> GetDataByLineIndex(int lineIndex, int companyIndex)
        {

            return await DbContext.GC_Lines_CheckOutCamera.Where(x => x.CompanyIndex == companyIndex && x.LineIndex == lineIndex).ToListAsync();
        }
    }
}
