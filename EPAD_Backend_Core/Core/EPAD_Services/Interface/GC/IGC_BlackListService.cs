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
    public interface IGC_BlackListService : IBaseServices<GC_BlackList, EPAD_Context>
    {
        Task<DataGridClass> GetByFilter(DateTime fromDate, DateTime? toDate,
            string filter, int page, int pageSize, int pCompanyIndex);

        List<GC_BlackList> CheckExistBlackList(bool isEmployeeSystem, string pEmployeeATID, string nRIC, DateTime pFromDate, DateTime? pToDate, int pCompanyIndex);

        bool GetByFilterAndCompanyIndexExcludeThis(bool isEmployeeSystem, string pEmployeeATID, string nRIC, DateTime pFromDate, DateTime? pToDate, long pIndex, int pCompanyIndex);

        Task<GC_BlackList> GetDataByIndex(long pIndex);
        Task<GC_BlackList> AddBlackList(BlackListParams param, UserInfo user);
        Task<GC_BlackList> UpdateBlackList(BlackListParams param, UserInfo user);
        Task<bool> DeleteBlackList(List<long> indexes);
        Task<List<BlackListParams>> ImportBlackList(List<BlackListParams> param, UserInfo user);
        Task<bool> RemoveEmployeeInBlackList(RemoveEmployeeInBlackListParam param, UserInfo user);
        Task CreateCommandBlacklist(List<long> indexs, UserInfo user);
        Task DeleteBlackListByCreateCommand(List<long> indexs, UserInfo user);
    }
}
