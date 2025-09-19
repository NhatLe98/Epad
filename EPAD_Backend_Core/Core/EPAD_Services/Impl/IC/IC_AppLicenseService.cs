using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_AppLicenseService : BaseServices<IC_AppLicense, EPAD_Context>, IIC_AppLicenseService
    {
        public IC_AppLicenseService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
