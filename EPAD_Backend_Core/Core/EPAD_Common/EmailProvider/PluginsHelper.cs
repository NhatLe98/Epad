using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.EmailProvider
{
    public static class PluginsHelper
    {
        public static IServiceCollection RegisterPlugins(this IServiceCollection services)
        {
            services.AddTransient<IEmailSender, SmtpEmailSender>();
            return services;
        }
    }
}
