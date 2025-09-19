using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Repository.Interface;
using Microsoft.Extensions.Logging;

namespace EPAD_Repository.Impl
{
    public class IC_UserFaceTemplateRepository : BaseRepository<IC_UserFaceTemplate, EPAD_Context>, IIC_UserFaceTemplateRepository
    {
        readonly ILogger _logger;
        public IC_UserFaceTemplateRepository(ILoggerFactory loggerFactory, IUnitOfWork<EPAD_Context> unitOfWork, EPAD_Context appDbContext) 
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            _logger = loggerFactory.CreateLogger<IC_UserFaceTemplateRepository>();
        }
    }
}
