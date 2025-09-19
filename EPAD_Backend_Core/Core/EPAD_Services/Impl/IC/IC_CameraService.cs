using EPAD_Common.Extensions;
using EPAD_Common.Services;
using EPAD_Common.Types;
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
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Services.Impl
{
    public class IC_CameraService : BaseServices<IC_Camera, EPAD_Context>, IIC_CameraService
    {
        public EPAD_Context _dbContext;
        ConfigObject _Config;
        IMemoryCache _Cache;
        private ILogger _logger;
        private IConfiguration _configuration;
        private string _configClientName;
        public IC_CameraService(IServiceProvider serviceProvider, EPAD_Context context,
            ILoggerFactory loggerFactory, IConfiguration configuration) : base(serviceProvider)
        {
            _dbContext = context;
            _Cache = serviceProvider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _logger = loggerFactory.CreateLogger<GC_LinesService>();
            _configuration = configuration;
            _configClientName = _configuration.GetValue<string>("ClientName").ToUpper();
        }

        public async Task<List<IC_Camera>> GetAllCamera(int pCompanyIndex)
        {
            var dataQuery = _dbContext.IC_Camera.AsNoTracking().Where(x => x.CompanyIndex == pCompanyIndex);
            return await dataQuery.ToListAsync();
        }

        public CameraPictureResult GetCameraPictureByCameraIndex(int cameraIndex, string channel, UserInfo user)
        {
            IC_Camera cameraInfo = _dbContext.IC_Camera.Where(t => t.Index == cameraIndex).FirstOrDefault();
            if (cameraInfo == null)
            {
                return new CameraPictureResult { Success = false, Error = "Camera index invalid", Link = "" };
            }
            string error = "";
            string picturePath = ImageExtension.GetImageFromCamera(cameraInfo.IpAddress, cameraInfo.Port.ToString(),
                cameraInfo.UserName, cameraInfo.Password, channel, cameraInfo.Index, ref error, 4, user);
            if (error != "")
            {
                return new CameraPictureResult { Success = false, Error = error, Link = "" };
            }
            return new CameraPictureResult { Success = true, Error = "Camera index invalid", Link = "" };
        }
    }
}
