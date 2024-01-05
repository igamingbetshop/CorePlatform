using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class IqSoftHelpers
	{
		private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.IqSoft);

		internal static string GetUrl(BllGameProvider provider, BllPartner partner, BllProduct product, bool isForMobile, 
			string token, GetProductUrlInput input, SessionIdentity clientSession, log4net.ILog log)
		{
			var pKey = CacheManager.GetPartnerSettingByKey(partner.Id, PartnerKeys.IqSoftBrandId);
			if (pKey != null && pKey.Id > 0)
			{
				var httpRequestInput = new HttpRequestInput
				{
					ContentType = Constants.HttpContentTypes.ApplicationJson,
					RequestMethod = Constants.HttpRequestMethods.Post,
					Url = string.Format(provider.GameLaunchUrl, pKey.StringValue, "OpenGame"),
					PostData = JsonConvert.SerializeObject(new
					{ LanguageId = input.LanguageId, Token = token, IsForMobile = isForMobile, GameId = product.ExternalId, Domain = clientSession.Domain })
				};
				var resp = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
				if (resp.ResponseCode == 0)
					return resp.ResponseObject.ToString() + "&timezone=" + input.TimeZone;
				return resp.Description;
			}
			else
				return string.Format(Provider.GameLaunchUrl, product.ExternalId, token) + "&timezone=" + input.TimeZone;
		}
	}
}