using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_UserPrivilegeService : BaseServices<IC_UserPrivilege, EPAD_Context>, IIC_UserPrivilegeService
    {
        public IC_UserPrivilegeService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
