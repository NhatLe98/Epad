using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IGC_AreaGroupService : IBaseServices<GC_AreaGroup, EPAD_Context>
    {
        Task<Tuple<List<AreaGroupModel>, int>> GetAreaGroup(int page, string filter, int pageSize, UserInfo user);
        Task<bool> AddAreaGroup(AreaGroupRequestModel param, UserInfo user);
        Task<bool> UpdateAreaGroup(AreaGroupRequestModel param, UserInfo user);
        Task<bool> DeleteAreaGroup(List<int> indexes, UserInfo user);
        Task<List<GC_AreaGroup>> GetDataByCompanyIndex(int companyIndex);
        Task<GC_AreaGroup> GetDataByIndex(int index);
        Task<List<AreaGroupFullInfo>> GetFullDataByCompanyIndex(int companyIndex);
        Task<GC_AreaGroup> GetDataByCodeAndCompanyIndex(string code, int companyIndex);
        Task<List<GC_AreaGroup_GroupDevice>> GetDeviceByAreaAndCompanyIndex(int areaIndex, int companyIndex);
    }
}
