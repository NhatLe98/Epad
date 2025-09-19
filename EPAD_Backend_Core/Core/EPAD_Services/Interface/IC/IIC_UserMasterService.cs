using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_UserMasterService : IBaseServices<IC_UserMaster, EPAD_Context>
    {
        Task DownloadUserMasterSDK(UserInfoPram Pram, UserInfo user, ConfigObject config);
        Task<byte[]> GetFaceByEmployeeATID(string employeeATID);
    }
}
