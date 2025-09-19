using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EPAD_Data.Models;
using EPAD_Common.Types;

namespace EPAD_Services.Interface
{
    public interface IGC_AccessedGroupService : IBaseServices<GC_AccessedGroup, EPAD_Context>
    {
        Task<List<GC_AccessedGroup>> GetDataByCompanyIndex(int companyIndex);
        Task<List<GC_AccessedGroup>> GetAccessedGroupNormal(int companyIndex);
        GC_AccessedGroup GetDataByIndex(int index);
        Task<GC_AccessedGroup> GetDataByNameAndCompanyIndex(string name, int companyIndex);

        Task<DataGridClass> GetAccessedGroup(int page, int pageSize, string filter, UserInfo user);
        Task<bool> AddAreaGroup(GC_AccessedGroup param, UserInfo user);

        Task<List<GC_Rules_ParkingLot>> GetDataRulesParkingLotByCompanyIndex(int companyIndex);
        Task<List<GC_Rules_GeneralAccess>> GetDataRulesGeneralAccessByCompanyIndex(int companyIndex);
        bool CheckExistDriverGroup(int companyIndex, int index);
        bool CheckExistGuestGroup(int companyIndex, int index);
    }
}
