using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using System.Threading.Tasks;

namespace EPAD_Services.Interface
{
    public interface IIC_DashboardService : IBaseServices<IC_Dashboard, EPAD_Context>
    {
        Task<bool> SaveDashboard(string userName, string jsonString);
        Task<string> GetDashboard(string userName);
    }
}
