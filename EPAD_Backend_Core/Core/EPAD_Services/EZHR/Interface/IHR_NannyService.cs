using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_NannyService
    {
        public Task<List<HR_NannyInfoResult>> GetAllNannyInfo(string[] pEmployeeATIDs, int pCompanyIndex);
        public Task<HR_NannyInfoResult> GetNannyInfo(string pEmployeeATID, int pCompanyIndex);
    }
}
