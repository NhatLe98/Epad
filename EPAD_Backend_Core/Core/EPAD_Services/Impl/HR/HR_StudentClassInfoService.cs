using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EPAD_Services.Impl
{
    public class HR_StudentClassInfoService : BaseServices<HR_StudentClassInfo, EPAD_Context>, IHR_StudentClassInfoService
    {
        public HR_StudentClassInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
        public async Task<List<HR_StudentClassInfo>> CheckExistedOrCreate(List<HR_StudentClassInfo> listClassInfo) {

            return null;
        }
    }
}
