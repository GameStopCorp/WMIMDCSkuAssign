using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos;
using Serilog;
using SupplyChain.Svc.MSDCSKUAssign.Configurations;
using SupplyChain.Svc.MSDCSKUAssign.Services;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers;
using SupplyChain.Svc.MSDCSKUAssign.Services.Entities;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Factories;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;

namespace SupplyChain.Svc.MSDCSKUAssign
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddApplicationSettings("appsettings.json", optional: false)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddLogging();
            services.AddOptions();

            //Load the logger config
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            //Create a new LoggerFactory
            ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(logger);

            //Load the LoggerFactory into the DI container
            services.AddSingleton(loggerFactory);

            //TODO:: Get rid of this section once complete
            #region IOptions

            //Add PTL Settings to Options DI
            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            //var appSettings = new AppSettings();
            //Configuration.GetSection("AppSettings").Bind(appSettings);

            //var builder = new SettingConfigurationBuilder(Configuration.GetSection("AppSettings"), appSettings);
            //var c = builder.Build();

            //services.Configure<StoredSettings>(config =>
            //{
            //    config.BrianConnectionString = c[""];
            //    config.GlobalDBSpPropName = string.Empty;
            //});

            #endregion

            //Add the configuration collection to DI
            services.AddSingleton<IConfiguration>(Configuration);

            //Add a message repositories to DI
            services.AddTransient<IRepositorySvc<SkuContract>, SkuRepository>();
            services.AddTransient<IRepositorySvc<TNTContract>, TntRepository>();
            services.AddTransient<IRepositorySvc<AllocationContract>, AllocationRepository>();
            services.AddTransient<IRepositorySvc<PurchaseOrderContract>, PurchaseOrderRepository>();
            services.AddTransient<IControlRepository<ControlContract, SkuContract>, ControlRepository>();

            //add managers and factories to DI
            services.AddTransient<IAllocationManager<AllocationContract>, AllocationManager>();
            services.AddTransient<ISkuManager<SkuContract>, SkuManager>();
            services.AddTransient<ITnTManager<TNTContract>, TnTManager>();
            services.AddTransient<IPurchaseOrderManager<PurchaseOrderContract>, PurchaseOrderManager>();
            services.AddTransient<IDecisionTreeFactory<SkuContract, TNTContract, AllocationContract>, DecisionTreeFactory>();
            services.AddTransient<IDbFactory, DbFactory>();

            //add services to DI
            services.AddTransient<IMsdcService, MsdcService>();

            //Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();

            //app.UseSwagger();

            app.UseSwaggerUI();
        }
    }
}
