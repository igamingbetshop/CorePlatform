using IqSoft.CP.BetShopWebApi.Models.Common;
using IqSoft.CP.BetShopWebApiCore;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace IqSoft.CP.BetShopWebApi.Common
{
	public static class PlatformIntegration
	{
		public static ApiResponseBase SendRequestToPlatform<T>(T input, string method)
		{				
			var requestInput = new HttpRequestInput
			{
				Url = $"{Program.AppSetting.PlatformBetShopClientGatewayUrl}/{method}",
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = HttpMethod.Post,
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