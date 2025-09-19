using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Repository.Interface1;
using Microsoft.Extensions.Logging;

namespace EPAD_Repository.Impl1
{
    public class HR_EmployeeRepository : BaseRepository<HR_Employee, ezHR_Context>, IHR_EmployeeRepository
    {
        readonly ILogger _logger;
        public HR_EmployeeRepository(ILoggerFactory loggerFactory, IUnitOfWork<ezHR_Context> unitOfWork, ezHR_Context appDbContext) 
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            _logger = loggerFactory.CreateLogger<HR_EmployeeRepository>();
        }
    }
}
