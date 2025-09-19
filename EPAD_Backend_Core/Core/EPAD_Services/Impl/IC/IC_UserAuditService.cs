using EPAD_Common.Services;
using EPAD_Data.Entities;
using EPAD_Data;
using EPAD_Services.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_UserAuditService : BaseServices<IC_UserAudit, EPAD_Context>, IIC_UserAuditService
    {
        public IC_UserAuditService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task InsertAudit(List<string> employeeATIDs)
        {
            
        }

        public async Task DeleteAudit (List<string> employeeATIDs)
        {

        }

        public async Task InsertFaceAudit(List<string> employeeATIDs)
        {

        }
    }
}
