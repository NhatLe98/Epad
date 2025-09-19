using EPAD_Common;
using EPAD_Common.Repository;
using EPAD_Common.UnitOfWork;
using EPAD_Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EPAD_Repository
{
    public static class RepositoryHelper
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryFactory, UnitOfWork<EPAD_Context>>();
            services.AddScoped<IUnitOfWork, UnitOfWork<EPAD_Context>>();
            services.AddScoped<IUnitOfWork<EPAD_Context>, UnitOfWork<EPAD_Context>>();

            List<Dependency> dependencies = new List<Dependency>()
            {
                new Dependency(typeof(BaseRepository<EPAD_Context>), typeof(IBaseRepository<EPAD_Context>))
                //new Dependency(typeof(BaseRepository<ezHR_Context>), typeof(IBaseRepository<ezHR_Context>))
            };

            var assembly = Assembly.GetExecutingAssembly();
            var allClass = assembly.GetTypes()
                .Where(e => !string.IsNullOrEmpty(e.Namespace) && e.Namespace.EndsWith(".Impl"))
                .Where(e => e.IsClass && !e.IsAbstract && e.DeclaringType == null)
                .ToArray();

            if (allClass.Length > 0)
                dependencies.AddRange(allClass.Select(e => new Dependency(e)));

            foreach (var d in dependencies)
            {
                services.AddScoped(d.Interface, d.Implement);
            }
            return services;
        }
    }
}
