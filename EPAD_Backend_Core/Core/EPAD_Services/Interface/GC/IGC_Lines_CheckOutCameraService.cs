using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_Lines_CheckOutCameraService : IBaseServices<GC_Lines_CheckOutCamera, EPAD_Context>
    {
        Task<List<GC_Lines_CheckOutCamera>> GetDataByLineIndex(int lineIndex, int companyIndex);
    }
}
