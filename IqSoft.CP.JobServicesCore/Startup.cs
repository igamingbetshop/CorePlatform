using IqSoft.CP.JobService.Hubs;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

namespace IqSoft.CP.JobServicesCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {           
            var repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            var fileInfo = new FileInfo(@"log4net.config");
            XmlConfigurator.Configure(repository, fileInfo);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BaseHub>("/api/signalr/basehub");
            });
        }
    }
}
