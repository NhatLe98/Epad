using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_ShiftService : BaseServices<IC_Shift, EPAD_Context>, IIC_ShiftService
    {
        public IC_ShiftService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
