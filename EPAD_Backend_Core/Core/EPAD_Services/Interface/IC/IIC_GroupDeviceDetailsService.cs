using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_GroupDeviceDetailsService : IBaseServices<IC_GroupDeviceDetails, EPAD_Context>
    {
        Task<bool> InsertOrUpdateGroupDevicesDetail(List<string> pSerialNumberList, int pGroupDeviceIndex, UserInfo user);
    }
}
