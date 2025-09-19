using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_UserFingerService : BaseServices<IC_UserFinger, EPAD_Context>, IIC_UserFingerService
    {
        public IC_UserFingerService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
