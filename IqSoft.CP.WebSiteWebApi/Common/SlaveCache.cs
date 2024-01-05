using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.WebSiteWebApi.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using Enyim.Caching;
using Microsoft.Extensions.Configuration;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System.IO;
using Microsoft.Extensions.Logging;

namespace IqSoft.CP.WebSiteWebApi.Common
{
    public static class SlaveCache
    {
        private static readonly MemcachedClient _memcachedClient;
        private static readonly ILoggerFactory _loggerFacotry = new LoggerFactory();

        static SlaveCache()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configurationRoot = builder.Build();
            var options = new MemcachedClientOptions();
            configurationRoot.GetSection("enyimMemcached").Bind(options);
            options.Protocol = MemcachedProtocol.Binary;
            _memcachedClient = new MemcachedClient(_loggerFacotry, new MemcachedClientConfiguration(_loggerFacotry, options));
        }

        public static void RemoveFromCache(string key)
        {
            _memcachedClient.Remove(key);
        }

        public static BllPartnerProductSetting GetPartnerProductSettingByProductId(int partnerId, int productId)
        {
            var key = $"{CacheItems.PartnerProductSettings}_{partnerId}_{productId}";
            var oldValue = _memcachedClient.Get<BllPartnerProductSetting>(key);
            if (oldValue != null)
                return oldValue as BllPartnerProductSetting;
            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetPartnerProductSetting",
                new { PartnerId = partnerId, ProductId = productId });
            if (resp.ResponseCode == 0)
            {
                var newValue = JsonConvert.DeserializeObject<BllPartnerProductSetting>(JsonConvert.SerializeObject(resp.ResponseObject));
                _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
                return newValue;
            }
            return null;
        }

        public static List<BllBanner> GetImagesFromCache(int partnerId, ApiBannerInput request)
        {
            var key = $"{CacheItems.Banners}_{partnerId}_{request.Type}_{request.LanguageId}";
            var oldValue = _memcachedClient.Get<List<BllBanner>>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetBanners", request);
            var newValue = JsonConvert.DeserializeObject<List<BllBanner>>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static List<BllPromotion> GetPromotionsFromCache(int partnerId, ApiRequestBase request)
        {
            var key = $"{CacheItems.Promotions}_{partnerId}_{request.LanguageId}";
            var oldValue = _memcachedClient.Get<List<BllPromotion>>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetPromotions", request);
            var newValue = JsonConvert.DeserializeObject<List<BllPromotion>>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static BllProduct GetProductById(int partnerId, int productId, string languageId)
        {
            var key = $"{CacheItems.Products}_{productId}_{languageId}";
            var oldValue = _memcachedClient.Get<BllProduct>(key);
            if (oldValue != null) 
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetProductById", new { ProductId = productId, LanguageId = languageId });
            var newValue = JsonConvert.DeserializeObject<BllProduct>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static List<BllJobArea> GetJobAreasFromCache(int partnerId, ApiRequestBase apiRequest)
        {
            var key = $"{CacheItems.JobAreas}_{partnerId}_{apiRequest.LanguageId}";
            var oldValue = _memcachedClient.Get<List<BllJobArea>>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetJobAreas", apiRequest);
            var newValue = JsonConvert.DeserializeObject<List<BllJobArea>>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static List<BllReferralType> GetReferralTypesFromCache(int partnerId, ApiRequestBase apiRequest)
        {
            var key = $"{CacheItems.ReferralTypes}_{partnerId}_{apiRequest.LanguageId}";
            var oldValue = _memcachedClient.Get<List<BllReferralType>>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetReferralTypes", apiRequest);
            var newValue = JsonConvert.DeserializeObject<List<BllReferralType>>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static List<BllReferralType> GetExclusionReasonsFromCache(int partnerId, ApiRequestBase apiRequest)
        {
            var key = $"{CacheItems.ExclusionReasons}_{partnerId}_{apiRequest.LanguageId}";
            var oldValue = _memcachedClient.Get<List<BllReferralType>>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetExclusionReasons", apiRequest);
            var newValue = JsonConvert.DeserializeObject<List<BllReferralType>>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static BllGeolocationData GetGeolocationDataFromCache(int partnerId, ApiRequestBase input)
        {
            var key = $"{CacheItems.Countries}_{input.Domain.Replace(".", string.Empty)}_{input.CountryCode}";
            var oldValue = _memcachedClient.Get<BllGeolocationData>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetGeolocationData", input);
            var newValue = JsonConvert.DeserializeObject<BllGeolocationData>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static BllProduct GetProductFromCache(int partnerId, int id)
        {
            var key = $"{CacheItems.Products}_{id}";
            var oldValue = _memcachedClient.Get<BllProduct>(key);
            if (oldValue != null)
                return oldValue;
            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetProductById", new { ProductId = id });
            var response = JsonConvert.SerializeObject(resp.ResponseObject);
            var newValue = JsonConvert.DeserializeObject<BllProduct>(response);
            if (newValue.Id != 0)
            {
                _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
                return newValue;
            }
            return null;
        }

        public static BllGameProvider GetGameProviderFromCache(int partnerId, int id)
        {
            var key = $"{CacheItems.GameProviders}_{id}";
            var oldValue = _memcachedClient.Get<BllGameProvider>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetGameProviderById", new { ProviderId = id });
            var newValue = JsonConvert.DeserializeObject<BllGameProvider>(JsonConvert.SerializeObject(resp.ResponseObject));

            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static string GetInternalProductUrl(int partnerId, GetProductUrlInput input)
        {
            var key = $"{CacheItems.InternalProducts}_{partnerId}_{input.ProductId}_{input.LanguageId}_" +
                $"{(input.IsForMobile.HasValue && input.IsForMobile.Value ? 1 : 0)}_{input.Position}_{input.Domain}";
            var oldValue = _memcachedClient.Get<BllInternalProductUrl>(key);
            if (oldValue != null)
                return oldValue.Url;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetProductUrl", input);
            var newValue = JsonConvert.DeserializeObject<string>(JsonConvert.SerializeObject(resp.ResponseObject));

            _memcachedClient.Store(StoreMode.Set, key, new BllInternalProductUrl { Url = newValue }, TimeSpan.FromMinutes(5));
            return newValue;
        }

        public static List<DateTime> GetIpCount(int partnerId, string methodName, string ip)
        {
            var key = $"{CacheItems.Ips}_{partnerId}_{methodName}_{ip}";
            var oldValue = _memcachedClient.Get<BllIpCount>(key);

            if (oldValue != null)
            {
                var lastTime = DateTime.UtcNow.AddMinutes(-5);
                var dates = JsonConvert.DeserializeObject<List<DateTime>>(oldValue.Dates);
                dates.RemoveAll(x => x < lastTime);
                oldValue.Dates = JsonConvert.SerializeObject(dates, new JsonSerializerSettings
                {
                    DateParseHandling = DateParseHandling.DateTimeOffset
                });
                var res = _memcachedClient.Store(StoreMode.Set, key, oldValue, TimeSpan.FromMinutes(5));
                return dates;
            }
            return new List<DateTime>();
        }

        public static BllIpCount UpdateIpCount(int partnerId, string methodName, string ip)
        {
            var key = $"{CacheItems.Ips}_{partnerId}_{methodName}_{ip}";
            var oldValue = _memcachedClient.Get<BllIpCount>(key);
            var currentTime = DateTime.UtcNow;

            var dates = new List<DateTime>();
            if (oldValue != null)
            {
                var lastTime = currentTime.AddMinutes(-5);
                dates = JsonConvert.DeserializeObject<List<DateTime>>(oldValue.Dates);
                dates.RemoveAll(x => x < lastTime);
            }
            else
                oldValue = new BllIpCount { Dates = JsonConvert.SerializeObject(dates) };

            dates.Add(currentTime);
            oldValue.Dates = JsonConvert.SerializeObject(dates, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            });

            _memcachedClient.Store(StoreMode.Set, key, oldValue, TimeSpan.FromMinutes(5));
            return oldValue;
        }

        public static ApiRestrictionModel GetApiRestrictions(int partnerId)
        {
            var key = $"{CacheItems.Restrictions}_{partnerId}";
            var oldValue = _memcachedClient.Get<ApiRestrictionModel>(key);
            if (oldValue != null)
                return oldValue;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetApiRestrictions", partnerId);
            var newValue = JsonConvert.DeserializeObject<ApiRestrictionModel>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static int GetRegistrationsCount(int partnerId, string ip)
        {
            var key = $"{CacheItems.Registrations}_{partnerId}_{ip}";
            var oldValue = _memcachedClient.Get(key);
            if (oldValue != null)
                return Convert.ToInt32(oldValue.ToString());
            
            var currentDate = DateTime.UtcNow;
            _memcachedClient.Store(StoreMode.Set, key, "0", TimeSpan.FromHours(24 - currentDate.Hour));
            return 0;
        }

        public static void IncrementRegistrationsCount(int partnerId, string ip)
        {
            var key = $"{CacheItems.Registrations}_{partnerId}_{ip}";
            var oldValue = _memcachedClient.Get(key);
            if (oldValue != null)
                _memcachedClient.Increment(key, 1, 1);
            else
            {
                var currentDate = DateTime.UtcNow;
                _memcachedClient.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(24 - currentDate.Hour));
            }
        }

        public static BllFnErrorType GetErrorTypeById(int partnerId, int id, string languageId)
        {
            var key = $"{CacheItems.Errors}_{id}_{languageId}";
            var oldValue = _memcachedClient.Get(key);
            if (oldValue != null) return oldValue as BllFnErrorType;

            var resp = MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, "GetErrorType", new { Id = id, LanguageId = languageId });
            var newValue = JsonConvert.DeserializeObject<BllFnErrorType>(JsonConvert.SerializeObject(resp.ResponseObject));
            _memcachedClient.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }
    }
}