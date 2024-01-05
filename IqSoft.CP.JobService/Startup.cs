using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(IqSoft.CP.JobService.Startup))]

namespace IqSoft.CP.JobService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

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

            app.Map("/api/signalr", map =>
            {
                var hubConfiguration = new HubConfiguration
                {
                    EnableJSONP = true,
                    EnableJavaScriptProxies = true,
                    EnableDetailedErrors = true
                };
                map.RunSignalR(hubConfiguration);
            });
        }

        //public void Configuration(IAppBuilder app)
        //{
        //    app.Map("/api", map =>
        //    {
        //        map.UseCors(CorsOptions.AllowAll);

        //        var hubConfiguration = new HubConfiguration
        //        {
        //            EnableDetailedErrors = true,
        //            EnableJSONP = true
        //        };

        //        map.RunSignalR(hubConfiguration);
        //    });
        //}
    }
}
