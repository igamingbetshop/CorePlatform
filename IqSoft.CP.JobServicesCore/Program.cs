using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace IqSoft.CP.JobServicesCore
{
    public class Program
    {
        public static readonly ILog DbLogger = LogManager.GetLogger("AdoNetAppender");
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .UseWindowsService()
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddHostedService<JobService>();
                   services.AddSignalR(hubOptions =>
                   {
                       hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                       hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(110);
                       hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
                       hubOptions.MaximumReceiveMessageSize = 1024 * 1024; //1MB;
                   });
               })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               })
               .ConfigureWebHost(config =>
               {
                   config.UseUrls("http://*:9010/");
               })
            ;
    }
}