using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApiCore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Concurrent;

namespace IqSoft.CP.BetShopWebApiCore
{
    public class Program
    {
        public static AppConfigurationModel AppSetting;

        public static ConcurrentDictionary<string, CashierIdentity> Clients = new ConcurrentDictionary<string, CashierIdentity>();
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Map("FileName", "", (fileName, wt) => wt.File($"accesslogs/{fileName}/log-.txt", 
                                                                rollingInterval: RollingInterval.Day))
                                                  .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}