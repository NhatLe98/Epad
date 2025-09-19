using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_ConfigByGroupMachineService : BaseServices<IC_ConfigByGroupMachine, EPAD_Context>, IIC_ConfigByGroupMachineService
    {
        public IC_ConfigByGroupMachineService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
