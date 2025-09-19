using EPAD_Backend_Core.Models.DTOs;
using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.HR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_StudentInfoService : IBaseServices<HR_StudentInfo, EPAD_Context>
    {
        public Task<DataGridClass> GetDataGrid(string pFilter, string[] pClassID, int pCompanyIndex, int pPage, int pLimit);

        public Task<List<HR_StudentInfoResult>> GetAllStudentInfo(string[] pStudentATIDs, int pCompanyIndex);
        public Task<List<VStarStudentInfoResult>> GetAllStudentInfoVStar(string[] pStudentATIDs, int pCompanyIndex);
        public Task<List<object>> GetAllStudentClassInfo(int pCompanyIndex);

        Task<List<HR_StudentInfoResult>> GetAllStudentInfoByClassIDs(string pClassIDs, int pCompanyIndex);

        public Task<HR_StudentInfoResult> GetStudentInfo(string pEmployeeATID, int pCompanyIndex);
        public Task<List<IC_StudentImportDTO>> ValidationImportStudent(List<IC_StudentImportDTO> param);
        public Task<List<VStarStudentInfoResult>> GetAllStudentInfoVStarType(string[] pStudentATIDs, int pCompanyIndex, long type);
    }
}
