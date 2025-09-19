using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;

namespace EPAD_Services.Interface
{
    public interface IIC_GroupDeviceService : IBaseServices<IC_GroupDevice, EPAD_Context>
    {
        object GetGroupDeviceResult(UserInfo user);
        List<GroupDeviceParam> GetGroupDevice(int companyIndex);
    }
}
