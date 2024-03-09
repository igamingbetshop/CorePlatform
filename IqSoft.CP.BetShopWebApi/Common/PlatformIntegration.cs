using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;

namespace IqSoft.CP.BetShopWebApi.Common
{
	public static class PlatformIntegration
	{
		private static readonly string PlatformUrl =  ConfigurationManager.AppSettings["PlatformBetShopClientGatewayUrl"];
		private const string PlatformRequestUrlFormat = "{0}/{1}";

		public static ApiResponseBase SendRequestToPlatform<T>(T input, string method)
		{
			var url = string.Format(PlatformRequestUrlFormat, PlatformUrl, method);
				
			var requestInput = new HttpRequestInput
			{
				Url = url,
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
				PostData = JsonConvert.SerializeObject(input)
			};
			var responseStr = CommonFunctions.SendHttpRequest(requestInput, out _);
			return JsonConvert.DeserializeObject<ApiResponseBase>(responseStr);
		}			
	
		public static string GetErrorById(GetErrorInput input)
        {
            var res = PlatformIntegration.SendRequestToPlatform(input,  ApiMethods.GetErrorById);
			if (res.ResponseCode != Constants.SuccessResponseCode)
				throw new Exception(JsonConvert.SerializeObject(res));
			return res.Description;
		}
	}
}