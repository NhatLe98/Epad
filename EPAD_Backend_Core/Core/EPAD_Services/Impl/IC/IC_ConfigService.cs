using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_ConfigService : BaseServices<IC_Config, EPAD_Context>, IIC_ConfigService
    {
        public IC_ConfigService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IC_Config> GetConfigByEventType(string pEventType, int pCompanyIndex)
        {
            return await FirstOrDefaultAsync(x => x.EventType == pEventType && x.CompanyIndex == pCompanyIndex);
        }
    }
}
