using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IqSoft.CP.IqWalletWebApiCore
{
    public class Program
    {
        public static ILog DbLogger;
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
