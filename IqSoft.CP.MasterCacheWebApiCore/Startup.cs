using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.Enums;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using log4net.Config;
using Microsoft.AspNetCore.SignalR.Client;
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
using System.Threading.Tasks;

namespace IqSoft.CP.MasterCacheWebApiCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public static HubConnection JobConnection;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Program.AppSetting = configuration.GetSection("AppConfiguration").Get<AppSettingModel>();
            
            Application_Start();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
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
            var qParams = new Dictionary<string, string> { { "ProjectId", ((int)ProjectTypes.MasterCache).ToString() } };
            JobConnection = new HubConnectionBuilder()
                              .WithUrl(new Uri(Program.AppSetting.JobHostAddress + "/api/signalr/basehub"))
                              .WithAutomaticReconnect()
                              .Build();
            JobConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await JobConnection.StartAsync();
            };

            JobConnection.On<int>("onClientDepositWithBonus", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientDepositCount(clientId);
                CacheManager.RemoveClientActiveBonus(clientId);
            });
            JobConnection.On<int>("onClientDeposit", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientDepositCount(clientId);
                CacheManager.RemoveTotalDepositAmount(clientId);
            });
            JobConnection.On<int>("onClientBonus", (clientId) =>
            {
                CacheManager.RemoveClientBalance(clientId);
                CacheManager.RemoveClientActiveBonus(clientId);
            });
            JobConnection.On<int, string>("onLoginClient", (clientId, ip) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
                CacheManager.UpdateClientLastLoginIp(clientId, ip);
                CacheManager.RemoveClientNotAwardedCampaigns(clientId);
                CacheManager.RemoveClientFailedLoginCount(clientId);
            });
            JobConnection.On<string, object, TimeSpan>("onUpdateCacheItem", (key, newValue, timeSpan) =>
            {
                CacheManager.UpdateCacheItem(key, newValue, timeSpan);
            });
            JobConnection.On<int>("onUpdateClientFailedLoginCount", (clientId) =>
            {
                CacheManager.UpdateClientFailedLoginCount(clientId);
            });
            JobConnection.On<int>("onUpdateProduct", (productId) =>
            {
                CacheManager.UpdateProductById(productId);
            });
            JobConnection.On<int, long, int, int>("onUpdateProductLimit", (objectTypeId, objectId, limitTypeId, productId) =>
            {
                CacheManager.UpdateProductLimit(objectTypeId, objectId, limitTypeId, productId);
            });
            JobConnection.On<int>("onRemovePartnerProductSettings", (partnerId) =>
            {
                CacheManager.RemovePartnerProductSettings(partnerId);
            });
            JobConnection.On<string>("onRemoveKeyFromCache", (data) =>
            {
                CacheManager.RemoveFromCache(data);
            });
            JobConnection.On<int, int>("onRemoveBanners", (partnerId, type) =>
            {
                CacheManager.RemoveBanners(partnerId, type);
            });
            JobConnection.On<int>("onRemoveClient", (clientId) =>
            {
                CacheManager.RemoveClientFromCache(clientId);
            });
            JobConnection.On<int>("ExpireClientPlatformSessions", (id) =>
            {
                BroadcastListener.ExpireClientPlatformSessions();
            });
            JobConnection.On<int>("ExpireClientProductSessions", (id) =>
            {
                BroadcastListener.ExpireClientProductSessions();
            });

            JobConnection.StartAsync();
        }
    }
}
