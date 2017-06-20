using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;
using System.Linq;
using AspNetCoreRateLimit;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Reflection;
using NG.Persistence;
using NG.Domain.Users;
using NG.Common.Services;
using NG.Common;
using NG.Domain.Departments;
using NLog.Extensions.Logging;
using NG.Service.Controllers.Core;
using NG.Application;
using NG.Service.Departments;
using NG.Service.Core;

namespace NG.Service
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Department, DepartmentDto>();
                cfg.CreateMap<DepartmentForUpdationDto, Department>();
                cfg.CreateMap<Department, DepartmentForUpdationDto>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForCreationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForCreationDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForUpdationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForUpdationDto>();
                cfg.CreateMap<AppUser, Core.AppUserDto>();
                cfg.CreateMap<Core.AppUserForCreationDto, AppUser>();
                cfg.CreateMap<IdentityRole, Core.AppRoleDto>();
                cfg.CreateMap<Core.AppRoleForCreationDto, IdentityRole>();
                cfg.CreateMap<AppUserForCreationDto, AppUser>();
                cfg.CreateMap<IdentityRole, AppRoleDto>();
                cfg.CreateMap<AppRoleForCreationDto, IdentityRole>();
            });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:nationalGasDBConnectionString"];
            services.AddDbContext<ApplicationContext>(o => o.UseSqlServer(connectionString, b => b.MigrationsAssembly("NG.Service")));
            services.AddTransient<IdentityInitializer>();
            services.AddTransient<Microsoft.EntityFrameworkCore.DbContext, ApplicationContext>();
            services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>();
            services.Configure<IdentityOptions>(config =>
            {
                config.Cookies.ApplicationCookie.Events =
                    new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                            }

                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 403;
                            }

                            return Task.CompletedTask;
                        }
                    };
            });
            services.AddCors();
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                // setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

                var xmlDataContractSerializerInputFormatter =
                new XmlDataContractSerializerInputFormatter();
                xmlDataContractSerializerInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.marvin.authorwithdateofdeath.full+xml");
                setupAction.InputFormatters.Add(xmlDataContractSerializerInputFormatter);

                var jsonInputFormatter = setupAction.InputFormatters
                .OfType<JsonInputFormatter>().FirstOrDefault();

                if (jsonInputFormatter != null)
                {
                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.marvin.author.full+json");
                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.marvin.authorwithdateofdeath.full+json");
                }

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }

            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                // options.SerializerSettings.PreserveReferencesHandling =
                // PreserveReferencesHandling.Objects;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsSuperAdmin", policy => policy.RequireClaim("IsSuperAdmin"));

                Type type = typeof(Permissions);
                foreach (var p in type.GetFields())
                {
                    options.AddPolicy(Convert.ToString(p.GetValue(null)), policy => policy.RequireClaim(Convert.ToString(p.GetValue(null))));
                }
            });

            services.AddNodeServices();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>()
                .ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddHttpCacheHeaders(
                (expirationModelOptions)
                =>
                {
                    expirationModelOptions.MaxAge = 600;
                },
                (validationModelOptions)
                =>
                {
                    validationModelOptions.AddMustRevalidate = true;
                });
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>((options) =>
            {
                options.GeneralRules = new System.Collections.Generic.List<RateLimitRule>()
                {
                    new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 1000,
                        Period = "5m"
                    },
                    new RateLimitRule()
                    {
                        Endpoint = "*",
                        Limit = 200,
                        Period = "10s"
                    }
                };
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, ApplicationContext libraryContext,
            IdentityInitializer identitySeeder)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug(LogLevel.Information);
            loggerFactory.AddNLog();
            app.UseCors(cfg =>
            {
                cfg.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500,
                                exceptionHandlerFeature.Error,
                                exceptionHandlerFeature.Error.Message);
                        }

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            // AutoMapper.Mapper.Initialize(cfg =>
            // {
            //     cfg.CreateMap<Department, DepartmentDto>();
            //     cfg.CreateMap<DepartmentForUpdationDto, Department>();
            //     cfg.CreateMap<Department, DepartmentForUpdationDto>();
            //     cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerDto>();
            //     cfg.CreateMap<NG.Service.Customers.CustomerForCreationDto, NG.Domain.Customers.Customer>();
            //     cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForCreationDto>();
            //     cfg.CreateMap<NG.Service.Customers.CustomerForUpdationDto, NG.Domain.Customers.Customer>();
            //     cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForUpdationDto>();
            //     cfg.CreateMap<AppUser, Core.AppUserDto>();
            //     cfg.CreateMap<Core.AppUserForCreationDto, AppUser>();
            //     cfg.CreateMap<IdentityRole, Core.AppRoleDto>();
            //     cfg.CreateMap<Core.AppRoleForCreationDto, IdentityRole>();
            //     cfg.CreateMap<AppUserForCreationDto, AppUser>();
            //     cfg.CreateMap<IdentityRole, AppRoleDto>();
            //     cfg.CreateMap<AppRoleForCreationDto, IdentityRole>();
            // });

            identitySeeder.Seed().Wait();
            libraryContext.EnsureSeedDataForContext();
            app.UseIpRateLimiting();
            app.UseHttpCacheHeaders();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidAudience = Configuration["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });
            app.UseMvc();
        }
    }
}