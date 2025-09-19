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
    public interface IGC_GatesService : IBaseServices<GC_Gates, EPAD_Context>
    {
        Task<List<GC_Gates>> GetDataByCompanyIndex(int companyIndex);
        Task<GC_Gates> GetDataByIndex(int index);
        Task<GC_Gates> GetDataByNameAndCompanyIndex(string name, int companyIndex);
        Task<List<int>> GetListIndexByGateMandatory(int companyIndex);
        Task<bool> AddGates(GatesModel param, UserInfo user);
        Task<bool> UpdateGates(GatesModel param, UserInfo user);
        Task<bool> DeleteGates(List<int> indexes);
        Task<bool> CheckGateUsing(List<int> indexes);
        Task<bool> UpdateGateLineDevice(GatesModel param, UserInfo user);
    }
}
