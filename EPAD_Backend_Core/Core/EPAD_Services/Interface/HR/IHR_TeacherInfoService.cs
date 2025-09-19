using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_TeacherInfoService : IBaseServices<HR_TeacherInfo, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(int pCompanyIndex, int pPage, int pLimit);

        public Task<List<HR_TeacherInfoResult>> GetAllTeacherInfo(string[] pEmployeeATIDs, int pCompanyIndex);

        public Task<HR_TeacherInfoResult> GetTeacherInfo(string pEmployeeATID, int pCompanyIndex);
    }
}
