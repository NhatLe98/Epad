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
    public interface IGC_Gates_LinesService : IBaseServices<GC_Gates_Lines, EPAD_Context>
    {
        Task<List<GC_Gates_Lines>> GetDataByGateIndexAndCompanyIndex(int gateIndex, int companyIndex);
        Task<List<GC_Gates_Lines>> GetDataByListGateIndexAndCompanyIndex(List<int> gateIndex, int companyIndex);
        Task<List<GC_Gates>> GetAllGates(int companyIndex);
        Task<List<GC_Gates_Lines>> GetAllGatesLines(int companyIndex);
        Task<bool> UpdateGateLine(GateLineParam param, UserInfo user);
    }
}
