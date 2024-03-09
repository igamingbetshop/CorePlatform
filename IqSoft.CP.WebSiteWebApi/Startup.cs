using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.WebSiteWebApi.Common;
using IqSoft.CP.WebSiteWebApi.Hubs;
using IqSoft.CP.WebSiteWebApi.Models;
using System.Linq;
using System;
using IqSoft.CP.Common;
using Serilog;
using IqSoft.CP.CommonCore.Models.WebSiteModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using IqSoft.CP.Common.Models.AdminModels;

namespace IqSoft.CP.WebSiteWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Program.AppSetting = configuration.GetSection("AppConfiguration").Get<AppConfigurationModel>();
            Application_Start();
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        if (Program.AppSetting.AllowOrigins.Contains("*"))
                            builder.SetIsOriginAllowed(_ => true).AllowAnyHeader().WithMethods("POST", "GET").AllowCredentials();
                        else
                            builder.WithOrigins(Program.AppSetting.AllowOrigins).AllowAnyHeader().WithMethods("POST", "GET").AllowCredentials();
                    });
            });
            services.AddSignalR().AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.AddRazorPages();
            services.AddMvc(options =>
            {
                options.Filters.Add(new ConsumesAttribute("application/json"));
            });

            services.AddHttpClient();
            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.Preload = true;
                options.MaxAge = TimeSpan.FromHours(100);
            });
            services.AddSwaggerGen();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
              context.Response.Headers.Add("X-Frame-Options", "DENY");
              context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
              context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
              context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Orign"]);
              context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
              context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST");
              await next.Invoke();
            });

            app.UseAuthentication();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features
                    .Get<IExceptionHandlerPathFeature>()
                    .Error;
                var response = new { error = exception.Message };
                Log.Error("handled error: " + exception.Message);
               await context.Response.WriteAsJsonAsync(response);
            }));
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<BaseHub>("/basehub");
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSiteWebApi");
            });
        }

        private static HubConnection _productGatewayConnection;
        private static HubConnection _paymentGatewayConnection;
        private static HubConnection _adminConnection;

        private static IHubProxy _productGatewayHubProxy;
        private static IHubProxy _paymentGatewayHubProxy;
        private static IHubProxy _adminHubProxy;

        #region CacheUpdaters

        public static Timer ReconnectTimer;
        public static Timer SessionTimer;

        #endregion

        protected void Application_Start()
        {
            _productGatewayConnection = new HubConnection(Program.AppSetting.ProductGatewayHostAddress);
            _productGatewayHubProxy = _productGatewayConnection.CreateHubProxy("BaseHub");
            _productGatewayConnection.Closed += () => { ReconnectTimer.Change(5000, 5000); };

            _paymentGatewayConnection = new HubConnection(Program.AppSetting.PaymentGatewayHostAddress);
            _paymentGatewayHubProxy = _paymentGatewayConnection.CreateHubProxy("BaseHub");
            _paymentGatewayConnection.Closed += () => { ReconnectTimer.Change(5000, 5000); };

            _adminConnection = new HubConnection(Program.AppSetting.AdminHostAddress);
            _adminHubProxy = _adminConnection.CreateHubProxy("WebSiteHub");
            _adminConnection.Closed += () => { ReconnectTimer.Change(5000, 5000); };

            _adminHubProxy.On<string>("BroadcastCacheChanges", (data) =>
            {
                Log.Information("BroadcastCacheChanges_" + data);
                SlaveCache.RemoveFromCache(data);
            });
            _adminHubProxy.On<ApiPopup>("BroadcastPopup", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastPopup(data));
            });

            _productGatewayHubProxy.On<ApiWin>("BroadcastWin", (data) =>
            {
                var win = new
                {
                    GameName = data.GameName,
                    ClientName = data.ClientName,
                    Amount = data.Amount,
                    CurrencyId = data.CurrencyId,
                    ProductId = data.ProductId,
                    ProductName = data.ProductName,
                    ImageUrl = data.ImageUrl
                };
                Task.Run(() => BroadcastService.BroadcastWin(data, win));
            });
            _productGatewayHubProxy.On<ApiWin>("BroadcastBalance", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastBalance(data.ClientId, data.ApiBalance));
            });
            _productGatewayHubProxy.On<LimitInfo>("BroadcastBetLimit", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastBetLimit(data));
            });
            
            _paymentGatewayHubProxy.On<ApiWin>("BroadcastBalance", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastBalance(data.ClientId, data.ApiBalance));
            });
            _paymentGatewayHubProxy.On<LimitInfo>("BroadcastDepositLimit", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastDepositLimit(data));
            });

            ReconnectTimer = new Timer(Reconnect, null, 5000, 5000);
            SessionTimer = new Timer(CheckSessionTokens, null, 60000, 60000);
        }

        private void Reconnect(object sender)
        {
            ReconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
            bool activeConnections = true;
            if (_productGatewayConnection.State != ConnectionState.Connected)
            {
                try
                {
                    _productGatewayConnection.Start().Wait();
                }
                catch
                {
                    activeConnections = false;
                }
            }
            if (_paymentGatewayConnection.State != ConnectionState.Connected)
            {
                try
                {
                    _paymentGatewayConnection.Start().Wait();
                }
                catch
                {
                    activeConnections = false;
                }
            }
            if (_adminConnection.State != ConnectionState.Connected)
            {
                try
                {
                    _adminConnection.Start().Wait();
                }
                catch
                {
                    activeConnections = false;
                }
            }

            if (!activeConnections)
            {
                ReconnectTimer.Change(5000, 5000);
            }
        }

        private void CheckSessionTokens(object sender)
        {
            SessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Parallel.ForEach(BaseHub.ConnectedClients, c =>
            {
                if (!string.IsNullOrEmpty(c.Value.Token))
                {
                    try
                    {
                        var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(c.Value.PartnerId, "CheckClientToken",
                            new RequestBase { ClientId = c.Value.ClientId, Token = c.Value.Token, LanguageId = c.Value.LanguageId });
                        if (resp.ResponseCode != Constants.SuccessResponseCode)
                        {
                            c.Value.Token = String.Empty;
                            Task.Run(() => BroadcastService.BroadcastLogout(c.Key));
                        }
                    }
                    catch(Exception e)
                    {
                        Log.Error(e, "CheckSessionTokens_" + c.Value.ClientId);
                    }
                }
            });
            SessionTimer.Change(60000, 60000);
        }
    }
}
