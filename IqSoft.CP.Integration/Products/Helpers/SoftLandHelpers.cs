using GraphQLParser;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.SoftLand;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.WebRequestMethods;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class SoftLandHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftLand);
		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
		{

			var product = CacheManager.GetProductById(productId);
			var client = CacheManager.GetClientById(clientId);
			if (client == null)
				throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
			var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SoftLandIdentityUrl).StringValue;
			var identityId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SoftLandIdentityId).StringValue;
			var siteApiKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SoftLandSiteApiKey).StringValue;
			var clientSecret = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SoftLandClientSecret).StringValue;
			var postData = new
			{
				clientId = identityId,
				clientSecret = clientSecret,
				externalPlayerId = client.Id.ToString(),
				gameId = Convert.ToUInt32(product.ExternalId),
				gameCurrency = client.CurrencyId,
				balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientId).AvailableBalance, 2),
				platformToken = token ?? "Dummy",
				depositUrl = session.Domain,
			    returnUrl = session.Domain,
				language = session.LanguageId.ToUpper(),
				isDemo = isForDemo,
				region = session.Country,
				siteId = session.Domain,
				siteApiKey = siteApiKey,
				deviceType = session.DeviceType == (int)DeviceTypes.Desktop ? "Desktop" : "Mobile",
			};
			log.Info(JsonConvert.SerializeObject(postData));
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,  
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = $"{url}/api/v1/auth",
				PostData = JsonConvert.SerializeObject(postData), 
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var authorizationAutput = JsonConvert.DeserializeObject<AuthorizationOutput>(response);
			if (string.IsNullOrEmpty(authorizationAutput.AccessToken))
				return response;
			else
			{
				return $"{Provider.GameLaunchUrl}?token={authorizationAutput.AccessToken}&refresh_token={authorizationAutput.RefreshToken}";
			}
		}

		public static List<Game> GetGames(int partnerId)
		{
			var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftLandIdentityUrl).StringValue;
			var identityId = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftLandIdentityId).StringValue;
			var siteApiKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftLandSiteApiKey).StringValue;
			var clientSecret = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SoftLandClientSecret).StringValue;
			var partner = CacheManager.GetPartnerById(partnerId);
			var siteId = "zorrobet365.com"; // partner.SiteUrl.Split(',')[0];
			var postData = new
			{
				clientId = identityId,
				clientSecret = clientSecret,
				siteId = siteId,
				siteApiKey = siteApiKey
			};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = $"{url}/api/v1/auth/site",
				PostData = JsonConvert.SerializeObject(postData),
			};
			var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var output = JsonConvert.DeserializeObject<AuthorizationOutput>(response);
			
			httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Get,
				Url = $"{url}/api/v1/games?token={output.AccessToken}"
			};
			response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var games = JsonConvert.DeserializeObject<List<Game>>(response);
			return games;
		}
	}
}
