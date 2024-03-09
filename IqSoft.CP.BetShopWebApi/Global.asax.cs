using IqSoft.CP.BetShopWebApi.Common;
using IqSoft.CP.BetShopWebApi.Models;
using IqSoft.CP.BetShopWebApi.Models.Common;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace IqSoft.CP.BetShopWebApi
{
	public class WebApiApplication : HttpApplication
	{
		public static ILog LogWriter;

		public static string BetShopConnectionUrl;

		public static ConcurrentDictionary<string, CashierIdentity> Clients;

		public static List<string> WhitelistedCountries { get; private set; }
		public static List<string> BlockedIps { get; private set; }
		public static List<string> WhitelistedIps { get; private set; }

        private static HubConnection _productGatewayConnection;
        private static IHubProxy _productGatewayHubProxy;
        private static Timer ReconnectTimer;

        protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			BetShopConnectionUrl = ConfigurationManager.AppSettings["BetShopConnectionUrl"];
			log4net.Config.XmlConfigurator.Configure();
			LogWriter = LogManager.GetLogger("DbLogAppender");

			Clients = new ConcurrentDictionary<string, CashierIdentity>();
			WhitelistedCountries = JsonConvert.DeserializeObject<List<string>>(ConfigurationManager.AppSettings["WhitelistedCountries"]);
			BlockedIps = JsonConvert.DeserializeObject<List<string>>(ConfigurationManager.AppSettings["BlockedIps"]);
			WhitelistedIps = JsonConvert.DeserializeObject<List<string>>(ConfigurationManager.AppSettings["WhitelistedIps"]);

            _productGatewayConnection = new HubConnection(ConfigurationManager.AppSettings["ProductGatewayHostAddress"]);
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
