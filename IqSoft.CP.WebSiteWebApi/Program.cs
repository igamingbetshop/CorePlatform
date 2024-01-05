using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

using IqSoft.CP.WebSiteWebApi.Models;

namespace IqSoft.CP.WebSiteWebApi
{
    public class Program
    {
        public static AppConfigurationModel AppSetting;
        /*public static readonly string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        public static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();*/

        public static void Main(string[] args)
        {
            /*Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(Configuration)
                            .CreateLogger();*/
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Map("FileName", "", (fileName, wt) => wt.File($"accesslogs/{fileName}/log-.txt", rollingInterval: RollingInterval.Day))
                //.WriteTo.File("accesslogs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) => {                        
                        options.AddServerHeader = false;
                    });
                    webBuilder.UseStartup<Startup>();
                });
      
    }
}