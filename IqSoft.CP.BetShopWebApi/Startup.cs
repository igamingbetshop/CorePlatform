using System;
using IqSoft.CP.BetShopWebApi.Infrastructure;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(IqSoft.CP.BetShopWebApi.Startup))]

namespace IqSoft.CP.BetShopWebApi
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
			GlobalHost.HubPipeline.AddModule(new WebSitePipelineModule());

			app.Map("/api", map =>
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
	}
}
