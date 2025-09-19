using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Repository.Interface;
using Microsoft.Extensions.Logging;

namespace EPAD_Repository.Impl
{
    public class HR_NannyInfoRepository : BaseRepository<HR_NannyInfo, EPAD_Context>, IHR_NannyInfoRepository
    {
        readonly ILogger _logger;
        public HR_NannyInfoRepository(ILoggerFactory loggerFactory, IUnitOfWork<EPAD_Context> unitOfWork, EPAD_Context appDbContext) 
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            _logger = loggerFactory.CreateLogger<HR_NannyInfoRepository>();
        }
    }
}
