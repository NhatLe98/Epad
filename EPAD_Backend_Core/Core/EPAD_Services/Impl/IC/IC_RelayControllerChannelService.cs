using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_RelayControllerChannelService : BaseServices<IC_RelayControllerChannel, EPAD_Context>, IIC_RelayControllerChannelService
    {
        public IC_RelayControllerChannelService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
