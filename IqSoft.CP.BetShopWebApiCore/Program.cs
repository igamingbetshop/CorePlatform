using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApiCore.Models;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopWebApiCore
{
    public class Program
    {
        public static ILog LogWriter = LogManager.GetLogger("AdoNetAppender");

        public static AppConfigurationModel AppSetting;

        public static ConcurrentDictionary<string, CashierIdentity> Clients = new ConcurrentDictionary<string, CashierIdentity>();
        public static void Main(string[] args)
        {
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