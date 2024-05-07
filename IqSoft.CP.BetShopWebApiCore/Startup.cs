using IqSoft.CP.BetShopWebApi.Hubs;
using IqSoft.CP.BetShopWebApiCore.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using System.Threading.Tasks;
using IqSoft.CP.BetShopWebApi.Models.Common;
using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using IqSoft.CP.BetShopWebApi.Common;

namespace IqSoft.CP.BetShopWebApiCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Program.AppSetting = configuration.GetSection("AppConfiguration").Get<AppConfigurationModel>();
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
        private static IHubProxy _productGatewayHubProxy;
        public static Timer ReconnectTimer;

        protected void Application_Start()
        {
            _productGatewayConnection = new HubConnection(Program.AppSetting.ProductGatewayHostAddress);
            _productGatewayHubProxy = _productGatewayConnection.CreateHubProxy("BaseHub");
            _productGatewayConnection.Closed += () => { ReconnectTimer.Change(5000, 5000); };

            _productGatewayHubProxy.On<PlaceBetOutput>("BroadcastBet", (data) =>
            {
                Task.Run(() => BroadcastService.BroadcastBet(data));
            });

            ReconnectTimer = new Timer(Reconnect, null, 5000, 5000);
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

            if (!activeConnections)
            {
                ReconnectTimer.Change(5000, 5000);
            }
        }
    }
}
