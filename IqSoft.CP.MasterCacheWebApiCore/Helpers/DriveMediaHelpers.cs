using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class DriveMediaHelpers
    {
        private readonly static Dictionary<string, int> Spaces = new Dictionary<string, int>
        {
            { "craftbet", 1000080 },
            { "betslig",  1000181 },
            { "ssbets",   1000264 },
            { "iranbets", 1000305 }
        };
        private readonly static Dictionary<string, string> PartnerUrls = new Dictionary<string, string>
        {
            { "craftbet", "http://craftbet.com/#/products/slots" },
            { "betslig",  "http://betslig90.com/#/products/slots" },
            { "ssbets",   "http://ssbets.com/#/products/slots" },
            { "iranbets", "http://iranbets.com/#/products/slots" }
        };

        public static string GetUrl(GetProductUrlInput input, string playerCurrencyId, string token)
        {
            var partner = CacheManager.GetPartnerById(input.PartnerId);
            var product = CacheManager.GetProductById(input.ProductId);
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            var productExternalId = product.ExternalId;
            var urlBytes = Encoding.UTF8.GetBytes(PartnerUrls[partner.Name.ToLower()]);
            var backUrl = Convert.ToBase64String(urlBytes);
            var encodedUrl = backUrl;
            if (backUrl.IndexOf('=') != -1)
                encodedUrl = backUrl.Replace("=", "%3D");

            var message = string.Format("space={0}&currency={1}&login={2}&game={3}&tech={4}&lang={5}&demo={6}&params={7}&back_url={8}",
                Spaces[partner.Name.ToLower()], partner.Name.ToLower() == "ssbets" && !input.IsForDemo ? playerCurrencyId : "FUN", input.IsForDemo ? "demo" : input.ClientId.ToString(), 
                productExternalId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "desktop", input.LanguageId, input.IsForDemo ? 1 : 0, token, encodedUrl);
            var sign = CommonFunctions.ComputeMd5(GetDriveMediaSecretKey(partner.Id) + message).ToUpper();
            var callUrl = string.Format(provider.GameLaunchUrl, sign,
                Spaces[partner.Name.ToLower()], partner.Name.ToLower() == "ssbets" && !input.IsForDemo ? playerCurrencyId : "FUN", input.IsForDemo ? "demo" : input.ClientId.ToString(), 
                productExternalId, (input.IsForMobile.HasValue && input.IsForMobile.Value) ? "mobile" : "desktop", input.LanguageId, input.IsForDemo ? 1 : 0, token, encodedUrl);

            var response = CommonFunctions.SendHttpRequest(new HttpRequestInput { Url = callUrl, RequestMethod = HttpMethod.Post }, out _);
            var responseObject = JsonConvert.DeserializeObject<GetUrlResponse>(response);
            if (responseObject.success == 0)
                throw new ArgumentNullException(responseObject.message);
            return responseObject.content.url;
        }

        private class GetUrlResponse

        {
            public int success { get; }
            public string message { get; }
            public Content content { get; }

            public class Content
            {
                public string url { get; }
            }
        }

        private static string GetDriveMediaSecretKey(int partnerId)
        {
            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, CacheManager.GetGameProviderByName(Constants.GameProviders.DriveMedia).Id, Constants.PartnerKeys.DriveMediaSecretKey);
            if (partnerKey == null || partnerKey == string.Empty)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            return partnerKey;
        }
    }
}