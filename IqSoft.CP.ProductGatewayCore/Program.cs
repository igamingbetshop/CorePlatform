using IqSoft.CP.Common.Models.WebSiteModels;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IqSoft.CP.ProductGateway
{
    public class Program
    {
        public static IHubProxy JobHubProxy;
        public static AppSettingModel AppSetting;
        public static readonly ILog DbLogger = LogManager.GetLogger("AdoNetAppender");

        public static string BetShopConnectionUrl { get; set; }
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.AddServerHeader = false;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}