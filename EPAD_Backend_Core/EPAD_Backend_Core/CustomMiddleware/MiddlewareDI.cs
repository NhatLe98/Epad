using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.CustomMiddleware
{
    public static class MiddlewareDI
    {
        public static void UseCustomMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<UserSessionMiddleware>();
            app.UseMiddleware<CaptureRequestMiddleware>();
            app.UseMiddleware<PrivilegeMiddleware>();
        }
    }
}
