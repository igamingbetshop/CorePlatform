using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
	public static class InternalHelpers
	{
		internal static string GetUrl(BllProduct product, BllPartner partner, string token, BllGameProvider provider, GetProductUrlInput input, SessionIdentity session)
		{
			//Temp Code
			var env = "production"; //development,production 
			var domain = session.Domain;
			var pKey = CacheManager.GetPartnerSettingByKey(partner.Id, PartnerKeys.IgnoreSessionDomain);
			if (pKey != null && pKey.Id > 0 && pKey.NumericValue == 1)
				domain = partner.SiteUrl.Split(',')[0];
			if (product.ParentId == 31) //sportsbook
			{
				if (product.Id == 1100) //comparison
				{
					return string.IsNullOrEmpty(token) ? string.Empty : string.Format("https://comparisonwebsite.{0}/#/home?partnerid={1}&languageid={2}&token={3}&timezone={4}", domain,
						partner.Id, input.LanguageId, token, input.TimeZone);
				}
				else if (product.Id == 1101) //pool betting
				{
					return string.Format("https://poolbetting.{0}?partnerid={1}&languageid={2}&token={3}&timezone={4}", domain,
						partner.Id, input.LanguageId, token, input.TimeZone);
				}
                var resp = string.Format((env == "production" ? "https://sportsbookwebsite.{0}" : "http://10.50.17.10:13001") + 
					"/{1}/{2}?partnerid={3}&languageid={4}&token={5}&timezone={6}", domain,
					(input.Position == "asianweb" ? "asianweb" : "website"),
					(input.Position == "asianweb" ? "prematch" : input.Position),
					partner.Id, input.LanguageId, token, input.TimeZone);

				if (input.IsForMobile.HasValue)
					resp += "&isForMobile=" + (input.IsForMobile.Value ? "true" : "false");
				return resp;
			}
			if (product.ParentId == 51) //virtual games
			{
				return string.Format((env == "production" ? "https://virtualgameswebsite.{0}" : "http://10.50.17.10:11006") +
                    "/{1}/{2}?gameid={3}&partnerid={4}&languageid={5}&token={6}&viewtype=1&timezone={8}", domain,
                    product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", product.ExternalId,
                    partner.Id, input.LanguageId, token, input.Position, input.TimeZone);//check position
            }
            if (product.ParentId == 28) //skill games
            {
                return string.Format((env == "production" ? "https://skillgameswebsite.{0}" : "http://10.50.17.10:12006") +
                    "/{1}/{2}/{7}?partnerid={3}&gameid={4}&languageid={5}&token={6}&timezone={8}", domain,
                    product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", partner.Id, product.ExternalId,
                    input.LanguageId, token, input.Position, input.TimeZone);
            }
            if (product.ParentId == 151) //blockchain games
			{
				return string.Format((env == "production" ? "https://virtualgameswebsite.{0}" : "http://10.50.17.10:13006") +
					"/{1}/{2}/{7}?gameid={3}&partnerid={4}&languageid={5}&token={6}&timezone={8}",
					domain, product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", product.ExternalId,
					partner.Id, input.LanguageId, token, input.Position, input.TimeZone);
			}

			return string.Empty;
		}
    }
}