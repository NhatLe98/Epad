using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_RelayControllerService : IBaseServices<IC_RelayController, EPAD_Context>
    {
        Task<List<IC_RelayController>> GetControllerByIndexes(List<int> indexes);
        Task<Dictionary<int, bool>> MultipleTelnetRelayController(List<IC_RelayController> dicIPAndPort);
    }
}
