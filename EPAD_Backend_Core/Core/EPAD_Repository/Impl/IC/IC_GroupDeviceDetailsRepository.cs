using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Repository.Interface;
using Microsoft.Extensions.Logging;

namespace EPAD_Repository.Impl
{
    public class IC_GroupDeviceDetailsRepository : BaseRepository<IC_GroupDeviceDetails, EPAD_Context>, IIC_GroupDeviceDetailsRepository
    {
        readonly ILogger _logger;
        public IC_GroupDeviceDetailsRepository(ILoggerFactory loggerFactory, IUnitOfWork<EPAD_Context> unitOfWork, EPAD_Context appDbContext) 
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            _logger = loggerFactory.CreateLogger<IC_GroupDeviceDetailsRepository>();
        }
    }
}
