using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.FR05.Interface
{
    public interface IIC_DeviceMachineService
    {
        Task RestartDevice(CommandResult param);
        Task<DeviceParamInfo> GetDeviceInfo(CommandResult pSystemCommand);
        Task<DeviceCapacityParam> GetDeviceCapacityInfo(CommandResult pSystemCommand);
        Task SetDeviceTime(CommandResult param);
    }
}
