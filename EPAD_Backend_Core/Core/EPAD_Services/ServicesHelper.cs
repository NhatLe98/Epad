using EPAD_Common;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EPAD_Services
{
    public static class ServicesHelper
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            List<Dependency> dependencies = new List<Dependency>()
            {
               // new Dependency(typeof(BaseServices<EPAD_Context>), typeof(IBaseServices<EPAD_Context>)),
               // new Dependency(typeof(BaseServices<ezHR_Context>), typeof(IBaseServices<ezHR_Context>))
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
