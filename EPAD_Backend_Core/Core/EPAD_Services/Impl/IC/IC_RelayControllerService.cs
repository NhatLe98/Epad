using EPAD_Common.Services;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_RelayControllerService : BaseServices<IC_RelayController, EPAD_Context>, IIC_RelayControllerService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public IC_RelayControllerService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<IC_RelayController>> GetControllerByIndexes(List<int> indexes)
        {
            return await DbContext.IC_RelayController.AsNoTracking().Where(x
                => indexes.Contains(x.Index)).ToListAsync();
        }

        public async Task<Dictionary<int, bool>> MultipleTelnetRelayController(List<IC_RelayController> param)
        {
            Dictionary<int, bool> results = new Dictionary<int, bool>();
            List<Task> tasks = new List<Task>();

            foreach (var item in param) 
            {
                var key = item.Index;
                var task = ConnectToTelnet(item.IpAddress, item.Port).ContinueWith(t =>
                {
                    bool success = t.Result;
                    results.Add(key, success);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            return results;
        }

        private async Task<bool> ConnectToTelnet(string ipAddress, int port)
        {
            TcpClient client = new TcpClient();

            try
            {
                var test = client.BeginConnect(ipAddress, port, null, null);

                var success = test.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));

                if (!success)
                {
                    throw new Exception("CONNECT_FAILED");
                }

                client.EndConnect(test);
                client.Close();

                //await client.ConnectAsync(ipAddress, port);
                //client.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
