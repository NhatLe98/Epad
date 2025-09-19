using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Entities.HR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IHR_UserContactInfoService: IBaseServices<HR_UserContactInfo, EPAD_Context>
    {
        public Task<List<HR_UserContactInfo>> GetUserContactInfoById(string EmployeeATID);
    }
}
