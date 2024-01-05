using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Platforms.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public class CloudflareHelpers
    {
        public static void PurgeCache(int partnerId)
        {
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.CloudflareApiUrl).StringValue; 
            var apiKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareApiKey).StringValue; 
            var email = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareEmail).StringValue;

            var zoneIds = CacheManager.GetConfigParameters(partnerId, Constants.PartnerKeys.CloudflareZoneId).Select(x => x.Value).ToList();

            var requestHeaders = new Dictionary<string, string> { { "X-Auth-Email", email },
                                                                  { "X-Auth-Key", apiKey } };
            foreach (var zoneId in zoneIds)
            {
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = string.Format("{0}/{1}/purge_cache", url, zoneId),
                    RequestHeaders = requestHeaders,
                    PostData = "{\"purge_everything\":true}"
                };

                var res = JsonConvert.DeserializeObject<PurgeOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                if (!res.Success)
                    throw new System.Exception(res.Messages.ToString());
            }
        }
    }
}
