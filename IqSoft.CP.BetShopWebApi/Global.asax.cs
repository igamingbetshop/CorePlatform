using IqSoft.CP.BetShopWebApi.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
		}
	}
}
