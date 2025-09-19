using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace EPAD_Backend_Core.CustomMiddleware
{
    public static class ErrorHandler
    {
        public static void UseCustomErrorsHandler(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.Use(WriteDevelopmentResponse);
            }
            else
            {
                app.Use(WriteProductionResponse);
            }
        }

        private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
        => WriteResponse(httpContext, includeDetails: true);

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: false);

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            if (ex != null)
            {
                var logger = httpContext.RequestServices.GetService<ILogger>();
                logger?.LogError(ex, "System error");
                httpContext.Response.ContentType = "application/problem+json";
                var details = ex.Message;
                var stackTracce = includeDetails ? ex.StackTrace : null;

                var problem = new ErrorDetails
                {
                    Status = "false",
                    MessageCode = "Exception",
                    MessageDetail = details
                };

                var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
                if (traceId != null)
                {
                    problem.Extensions["traceId"] = traceId;
                }
                if (stackTracce != null)
                {
                    problem.Extensions["stackTrace"] = stackTracce;
                }
                string json = JsonConvert.SerializeObject(problem);

                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await httpContext.Response.WriteAsync(json);
            }
        }
    }

    public class ErrorDetails
    {
        public ErrorDetails()
        {
            Extensions = new Dictionary<string, object>();
        }

        public string Status { get; set; }
        public string MessageCode { get; set; }
        public string MessageDetail { get; set; }
        public IDictionary<string, object> Extensions { get; }
    }
}
