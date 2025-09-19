using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using EPAD_Data.Models.HR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_PositionInfoService : IBaseServices<HR_PositionInfo, EPAD_Context>
    {
        Task<List<HR_PositionInfoResult>> GetAllPositionInfo(int companyIndex);
        Task<HR_PositionInfoResult> GetPositionInfoByIndex(long index,int companyIndex);
        Task<List<HR_PositionInfoResult>> GetListPositionInfoByIndex(long[] index, int companyIndex);
        Task<DataGridClass> GetPage(List<AddedParam> addedParam, int pCompanyIndex);
    }
}
