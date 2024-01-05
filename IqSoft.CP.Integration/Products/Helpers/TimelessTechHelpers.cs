using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.TimelessTech;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class TimelessTechHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.TimelessTech);
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
		{
			var product = CacheManager.GetProductById(productId);
			var client = CacheManager.GetClientById(clientId);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var device = session.DeviceType == (int)DeviceTypes.Desktop ? "desktop" : "mobile";
			var mode = isForDemo ? "fun" : "real";
			var url = $"{Provider.GameLaunchUrl}/?mode={mode}&game_id={product.ExternalId}&language={session.LanguageId}&operator_id={operatorID}&device={device}";			
			if (isForDemo)
			   return url;
			else
			   return $"{url}&currency={client.CurrencyId}&token={token}";
		}

		public static List<Game> GetGames(int partnerId)
		{
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechSecretkey);
			var operatorID = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechOperatorID);
			var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TimelessTechUrl);
			var authorization = CommonFunctions.ComputeSha1($"games{operatorID}{sectetKey}");
			var headers = new Dictionary<string, string> {
				 { "X-Authorization", authorization } ,
				 { "X-Operator-Id", operatorID }
			};
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Get,
				RequestHeaders = headers,
				Url = url
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var games = JsonConvert.DeserializeObject<Data>(res);
			return games.games;
		}
	}
}
