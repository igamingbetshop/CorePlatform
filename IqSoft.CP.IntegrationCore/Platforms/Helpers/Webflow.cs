using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Platforms.Models.Webflow;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class Webflow
    {
        public static object GetWebflowItemByType(ApiWebflowInput apiWebflowInput, string languageId)
        {
            switch (apiWebflowInput.WebflowItemType)
            {
                case (int)WebflowItemTypes.Site:
                    return GetSites(apiWebflowInput.PartnerId, languageId);
                case (int)WebflowItemTypes.Collection:
                    if (string.IsNullOrEmpty(apiWebflowInput.SiteId))
                        throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters);
                    return GetCollections(apiWebflowInput.PartnerId, languageId, apiWebflowInput.SiteId);
                case (int)WebflowItemTypes.Item:
                    if (string.IsNullOrEmpty(apiWebflowInput.CollectionId))
                        throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters);
                    return GetItems(apiWebflowInput.PartnerId, languageId, apiWebflowInput.CollectionId);
                case (int)WebflowItemTypes.ItemBody:
                    if (string.IsNullOrEmpty(apiWebflowInput.CollectionId) || string.IsNullOrEmpty(apiWebflowInput.ItemId))
                        throw BaseBll.CreateException(languageId, Constants.Errors.WrongInputParameters);
                    return GetItemById(apiWebflowInput.PartnerId, languageId, apiWebflowInput.CollectionId, apiWebflowInput.ItemId);
            }
            throw BaseBll.CreateException(languageId, Constants.Errors.WrongParameters);
        }

        public static List<BaseOutput> GetSites(int partnerId, string languageId)
        {
            var token = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WebflowAccessToken).StringValue;
            if (string.IsNullOrEmpty(token))
                throw BaseBll.CreateException(languageId, Constants.Errors.PartnerKeyNotFound);
            var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token }, { "accept-version", "1.0.0" } };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = requestHeaders,
                Url = "https://api.webflow.com/sites"
            };
            return JsonConvert.DeserializeObject<List<BaseOutput>>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }
        public static List<BaseOutput> GetCollections(int partnerId, string languageId, string siteId)
        {

            var token = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WebflowAccessToken).StringValue;
            if (string.IsNullOrEmpty(token))
                throw BaseBll.CreateException(languageId, Constants.Errors.PartnerKeyNotFound);

            var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token }, { "accept-version", "1.0.0" } };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = requestHeaders,
                Url =string.Format("https://api.webflow.com/sites/{0}/collections", siteId)
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            return JsonConvert.DeserializeObject<List<BaseOutput>>(res);
        }

        public static List<BaseOutput> GetItems(int partnerId, string languageId, string collectionId)
        {

            var token = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WebflowAccessToken).StringValue;
            if (string.IsNullOrEmpty(token))
                throw BaseBll.CreateException(languageId, Constants.Errors.PartnerKeyNotFound);

            var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token }, { "accept-version", "1.0.0" } };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = requestHeaders,
                Url = string.Format("https://api.webflow.com/collections/{0}/items", collectionId)
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            return JsonConvert.DeserializeObject<CollectionOutput>(res).Items;
        }
        public static string GetItemById(int partnerId, string languageId, string collectionId, string itemId)
        {

            var token = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.WebflowAccessToken).StringValue;
            if (string.IsNullOrEmpty(token))
                throw BaseBll.CreateException(languageId, Constants.Errors.PartnerKeyNotFound);

            var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + token }, { "accept-version", "1.0.0" } };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = requestHeaders,
                Url = string.Format("https://api.webflow.com/collections/{0}/items/{1}", collectionId, itemId)
            };
            var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            return JsonConvert.DeserializeObject<ItemOutput>(res).Items[0].PostBody;
        }
    }
}
