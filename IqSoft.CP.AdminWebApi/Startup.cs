using System;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(IqSoft.CP.AdminWebApi.Startup))]

namespace IqSoft.CP.AdminWebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Make long polling connections wait a maximum of 110 seconds for a
            // response. When that time expires, trigger a timeout command and
            // make the client reconnect.

            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.

            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds. 
            // This value must be no more than 1/3 of the DisconnectTimeout value.

            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 1024 * 1024; //1MB
            //GlobalHost.HubPipeline.AddModule(new WebSitePipelineModule());

            app.Map(string.Format("/{0}/{1}", "api", "signalr"), map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.RunSignalR(new HubConfiguration
                {
                    EnableDetailedErrors = true
                });
            });
            //now start the WebAPI app
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}