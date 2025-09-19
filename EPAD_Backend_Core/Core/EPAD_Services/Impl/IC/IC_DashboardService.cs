using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_DashboardService : BaseServices<IC_Dashboard, EPAD_Context>, IIC_DashboardService
    {
        private ILogger _logger;
        private readonly ILoggerFactory _LoggerFactory;
        private readonly EPAD_Context _context;
        private IConfiguration _Configuration;
        private string _configClientName;
        public IC_DashboardService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            IConfiguration configuration) : base(serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<IC_DeviceService>();
            _context = serviceProvider.GetService<EPAD_Context>();
            _Configuration = configuration;
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<bool> SaveDashboard(string userName,string jsonString)
        {
            try
            {
                var existedDashboard = _context.IC_Dashboard.FirstOrDefault(x => x.UserName == userName);
                if (existedDashboard != null)
                {
                    existedDashboard.DashboardConfig = jsonString;
                    existedDashboard.UpdatedDate = DateTime.Now;
                    existedDashboard.UpdatedUser = userName;
                    _context.IC_Dashboard.Update(existedDashboard);
                }
                else
                {
                    existedDashboard = new IC_Dashboard();
                    existedDashboard.UserName = userName;
                    existedDashboard.DashboardConfig = jsonString;
                    existedDashboard.CreatedDate = DateTime.Now;
                    existedDashboard.UpdatedDate = DateTime.Now;
                    existedDashboard.UpdatedUser = userName;
                    _context.IC_Dashboard.Add(existedDashboard);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetDashboard(string userName)
        {
            try
            {
                var existedDashboard = _context.IC_Dashboard.FirstOrDefault(x => x.UserName == userName);
                var result = string.Empty;
                if (existedDashboard != null)
                {
                    result = existedDashboard.DashboardConfig;
                }
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
