using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_UserFaceTemplate_v2Service : BaseServices<IC_UserFaceTemplate_v2, EPAD_Context>, IIC_UserFaceTemplate_v2Service
    {
        public IC_UserFaceTemplate_v2Service(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
