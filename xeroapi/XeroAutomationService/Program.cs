using CosmicApiHelper;
using XeroAutomationService.Common;
using Flexis.Log;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace XeroAutomationService
{
    static class Program
    {
        public static ServiceProvider ServiceProvider;  //todo should this really be public?
        public static IConfiguration Configuration;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            Logger Log = CosmicLogger.SetLog();
            SetupDependencyInjection();
            CreateInstanceConfigFile();

            Log.Info("Startup...");

            var xeroService = ServiceProvider.GetService<XeroService>();

            var servicesToRun = new ServiceBase[]
            {
                xeroService
            };

            ExceptionHelper.Setup();

            if (Environment.UserInteractive)
            {
                RunInteractive(servicesToRun);
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }

        private static void CreateInstanceConfigFile()
        {
            var fullPath = GetInstanceConfigPath();

            if (File.Exists(fullPath))
                return;

            //todo

        }

        private static void RunInteractive(ServiceBase[] servicesToRun)
        {
            Logger Log = CosmicLogger.SetLog();
            Log.Info("Services running in interactive mode.");

            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart",
                BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (ServiceBase service in servicesToRun)
            {
                Log.Info($"Starting {0}...{ service.ServiceName}");
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Log.Info("Started");
            }

            Log.Info("");
            Log.Info("");
            Log.Info("Press any key to stop the services and end the process...");

            Console.ReadKey();

            Log.Info("");

            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop",
                BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Log.Info($"Stopping {service.ServiceName}...");
                onStopMethod.Invoke(service, null);
                Log.Info("Stopped");
            }

            Log.Info("All services stopped.");
            // Keep the console alive for a second to allow the user to see the message.
            Thread.Sleep(1000);
        }

        private static readonly AssemblyName _assemblyName = Assembly.GetEntryAssembly().GetName();

        private static string GetInstanceConfigPath()
        {
            var appName = _assemblyName.Name;
            var rootFolderVariable = "%AppData%";
            var fileName = "instance.json";
            var rootFolder = Environment.ExpandEnvironmentVariables(rootFolderVariable);
            var fullPath = Path.Combine(rootFolder, appName, fileName);
            return fullPath;
        }

        private static void SetupDependencyInjection()
        {
            //ServiceProvider
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            ////see comments at https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/.  
            //// now we don't need SetupSerilog in Logging.cs
            serviceCollection
                .AddSingleton<XeroService>() //todo transient?
                //todo transient?
                ;
        }

    }
}
