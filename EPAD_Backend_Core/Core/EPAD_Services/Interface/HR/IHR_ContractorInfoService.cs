using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_ContractorInfoService : IBaseServices<HR_ContractorInfo, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(int companyIndex, int page, int pageSize);
        public Task<List<HR_ContractorInfoResult>> GetAllContractorInfo(string[] pContractorATIDs, int companyIndex);
        public Task<HR_ContractorInfoResult> GetContractorInfo(string employeeATID, int companyIndex);
    }
}
