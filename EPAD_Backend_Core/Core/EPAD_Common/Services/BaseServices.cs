using EPAD_Common.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;
using EPAD_Common.Extensions;
using EPAD_Common.UnitOfWork;
using EPAD_Common.UnitOfWork.Collections;
using EPAD_Common.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EPAD_Common.Services
{
    public abstract class BaseServices<U> : IBaseServices<U> where U : DbContext
    {
        public IBaseRepository<U> BaseRepository { get; }

        public IUnitOfWork UnitOfWork { get; }

        public U DbContext { get; }

        public IHttpContextAccessor HttpContext;

        protected IServiceProvider ServiceProvider;

        public BaseServices(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DbContext = serviceProvider.GetService<U>();
            BaseRepository = serviceProvider.GetService<IBaseRepository<U>>();
            UnitOfWork = BaseRepository.UnitOfWork;
            HttpContext = serviceProvider.GetService<IHttpContextAccessor>();
        }
    }

    public abstract class BaseServices<T, U> : BaseServices<U>, IBaseServices<T, U>
        where T : class, new()
        where U : DbContext
    {
        public Expression<Func<T, bool>> GlobalFilter { get; }
        public IRepository<T> Repository { get; }

        public DbSet<T> DbSet { get; }

        public AutoMapper.IMapper _Mapper;

        public IMemoryCache _Cache;

        public BaseServices(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Repository = BaseRepository.GetRepository<T>();
            DbSet = DbContext.Set<T>();

            _Mapper = serviceProvider.GetService<AutoMapper.IMapper>();
            _Cache = serviceProvider.GetService<IMemoryCache>();
        }

        public virtual async Task<T> GetById(object[] param)
        {
            return await Repository.FindAsync(param);
        }
        public virtual IPagedList<T> GetPagedList(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int pageIndex = 0, int pageSize = 10, bool disableTracking = false)
        {
            return Repository.GetPagedList(predicate, orderBy, include, pageIndex, pageSize, disableTracking);
        }

        public virtual async Task<IPagedList<T>> GetPagedListAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int pageIndex = 0, int pageSize = 10, bool disableTracking = false)
        {
            // var condition = CompanyFilter().AndAlso(predicate);
            var result = await Repository.GetPagedListAsync(predicate, orderBy, include, pageIndex, pageSize, disableTracking);
            return result;
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false)
        {
            var dummy = GetPagedList(predicate, orderBy, include, 0, int.MaxValue, disableTracking);
            return dummy.Items;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = false)
        {
            var dummy = await GetPagedListAsync(predicate, orderBy, include, 0, int.MaxValue, disableTracking);
            return dummy.Items;
        }

        /// <summary>
        /// Need CompanyIndex
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="disableTracking"></param>
        /// <returns></returns>
        public T FirstOrDefault(Expression<Func<T, bool>> predicate = null, bool disableTracking = false)
        {
            // var condition = CompanyFilter().AndAlso(predicate);
            return Repository.GetFirstOrDefault<T>(e => e, predicate, disableTracking: disableTracking);
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, bool disableTracking = false)
        {
            // var condition = CompanyFilter().AndAlso(predicate);
            return Repository.GetFirstOrDefaultAsync<T>(e => e, predicate, disableTracking: disableTracking);
        }

        public T FirstOrDefaultOrigin(Expression<Func<T, bool>> predicate = null, bool disableTracking = false)
        {
            return Repository.GetFirstOrDefault<T>(e => e, predicate, disableTracking: disableTracking);
        }

        public Task<T> FirstOrDefaultOriginAsync(Expression<Func<T, bool>> predicate = null, bool disableTracking = false)
        {
            return Repository.GetFirstOrDefaultAsync<T>(e => e, predicate, disableTracking: disableTracking);
        }

        public bool Any() => DbSet.Any();

        public bool Any(Expression<Func<T, bool>> predicate) => DbSet.Any(predicate);

        public virtual T AddOrUpdate(T entity, bool IgnoreEmpty = false)
        {
            entity = ModifyEntry(entity);
            var entry = DbContext.Entry(entity);

            var pr = entry.Context.Model.FindEntityType(type: typeof(T)).FindPrimaryKey().Properties.Select(n => entity.GetType().GetProperty(n.Name).GetValue(entity)).ToArray();
            var dummy = DbSet.Find(pr);
            if (dummy != null)
            {
                if (!IgnoreEmpty)
                    dummy.PopulateWith(entity);
                else
                    dummy.PopulateWithoutNullValue(entity);
            }
            else
            {
                entry.State = EntityState.Added;
            }
            return dummy ?? entry.Entity;
        }

        public virtual T Insert(T entry)
        {
            entry = ModifyEntry(entry);
            Repository.Insert(entry);
            return entry;
        }

        public virtual T InsertWithOutModify(T entry)
        {
            Repository.Insert(entry);
            return entry;
        }

        public virtual IEnumerable<T> Insert(IEnumerable<T> entities)
        {
            var editEntities = entities.ToList();
            for (int i = 0; i < editEntities.Count(); i++)
            {
                editEntities[i] = ModifyEntry(editEntities[i]);
            }
            Repository.Insert(editEntities);
            return entities;
        }

        public virtual async Task<T> InsertAsync(T entry)
        {
            entry = ModifyEntry(entry);
            await Repository.InsertAsync(entry);
            return entry;
        }

        public virtual IEnumerable<T> InsertMulti(T entry)
        {
            var emId = entry.TryGetValue<string>("EmployeeATID");
            if (!string.IsNullOrEmpty(emId))
            {
                var employeeId = emId.Split(new char[] { '|', ';' });
                if (employeeId != null && employeeId.Length > 0)
                {
                    List<T> lstTemp = new List<T>();
                    entry = ModifyEntry(entry);
                    foreach (var n in employeeId)
                    {
                        var dummy = new T().PopulateWith(entry);
                        dummy.TrySetValue("EmployeeATID", n);
                        lstTemp.Add(dummy);
                        Repository.Insert(dummy);
                    }
                    return lstTemp;
                }
            }
            throw new Exception("");
        }

        public virtual async Task<IEnumerable<T>> InsertMultiAsync(T entry)
        {
            var emId = entry.TryGetValue<string>("EmployeeATID");
            if (!string.IsNullOrEmpty(emId))
            {
                var employeeId = emId.Split(new char[] { '|', ';' });
                if (employeeId != null && employeeId.Length > 0)
                {
                    List<T> lstTemp = new List<T>();
                    entry = ModifyEntry(entry);
                    foreach (var n in employeeId)
                    {
                        var dummy = new T().PopulateWith(entry);
                        dummy.TrySetValue("EmployeeATID", n);
                        lstTemp.Add(dummy);
                        await Repository.InsertAsync(dummy);
                    }
                    return lstTemp;
                }
            }
            throw new Exception("");
        }

        public virtual T Update(T entry)
        {
            entry = ModifyEntry(entry);
            Repository.Update(entry);
            return entry;
        }

        public virtual T UpdateWithoutModify(T entry)
        {
            Repository.Update(entry);
            return entry;
        }

        public virtual async Task<T> UpdateAsync(T entry, params object[] param)
        {
            // ModifyEntry làm cho entry không nhận state update
            entry = ModifyEntry(entry);
            T item = await Repository.FindAsync(param);
            item = _Mapper.Map<T, T>(entry, item);
            // Repository.Update(item);
            return item;
        }

        public virtual T Delete(object keyId)
        {
            T item = Repository.Find(keyId);
            if (item != null)
                Repository.Delete(item);
            return item;
        }

        public virtual T Delete(T entry)
        {
            Repository.Delete(entry);
            return entry;
        }

        public IEnumerable<T> Delete(Expression<Func<T, bool>> predicate = null)
        {
            var dummy = GetAll(predicate);
            if (dummy != null && dummy.Count() > 0)
            {
                Repository.Delete(dummy);
            }
            return dummy;
        }

        public virtual async Task<T> DeleteAsync(params object[] param)
        {
            var item = await Repository.FindAsync(param);
            if (item != null)
                Repository.Delete(item);
            return item;
        }

        public virtual async Task<T> DeleteAsync(object keyId)
        {
            T item = await Repository.FindAsync(keyId);
            if (item != null)
                Repository.Delete(item);
            return item;
        }

        public virtual async Task<IEnumerable<T>> DeleteAsync(Expression<Func<T, bool>> predicate = null)
        {
            var dummy = await GetAllAsync(predicate);
            if (dummy != null && dummy.Count() > 0)
            {
                foreach (var item in dummy)
                {
                    Repository.Delete(item);
                }
            }
            return dummy;
        }

        public virtual IEnumerable<T> Where(Func<T, bool> predicate, bool tracking = true)
            => tracking ? DbSet.Where(predicate) : DbSet.AsNoTracking().Where(predicate);

        public virtual int Count(Func<T, bool> predicate, bool tracking = true)
            => tracking ? DbSet.Count(predicate) : DbSet.AsNoTracking().Count(predicate);

        public virtual IQueryable<T> Query() => DbSet.AsQueryable();

        public virtual T FirstOrDefault(Expression<Func<T, bool>> predicate = null)
        {
            // var condition = CompanyFilter().AndAlso(predicate);
            var dummy = Repository.GetFirstOrDefault(predicate, null, null, true);
            return dummy;
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null)
        {
            var dummy = await Repository.GetFirstOrDefaultAsync(predicate, null, null, true);
            return dummy;
        }

        private T ModifyEntry(T entry)
        {
            entry.SetDefaultString();
            var UpdatedDate = entry.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals("UpdatedDate"));
            if (UpdatedDate != null)
                UpdatedDate.SetValue(entry, DateTime.Now);

            var UpdatedUser = entry.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals("UpdatedUser"));
            if (UpdatedUser != null && HttpContext.GetCurrentUserName() != "")
                UpdatedUser.SetValue(entry, HttpContext.GetCurrentUserName());

            var CompanyIndex = entry.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals("CompanyIndex"));
            if (CompanyIndex != null && HttpContext.GetCompanyIndex() != null && (CompanyIndex.GetValue(entry) == null) || (int)CompanyIndex.GetValue(entry) == 0)
                CompanyIndex.SetValue(entry, HttpContext.GetCompanyIndex());
            return entry;
        }

        public async Task<IEnumerable<T>> GetPagedListByIDAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int id = 0, int pageSize = 10, bool disableTracking = false)
        {

            IQueryable<T> query = DbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            var hasIndex = typeof(T).HasProperty("Index");
            if (predicate != null)
            {
                var conditon = predicate;
                if (hasIndex)
                {
                    conditon = predicate.AndAlso(e => (e.TryGetValue<int>("Index") > id));
                }
                else
                {
                    conditon = predicate.AndAlso(e => true);
                }
                query = query.Where(conditon);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else if (hasIndex)
            {
                query = query.OrderBy(p => EF.Property<int>(p, "Index"));
            }

            return await query.Take(pageSize).ToListAsync();
        }

        public virtual DataGridClass GetDataGrid(Expression<Func<T, bool>> predicate, int pPage, int pCount)
        {
            var dummy = Repository.GetPagedList(predicate, null, null, pPage, pCount, false);
            var rs = new DataGridClass(dummy.TotalCount, dummy.Items);
            return rs;
        }
    }
}
