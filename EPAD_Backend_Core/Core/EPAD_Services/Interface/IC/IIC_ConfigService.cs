using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_ConfigService : IBaseServices<IC_Config, EPAD_Context>
    {
        public Task<IC_Config> GetConfigByEventType(string pEventType, int pCompanyIndex);
    }
}
