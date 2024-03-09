using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.BGGames;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IqSoft.CP.Integration.Products.Helpers
{
	public class BGGamesHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.BGGames);

		public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
		{
			var response = string.Empty;
			var client = CacheManager.GetClientById(clientId);
			var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiKey);
			var signatureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesSignature);
			var product = CacheManager.GetProductById(productId);
			//var hostName = Dns.GetHostName();
			//var serverIP = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
			//log.Info(JsonConvert.SerializeObject(serverIP.ToString()));
			using (var regionBl = new RegionBll(session, log))
			{
				var region = regionBl.GetRegionByCountryCode(session.Country);
				var data = new BaseInput()
				{
					appID = apiKey,
					userID = client?.Id.ToString() ?? "0",
					userIP = "109.75.47.208", //serverIP.FirstOrDefault().ToString(),
					currency = client?.CurrencyId ?? "EUR",
					customer = token,
					country = region.IsoCode3
				};
				if (product.NickName == "pregame" || product.NickName == "live")
				{
					data.action = product.NickName;
					data.lang = session.LanguageId == "en" ? "en_us" : null;
					data.device = session.DeviceType == (int)DeviceTypes.Desktop ? "D" : "M";
				}
				else
				{
					data.action = "get_game";
					data.license = "2";
					data.game = product.ExternalId;
					data.demo = isForDemo ? "true" : null;
				}
				
				var a = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
				var signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
				var httpRequestInput = new HttpRequestInput
				{
					RequestMethod = Constants.HttpRequestMethods.Post,
					ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
					Url = Provider.GameLaunchUrl,
					PostData = CommonFunctions.GetUriEndocingFromObject(data) + $"&signature={signature}"
				};
				var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
				var regex = new Regex(@"<iframe.*?src=[""'](.*?)[""'].*?>", RegexOptions.IgnoreCase);
				Match match = regex.Match(res);
				if (match.Success)
					response = match.Groups[1].Value;
				return response.Replace("yourdomain.here", $"{session.Domain}");
			}
		}

		public static List<Game> GetGames(int partnerId, ILog log)
		{
			var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiKey);
			var signatureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesSignature);
			var data = new
			{
				action = "get_gameList",
				appID = apiKey,
				per_page = "10000"
			};
			var signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data) + signatureKey);
			var httpRequestInput = new HttpRequestInput
			{
				RequestMethod = Constants.HttpRequestMethods.Post,
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
				Url = Provider.GameLaunchUrl,
				PostData = CommonFunctions.GetUriEndocingFromObject(data) + $"&signature={signature}"
			};
			var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			var resData = JsonConvert.DeserializeObject<Data>(res);
            var response = JsonConvert.DeserializeObject<List<Game>>(JsonConvert.SerializeObject(resData.data));
            return response;
		}


		//public static string GetUrl(string token, int clientId, int partnerId, int productId, bool isForDemo, SessionIdentity session, log4net.ILog log)
		//{
		//	var client = CacheManager.GetClientById(clientId);
		//	var product = CacheManager.GetProductById(productId);
		//	var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiUrl);
		//	var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiKey);
		//	var signatureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesSignature);
		//	var casinoPageUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CasinoPageUrl).StringValue;
		//	if (string.IsNullOrEmpty(casinoPageUrl))
		//		casinoPageUrl = string.Format("https://{0}/casino/all-games", session.Domain);
		//	else
		//		casinoPageUrl = string.Format(casinoPageUrl, session.Domain);
		//	var data = new BaseInput() { operation = "CreateToken", request = new { currency = client.CurrencyId,  userId = clientId} };
		//	var stringData = JsonConvert.SerializeObject(data);
		//	var signature = CommonFunctions.ComputeSha512(signatureKey + stringData);
		//	var headers = new Dictionary<string, string>
		//	{
		//		{ "x-auth", apiKey },
		//		{ "x-signature", signature}
		//	};
		//	var httpRequestInput = new HttpRequestInput
		//	{
		//		RequestMethod = Constants.HttpRequestMethods.Post,
		//		RequestHeaders = headers,
		//		Url = url,
		//		PostData = stringData
		//	};
		//	var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		//	var output = JsonConvert.DeserializeObject<BaseOutput>(res);
		//	if (output.error != null)
		//	{
		//		log.Error(JsonConvert.SerializeObject(output.error));
		//		return null;
		//	}
		//	var input = new
		//	{
		//		country = session.Country,
		//		gamecode = product.ExternalId,
		//		language = session.LanguageId,
		//		mode = isForDemo ? "DEMO" : "REAL",
		//		platform = session.DeviceType == (int)DeviceTypes.Desktop ? "DESKTOP" : "MOBILE",
		//		returnUrl = casinoPageUrl,
		//		token,
		//		username = clientId
		//	};
		//	var queryString = CommonFunctions.GetUriEndocingFromObject(input);
		//	var launchUrl = "&{Provider.GameLaunchUrl}/?{queryString}";
		//	return launchUrl;					
		//}

		//public static List<Response> GetGames(int partnerId, log4net.ILog log)
		//{
		//	var url = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiUrl);
		//	var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesApiKey);
		//	var signatureKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.BGGamesSignature);
		//	var data = new BaseInput() { operation = "GetGames", request = new Object() };
		//	var stringData = JsonConvert.SerializeObject(data);
		//	var signature = CommonFunctions.ComputeSha512(signatureKey + stringData);
		//	var headers = new Dictionary<string, string>
		//	{
		//		{ "x-auth", apiKey },
		//		{ "x-signature", signature}
		//    };
		//	var httpRequestInput = new HttpRequestInput
		//	{
		//		RequestMethod = Constants.HttpRequestMethods.Post,
		//		RequestHeaders = headers,
		//		Url = url,
		//		PostData = stringData
		//	};
		//	var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		//	var output = JsonConvert.DeserializeObject<BaseOutput>(res);
		//	if (output.error != null) 
		//	{
		//		log.Error(JsonConvert.SerializeObject(output.error));
		//		return new List<Response>();
		//	}
		//	else 
		//	  return output.response;
		//}
	}
}
