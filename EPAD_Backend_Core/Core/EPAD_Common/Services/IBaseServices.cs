using EPAD_Common.Repository;
using EPAD_Common.Types;
using EPAD_Common.UnitOfWork;
using EPAD_Common.UnitOfWork.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EPAD_Common.Services
{
    public interface IBaseServices<U> where U : DbContext
    {
        IBaseRepository<U> BaseRepository { get; }
        IUnitOfWork UnitOfWork { get; }
        U DbContext { get; }
        // IUserSession Context { get; }
    }

    public interface IBaseServices<T, U> : IBaseServices<U> where T : class where U : DbContext
    {
        IRepository<T> Repository { get; }

        DbSet<T> DbSet { get; }

        Task<T> GetById(object[] param);

        Task<IPagedList<T>> GetPagedListAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int pageIndex = 0,
            int pageSize = 10,
            bool disableTracking = true);

        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false);

        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false);

        Task<IEnumerable<T>> GetPagedListByIDAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int id = 0,
            int pageSize = 10,
            bool disableTracking = false);
        T AddOrUpdate(T entry, bool IgnoreEmpty = false);

        bool Any();

        bool Any(Expression<Func<T, bool>> predicate);

        //bool Any(Func<T, bool> predicate);

        T FirstOrDefault(Expression<Func<T, bool>> predicate = null, bool disableTracking = false);
        T FirstOrDefaultOrigin(Expression<Func<T, bool>> predicate = null, bool disableTracking = false);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, bool disableTracking = false);
        Task<T> FirstOrDefaultOriginAsync(Expression<Func<T, bool>> predicate = null, bool disableTracking = false);

        T Insert(T entry);

        T InsertWithOutModify(T entity);

        IEnumerable<T> Insert(IEnumerable<T> entities);

        Task<T> InsertAsync(T entry);

        IEnumerable<T> InsertMulti(T entry);

        Task<IEnumerable<T>> InsertMultiAsync(T entry);

        T Update(T entry);

        T UpdateWithoutModify(T entry);

        Task<T> UpdateAsync(T entry, params object[] param);

        T Delete(T entry);

        T Delete(object keyId);

        IEnumerable<T> Delete(Expression<Func<T, bool>> predicate = null);

        Task<T> DeleteAsync(object[] param);

        Task<T> DeleteAsync(object keyId);

        Task<IEnumerable<T>> DeleteAsync(Expression<Func<T, bool>> predicate = null);

        IEnumerable<T> Where(Func<T, bool> predicate, bool tracking = true);
        int Count(Func<T, bool> predicate, bool tracking = true);

        IQueryable<T> Query();

        DataGridClass GetDataGrid(Expression<Func<T, bool>> predicate, int pPage, int pCount);
    }
}
