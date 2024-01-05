using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Products.Models.SkyWind;
using Newtonsoft.Json;
using System.Collections.Generic;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class SkyWindHelpers
	{
		private static string _url = CacheManager.GetGameProviderByName(GameProviders.SkyWind).GameLaunchUrl;
		
		public static string UserLogin(int partnerId)
		{
			var secretKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SkyWindSecretKey).StringValue;
			var userName = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SkyWindUserName).StringValue;
			var pass = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.SkyWindPwd).StringValue;

			var resp = SendRequestToProvider(string.Format("{0}/login", _url), 
				JsonConvert.SerializeObject(new { secretKey = secretKey, username = userName, password = pass }), Constants.HttpRequestMethods.Post, new Dictionary<string, string>());

			var obj = JsonConvert.DeserializeObject<LoginUserResponse>(resp);
			if (obj == null)
				return string.Empty;

			return obj.AccessToken;
		}

		public static string GetGameLaunchUrl(string productExternalId, int clientId, string token, string languageId, string header)
		{
			var resp = SendRequestToProvider(string.Format("{0}/players/{1}/games/{2}?ticket={3}&language={4}", _url, clientId, productExternalId, token, languageId), 
				string.Empty, Constants.HttpRequestMethods.Get, new Dictionary<string, string> { { "X-ACCESS-TOKEN", header } });

			var obj = JsonConvert.DeserializeObject<GetGameUrlResponse>(resp);
			if (obj == null)
				return string.Empty;

			return obj.Url;
		}

		private static string SendRequestToProvider(string url, string content, string method, Dictionary<string, string> headers)
		{
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = method,
				Url = url,
				PostData = content,
				RequestHeaders = headers
			};
			return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}
	}
}
