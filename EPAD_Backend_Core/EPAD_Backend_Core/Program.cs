using System;
using System.Net;
using System.Threading.Tasks;
using EPAD_Data.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace EPAD_Backend_Core
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();

            //await host.EnsureSeedData(); //nhằm mục đích là tự update migrate trên môi trường production mỗi khi update source
#if !DEBUG
            await host.EnsureSeedData();
#endif

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            ///
            /// Tại sao lại ConfigureAppConfiguration trong Program ?
            /// => Vì configurate trong này thì những chỗ khác trong DI mới nhận được file json customize (VD: Integrate.json)
            /// còn configurate trong Startup thì chỉ trong Startup nhận được thôi
            ///
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var hostBulder = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("Integrate.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseSerilog()
                .UseStartup<Startup>();

#if DEBUG
            //hostBulder.ConfigureKestrel(opt =>
            //{
            //    opt.Listen(IPAddress.Loopback, 5000, x => x.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
            //});
#endif

            return hostBulder;
        }
    }
}
