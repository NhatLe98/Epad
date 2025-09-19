using EPAD_Common.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Repository
{
    public interface IBaseRepository<U> where U : DbContext
    {

        U AppDbContext { get; }

        IRepository<T> GetRepository<T>() where T : class;

        IUnitOfWork UnitOfWork { get; }

        int SaveChange(bool ensureAutoHistory = false);

        Task<int> SaveChangeAsync(bool ensureAutoHistory = false);
    }

    public interface IBaseRepository<T, U> where T : class
    {
        IRepository<T> Repository { get; }
    }
}
