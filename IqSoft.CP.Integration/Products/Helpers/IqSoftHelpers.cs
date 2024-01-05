using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Bonus;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Products.Models.IqSoft;
using log4net;
using Newtonsoft.Json;
using ProductCategory = IqSoft.CP.Integration.Products.Models.IqSoft.ProductCategory;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class IqSoftHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.IqSoft);
        public static HttpRequestInput GetBetInfo(string pInfo, BllGameProvider provider, string externalTransactionId,
            string languageId, string productId, int partnerid)
        {
            var request = new
            {
                LanguageId = languageId,
                Credentials = ConfigurationManager.AppSettings["WebSiteCredentials"],
                RequestData = externalTransactionId,
                ProductId = productId,
                PartnerId = partnerid
            };
            return new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(request),
                Url = string.Format(provider.GameLaunchUrl, pInfo, "GetBetInfo"),
            };
        }

        public static List<PartnerProductModel> GetPartnerGames(int partnerId)
        {
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiUrl);
            var apiKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiKey);
            var userId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiUserId);
            var apiPartnerId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiPartnerId);
            var resourcesUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiResourcesUrl);
            var categoriesInput = new
            {
                Controller = "Product",
                Method = "GetProductCategories",
                UserId = userId,
                ApiKey = apiKey
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(categoriesInput),
                Url = apiUrl
            };

            var responseObject = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (responseObject.ResponseCode != 0)
                throw new System.Exception(responseObject.Description);
            var categories = JsonConvert.DeserializeObject<List<ProductCategory>>(JsonConvert.SerializeObject(responseObject.ResponseObject));
            var result = new List<PartnerProductModel>();
            var nextPage = true;
            var skipCount = 0;
            while (nextPage)
            {
                var productsInput = new
                {
                    Controller = "Product",
                    Method = "GetPartnerProductSettings",
                    UserId = userId,
                    ApiKey = apiKey,
                    RequestObject = new
                    {
                        PartnerId = apiPartnerId,
                        SkipCount = skipCount,
                        TakeCount = 100
                    }
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(productsInput);
                responseObject = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (responseObject.ResponseCode != 0)
                    throw new System.Exception(responseObject.Description);
                var res = JsonConvert.DeserializeObject<PagedModel<PartnerProductModel>>(JsonConvert.SerializeObject(responseObject.ResponseObject))
                                         .Entities.Select(x =>
                                         {
                                             x.CategoryName = categories.FirstOrDefault(y => y.Id == x.CategoryIds?[0])?.Name;
                                             if (!string.IsNullOrEmpty(resourcesUrl))
                                             {
                                                 if (!string.IsNullOrEmpty(x.WebImageUrl) && !x.WebImageUrl.StartsWith("http"))
                                                     x.WebImageUrl = resourcesUrl + x.WebImageUrl;
                                                 if (!string.IsNullOrEmpty(x.MobileImageUrl) && !x.MobileImageUrl.StartsWith("http"))
                                                     x.MobileImageUrl = resourcesUrl + x.MobileImageUrl;
                                             }
                                             return x;
                                         });
                nextPage = !(res.Count() < 100);
                ++skipCount;
                result.AddRange(res);
            }
            return result;
        }

        public static void AddFreeRound(FreeSpinModel freeSpinModel)
        {
            var client = CacheManager.GetClientById(freeSpinModel.ClientId);
            var url = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.IqSoftFreeSpinApiUrl);
            var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.IqSoftFreeSpinApiKey);
            var apiPartnerId = CacheManager.GetGameProviderValueByKey(client.PartnerId, Provider.Id, Constants.PartnerKeys.IqSoftApiPartnerId);

            var input = new
            {
                freeSpinModel.ClientId,
                freeSpinModel.SpinCount,
                ValidUntil = freeSpinModel.FinishTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ProductId = freeSpinModel.ProductExternalId,
                ApiToken = CommonFunctions.ComputeMd5($"{apiPartnerId}:{freeSpinModel.ClientId}:{apiKey}"),
                freeSpinModel.Lines,
                freeSpinModel.Coins,
                freeSpinModel.CoinValue,
                freeSpinModel.BetValueLevel
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(input),
                Url = url
            };
            var responseObject = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (responseObject.ResponseCode != 0)
                throw new Exception(responseObject.Description);
        }
    }
}