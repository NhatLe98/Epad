using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_PrivilegeDetailsService : BaseServices<IC_PrivilegeDetails, EPAD_Context>, IIC_PrivilegeDetailsService
    {
        public IC_PrivilegeDetailsService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
