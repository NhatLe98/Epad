using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_AccessTokenService : BaseServices<IC_AccessToken, EPAD_Context>, IIC_AccessTokenService
    {
        public IC_AccessTokenService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
