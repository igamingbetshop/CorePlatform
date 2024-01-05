using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.DAL.Models;
using System;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class EndorphinaHelpers
    {
        public static string GetUrl(int partnerId, string sessionToken, int productId, bool isForDemo, SessionIdentity session)
        {
            var gameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.Endorphina);
            var product = CacheManager.GetProductById(productId);
            var siteUrl = "https://" + session.Id;
            var url = gameProvider.GameLaunchUrl;
            if (isForDemo)
            {
                 url = string.Format("https://edemo.endorphina.com/api/link/accountId/823/hash/{0}/returnURL/{1}", CommonFunctions.ComputeMd5(product.ExternalId),
                                  Uri.EscapeDataString(Uri.EscapeDataString(siteUrl)));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = url
                };
                return CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            }
                       
            var salt = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.EndorphinaSalt);
            var merchantId = CacheManager.GetGameProviderValueByKey(partnerId, gameProvider.Id, Constants.PartnerKeys.EndorphinaMerchantId);

            var requestData = new EndorphinaInput
            {
                exit = siteUrl,
                nodeId = merchantId,
                token = sessionToken
            };          
            requestData.sign = CommonFunctions.ComputeSha1(CommonFunctions.GetSortedValuesAsString(requestData, string.Empty) + salt);
            return string.Format(url, Uri.EscapeDataString(requestData.exit), merchantId, sessionToken, requestData.sign);

        }
    }
}