using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Products.Models.Evenbet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class EvenBetHelpers
    {
        private static int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.EvenBet).Id;
        public static string GetSessionUrl(int partnerId, int clientId, bool isForDemo, string languageId)
        {           
            var secureKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EvenBetSecureKey);
            var casinoId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EvenBetCasinoId);
            var url = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.EvenBetUrl);
            var client = CacheManager.GetClientById(clientId);
            var requestInput = new
            {
                nick = client.Id.ToString(),
                lang = languageId,
                currency = Constants.Currencies.USADollar 
            };

            var sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(requestInput) + client.Id + secureKey);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                Accept = "application/vnd.api+json",
                RequestMethod = HttpMethod.Post,
                Url = string.Format(url, clientId, "session", casinoId),
                RequestHeaders = new Dictionary<string, string> { { "sign", sign } },
                PostData = CommonFunctions.GetSortedParamWithValuesAsString(requestInput, "&")
            };

            var resp = JsonConvert.DeserializeObject<BaseOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Error != null)
                throw new Exception(JsonConvert.SerializeObject(resp.Error));
            return resp.MainData.Attributes.RedirectUrl;
        }
    }
}