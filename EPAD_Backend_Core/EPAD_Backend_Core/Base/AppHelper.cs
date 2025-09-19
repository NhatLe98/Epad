using EPAD_Backend_Core.WebUtilitys;
using EPAD_Common;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Models;
using EPAD_Logic.MainProcess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.IO;
using System.Text;

namespace EPAD_Backend_Core.Base
{
    public static class AppHelper
    {
        public static IServiceCollection AddDbContextConfiguration(this IServiceCollection services, IConfiguration Configuration)
        {
            // this connecttion using for epad db
            //services.AddDbContext<EPAD_Context>(o => o.UseSqlServer(connectionString: Configuration.GetConnectionString("connectionString"))
            //    .ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>(), ServiceLifetime.Transient);
            services.AddDbContext<EPAD_Context>(o => o.UseSqlServer(Configuration.GetConnectionString("connectionString")), ServiceLifetime.Transient);

            // this connect tion using for integrate with ezHR system
            string connectionStringOther = Configuration.GetConnectionString("connectionStringOtherDB");
            if (connectionStringOther != null && connectionStringOther != "")
            {
                services.AddDbContext<ezHR_Context>(o => o.UseSqlServer(connectionStringOther));
            }
            else
            {
                services.AddDbContext<ezHR_Context>();
            }

            // ths connect using only for integrate employee from customer with only 1 table format
            string connectionStringCustomer = Configuration.GetConnectionString("connectionStringCustomerDB");
            if (connectionStringCustomer != null && connectionStringCustomer != "")
            {
                services.AddDbContext<Sync_Context>(o => o.UseSqlServer(connectionStringCustomer), ServiceLifetime.Transient);
            }
            else
            {
                services.AddDbContext<Sync_Context>();
            }
            return services;
        }

        public static IServiceCollection AddMVCConfiguration(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = false;
                options.Cookie.IsEssential = true;
            });

            services.AddCors();
            services.AddMemoryCache();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services
                .AddMvc()
                .AddNewtonsoftJson(opt => 
                {
                    opt.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });
            return services;
        }

        public static IServiceCollection AddJWTConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var SecretKey = Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
            //Configure JWT Token Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(token =>
            {
                token.RequireHttpsMetadata = false;
                token.SaveToken = true;
                token.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(SecretKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            //var ezAuthSection = configuration.GetSection("Auth").Get<AuthSection>();

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //   .AddJwtBearer(options =>
            //   {
            //       options.Authority = ezAuthSection.Url;
            //       options.RequireHttpsMetadata = false;
            //       options.Audience = $"{ezAuthSection.Url}/resources";

            //       options.TokenValidationParameters = new TokenValidationParameters
            //       {
            //           NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            //       };
            //   })
            //   .AddJwtBearer("ezHR9", token =>
            //   {
            //       token.RequireHttpsMetadata = false;
            //       token.SaveToken = true;
            //       token.TokenValidationParameters = new TokenValidationParameters
            //       {
            //           ValidateIssuerSigningKey = true,
            //           IssuerSigningKey = new SymmetricSecurityKey(SecretKey),
            //           ValidateIssuer = false,
            //           ValidateAudience = false,
            //           RequireExpirationTime = true,
            //           ValidateLifetime = true,
            //           ClockSkew = TimeSpan.Zero
            //       };
            //   });

            //services.AddAuthorization(options =>
            //{
            //    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "ezHR9");
            //    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

            //    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            //});
            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "Development",
                    Title = "EPAD Public API",
                    Description = "ASP.NET Core Web API",
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }

        public static void EnrichLog(this IApplicationBuilder appHost)
        {
            IConfiguration configuration = appHost.ApplicationServices.GetRequiredService<IConfiguration>();
            IServiceProvider provider = appHost.ApplicationServices.GetRequiredService<IServiceProvider>();
            var log = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .Enrich.With(new LogEnricher(provider))
                .Filter.ByExcluding(Serilog.Filters.Matching.FromSource("Microsoft.EntityFrameworkCore.Database.Command"))
                .Filter.ByExcluding(Serilog.Filters.Matching.FromSource("Microsoft.EntityFrameworkCore.Update"))
                .Filter.ByExcluding(Serilog.Filters.Matching.FromSource("Microsoft.AspNetCore.SpaServices.SpaDefaultPageMiddleware"))
                .CreateLogger();

            Log.Logger = log;
        }

        public static void ID2Configuration(this IApplicationBuilder appHost)
        {
            IConfiguration configuration = appHost.ApplicationServices.GetRequiredService<IConfiguration>();
            IMemoryCache pCache = appHost.ApplicationServices.GetRequiredService<IMemoryCache>();
            //save token to cache
            var tokenID = configuration.GetValue<string>("TokenID");
            if (!string.IsNullOrEmpty(tokenID))
            {
                pCache.Set(StringHelper.ComputerIdentifyKey, tokenID);
            }
        }

        public static void ComunicateTokenConfiguration(this IApplicationBuilder appHost)
        {
            IConfiguration configuration = appHost.ApplicationServices.GetRequiredService<IConfiguration>();
            IMemoryCache pCache = appHost.ApplicationServices.GetRequiredService<IMemoryCache>();
            string communicateToken = configuration.GetValue<string>("CommunicateToken");
            pCache.Set("CommunicateToken", communicateToken);
        }

        public static void CorsConfiguration(this IApplicationBuilder appHost)
        {
            appHost.UseCors(builder => {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
        }

        public static void SwaggerConfiguration(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API EPAD");
            });
        }

        public static void FileConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                 Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files")),
                RequestPath = "/Files"
            });
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files")),
                RequestPath = new Microsoft.AspNetCore.Http.PathString("/Files"),
                EnableDirectoryBrowsing = true
            });

            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "epad";
                if (env.IsDevelopment())
                {
                    // Launch development server for Vue.js
                    // spa.UseVueDevelopmentServer();
                }
            });
        }

        public static void AfterConfiguration(this IApplicationBuilder app)
        {
            IServiceProvider provider = app.ApplicationServices.GetRequiredService<IServiceProvider>();
            IMemoryCache cache = app.ApplicationServices.GetRequiredService<IMemoryCache>();
            IConfiguration configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            using(var scope = provider.CreateScope())
            {
                EPAD_Context context = scope.ServiceProvider.GetRequiredService<EPAD_Context>();
                //create company cache
                PublicFunctions.CreateCompanyCache(context, cache);

                LoadLicenceCache(provider);

                CommandProcess.CreateGeneralCacheFromDB(cache, context);

                //set config
                ConfigObject config = ConfigObject.GetConfig(cache);
                GlobalParams.MAX_LENGTH_ATID = config.MaxLenghtEmployeeATID;

                string connectionStringOther = configuration.GetConnectionString("connectionStringOtherDB");
                if (string.IsNullOrEmpty(connectionStringOther))
                {
                    config.IntegrateDBOther = false;
                }

                if (!config.IntegrateDBOther)
                {
                    //CreateWorkingInfoForEmployee(context);
                }
            }
            
        }

        private static void LoadLicenceCache(IServiceProvider serviceProvider)
        {
            var checkHardwareLicense = serviceProvider.GetService<EPAD_Background.Schedule.Job.CheckHardwareLicense>();
            checkHardwareLicense.DoWorkAsync().Wait();
        }

    }
}
