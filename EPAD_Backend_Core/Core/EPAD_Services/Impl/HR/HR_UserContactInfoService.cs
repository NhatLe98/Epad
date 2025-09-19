using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities.HR;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class HR_UserContactInfoService : BaseServices<HR_UserContactInfo, EPAD_Context>, IHR_UserContactInfoService
    {
        public HR_UserContactInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<List<HR_UserContactInfo>> GetUserContactInfoById(string EmployeeATID)
        {
            return await DbContext.HR_UserContactInfo.Where(x => x.UserIndex == EmployeeATID).ToListAsync();
        }
    }
}
