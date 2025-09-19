using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.FileProvider
{
    public static class ProviderHelper
    {
        public static IServiceCollection RegisterProvider(this IServiceCollection services)
        {
            services.AddScoped<IStoreFileProvider, StoreFileProvider>();
            return services;
        }
    }
}
