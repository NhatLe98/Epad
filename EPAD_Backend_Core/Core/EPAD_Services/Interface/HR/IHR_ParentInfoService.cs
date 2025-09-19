using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_ParentInfoService : IBaseServices<HR_ParentInfo, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(string filter, int pCompanyIndex, int pPage, int pLimit);

        public Task<List<HR_ParentInfoResult>> GetAllParentInfo(string[] pEmployeeATIDs, int pCompanyIndex);

        public Task<HR_ParentInfoResult> GetParentInfo(string pEmployeeATID, int pCompanyIndex);
        public Task<List<IC_ParentImportDTO>> ValidationImportParent(List<IC_ParentImportDTO> param);
    }
}
