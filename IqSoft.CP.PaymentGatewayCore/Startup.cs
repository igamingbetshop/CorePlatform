using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using log4net.Config;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace IqSoft.CP.PaymentGateway
{
    public class Startup
    {
        private static HubConnection JobConnection;
        private static Timer Timer;
        public Startup(IConfiguration configuration)
        {
            Program.AppSetting = configuration.GetSection("AppConfiguration").Get<AppSettingModel>();
            Application_Start();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddSignalR().AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().WithMethods("POST", "GET").AllowCredentials());
            });

            services.AddRazorPages();
            services.AddMvc(options =>
            {
                options.Filters.Add(new ConsumesAttribute("application/json"));
            });
            services.AddHttpClient();
            var repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            var fileInfo = new FileInfo(@"log4net.config");
            XmlConfigurator.Configure(repository, fileInfo);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        protected void Application_Start()
        {
            var qParams = new Dictionary<string, string> { { "ProjectId", ((int)ProjectTypes.PaymentGateway).ToString() } };
            JobConnection = new HubConnection(Program.AppSetting.JobHostAddress, qParams);
            Program.JobHubProxy = JobConnection.CreateHubProxy("BaseHub");
            JobConnection.Closed += () => { Timer.Change(5000, 5000); };

            Program.JobHubProxy.On<string, object, TimeSpan>("onUpdateCacheItem", (key, newValue, timeSpan) =>
            {
                CacheManager.UpdateCacheItem(key, newValue, timeSpan);
            });
            Program.JobHubProxy.On<string>("onRemoveKeyFromCache", (data) =>
            {
                CacheManager.RemoveFromCache(data);
            });
            Program.JobHubProxy.On<int>("onRemoveClient", (clientId) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
            });
            Timer = new Timer(Reconnect, null, 5000, 5000);
        }
        private void Reconnect(object sender)
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            bool activeConnections = true;
            if (JobConnection.State != ConnectionState.Connected)
            {
                try
                {
                    JobConnection.Start().Wait();
                }
                catch (Exception)
                {
                    activeConnections = false;
                }
            }

            if (!activeConnections)
            {
                Timer.Change(5000, 5000);
            }
        }
    }
}
