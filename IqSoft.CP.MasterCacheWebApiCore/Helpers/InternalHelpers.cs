using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
	public static class InternalHelpers
	{
		internal static string GetUrl(BllProduct product, int partnerId, string token, BllGameProvider provider, GetProductUrlInput input, SessionIdentity session)
		{
			//Temp Code
			if (product.ParentId == 31) //sportsbook
			{
				if (product.Id == 1100) //comparison
				{
					return string.IsNullOrEmpty(token) ? string.Empty : string.Format("https://comparisonwebsite.{0}/#/home?partnerid={1}&languageid={2}&token={3}", session.Domain,
						partnerId, input.LanguageId, token);
				}
				var resp = string.Format("https://sportsbookwebsite.{0}/{1}/{2}?partnerid={3}&languageid={4}&token={5}", session.Domain,
					(input.Position == "asianweb" ? "asianweb" : "website"/*(isForMobile ? "mobile" : "web")*/), (input.Position == "asianweb" ? "prematch" : input.Position),
					partnerId, input.LanguageId, token);
                if (input.IsForMobile.HasValue)
                    resp += "&isForMobile=" + (input.IsForMobile.Value ? "true" : "false");
                return resp;
			}
            //if (product.ParentId == 11) //virtual games for staging
            //{
            //    return string.Format("http://10.50.17.10:11009/{1}/{2}/{7}?gameid={3}&partnerid={4}&languageid={5}&token={6}&viewtype=1", session.Domain,
            //        product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", product.ExternalId,
            //        partnerId, input.LanguageId, token, input.Position);
            //}
            if (product.ParentId == 51) //virtual games
            {
                return string.Format("https://virtualgameswebsite.{0}/{1}/{2}/{7}?gameid={3}&partnerid={4}&languageid={5}&token={6}&viewtype=1", session.Domain,
                    product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", product.ExternalId,
                    partnerId, input.LanguageId, token, input.Position);
            }
            if (product.ParentId == 28) //skill games
				return string.Format("https://skillgameswebsite.{0}/{1}/{2}/{7}?partnerid={3}&gameid={4}&languageid={5}&token={6}", session.Domain,
					product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", partnerId, product.ExternalId, 
					input.LanguageId, token, input.Position);
			if (product.ParentId == 151) //blockchain games
			{
				return string.Format("https://blockchaingameswebsite.{0}/{1}/{2}/{7}?gameid={3}&partnerid={4}&languageid={5}&token={6}",
					session.Domain, product.NickName.ToLower(), (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "web", product.ExternalId,
					partnerId, input.LanguageId, token, input.Position);
			}

			return string.Empty;
		}
		
        /*internal static string GetUrl(BllProduct product, BllPartner partner, bool isForMobile, string position, string token, string language)
        {
            if (product.ParentId == 31) //sportsbook
                return string.Format("http://109.75.47.208:13005/{0}/{1}?partnerid={2}&languageid={3}&token={4}", isForMobile ? "mobile" : "web", position, partner.Id, language, token);
            if (product.ParentId == 51) //virtual games
            {
                //return string.Format("https://virtualgameswebsite.{0}/{1}?partnerid={2}&languageid={3}&token={4}", partner.SiteUrl, isForMobile ? "mobile" : "web", partner.Id, language, token);
                if (product.Id == 1002)
                    return string.Format("http://109.75.47.208:11006/{0}/{1}?gameid={2}&partnerid={3}&languageid={4}&token={5}&viewtype=1",
                        product.Description.ToLower(), isForMobile ? "mobile" : "web", product.ExternalId, partner.Id, language, token);

                return string.Format("http://109.75.47.208:11006/{0}/{1}?gameid={2}&partnerid={3}&languageid={4}&token={5}&&viewtype=1",
                     product.Description.ToLower(), isForMobile ? "mobile" : "web", product.ExternalId, partner.Id, language, token);
            }
            if (product.ParentId == 28) //skill games
                return string.Format("https://skillgameswebsite.{0}/{1}?partnerid={2}&languageid={3}&token={4}", partner.SiteUrl, isForMobile ? "mobile" : "web", partner.Id, language, token);

            #region temp

            //var url = string.Format("http://13.251.183.253:9097/{1}/{2}?partnerid={3}&languageid={4}&token={5}", partner.SiteUrl, isForMobile ? "mobile" : "web", position, partner.Id, language, token);

            #endregion

            return string.Empty;
        }*/
    }
}