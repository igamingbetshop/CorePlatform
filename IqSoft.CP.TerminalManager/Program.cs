using Serilog;
using System.Diagnostics;
using IqSoft.CP.TerminalManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IqSoft.CP.TerminalManager.Helpers;

namespace IqSoft.CP.TerminalManager
{
    public class Program
    {
        public static AppSetting AppSetting;
        public static void Main(string[] args)
        {
            string appName = Process.GetCurrentProcess().ProcessName;
            var mutex = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                Console.WriteLine(appName + " is already running! Exiting the application.");
                return;
            }
            Console.WriteLine("Continuing with the application");
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Map("FileName", "", (fileName, wt) => wt.File($"accesslogs/{fileName}/log.txt", rollingInterval: RollingInterval.Day))
               .CreateLogger();
            ReadConfigFile();
            CreateHostBuilder(args).Build().Run();

        }       

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSignalR(hubOptions =>
                    {
                        hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                        hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(110);
                        hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureWebHost(config =>
                {
                    config.UseUrls("http://*:9011/");
                });

        private static void ReadConfigFile()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSetting");
            AppSetting = configuration.Get<AppSetting>() ?? new AppSetting();
            AppSetting.SerialNumber = CommonHelpers.GetMotherBoardID();

        }


    }
}