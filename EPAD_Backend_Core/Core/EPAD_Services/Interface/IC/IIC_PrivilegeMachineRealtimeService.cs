using EPAD_Common.Services;
using EPAD_Common.Types;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Data.Models.IC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_PrivilegeMachineRealtimeService : IBaseServices<IC_PrivilegeMachineRealtime, EPAD_Context>
    {
        Task<DataGridClass> GetPrivilegeMachineRealtime(string filter,
            int page, int pageSize, int companyIndex);
        List<string> GetPrivilegeMachineRealtimeByUserName(string userName, int companyIndex);
        Task<List<string>> AddPrivilegeMachineRealtime(IC_PrivilegeMachineRealtimeDTO param, UserInfo user);
        Task<List<string>> UpdatePrivilegeMachineRealtime(IC_PrivilegeMachineRealtimeDTO param, UserInfo user);
        Task<List<string>> DeletePrivilegeMachineRealtime(List<IC_PrivilegeMachineRealtimeDTO> param, UserInfo user);
    }
}
