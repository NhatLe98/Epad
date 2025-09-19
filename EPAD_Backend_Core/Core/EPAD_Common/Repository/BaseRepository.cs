using EPAD_Common.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Repository
{
    public class BaseRepository<U> : IBaseRepository<U> where U : DbContext
    {
        readonly ILoggerFactory _loggerFactory;
        public IUnitOfWork UnitOfWork { get; }
        public U AppDbContext { get; }
        public BaseRepository(ILoggerFactory loggerFactory, IUnitOfWork<U> unitOfWork, U appDbContext)
        {
            _loggerFactory = loggerFactory;
            AppDbContext = appDbContext;
            UnitOfWork = unitOfWork;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return UnitOfWork.GetRepository<T>();
        }

        public int SaveChange(bool ensureAutoHistory = false)
        {
            return UnitOfWork.SaveChanges(ensureAutoHistory);
        }

        public async Task<int> SaveChangeAsync(bool ensureAutoHistory = false)
        {
            return await UnitOfWork.SaveChangesAsync(ensureAutoHistory);
        }
    }

    public abstract class BaseRepository<T, U> : BaseRepository<U>, IBaseRepository<T, U> where T : class where U : DbContext
    {
        public IRepository<T> Repository { get; }
        public BaseRepository(ILoggerFactory loggerFactory, IUnitOfWork<U> unitOfWork, U appDbContext)
            : base(loggerFactory, unitOfWork, appDbContext)
        {
            Repository = GetRepository<T>();
        }
    }
}
