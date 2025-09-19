using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_HardwareLicenseService : BaseServices<IC_HardwareLicense, EPAD_Context>, IIC_HardwareLicenseService
    {
        public IC_HardwareLicenseService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
