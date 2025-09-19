using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_UserInfoService : BaseServices<IC_UserInfo, EPAD_Context>, IIC_UserInfoService
    {
        public IC_UserInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
