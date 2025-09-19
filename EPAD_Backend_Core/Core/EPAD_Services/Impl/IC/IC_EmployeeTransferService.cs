using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_EmployeeTransferService : BaseServices<IC_EmployeeTransfer, EPAD_Context>, IIC_EmployeeTransferService
    {
        public IC_EmployeeTransferService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
