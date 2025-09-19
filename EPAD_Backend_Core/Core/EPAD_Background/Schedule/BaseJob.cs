using EPAD_Common.Clients;
using EPAD_Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;

namespace EPAD_Background.Schedule
{
    public abstract class BaseJob : IJob
    {
        protected readonly EPAD_Context _db;
        protected IMemoryCache _cache;
        protected readonly IServiceScopeFactory _provider;
        protected readonly IServiceScope _scope;
        protected readonly LicenseClient _licenseClient;

        public BaseJob(IServiceScopeFactory provider)
        {
            _provider = provider;
            _scope = provider.CreateScope();
            _db = TryRessolve<EPAD_Context>();
            _cache = TryRessolve<IMemoryCache>();
            _licenseClient = TryRessolve<LicenseClient>();
        }

        public T TryRessolve<T>()
        {
            return _scope.ServiceProvider.GetService<T>();
        }
        public virtual Task Execute(IJobExecutionContext context) => throw new System.NotImplementedException();
    }
}
