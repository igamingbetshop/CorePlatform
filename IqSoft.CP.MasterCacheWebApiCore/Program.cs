using IqSoft.CP.Common.Models.WebSiteModels;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IqSoft.CP.MasterCacheWebApiCore
{
    public class Program
    {
        public static AppSettingModel AppSetting;
        public static readonly ILog DbLogger = LogManager.GetLogger("AdoNetAppender");

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
