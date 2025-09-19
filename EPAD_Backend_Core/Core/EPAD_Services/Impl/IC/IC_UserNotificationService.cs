using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using System;

namespace EPAD_Services.Impl
{
    public class IC_UserNotificationService : BaseServices<IC_UserNotification, EPAD_Context>, IIC_UserNotificationService
    {
        public IC_UserNotificationService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
