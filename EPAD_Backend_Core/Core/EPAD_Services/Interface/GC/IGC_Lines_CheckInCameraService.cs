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
    public interface IGC_Lines_CheckInCameraService : IBaseServices<GC_Lines_CheckInCamera, EPAD_Context>
    {
        Task<List<GC_Lines_CheckInCamera>> GetDataByLineIndex(int lineIndex, int companyIndex);
        Task<List<int>> GetAllCameraInAndOutByCompanyIndexAndIgnoreLineAndInOutMode(int companyIndex, int lineIndex, bool lineIn);

    }
}
