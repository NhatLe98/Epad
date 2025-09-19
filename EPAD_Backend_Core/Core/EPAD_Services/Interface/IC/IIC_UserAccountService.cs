using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;

namespace EPAD_Services.Interface
{
    public interface IIC_UserAccountService : IBaseServices<IC_UserAccount, EPAD_Context>
    {
        bool CheckAccountExisted(string username, string password);
    }
}
