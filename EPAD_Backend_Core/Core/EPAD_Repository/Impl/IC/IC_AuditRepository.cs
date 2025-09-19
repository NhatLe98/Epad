using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Repository.Interface;
using Microsoft.Extensions.Logging;

namespace EPAD_Repository.Impl
{
    public class IC_AuditRepository : BaseRepository<IC_Audit, EPAD_Context>, IIC_AuditRepository
    {
        readonly ILogger _logger;
        public IC_AuditRepository(ILoggerFactory loggerFactory, IUnitOfWork<EPAD_Context> unitOfWork, EPAD_Context appDbContext) 
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            _logger = loggerFactory.CreateLogger<IC_AuditRepository>();
        }
    }
}
