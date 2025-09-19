using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.CustomMiddleware;
using EPAD_Background;
using EPAD_Common.Clients;
using EPAD_Common.EmailProvider;
using EPAD_Common.FileProvider;
using EPAD_Common.Locales;
using EPAD_Data;
using EPAD_Data.Models.EZHR;
using EPAD_Logic;
using EPAD_Repository;
using EPAD_Services;
using EPAD_Services.Business;
using EPAD_Services.Business.Parking;
using EPAD_Services.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPAD_Backend_Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMVCConfiguration();
            services.AddDbContextConfiguration(Configuration);
            services.AddJWTConfiguration(Configuration);
            services.AddSwaggerConfiguration();
            services.AddRepository();
            services.AddServices();
            services.AddScoped<CustomerProcess>();
            services.AddScoped<ParkingProcess>();
            services.RegisterProvider();
            services.RegisterPlugins();
            services.RegisterServices(Configuration);
            services.AddSingleton<ILocales, Locales>();
            services.AddLogic();
            services.AddClients();
            services.AddBackgroundTask();
            services.AddAutoMapper(typeof(EPAD_Common.VueHelper), typeof(Startup));
            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                opt.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddSpaStaticFiles(options => options.RootPath = "epad/dist");
            services.AddGrpc();

            //Add new EZHR API Client
            var ezhrConfig = Configuration.GetSection(EzHRConfiguration.Section).Get<EzHRConfiguration>();
            services.RegisterClientServices(ezhrConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, EPAD_Context context, IMemoryCache pCache)
        {
            app.UseWebSockets();
            app.CorsConfiguration();
            app.UseExceptionHandler(err => err.UseCustomErrorsHandler(env));
            app.UseHttpsRedirection();
            app.UseSession();
            app.EnrichLog();
            app.ID2Configuration();
            app.ComunicateTokenConfiguration();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCustomMiddleware();
            app.SwaggerConfiguration();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<Grpc.Services.EpadService>();
            });
            app.FileConfiguration(env);
            app.AfterConfiguration();

            // load devices list are monitoring device
            Misc.LoadMonitoringDeviceList(context, pCache);
        }
    }
}
