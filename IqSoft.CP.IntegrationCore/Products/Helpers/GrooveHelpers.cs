using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Products.Models.Groove;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class GrooveHelpers
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.GrooveGaming).Id;
        public static List<GameItem> GetGames(int partnerId)
        {
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GrooveAPIUrl);
            var email = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GrooveEmail);
            var password = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.GroovePassword);
            var loginInput = new { email, password };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = string.Format("{0}/1.0/login", apiUrl),
                PostData = JsonConvert.SerializeObject(loginInput)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out HttpResponseHeaders outputHeaders);
            var token = outputHeaders.GetValues("jwt-auth").First();
            var headers = new Dictionary<string, string> { { "jwt-auth", token } };
            httpRequestInput.RequestHeaders = headers;
            httpRequestInput.RequestMethod = System.Net.Http.HttpMethod.Get;
            httpRequestInput.Url = string.Format("{0}/games/1.0/view/detailed?version=1.0", apiUrl);
            httpRequestInput.PostData = string.Empty;
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            try
            {
                return JsonConvert.DeserializeObject<List<GameItem>>(resp);
            }
            catch
            {
                throw new Exception(resp);
            }
        }
    }
}
