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
        public static string PurgeCache(int partnerId)
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
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/{1}/purge_cache", url, zoneIds[0]),
                    RequestHeaders = requestHeaders,
                    PostData = "{\"purge_everything\":true}"
                };
                var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var res = JsonConvert.DeserializeObject<CloudOutput>(result);
                if (!res.Success)
                    throw new System.Exception(res.Messages.ToString());
            }
            return "OK";
        }

        public static List<DnsItem> GetDnsRecords(int partnerId, string zoneId)
        {
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.CloudflareApiUrl).StringValue;
            var apiKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareApiKey).StringValue;
            var email = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareEmail).StringValue;

            var requestHeaders = new Dictionary<string, string> { { "X-Auth-Email", email },
                                                                  { "X-Auth-Key", apiKey } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}/{1}/dns_records", url, zoneId),
                RequestHeaders = requestHeaders
            };
            var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var res = JsonConvert.DeserializeObject<CloudOutput>(result);
            if (!res.Success)
                throw new System.Exception(result);
            return JsonConvert.DeserializeObject<List<DnsItem>>(JsonConvert.SerializeObject(res.ResultData));
        }

        public static string AddDnsRecord(DnsItem input, int partnerId, string zoneId)
        {
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.CloudflareApiUrl).StringValue;
            var apiKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareApiKey).StringValue;
            var email = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareEmail).StringValue;

            var requestHeaders = new Dictionary<string, string> { { "X-Auth-Email", email },
                                                                  { "X-Auth-Key", apiKey } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/{1}/dns_records", url, zoneId),
                RequestHeaders = requestHeaders,
                PostData = JsonConvert.SerializeObject(new
                {
                    type = input.Type,
                    name = input.Name,
                    content = input.Content,
                    ttl = input.TTL,
                    proxied = input.Proxied,
                    priority = input.Priority
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };
            var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var res = JsonConvert.DeserializeObject<CloudOutput>(result);
            if (!res.Success)
                throw new System.Exception(result);

            return "OK";
        }

        public static string DeleteDnsRecord(string id, int partnerId, string zoneId)
        {
            var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.CloudflareApiUrl).StringValue;
            var apiKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareApiKey).StringValue;
            var email = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CloudflareEmail).StringValue;

            var requestHeaders = new Dictionary<string, string> { { "X-Auth-Email", email },
                                                                  { "X-Auth-Key", apiKey } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Delete,
                Url = string.Format("{0}/{1}/dns_records/{2}", url, zoneId, id),
                RequestHeaders = requestHeaders
            };
            var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var res = JsonConvert.DeserializeObject<CloudOutput>(result);
            if (!res.Success)
                throw new System.Exception(result);

            return "OK";
        }
    }
}
