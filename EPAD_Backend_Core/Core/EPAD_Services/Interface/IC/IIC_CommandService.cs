using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_CommandService : IBaseServices<IC_Command, EPAD_Context>
    {
        Task UploadACUsers(int groupIndex, List<string> listUser, UserInfo user);
        Task UploadTimeZone(int groupIndex, UserInfo user);
        Task UploadUsers(int groupIndex, List<string> listUser, UserInfo user);
    }
}
