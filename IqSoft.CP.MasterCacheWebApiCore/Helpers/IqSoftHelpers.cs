using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;
using System.Net.Http;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class IqSoftHelpers
	{
		internal static string GetUrl(BllGameProvider provider, BllPartner partner, BllProduct product, bool isForMobile, string token, string language, SessionIdentity clientSession, log4net.ILog log)
		{
			var pKey = CacheManager.GetPartnerSettingByKey(partner.Id, PartnerKeys.IqSoftBrandId);
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = HttpMethod.Post,
				Url = string.Format(provider.GameLaunchUrl, pKey.StringValue, "OpenGame"),
				PostData = JsonConvert.SerializeObject(new 
					{ LanguageId = language, Token = token, IsForMobile = isForMobile, GameId = product.ExternalId, Domain = clientSession.Domain })
			};
			var resp = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
			if (resp.ResponseCode == 0)
				return resp.ResponseObject.ToString();
			return resp.Description;
		}
	}
}