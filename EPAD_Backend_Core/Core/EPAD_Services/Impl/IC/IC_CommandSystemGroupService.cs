using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_CommandSystemGroupService : BaseServices<IC_CommandSystemGroup, EPAD_Context>, IIC_CommandSystemGroupService
    {
        public IC_CommandSystemGroupService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
