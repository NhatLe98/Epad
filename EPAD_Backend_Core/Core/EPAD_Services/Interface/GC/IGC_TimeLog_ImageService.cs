using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_TimeLog_ImageService : IBaseServices<GC_TimeLog_Image, EPAD_Context>
    {
        Task AddTimeLogImage(GC_TimeLog_Image timeLog);
    }
}
