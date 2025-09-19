using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_WorkingInfoService : IBaseServices<IC_WorkingInfo, EPAD_Context>
    {
        Task<List<IC_WorkingInfo>> GetDataByCompanyIndex(int companyIndex = 0, List<string> employeeIds = null, List<long> departmentIDs = null);
        Task<IC_WorkingInfo> GetNewestDataByEmployeeATID(int companyIndex, string employeeATID);
    }
}
