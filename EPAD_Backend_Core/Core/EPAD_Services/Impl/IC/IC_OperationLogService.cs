using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_OperationLogService : BaseServices<IC_OperationLog, EPAD_Context>, IIC_OperationLogService
    {
        public IC_OperationLogService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
