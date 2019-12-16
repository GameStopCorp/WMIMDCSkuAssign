using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SupplyChain.App.MDCSkuAssign.Services;
using SupplyChain.App.MDCSkuAssign.Data;
using SupplyChain.App.MDCSkuAssign.Entities;
using SupplyChain.App.MDCSkuAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Repos;
using SupplyChain.Svc.MSDCSKUAssign.Services;
using SupplyChain.Svc.MSDCSKUAssign.Contracts;
using SupplyChain.Svc.MSDCSKUAssign.Services.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Services.Managers;
using SupplyChain.Svc.MSDCSKUAssign.Services.Entities;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Factories;
using SupplyChain.Svc.MSDCSKUAssign.Repositories.Interfaces;
using SupplyChain.Svc.MSDCSKUAssign.Configurations;
using SupplyChain.DCAutoMailer;

namespace SupplyChain.App.MDCSkuAssign
{
    public class Program
    {
        static IConfigurationRoot _configuration { get; set; }

        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            ILogger<Program> logger = null;
            try
            {
                //Load configuration data from settings files
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                    .AddEnvironmentVariables();
                _configuration = builder.Build();

                var builderSvc = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddApplicationSettings("appsettingsSvc.json", optional: false)
                     .AddJsonFile("appsettingsSvc.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

                builderSvc.AddEnvironmentVariables();
                Configuration = builderSvc.Build();

                //Create a new services DI container
                IServiceCollection serviceCollection = new ServiceCollection();

                //Configure services DI container
                configureServices(serviceCollection);

                //Build the service provider based on the service collection
                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                logger = serviceProvider.GetService<ILogger<Program>>();

                //Create a new environment validation service
                IProcessor processor = serviceProvider.GetService<IProcessor>();
                processor.Process();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                Environment.Exit(-1);
            }
            logger.LogInformation("Process Completed");
            Environment.Exit(0);
        }

        private static void configureServices(IServiceCollection serviceCollection)
        {
            //Load the logging configuration
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            //Create a new LoggerFactory
            ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(logger);

            //Load the LoggerFactory into the DI container
            serviceCollection.AddSingleton(loggerFactory);

            //Configure DI to support logging
            serviceCollection.AddLogging();

            //Configure DI to support Options
            serviceCollection.AddOptions();

            //Load Options for all settings
            serviceCollection.Configure<AppSettings>(_configuration.GetSection("AppSettings"));
            serviceCollection.Configure<SupplyChain.DCAutoMailer.DCEmailSettings>(_configuration.GetSection("DCEmailSettings"));

            //Load transient services
            serviceCollection.AddTransient<IRESTService, RESTService>();
            //serviceCollection.AddTransient<IProcessor, Processor>();
            serviceCollection.AddTransient<IProcessor, AppProcessor>();
            serviceCollection.AddTransient<IFileService, FileService>();
            serviceCollection.AddTransient<IDistroService<Allocation>, DistroService>();
            serviceCollection.AddTransient<IPickTicketService<Allocation>, PickTicketService>();
            serviceCollection.AddTransient<IRepository<Allocation>, WMRepository>();
            serviceCollection.AddTransient<IPtlRepository<TntItem>, PtlRepository>();
            serviceCollection.AddTransient<IBrianRepository<MasterAllocDetail>, BrianRepository>();

            //Load Transient E-mail dependencies
            serviceCollection.AddTransient<DCAutoMailer.SettingsRepository.IRepository, DCAutoMailer.SettingsRepository.GlobalSettingsRepository>();
            serviceCollection.AddTransient<IDCEMailService, DCEMailService>();
            /////svc class's DI
            serviceCollection.AddSingleton<IConfiguration>(Configuration);

            serviceCollection.AddMvc();
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();

            //Add a message repositories to DI
            serviceCollection.AddTransient<IRepositorySvc<SkuContract>, SkuRepository>();
            serviceCollection.AddTransient<IRepositorySvc<TNTContract>, TntRepository>();
            serviceCollection.AddTransient<IRepositorySvc<AllocationContract>, AllocationRepository>();
            serviceCollection.AddTransient<IRepositorySvc<PurchaseOrderContract>, PurchaseOrderRepository>();
            serviceCollection.AddTransient<IControlRepository<ControlContract, SkuContract>, ControlRepository>();

            //add managers and factories to DI
            serviceCollection.AddTransient<IAllocationManager<AllocationContract>, AllocationManager>();
            serviceCollection.AddTransient<ISkuManager<SkuContract>, SkuManager>();
            serviceCollection.AddTransient<ITnTManager<TNTContract>, TnTManager>();
            serviceCollection.AddTransient<IPurchaseOrderManager<PurchaseOrderContract>, PurchaseOrderManager>();
            serviceCollection.AddTransient<IDecisionTreeFactory<SkuContract, TNTContract, AllocationContract>, DecisionTreeFactory>();
            serviceCollection.AddTransient<IDbFactory, DbFactory>();

            //    //add services to DI
            serviceCollection.AddTransient<IMsdcService, MsdcService>();
            serviceCollection.AddTransient<Svc.MSDCSKUAssign.Controllers.IMsdcController, Svc.MSDCSKUAssign.Controllers.MsdcControllerDirect>();

            //Inject an implementation of ISwaggerProvider with defaulted settings applied
            serviceCollection.AddSwaggerGen();
        }
    }
}
