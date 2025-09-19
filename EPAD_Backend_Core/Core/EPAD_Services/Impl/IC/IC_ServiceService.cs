using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_ServiceService : BaseServices<IC_Service, EPAD_Context>, IIC_ServiceService
    {
        public IC_ServiceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
