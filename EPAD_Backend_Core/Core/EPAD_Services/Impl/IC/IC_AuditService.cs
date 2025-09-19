using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_AuditService : BaseServices<IC_Audit, EPAD_Context>, IIC_AuditService
    {
        public IC_AuditService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
