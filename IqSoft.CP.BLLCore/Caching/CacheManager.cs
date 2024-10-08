using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using Newtonsoft.Json;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Helpers;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using IqSoft.CP.Common.Models.CacheModels;
using Microsoft.EntityFrameworkCore;

namespace IqSoft.CP.BLL.Caching
{
    public static class CacheManager
    {
        private static readonly MemcachedClient MemcachedCache;
        private static readonly ILoggerFactory _loggerFacotry = new LoggerFactory();
        static CacheManager()
        {
            var fl = string.Format(@"{0}\appsettings.json", Directory.GetCurrentDirectory());
            IConfigurationBuilder builder;
            if (File.Exists(fl))
                builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            else
                builder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Environment.ProcessPath)).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configurationRoot = builder.Build();
            var options = new MemcachedClientOptions();
            configurationRoot.GetSection("enyimMemcached").Bind(options);
            options.Protocol = MemcachedProtocol.Binary;
            MemcachedCache = new MemcachedClient(_loggerFacotry, new MemcachedClientConfiguration(_loggerFacotry, options));
        }

        public static void RemoveFromCache(string key)
        {
            MemcachedCache.Remove(key);
        }
        public static void UpdateCacheItem(string key, object newValue, TimeSpan timeSpan)
        {
            MemcachedCache.Store(StoreMode.Set, key, newValue, timeSpan);
        }

        #region Partners

        public static BllPartner GetPartnerById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Partners, id);
            var oldValue = MemcachedCache.Get<BllPartner>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemovePartner(int partnerId)
        {
            MemcachedCache.Remove(string.Format("{0}_{1}", Constants.CacheItems.Partners, partnerId));
        }

        private static BllPartner GetPartnerFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Partners.Select(x => new BllPartner
            {
                Id = x.Id,
                Name = x.Name,
                CurrencyId = x.CurrencyId,
                SiteUrl = x.SiteUrl,
                AdminSiteUrl = x.AdminSiteUrl,
                State = x.State,
                SessionId = x.SessionId,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                AccountingDayStartTime = x.AccountingDayStartTime,
                ClientMinAge = x.ClientMinAge,
                PasswordRegExp = x.PasswordRegExp,
                VerificationType = x.VerificationType,
                EmailVerificationCodeLength = x.EmailVerificationCodeLength,
                MobileVerificationCodeLength = x.MobileVerificationCodeLength,
                UnusedAmountWithdrawPercent = x.UnusedAmountWithdrawPercent,
                UserSessionExpireTime = x.UserSessionExpireTime,
                UnpaidWinValidPeriod = x.UnpaidWinValidPeriod,
                VerificationKeyActiveMinutes = x.VerificationKeyActiveMinutes,
                AutoApproveBetShopDepositMaxAmount = x.AutoApproveBetShopDepositMaxAmount,
                AutoApproveWithdrawMaxAmount = x.AutoApproveWithdrawMaxAmount,
                AutoConfirmWithdrawMaxAmount = x.AutoConfirmWithdrawMaxAmount,
                ClientSessionExpireTime = x.ClientSessionExpireTime
            }).FirstOrDefault(x => x.Id == id);
        }

        public static DAL.Models.Cache.PartnerKey GetPartnerSettingByKey(int? partnerId, string nickName)
        {
            var key = nickName;
            if (partnerId.HasValue)
                key = string.Format("{0}_{1}", nickName, partnerId);

            var oldValue = MemcachedCache.Get<DAL.Models.Cache.PartnerKey>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetPartnerSettingByKeyFromDb(partnerId, nickName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }
        public static void RemovePartnerSettingByKey(int? partnerId, string nickName)
        {
            var key = nickName;
            if (partnerId.HasValue)
                key = string.Format("{0}_{1}", nickName, partnerId);
            MemcachedCache.Remove(key);
        }

        private static DAL.Models.Cache.PartnerKey GetPartnerSettingByKeyFromDb(int? partnerId, string nickName)
        {
            using var db = new IqSoftCorePlatformEntities();
            var res = db.PartnerKeys.Where(x => x.Name == nickName && x.PartnerId == partnerId)
                                 .Select(x => new DAL.Models.Cache.PartnerKey
                                 {
                                     Id = x.Id,
                                     PartnerId = x.PartnerId,
                                     GameProviderId = x.GameProviderId,
                                     PaymentSystemId = x.PaymentSystemId,
                                     Name = x.Name,
                                     StringValue = x.StringValue,
                                     DateValue = x.DateValue,
                                     NumericValue = x.NumericValue,
                                     NotificationServiceId = x.NotificationServiceId
                                 }).FirstOrDefault();
            if (res == null)
                return new DAL.Models.Cache.PartnerKey();
            return res;
        }

        public static string GetGameProviderValueByKey(int partnerId, int gameProviderId, string nickName)
        {
            var key = string.Format("{0}_{1}_{2}", nickName, gameProviderId, partnerId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetGameProviderValueByKeyFromDb(partnerId, gameProviderId, nickName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static string GetGameProviderValueByKeyFromDb(int partnerId, int gameProviderId, string key)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PartnerKeys.Where(x => x.Name == key && (partnerId != 0 ? x.PartnerId == partnerId : x.PartnerId == null) &&
                (gameProviderId != 0 ? x.GameProviderId == gameProviderId : x.GameProviderId == null)).Select(x => x.StringValue).FirstOrDefault();
        }

        public static void RemovePartnerSettingByKey(int? partnerId, int? gameProviderId, int? paymentSystemId, int? notificationServiceId, string nickName)
        {
            var key = string.Format("{0}_{1}", nickName, partnerId);
            MemcachedCache.Remove(key);
            if (gameProviderId.HasValue)
            {
                key = string.Format("{0}_{1}_{2}", nickName, gameProviderId, partnerId);
                MemcachedCache.Remove(key);
            }
            if (paymentSystemId.HasValue)
            {
                key = string.Format("{0}_{1}_{2}", nickName, paymentSystemId, partnerId);
                MemcachedCache.Remove(key);
            }
            if (notificationServiceId.HasValue)
            {
                key = string.Format("NotificationService_{0}_{1}_{2}", nickName, notificationServiceId, partnerId);
                MemcachedCache.Remove(key);
            }
        }

        public static string GetPartnerPaymentSystemByKey(int partnerId, int paymentSystemId, string nickName)
        {
            var key = string.Format("{0}_{1}_{2}", nickName, paymentSystemId, partnerId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerPaymentSystemByKeyFromDb(partnerId, paymentSystemId, nickName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static string GetPartnerPaymentSystemByKeyFromDb(int partnerId, int paymentSystemId, string key)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                return db.PartnerKeys.Where(x => x.Name == key && (partnerId != 0 ? x.PartnerId == partnerId : x.PartnerId == null) &&
                    (paymentSystemId != 0 ? x.PaymentSystemId == paymentSystemId : x.PaymentSystemId == null)).Select(x => x.StringValue).FirstOrDefault();
            }
        }

        public static string GetNotificationServiceValueByKey(int partnerId, string nickName, int notificationServiceId)
        {
            var key = string.Format("NotificationService_{0}_{1}_{2}", nickName, notificationServiceId, partnerId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetNotificationServiceByKeyFromDb(partnerId, notificationServiceId, nickName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static string GetNotificationServiceByKeyFromDb(int partnerId, int notificationServiceId, string key)
        {
            using var db = new IqSoftCorePlatformEntities();
            var query = db.PartnerKeys.Where(x => x.Name == key && x.NotificationServiceId == notificationServiceId);
            if (partnerId == 0)
                query = query.Where(x => x.PartnerId == null);
            else
                query = query.Where(x => x.PartnerId == partnerId);

            return query.Select(x => x.StringValue).FirstOrDefault();
        }

        public static List<BllSecurityQuestion> GetPartnerSecurityQuestions(int partnerId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.SecurityQuestions, partnerId, languageId);
            var oldValue = MemcachedCache.Get<List<BllSecurityQuestion>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerSecurityQuestionsFromDB(partnerId, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemovePartnerSecurityQuestionsByKey(int partnerId, string languageId)
        {
            if (!string.IsNullOrEmpty(languageId))
            {
                MemcachedCache.Remove(string.Format("{0}_{1}_{2}", Constants.CacheItems.SecurityQuestions, partnerId, languageId));
                return;
            }
            var languages = GetAvailableLanguages();
            var key = string.Format("{0}_{1}_", Constants.CacheItems.SecurityQuestions, partnerId);
            foreach (var l in languages)
            {
                MemcachedCache.Remove(key + "_" + l.Id);
            }
        }

        private static List<BllSecurityQuestion> GetPartnerSecurityQuestionsFromDB(int partnerId, string languageId)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                return db.fn_SecurityQuestion(languageId).Where(x => x.PartnerId == partnerId)
                                           .Select(x => new BllSecurityQuestion
                                           {
                                               Id = x.Id,
                                               PartnerId = x.PartnerId,
                                               NickName = x.NickName,
                                               Status  =  x.Status,
                                               TranslationId = x.TranslationId.Value,
                                               QuestionText = x.QuestionText,
                                               CreationTime = x.CreationTime,
                                               LastUpdateTime = x.LastUpdateTime
                                           }).ToList();
            }
        }

        public static Dictionary<string, string> GetConfigParameters(int partnerId, string name)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, partnerId, name);
            var oldValue = MemcachedCache.Get<Dictionary<string, string>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetConfigParametersFromDB(partnerId, name);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static Dictionary<string, string> GetConfigParametersFromDB(int partnerId, string keyName)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.WebSiteSubMenuItems
                      .Where(x => x.MenuItem.Menu.PartnerId == partnerId && x.MenuItem.Menu.Type == Constants.WebSiteConfiguration.Config &&
                     x.MenuItem.Title == keyName
                      ).ToDictionary(x => x.Title, x => x.Href);
        }

        public static string GetConfigKey(int partnerId, string name)
        {
            var key = string.Format("ConfigKey_{0}_{1}", partnerId, name);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetConfigKeyFromDb(partnerId, name);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemoveConfigKey(int partnerId, string name)
        {
            var key = string.Format("ConfigKey_{0}_{1}", partnerId, name);
            MemcachedCache.Remove(key);
        }

        private static string GetConfigKeyFromDb(int partnerId, string name)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.WebSiteMenuItems.Where(x => x.Menu.PartnerId == partnerId &&
                                                  x.Menu.Type == Constants.WebSiteConfiguration.Config && x.Title == name).
                                                    Select(x => x.Href).FirstOrDefault();
        }

        public static void RemoveGameProviderValueByKey(int partnerId, int gameProviderId, string nickName)
        {
            var key = string.Format("{0}_{1}_{2}", nickName, gameProviderId, partnerId);
            MemcachedCache.Remove(key);
        }

        public static void RemovePartnerSettingKeyFromCache(int partnerId, string nickName)
        {
            var key = string.Format("{0}_{1}", nickName, partnerId);
            MemcachedCache.Remove(key);

        }

        public static BllMessageTemplate GetPartnerMessageTemplate(int partnerId, int clientInfoType, string LanguageId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.MessageTemplates, partnerId, clientInfoType, LanguageId);
            var oldValue = MemcachedCache.Get<BllMessageTemplate>(key);
            if (oldValue != null)
                return oldValue;

            var newValue = GetPartnerMessageTemplateFromDb(partnerId, clientInfoType, LanguageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static BllMessageTemplate GetPartnerMessageTemplateFromDb(int partnerId, int clientInfoType, string LanguageId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_MessageTemplate(LanguageId).Where(x => x.PartnerId == partnerId && x.ClientInfoType == clientInfoType).Select(x => new BllMessageTemplate
            {
                Id = x.Id,
                NickName = x.NickName,
                PartnerId = x.PartnerId,
                Text = x.Text,
                ClientInfoType = x.ClientInfoType,
                ExternalTemplateId = x.ExternalTemplateId
            }).FirstOrDefault();
        }

        public static void RemoveMessageTemplateFromCache(int partnerId, int clientInfoType, string LanguageId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.MessageTemplates, partnerId, clientInfoType, LanguageId);
            MemcachedCache.Remove(key);
        }

        public static List<BllPartnerCurrencySetting> GetPartnerCurrencies(int partnerId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.PartnerCurrencies, partnerId);
            var oldValue = MemcachedCache.Get<List<BllPartnerCurrencySetting>>(key);
            if (oldValue != null) return oldValue;

            var newValue = GetPartnerCurrenciesFromDb(partnerId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<BllPartnerCurrencySetting> GetPartnerCurrenciesFromDb(int partnerId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PartnerCurrencySettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerCurrencyStates.Active).Select(x => new BllPartnerCurrencySetting
            {
                Id = x.Id,
                PartnerId = x.PartnerId,
                CurrencyId = x.CurrencyId,
                State = x.State,
                UserMinLimit = x.UserMinLimit,
                UserMaxLimit = x.UserMaxLimit,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                Priority = x.Priority,
                ClientMinBet = x.ClientMinBet
            }).ToList();
        }

        public static List<BllSegmentSetting> GetSegmentSetting(int segmentId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.SegmentSetting, segmentId);
            var oldValue = MemcachedCache.Get<List<BllSegmentSetting>>(key);
            if (oldValue != null) return oldValue;

            var newValue = GetSegmentSettingFromDb(segmentId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<BllSegmentSetting> GetSegmentSettingFromDb(int segmentId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.SegmentSettings.Where(x => x.SegmentId == segmentId)
                                     .Select(x => new BllSegmentSetting
                                     {
                                         Id = x.Id,
                                         SegmentId = x.SegmentId,
                                         Name = x.Name,
                                         StringValue = x.StringValue,
                                         NumericValue = x.NumericValue,
                                         DateValue = x.DateValue,
                                         CreationTime = x.CreationTime,
                                         LastUpdateTime = x.LastUpdateTime
                                     }).ToList();
        }

        public static void RemoveSegmentSettingFromCache(int segmentId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.SegmentSetting, segmentId);
            MemcachedCache.Remove(key);
        }


        #endregion

        #region PaymentSystems

        public static BllMerchant GetMerchantById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Merchants, id);
            var oldValue = MemcachedCache.Get<BllMerchant>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetMerchantFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static BllMerchant GetMerchantFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Merchants.Where(x => x.Id == id)
                               .Select(x => new BllMerchant
                               {
                                   Id = x.Id,
                                   PartnerId = x.PartnerId,
                                   MerchantKey = x.MerchantKey,
                                   MerchantUrl = x.MerchantUrl
                               }).FirstOrDefault();
        }

        public static BllPaymentSystem GetPaymentSystemById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.PaymentSystems, id);
            var oldValue = MemcachedCache.Get<BllPaymentSystem>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPaymentSystemFromDb(id, string.Empty);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static BllPaymentSystem GetPaymentSystemByName(string paymentSystem)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.PaymentSystems, paymentSystem);
            var oldValue = MemcachedCache.Get<BllPaymentSystem>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPaymentSystemFromDb(0, paymentSystem);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

    private static BllPaymentSystem GetPaymentSystemFromDb(int id, string name)
    {
        using var db = new IqSoftCorePlatformEntities();
        return db.PaymentSystems.Where(x => (id != 0 && x.Id == id) || (id == 0 && x.Name == name))
                                .Select(x => new BllPaymentSystem
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    SessionId = x.SessionId,
                                    CreationTime = x.CreationTime,
                                    LastUpdateTime = x.LastUpdateTime,
                                    PeriodicityOfRequest = x.PeriodicityOfRequest,
                                    PaymentRequestSendCount = x.PaymentRequestSendCount,
                                    Type = x.Type,
                                    TranslationId = x.TranslationId,
                                    ContentType = x.ContentType
                                }).FirstOrDefault();
    }

        #endregion

        #region CashDesks

        public static BllCashDesk GetCashDeskById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.CashDesks, id);
            var oldValue = MemcachedCache.Get<BllCashDesk>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetCashDeskFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void UpdateCashDeskInCache(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.CashDesks, id);
            var newValue = GetCashDeskFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        private static BllCashDesk GetCashDeskFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.CashDesks.Where(x => x.Id == id).Select(x => new BllCashDesk
            {
                Id = x.Id,
                BetShopId = x.BetShopId,
                Name = x.Name,
                State = x.State,
                SessionId = x.SessionId,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                MacAddress = x.MacAddress,
                EncryptPassword = x.EncryptPassword,
                EncryptSalt = x.EncryptSalt,
                EncryptIv = x.EncryptIv,
                CurrentCasherId = x.CurrentCashierId,
                Type = x.Type
            }).FirstOrDefault();
        }

        #endregion

        #region BetShops

        public static BllBetShop GetBetShopById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BetShops, id);
            var oldValue = MemcachedCache.Get<BllBetShop>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetBetShopFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static BllBetShop UpdateBetShopById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BetShops, id);
            var newValue = GetBetShopFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static BllBetShop GetBetShopFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.BetShops.Where(x => x.Id == id).Select(x => new BllBetShop
            {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                PartnerId = x.PartnerId,
                Name = x.Name,
                Address = x.Address,
                PrintLogo = x.PrintLogo,
                Ips = x.Ips
            }).FirstOrDefault();
        }

        #endregion

        #region Products
        public static BllProduct GetProductGroupByName(string productGroupName)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ProductGroups, productGroupName);
            var oldValue = MemcachedCache.Get<BllProduct>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetProductGroupFromDb(productGroupName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static BllProduct GetProductGroupFromDb(string productGroupName)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Products.Where(x => x.Level == 2 && x.NickName == productGroupName).Select(x => new BllProduct
            {
                Id = x.Id,
                TranslationId = x.TranslationId,
                GameProviderId = x.GameProviderId,
                Level = x.Level,
                NickName = x.NickName,
                ParentId = x.ParentId,
                ExternalId = x.ExternalId,
                State = x.State
            }).FirstOrDefault();
        }

        public static BllProduct GetProductById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Products, id);
            var oldValue = MemcachedCache.Get<BllProduct>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetProductFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static BllProduct GetProductByExternalId(int providerId, string externalProductId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Products, providerId, externalProductId);
            var oldValue = MemcachedCache.Get<BllProduct>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetProductFromDb(providerId: providerId, externalProductId: externalProductId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void UpdateProductById(int id)
        {
            var newValue = GetProductFromDb(id);
            var key = string.Format("{0}_{1}", Constants.CacheItems.Products, id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Products, newValue.GameProviderId, newValue.ExternalId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        private static BllProduct GetProductFromDb(int? id = null, int? providerId = null, string externalProductId = null)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Products.Where(x => (id != null && x.Id == id.Value) ||
                                          (id == null && x.GameProviderId == providerId.Value &&
                                           x.ExternalId == externalProductId))
                              .Select(x => new BllProduct
                              {
                                  Id = x.Id,
                                  TranslationId = x.TranslationId,
                                  GameProviderId = x.GameProviderId,
                                  SubProviderId = x.SubproviderId,
                                  Level = x.Level,
                                  NickName = x.NickName,
                                  ParentId = x.ParentId,
                                  ExternalId = x.ExternalId,
                                  State = x.State,
                                  FreeSpinSupport = x.FreeSpinSupport,
                                  Jackpot = x.Jackpot,
                                  WebImageUrl = x.WebImageUrl,
                                  MobileImageUrl = x.MobileImageUrl,
                                  BackgroundImageUrl = x.BackgroundImageUrl,
                                  HasDemo = x.HasDemo
                              }).FirstOrDefault();
        }

        public static string GetTranslation(long translationId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Translation, translationId, languageId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (!string.IsNullOrEmpty(oldValue))
                return oldValue;
            var newValue = GetTranslationFromDb(translationId, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static string GetTranslationFromDb(long translationId, string languageId)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var result = db.TranslationEntries.FirstOrDefault(x => x.TranslationId == translationId && x.LanguageId == languageId);
                if (result == null)
                {
                    result = db.TranslationEntries.FirstOrDefault(x => x.TranslationId == translationId && x.LanguageId == Constants.DefaultLanguageId);
                    if (result == null)
                        return string.Empty;
                }
                return result.Text;
            }
        }

        public static List<BllProductCategory> GetProductCategories(int partnerId, string language, int type)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.PartnerProductCategories, partnerId, language, type);
            var oldValue = MemcachedCache.Get<List<BllProductCategory>>(key);
            if (oldValue != null)
                return oldValue;
            List<BllProductCategory> newValue;
            if (type == (int)ProductCategoryTypes.ForClient)
                newValue = GetProductCategoriesFromDb(partnerId, language);
            else
                newValue = GetPartnerProductCategoriesFromDb(partnerId, language);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<BllProductCategory> GetProductCategoriesFromDb(int partnerId, string language)
        {
            using var db = new IqSoftCorePlatformEntities();
            var pc = db.fn_ProductCategory(language).Select(x => new BllProductCategory
            {
                Id = x.Id,
                Nickname = x.Nickname,
                Name = x.Name,
                Type = x.Type,
                TranslationId = x.TranslationId
            }).ToList();
            var pps = db.PartnerProductSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerProductSettingStates.Active)
                                               .Select(x => x.Product.CategoryId).Distinct().ToList();
            return pc.Where(x => pps.Contains(x.Id)).ToList();
        }

        private static List<BllProductCategory> GetPartnerProductCategoriesFromDb(int partnerId, string language)
        {
            using var db = new IqSoftCorePlatformEntities();
            var pc = db.fn_ProductCategory(language).Select(x => new BllProductCategory
            {
                Id = x.Id,
                Nickname = x.Nickname,
                Name = x.Name,
                Type = x.Type,
                TranslationId = x.TranslationId
            }).ToList();
            var pps = db.PartnerProductSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerProductSettingStates.Active)
                                               .Select(x => x.CategoryId).Distinct().ToList();
            return pc.Where(x => pps.Contains(x.Id)).ToList();
        }


        public static void RemoveProductCategories(int partnerId, string language)
        {
            MemcachedCache.Remove(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.PartnerProductCategories, partnerId, language, (int)ProductCategoryTypes.ForClient));
            MemcachedCache.Remove(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.PartnerProductCategories, partnerId, language, (int)ProductCategoryTypes.ForPartner));
        }

        public static List<BllMenuItem> GetCasinoMenues(int partnerId)
        {
            var key = Constants.CacheItems.CasinoMenues + "_" + partnerId;
            var oldValue = MemcachedCache.Get<List<BllMenuItem>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetCasinoMenuesFromDb(partnerId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemoveCasinoMenues(int partnerId)
        {
            var key = Constants.CacheItems.CasinoMenues + "_" + partnerId;
            MemcachedCache.Remove(key);
        }

        private static List<BllMenuItem> GetCasinoMenuesFromDb(int partnerId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var resp = db.WebSiteMenuItems.Where(x => x.Menu.PartnerId == partnerId &&
                        x.Menu.Type == Constants.WebSiteConfiguration.CasinoMenu).Select(x => new BllMenuItem
                        {
                            Id = x.Id,
                            Icon = x.Icon,
                            Title = x.Title,
                            Type = x.Type,
                            StyleType = x.StyleType,
                            Href = x.Href,
                            OpenInRouting = x.OpenInRouting,
                            Orientation = x.Orientation,
                            Order = x.Order
                        }).OrderBy(y => y.Order).ToList();
            return resp;
        }

        #endregion

        #region PartnerPaymentSettings
        public static void UpdateParnerPaymentSettings(int partnerId, int paymentId, string currencyId, int type)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.PaymentSystems, partnerId, paymentId, currencyId, type);
            var newValue = GetPartnerPaymentSettingFromDb(partnerId, paymentId, currencyId, type);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        public static BllPartnerPaymentSetting GetPartnerPaymentSettings(int partnerId, int paymentId, string currencyId, int type)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.PaymentSystems, partnerId, paymentId, currencyId, type);
            var oldValue = MemcachedCache.Get<BllPartnerPaymentSetting>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerPaymentSettingFromDb(partnerId, paymentId, currencyId, type);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static BllPartnerPaymentSetting GetPartnerPaymentSettingFromDb(int partnerId, int paymentId, string currencyId, int type)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PartnerPaymentSettings.Include(x => x.PartnerPaymentCurrencyRates)
                .Where(x => x.PartnerId == partnerId && x.PaymentSystemId == paymentId && x.CurrencyId == currencyId && x.Type == type)
                .Select(x => new BllPartnerPaymentSetting
                {
                    Id = x.Id,
                    PartnerId = x.PartnerId,
                    PaymentSystemId = x.PaymentSystemId,
                    CurrencyId = x.CurrencyId,
                    State = x.State,
                    SessionId = x.SessionId,
                    CreationTime = x.CreationTime,
                    LastUpdateTime = x.LastUpdateTime,
                    UserName = x.UserName,
                    Password = x.Password,
                    PaymentSystemPriority = x.PaymentSystemPriority,
                    Type = x.Type,
                    Commission = x.Commission,
                    FixedFee = x.FixedFee,
                    Info = x.Info,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount,
                    AllowMultipleClientsPerPaymentInfo = x.AllowMultipleClientsPerPaymentInfo,
                    AllowMultiplePaymentInfoes = x.AllowMultiplePaymentInfoes,
                    OpenMode = x.OpenMode,
                    Countries = x.PartnerPaymentCountrySettings.Select(y => y.CountryId).ToList(),
                    OSTypesString = x.OSTypes,
                    CurrencyRates = x.PartnerPaymentCurrencyRates.Select(y => new BllCurrencyRate { Id = y.Id, CurrencyId = y.CurrencyId, Rate = y.Rate }).ToList()
                }).FirstOrDefault();
        }

        #endregion

        #region PartnerProductSettings

        public static List<BllProductSetting> GetPartnerProductSettingsForDesktop(int partnerId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}_Desktop", Constants.CacheItems.PartnerProductSettings, partnerId, languageId);
            var oldValue = MemcachedCache.Get<List<BllProductSetting>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetPartnerProductSettingsFromDb(partnerId, languageId, false);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static List<BllProductSetting> GetPartnerProductSettingsForMobile(int partnerId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}_Mobile", Constants.CacheItems.PartnerProductSettings, partnerId, languageId);
            var oldValue = MemcachedCache.Get<List<BllProductSetting>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetPartnerProductSettingsFromDb(partnerId, languageId, true);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }
        public static void RemovePartnerProductSettings(int partnerId)
        {
            var languages = GetAvailableLanguages();
            foreach (var l in languages)
            {
                var key = string.Format("{0}_{1}_{2}_Desktop", Constants.CacheItems.PartnerProductSettings, partnerId, l.Id);
                MemcachedCache.Remove(key);

                key = string.Format("{0}_{1}_{2}_Mobile", Constants.CacheItems.PartnerProductSettings, partnerId, l.Id);
                MemcachedCache.Remove(key);
                // string.Format("{0}_{1}", Constants.CacheItems.ProductCountrySetting, countryCode);
            }
        }

        private static List<BllProductSetting> GetPartnerProductSettingsFromDb(int partnerId, string languageId, bool isForMobile)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                return db.fn_PartnerProductSetting(languageId)
                          .Where(x => x.PartnerId == partnerId && x.State == (int)PartnerProductSettingStates.Active &&
                                      x.ProductState == (int)ProductStates.Active &&
                                      ((isForMobile && x.IsForMobile) || (!isForMobile && x.IsForDesktop)))
                          .Select(x => new BllProductSetting
                          {
                              PartnerId = x.PartnerId,
                              ProductId = x.ProductId,
                              Rating = x.Rating,
                              NickName = x.ProductNickName,
                              Name = x.ProductName,
                              OpenMode = x.OpenMode,
                              SubproviderId = x.SubproviderId ?? x.ProductGameProviderId.Value,
                              CategoryId = x.CategoryId,
                              HasDemo = x.HasDemo,
                              ProviderName = x.SubproviderId.HasValue ? x.SubproviderName : x.GameProviderName
                          }).ToList();
            }
        }

        public static List<int> GetRestrictedProductCountrySettings(string countryCode)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ProductCountrySetting, countryCode);
            var oldValue = MemcachedCache.Get<List<int>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetProductCountrySettingsFromDb(countryCode);
            var res = MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<int> GetProductCountrySettingsFromDb(string countryCode)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var result = db.ProductCountrySettings.Where(x => x.PartnerId == null && x.Type == (int)ProductCountrySettingTypes.Restricted &&
                                                     x.Region.IsoCode.ToLower() == countryCode.ToLower()).Select(x => x.ProductId).ToList();

                var whitelisted = db.ProductCountrySettings.Where(x => x.PartnerId == null && x.Type == (int)ProductCountrySettingTypes.Whitelisted).GroupBy(x => x.ProductId).
                    ToDictionary(x => x.Key, x => x.Select(y => y.Region.IsoCode).ToList());
                result.AddRange(whitelisted.Where(x => !x.Value.Contains(countryCode)).Select(x => x.Key).ToList());

                return result;
            }
        }
        public static BllPartnerProductSetting GetPartnerProductSettingByProductId(int partnerId, int productId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings, partnerId, productId);
            var oldValue = MemcachedCache.Get<BllPartnerProductSetting>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetPartnerProductSettingFromDb(partnerId: partnerId, productId: productId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemovePartnerProductSetting(int partnerId, int productId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings, partnerId, productId);
            MemcachedCache.Remove(key);
        }

        private static BllPartnerProductSetting GetPartnerProductSettingFromDb(int? id = null, int? partnerId = null, int? productId = null)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PartnerProductSettings.Where(x => (id != null && x.Id == id.Value) ||
                                                        (id == null && x.PartnerId == partnerId.Value &&
                                                         x.ProductId == productId.Value))
                                            .Select(x => new BllPartnerProductSetting
                                            {
                                                Id = x.Id,
                                                PartnerId = x.PartnerId,
                                                ProductId = x.ProductId,
                                                Percent = x.Percent,
                                                State = x.State,
                                                Rating = x.Rating,
                                                NickName = x.Product.NickName,
                                                OpenMode = x.OpenMode,
                                                ProviderId = x.Product.GameProviderId ?? 0,
                                                SubproviderId = x.Product.SubproviderId,
                                                CategoryId = x.CategoryId,
                                                HasDemo = x.HasDemo,
                                                RTP = x.RTP,
                                                Volatility = x.Volatility,
                                            }).FirstOrDefault();
        }

        #endregion

        #region Permissions

        public static List<BllPermission> GetPermissions()
        {
            var key = Constants.CacheItems.Permissions;
            var oldValue = MemcachedCache.Get<List<BllPermission>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPermissionsFromDb();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<BllPermission> GetPermissionsFromDb()
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Permissions.Select(x => new BllPermission
            {
                Id = x.Id,
                PermissionGroupId = x.PermissionGroupId,
                Name = x.Name,
                ObjectTypeId = x.ObjectTypeId
            }).ToList();
        }

        public static List<BllFnUserPermission> GetUserPermissions(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserPermissions, userId);
            var oldValue = MemcachedCache.Get<List<BllFnUserPermission>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetUserPermissionsFromDb(userId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void UpdateUserPermissionsInCache(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserPermissions, userId);
            var newValue = GetUserPermissionsFromDb(userId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        private static List<BllFnUserPermission> GetUserPermissionsFromDb(int userId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_UserPermission().Where(x => x.UserId == userId).Select(x => new BllFnUserPermission
            {
                UserId = x.UserId,
                PermissionId = x.PermissionId,
                IsForAll = x.IsForAll,
                IsAdmin = x.IsAdmin
            }).ToList();
        }

        public static List<BllAccessObject> GetUserAccessObjects(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.AccessObjects, userId);
            var oldValue = MemcachedCache.Get<List<BllAccessObject>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetUserAccessObjectsFromDb(userId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void UpdateUserAccessObjectsInCache(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.AccessObjects, userId);
            var newValue = GetUserAccessObjectsFromDb(userId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        private static List<BllAccessObject> GetUserAccessObjectsFromDb(int userId)
        {
            using var db = new IqSoftCorePlatformEntities();

            return db.AccessObjects.Where(x => x.UserId == userId).Select(x => new BllAccessObject
            {
                Id = x.Id,
                ObjectTypeId = x.ObjectTypeId,
                ObjectId = x.ObjectId,
                UserId = x.UserId,
                PermissionId = x.PermissionId
            }).ToList();
        }

        #endregion

        #region Real Time

        public static List<BllOnlineClient> OnlineClients(string currencyId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.OnlineClients, currencyId);
            var oldValue = MemcachedCache.Get<List<BllOnlineClient>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetOnlineClientsFromDb(currencyId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromSeconds(5));
            return newValue;
        }

        private static List<BllOnlineClient> GetOnlineClientsFromDb(string currencyId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var clients = db.fn_OnlineClient(currencyId).Select(x => new BllOnlineClient
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserName = x.UserName,
                RegionId = x.RegionId,
                CurrencyId = x.CurrencyId,
                PartnerId = x.PartnerId,
                IsDocumentVerified = x.IsDocumentVerified,
                RegistrationDate = x.RegistrationDate,
                HasNote = x.HasNote ?? false,
                PartnerName = x.PartnerName,
                CategoryId = x.CategoryId,
                LoginIp = x.LoginIp,
                SessionTime = x.SessionTime,
                SessionLanguage = x.SessionLanguage,
                CurrentPage = x.CurrentPage,
                TotalDepositsCount = x.TotalDepositsCount,
                PendingDepositsCount = x.PendingDepositsCount,
                CanceledDepositsCount = x.CanceledDepositsCount,
                TotalDepositsAmount = x.TotalDepositsAmount,
                PendingDepositsAmount = x.PendingDepositsAmount,
                LastDepositState = x.LastDepositState,
                TotalWithdrawalsCount = x.TotalWithdrawalsCount,
                PendingWithdrawalsCount = x.PendingWithdrawalsCount,
                TotalWithdrawalsAmount = x.TotalWithdrawalsAmount,
                PendingWithdrawalsAmount = x.PendingWithdrawalsAmount,
                TotalBetsCount = x.TotalBetsCount,
                GGR = x.GGR,
                Balance = x.Balance
            }).ToList();
            return clients;
        }

        public static List<BllRealTimeInfo> RealTimeInfo(string currencyId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.RealTimeInfo, currencyId);
            var oldValue = MemcachedCache.Get<List<BllRealTimeInfo>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetRealTimeInfoFromDb(currencyId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromSeconds(5));
            return newValue;
        }

        private static List<BllRealTimeInfo> GetRealTimeInfoFromDb(string currencyId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_RealTimeInfo(currencyId).Select(x => new BllRealTimeInfo
            {
                PartnerId = x.PartnerId,
                LoginCount = x.LoginCount,
                BetsCount = x.BetsCount,
                BetsAmount = x.BetsAmount,
                PlayersCount = x.PlayersCount,
                ApprovedDepositsCount = x.ApprovedDepositsCount,
                ApprovedDepositsAmount = x.ApprovedDepositsAmount,
                ApprovedWithdrawalsCount = x.ApprovedWithdrawalsCount,
                ApprovedWithdrawalsAmount = x.ApprovedWithdrawalsAmount,
                WonBetsCount = x.WonBetsCount,
                WonBetsAmount = x.WonBetsAmount,
                LostBetsCount = x.LostBetsCount,
                LostBetsAmount = x.LostBetsAmount
            }).ToList();
        }

        #endregion

        #region Providers

        public static BllGameProvider GetGameProviderById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.GameProviders, id);
            var oldValue = MemcachedCache.Get<BllGameProvider>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetGameProviderFromDb(id, string.Empty);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static BllGameProvider GetGameProviderByName(string provider)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.GameProviders, provider);
            var oldValue = MemcachedCache.Get<BllGameProvider>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetGameProviderFromDb(0, provider);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemoveGameProviderFromCache(int id, string providerName)
        {
            MemcachedCache.Remove(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, providerName));
            MemcachedCache.Remove(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, id));
        }

        private static BllGameProvider GetGameProviderFromDb(int id, string name)
        {
            using var db = new IqSoftCorePlatformEntities();
            return
                    db.GameProviders.Where(x => (id != 0 && x.Id == id) || (id == 0 && x.Name == name))
                        .Select(x => new BllGameProvider
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Type = x.Type,
                            SessionExpireTime = x.SessionExpireTime,
                            GameLaunchUrl = x.GameLaunchUrl
                        }).FirstOrDefault();
        }

        #endregion

        #region Enumerations

        public static List<BllFnEnumeration> GetEnumerations(string enumType, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Enumerations, enumType, languageId);
            var oldValue = MemcachedCache.Get<List<BllFnEnumeration>>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetEnumerationsFromDb(enumType, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(30d));
            return newValue;
        }

        private static List<BllFnEnumeration> GetEnumerationsFromDb(string enumType, string languageId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_Enumeration().Where(x => x.EnumType == enumType && x.LanguageId == languageId).Select(x => new BllFnEnumeration
            {
                Id = x.Id,
                EnumType = x.EnumType,
                NickName = x.NickName,
                Value = x.Value,
                TranslationId = x.TranslationId,
                Text = x.Text,
                LanguageId = x.LanguageId
            }).ToList();
        }

        public static List<BllJobArea> GetJobAreas(string languageId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.JobAreas, languageId);
            var oldValue = MemcachedCache.Get<List<BllJobArea>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetJobAreasDb(languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(30d));
            return newValue;
        }

        private static List<BllJobArea> GetJobAreasDb(string languageId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_JobArea(languageId).Select(x => new BllJobArea
            {
                Id = x.Id,
                NickName = x.NickName,
                Name = x.Name
            }).ToList();
        }

        public static void RemoveJobAreasFromCache(string languageId)
        {
            if (!string.IsNullOrEmpty(languageId))
                MemcachedCache.Remove(string.Format("{0}_{1}", Constants.CacheItems.JobAreas, languageId));
            var languages = GetAvailableLanguages();
            var key = Constants.CacheItems.JobAreas;
            foreach (var l in languages)
            {
                MemcachedCache.Remove(key + "_" + l.Id);
            }
        }
        #endregion

        #region ClientMessages     

        public static void UpdateClientUnreadTicketsCount(int clientId, int value)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, clientId);
            MemcachedCache.Store(StoreMode.Set, key, new BllUnreadTicketsCount { Count = value }, TimeSpan.FromHours(6));
        }

        public static BllUnreadTicketsCount GetClientUnreadTicketsCount(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, clientId);
            var oldValue = MemcachedCache.Get<BllUnreadTicketsCount>(key);
            if (oldValue != null) return oldValue ;
            var newValue = new BllUnreadTicketsCount { Count = GetClientUnreadTicketsCountFromDb(clientId) };
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static int GetClientUnreadTicketsCountFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Tickets.Count(x => x.Status == (int)MessageTicketState.Active && x.ClientId == clientId &&
                                             x.ClientUnreadMessagesCount > 0);
        }

        #endregion

        #region Categories

        public static BllClientCategory GetClientCategory(int categoryId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientCategories, categoryId);
            var oldValue = MemcachedCache.Get<BllClientCategory>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetClientCategoryFromDb(categoryId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static BllClientCategory GetClientCategoryFromDb(int categoryId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientCategories.Where(x => x.Id == categoryId).Select(x => new BllClientCategory
            {
                Id = x.Id,
                Name = x.NickName
            }).First();
        }

        #endregion

        #region Limits

        public static BllProductLimit GetProductLimit(int objectTypeId, long objectId, int limitTypeId, int productId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.ProductLimits, objectTypeId, objectId,
                limitTypeId, productId);
            var oldValue = MemcachedCache.Get<BllProductLimit>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetProductLimitFromDb(objectTypeId, objectId, limitTypeId, productId);
            if (newValue == null)
                newValue = new BllProductLimit();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static void UpdateProductLimit(int objectTypeId, long objectId, int limitTypeId, int productId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.ProductLimits, objectTypeId, objectId,
                limitTypeId, productId);
            var newValue = GetProductLimitFromDb(objectTypeId, objectId, limitTypeId, productId);
            if (newValue == null)
                newValue = new BllProductLimit();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            if (objectTypeId == (int)ObjectTypes.Partner)
                MemcachedCache.Remove(string.Format("{0}_{1}_{2}", Constants.CacheItems.ProductLimits, objectId,
                    productId));
            else if (objectTypeId == (int)ObjectTypes.Client)
                MemcachedCache.Remove(string.Format("{0}_Client_{1}_{2}", Constants.CacheItems.ProductLimits, objectId,
                    productId));
        }
        public static void SetFutureRollback(string cacheItem, string externalId, string documentId)
        {
            var key = string.Format("{0}_{1}", cacheItem, documentId);
            MemcachedCache.Store(StoreMode.Set, key, externalId, TimeSpan.FromMinutes(5));
        }
        public static string GetFutureRollback(string cacheItem, string documentId)
        {
            var key = string.Format("{0}_{1}", cacheItem, documentId);
            return MemcachedCache.Get<string>(key);
        }

        private static BllProductLimit GetProductLimitFromDb(int objectTypeId, long objectId, int limitTypeId, int productId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ProductLimits.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId && x.LimitTypeId == limitTypeId && x.ProductId == productId).Select(x => new BllProductLimit
            {
                Id = x.Id,
                ObjectId = x.ObjectId,
                ObjectTypeId = x.ObjectTypeId,
                ProductId = x.ProductId.Value,
                LimitTypeId = x.LimitTypeId,
                MinLimit = x.MinLimit,
                MaxLimit = x.MaxLimit
            }).FirstOrDefault();
        }

        public static BllProductLimit GetPartnerProductLimit(int productId, int partnerId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ProductLimits, partnerId, productId);
            var oldValue = MemcachedCache.Get<BllProductLimit>(key);
            if (oldValue != null) return oldValue;
            var newValue = new BllProductLimit();
            int pId = productId;
            while (true)
            {
                var product = GetProductById(pId);
                if (product == null)
                    break;

                var limit = GetProductLimit((int)ObjectTypes.Partner, partnerId,
                    (int)LimitTypes.FixedProductLimit, pId);
                if (limit.Id != 0)
                {
                    newValue = limit;
                    break;
                }

                if (product.ParentId == null)
                    break;

                pId = product.ParentId.Value;
            }
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1d));
            return newValue;
        }

        public static BllProductLimit GetClientProductLimit(int productId, int clientId)
        {
            var key = string.Format("{0}_Client_{1}_{2}", Constants.CacheItems.ProductLimits, clientId, productId);
            var oldValue = MemcachedCache.Get<BllProductLimit>(key);
            if (oldValue != null) return oldValue ;
            var newValue = new BllProductLimit();
            int pId = productId;
            while (true)
            {
                var product = GetProductById(pId);
                if (product == null)
                    break;

                var limit = GetProductLimit((int)ObjectTypes.Client, clientId,
                    (int)LimitTypes.FixedClientMaxLimit, pId);
                if (limit.Id != 0)
                {
                    newValue = limit;
                    break;
                }

                if (product.ParentId == null)
                    break;

                pId = product.ParentId.Value;
            }
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1d));
            return newValue;
        }

        public static decimal GetTotalDepositAmounts(int clientId, int period)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.TotalDepositAmounts, clientId, period);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null) return Convert.ToDecimal(oldValue);
            var newValue = GetTotalDepositAmountsFromDb(clientId, period);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static decimal GetTotalDepositAmountsFromDb(int clientId, int period)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            var fromDate = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            switch (period)
            {
                case (int)PeriodsOfTime.Weekly:
                    int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = fromDate.AddDays(-1 * diff);
                    break;
                case (int)PeriodsOfTime.Monthly:
                    fromDate = new DateTime(currentTime.Year, currentTime.Month, 1, 0, 0, 0);
                    break;
                default:
                    break;
            }
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            if (db.PaymentRequests.Any(x => x.ClientId == clientId &&
                                                   x.Type == (int)PaymentRequestTypes.Deposit &&
                                                   (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually) && x.Date >= fDate))
                return db.PaymentRequests.Where(x => x.ClientId == clientId &&
                                                   x.Type == (int)PaymentRequestTypes.Deposit &&
                                                   (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually) && x.Date >= fDate).Sum(x => x.Amount);
            else
                return 0;
        }

        public static void RemoveTotalDepositAmount(int clientId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.TotalDepositAmounts, clientId, (int)PeriodsOfTime.Daily);
            MemcachedCache.Remove(key);
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.TotalDepositAmounts, clientId, (int)PeriodsOfTime.Weekly);
            MemcachedCache.Remove(key);
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.TotalDepositAmounts, clientId, (int)PeriodsOfTime.Monthly);
            MemcachedCache.Remove(key);
        }

        public static int GetClientDepositCount(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientDeposit, clientId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null) return (int)oldValue;
            var newValue = GetClientDepositCountFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static int GetClientDepositCountFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PaymentRequests.Count(x => x.ClientId == clientId &&
                                                      x.Type == (int)PaymentRequestTypes.Deposit &&
                                                      (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually));
        }

        public static void RemoveClientDepositCount(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientDeposit, clientId);
            MemcachedCache.Remove(key);
        }

        public static decimal GetTotalBetAmounts(int clientId, int period)
        {
            var currentTime = DateTime.UtcNow;
            string key;
            if (period == (int)PeriodsOfTime.Daily)
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, period, currentTime.ToString("yyyyMMdd"));
            else if (period == (int)PeriodsOfTime.Weekly)
            {
                int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, period, currentTime.AddDays(-diff).ToString("yyyyMMdd"));
            }
            else
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, period, currentTime.AddDays(-(currentTime.Day - 1)).ToString("yyyyMMdd"));
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
                return Convert.ToDecimal(oldValue);
            var newValue = GetTotalBetAmountsFromDb(clientId, period, currentTime);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static decimal GetTotalBetAmountsFromDb(int clientId, int period, DateTime currentTime)
        {
            using var db = new IqSoftCorePlatformEntities();
            var fromDate = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            switch (period)
            {
                case (int)PeriodsOfTime.Weekly:
                    int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = fromDate.AddDays(-1 * diff);
                    break;
                case (int)PeriodsOfTime.Monthly:
                    fromDate = new DateTime(currentTime.Year, currentTime.Month, 1, 0, 0, 0);
                    break;
                default:
                    break;
            }
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            if (db.Bets.Any(x => x.ClientId == clientId && x.BetDate >= fDate))
                return db.Bets.Where(x => x.ClientId == clientId && x.BetDate >= fDate).Sum(x => x.BetAmount);
            else
                return 0;
        }

        public static void UpdateTotalBetAmount(int clientId, decimal amount)
        {
            var currentTime = DateTime.UtcNow;
            var clientSetting = new ClientCustomSettings();
            var betLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalBetAmountLimitDaily));
            var systemBetLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalBetAmountLimitDaily));
            if ((betLimitDaily != null && betLimitDaily.Id > 0 && betLimitDaily.NumericValue.HasValue) ||
                (systemBetLimitDaily != null && systemBetLimitDaily.Id > 0 && systemBetLimitDaily.NumericValue.HasValue))
            {
                var dailyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, (int)PeriodsOfTime.Daily, currentTime.ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(dailyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, dailyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, dailyKey, amount, TimeSpan.FromDays(1));
            }
            var betLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalBetAmountLimitWeekly));
            var systemBetLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalBetAmountLimitWeekly));
            if ((betLimitWeekly != null && betLimitWeekly.Id > 0 && betLimitWeekly.NumericValue.HasValue) ||
                (systemBetLimitWeekly != null && systemBetLimitWeekly.Id > 0 && systemBetLimitWeekly.NumericValue.HasValue))
            {
                int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weeklyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, (int)PeriodsOfTime.Weekly, currentTime.AddDays(-diff).ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(weeklyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, weeklyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, weeklyKey, amount, TimeSpan.FromDays(1));
            }
            var betLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalBetAmountLimitMonthly));
            var systemBetLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalBetAmountLimitMonthly));
            if ((betLimitMonthly != null && betLimitMonthly.Id > 0 && betLimitMonthly.NumericValue.HasValue) ||
                (systemBetLimitMonthly != null && systemBetLimitMonthly.Id > 0 && systemBetLimitMonthly.NumericValue.HasValue))
            {
                var monthlyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalBetAmounts, clientId, (int)PeriodsOfTime.Monthly, currentTime.AddDays(-(currentTime.Day - 1)).ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(monthlyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, monthlyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, monthlyKey, amount, TimeSpan.FromDays(1));
            }
        }

        public static decimal GetTotalLossAmounts(int clientId, int period)
        {
            var currentTime = DateTime.UtcNow;
            string key;
            if (period == (int)PeriodsOfTime.Daily)
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, period, currentTime.ToString("yyyyMMdd"));
            else if (period == (int)PeriodsOfTime.Weekly)
            {
                int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, period, currentTime.AddDays(-diff).ToString("yyyyMMdd"));
            }
            else
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, period, currentTime.AddDays(-(currentTime.Day - 1)).ToString("yyyyMMdd"));
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
                return Convert.ToDecimal(oldValue);
            var newValue = GetTotalLossAmountsFromDb(clientId, period, currentTime);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static decimal GetTotalLossAmountsFromDb(int clientId, int period, DateTime currentTime)
        {
            using var db = new IqSoftCorePlatformEntities();
            var fromDate = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
            switch (period)
            {
                case (int)PeriodsOfTime.Weekly:
                    int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = fromDate.AddDays(-1 * diff);
                    break;
                case (int)PeriodsOfTime.Monthly:
                    fromDate = new DateTime(currentTime.Year, currentTime.Month, 1, 0, 0, 0);
                    break;
                default:
                    break;
            }
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            if (db.Bets.Any(x => x.ClientId == clientId && x.BetDate >= fDate))
                return db.Bets.Where(x => x.ClientId == clientId && x.BetDate >= fDate).Sum(x => x.BetAmount - x.WinAmount);
            return 0;
        }

        public static void UpdateTotalLossAmount(int clientId, decimal amount)
        {
            var currentTime = DateTime.UtcNow;
            var clientSetting = new ClientCustomSettings();
            var lossLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalLossLimitDaily));
            var systemLossLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalLossLimitDaily));
            if ((lossLimitDaily != null && lossLimitDaily.Id > 0 && lossLimitDaily.NumericValue.HasValue) ||
                (systemLossLimitDaily != null && systemLossLimitDaily.Id > 0 && systemLossLimitDaily.NumericValue.HasValue))
            {
                var dailyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, (int)PeriodsOfTime.Daily, currentTime.ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(dailyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, dailyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, dailyKey, amount, TimeSpan.FromDays(1));
            }
            var lossLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalLossLimitWeekly));
            var systemLossLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalLossLimitWeekly));
            if ((lossLimitWeekly != null && lossLimitWeekly.Id > 0 && lossLimitWeekly.NumericValue.HasValue) ||
                (systemLossLimitWeekly != null && systemLossLimitWeekly.Id > 0 && systemLossLimitWeekly.NumericValue.HasValue))
            {
                int diff = (7 + (currentTime.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weeklyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, (int)PeriodsOfTime.Weekly, currentTime.AddDays(-diff).ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(weeklyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, weeklyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, weeklyKey, amount, TimeSpan.FromDays(1));
            }
            var lossLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.TotalLossLimitMonthly));
            var systemLossLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemTotalLossLimitMonthly));
            if ((lossLimitMonthly != null && lossLimitMonthly.Id > 0 && lossLimitMonthly.NumericValue.HasValue) ||
                (systemLossLimitMonthly != null && systemLossLimitMonthly.Id > 0 && systemLossLimitMonthly.NumericValue.HasValue))
            {
                var monthlyKey = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.TotalLossAmounts, clientId, (int)PeriodsOfTime.Monthly, currentTime.AddDays(-(currentTime.Day - 1)).ToString("yyyyMMdd"));
                var oldValue = MemcachedCache.Get(monthlyKey);
                if (oldValue != null)
                    MemcachedCache.Store(StoreMode.Set, monthlyKey, Convert.ToDecimal(oldValue) + amount, TimeSpan.FromDays(1));
                else
                    MemcachedCache.Store(StoreMode.Set, monthlyKey, amount, TimeSpan.FromDays(1));
            }
        }

        #endregion

        #region Accounts
        public static BllClientBalance GetClientCurrentBalance(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId);
            var oldValue = MemcachedCache.Get<BllClientBalance>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetClientCurrentBalanceFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static BllClientBalance GetClientCurrentBalanceFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var accounts = db.Accounts.Where(x => x.ObjectId == clientId && x.ObjectTypeId == (int)ObjectTypes.Client &&
                                                      x.Type.Kind != (int)AccountTypeKinds.Booked
                                                      ).ToList();
            return new BllClientBalance
            {
                ClientId = clientId,
                AvailableBalance = Math.Floor(accounts.Where(x => x.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                x.TypeId != (int)AccountTypes.ClientCoinBalance)
                                                      .Sum(x => x.Balance) * 100) / 100,
                CurrencyId = accounts.FirstOrDefault() == null ? string.Empty : accounts.First().CurrencyId,
                Balances = accounts.Select(x => (new BllClientAccount
                {
                    Id = x.Id,
                    Balance = Math.Floor(x.Balance * 100) / 100,
                    CurrencyId = x.CurrencyId,
                    TypeId = x.TypeId
                })).ToList()
            };
        }

        public static void RemoveClientBalance(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId);
            MemcachedCache.Remove(key);
        }

        public static BllAccountType GetAccountTypeById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.AccountTypes, id);
            var oldValue = MemcachedCache.Get<BllAccountType>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetAccountTypeFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static BllAccountType GetAccountTypeFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.AccountTypes.Where(x => x.Id == id).Select(x => new BllAccountType
            {
                Id = x.Id,
                Kind = x.Kind,
                CanBeNegative = x.CanBeNegative,
                TranslationId = x.TranslationId,
                NickName = x.NickName
            }).FirstOrDefault();
        }

        public static List<BllAccountType> GetAccountTypes(string languageId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.AccountTypes, languageId);
            var oldValue = MemcachedCache.Get<List<BllAccountType>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetfnAccountTypesFromDb(languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static List<BllAccountType> GetfnAccountTypesFromDb(string languageId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_AccountType(languageId).Select(x => new BllAccountType
            {
                Id = x.Id,
                Kind = x.Kind,
                CanBeNegative = x.CanBeNegative,
                TranslationId = x.TranslationId,
                NickName = x.NickName,
                Name = x.Name
            }).ToList();
        }

        public static BllAccountBalance GetAccountBalanceByDate(long accountId, DateTime date)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.AccountBalances, accountId, date.Year, date.Month, date.Day);
            var oldValue = MemcachedCache.Get<BllAccountBalance>(key);

            return oldValue;
        }

        public static void SetAccountBalanceByDate(BllAccountBalance accountBalance)
        {
            var key = string.Format("{0}_{1}_{2}_{3}_{4}", Constants.CacheItems.AccountBalances, accountBalance.AccountId, accountBalance.Date.Year, accountBalance.Date.Month, accountBalance.Date.Day);
            MemcachedCache.Store(StoreMode.Set, key, accountBalance, TimeSpan.FromHours(1));
        }

        public static List<int> GetOrderedAccountTypesByOperationTypeId(int operationTypeId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.AccountTypePriorities, operationTypeId);
            var oldValue = MemcachedCache.Get<List<int>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetOrderedAccountTypesFromDb(operationTypeId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static List<int> GetOrderedAccountTypesFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.AccountTypePriorities.Where(x => x.OperationTypeId == id)
                                           .OrderBy(x => x.Priority)
                                           .Select(x => x.AccountTypeId)
                                           .ToList();
        }

        public static List<BllAccountInfo> GetAccountsInfo(int objectTypeId, int objectId, string currencyId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Accounts, objectTypeId, objectId, currencyId);
            var oldValue = MemcachedCache.Get<List<BllAccountInfo>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetAccountsInfoFromDb(objectTypeId, objectId, currencyId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static List<BllAccountInfo> GetAccountsInfoFromDb(int objectTypeId, int objectId, string currencyId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Accounts.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId && x.CurrencyId == currencyId)
                              .Select(x => new BllAccountInfo
                              {
                                  Id = x.Id,
                                  TypeId = x.TypeId
                              }).ToList();
        }

        public static void RemoveAccountsInfo(int objectTypeId, int objectId, string currencyId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Accounts, objectTypeId, objectId, currencyId);
            MemcachedCache.Remove(key);
        }

        public static List<BllFnErrorType> GetfnErrorTypes(string languageId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.fnErrorTypes, languageId);
            var oldValue = MemcachedCache.Get<List<Common.Models.CacheModels.BllFnErrorType>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetfnErrorTypesFromDb(languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(30d));
            return newValue;
        }

        private static List<BllFnErrorType> GetfnErrorTypesFromDb(string languageId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_ErrorType(languageId).Select(x => new BllFnErrorType
            {
                Id = x.Id,
                NickName = x.NickName,
                TranslationId = x.TranslationId,
                Message = x.Message
            }).ToList();
        }

        public static void RemovefnErrorTypes(string languageId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.fnErrorTypes, languageId);
            MemcachedCache.Remove(key);
        }

        public static void RemoveCurrencyById(string id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Currencies, id);
            MemcachedCache.Remove(key);
        }

        public static BllCurrency GetCurrencyById(string id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Currencies, id);
            var oldValue = MemcachedCache.Get<BllCurrency>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetCurrencyByIdFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(30d));
            return newValue;
        }

        private static BllCurrency GetCurrencyByIdFromDb(string id)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Currencies.Where(x => x.Id == id).Select(x => new BllCurrency
            {
                Id = x.Id,
                CurrentRate = x.CurrentRate,
                Symbol = x.Symbol,
                SessionId = x.SessionId,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                Code = x.Code,
                Name =x.Name
            }).FirstOrDefault();
        }

        public static BllCurrency GetCurrencyByCode(string code)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Currencies, code);
            var oldValue = MemcachedCache.Get<BllCurrency>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetCurrencyByCodeFromDb(code);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(30d));
            return newValue;
        }

        private static BllCurrency GetCurrencyByCodeFromDb(string code)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Currencies.Where(x => x.Code == code)
                                .Select(x => new BllCurrency
                                {
                                    Id = x.Id,
                                    CurrentRate = x.CurrentRate,
                                    Symbol = x.Symbol,
                                    SessionId = x.SessionId,
                                    CreationTime = x.CreationTime,
                                    LastUpdateTime = x.LastUpdateTime,
                                    Code = x.Code,
                                    Name = x.Name
                                }).FirstOrDefault();
        }

        public static List<string> GetSupportedCurrencies()
        {
            var key = Constants.CacheItems.Currencies;
            var oldValue = MemcachedCache.Get<List<string>>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetSupportedCurrenciesFromDb();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(60d));
            return newValue;
        }

        private static List<string> GetSupportedCurrenciesFromDb()
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Currencies.Select(x => x.Id).ToList();
        }

        public static int GetClientsCount(int? partnerId)
        {
            var key = partnerId == null
                ? Constants.CacheItems.ClientCounts
                : string.Format("{0}_{1}", Constants.CacheItems.ClientCounts, partnerId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null) return Convert.ToInt32(oldValue);
            var newValue = GetClientsCountFromDb(partnerId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(10d));
            return newValue;
        }

        private static int GetClientsCountFromDb(int? partnerId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Clients.Count(c => partnerId == null || c.PartnerId == partnerId.Value);
        }

        public static List<BllLanguage> GetAvailableLanguages()
        {
            var key = Constants.CacheItems.Languages;
            var oldValue = MemcachedCache.Get<List<BllLanguage>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetAvailableLanguagesFromDb();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static List<BllLanguage> GetAvailableLanguagesFromDb()
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Languages.Select(x => new BllLanguage
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        #endregion

        #region Client

        public static List<BllClientClasification> GetClientClasifications(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientClassifications, clientId);
            var oldValue = MemcachedCache.Get<List<BllClientClasification>>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientClasificationsFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(2));
            return newValue;
        }

        private static List<BllClientClasification> GetClientClasificationsFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientClassifications.Where(x => x.ClientId == clientId)
                                           .Select(x => new BllClientClasification
                                           {
                                               Id = x.Id,
                                               ClientId = x.ClientId,
                                               CategoryId = x.CategoryId,
                                               ProductId = x.ProductId,
                                               SessionId = x.SessionId ?? 0,
                                               SegmentId = x.SegmentId,
                                               LastUpdateTime = x.LastUpdateTime,
                                               State = x.State
                                           }).ToList();
        }
        public static void RemoveClientFailedLoginCount(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientFailedLoginCount, clientId);
            MemcachedCache.Remove(key);
        }

        public static int UpdateClientFailedLoginCount(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientFailedLoginCount, clientId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
            {
                MemcachedCache.Increment(key, 1, 1);
                return Convert.ToInt32(oldValue.ToString()) + 1;
            }

            MemcachedCache.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(1));
            return 1;
        }

        public static int UpdateVerifyCodeRequestCount(string mobileOrEmail)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.VerificationCodeRequestCount, mobileOrEmail.Replace("@", string.Empty).Replace("+", string.Empty));
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
            {
                MemcachedCache.Increment(key, 1, 1);
                return Convert.ToInt32(oldValue.ToString()) + 1;
            }
            MemcachedCache.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(1));
            return 1;
        }

        public static BllClient GetClientById(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Clients, clientId);
            var oldValue = MemcachedCache.Get<BllClient>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientFromDb(clientId);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static BllClient GetClientByEmail(int partnerId, string email)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, partnerId, email);
            var oldValue = MemcachedCache.Get<BllClient>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientByEmailFromDb(partnerId, email);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static BllClient GetClientByUserName(int partnerId, string userName)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, partnerId, userName);
            var oldValue = MemcachedCache.Get<BllClient>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetClientFromDb(partnerId, userName);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }
        public static BllClient GetClientByNickName(int partnerId, string nickName)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, partnerId, nickName);
            var oldValue = MemcachedCache.Get<BllClient>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientByNickNameFromDb(partnerId, nickName);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static BllClient GetClientByMobileNumber(int partnerId, string mobileNumber)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, partnerId, mobileNumber);
            var oldValue = MemcachedCache.Get<BllClient>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetClientByMobileNumberFromDb(partnerId, mobileNumber);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static void RemoveClientFromCache(int clientId)
        {
            var client = GetClientById(clientId);
            var key = string.Format("{0}_{1}", Constants.CacheItems.Clients, clientId);
            MemcachedCache.Remove(key);
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, client.PartnerId, client.UserName);
            MemcachedCache.Remove(key);
            if (!string.IsNullOrEmpty(client.NickName))
            {
                key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, client.PartnerId, client.NickName);
                MemcachedCache.Remove(key);
            }
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, client.PartnerId, client.MobileNumber);
            MemcachedCache.Remove(key);
            key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Clients, client.PartnerId, client.Email);
            MemcachedCache.Remove(key);
        }

        private static BllClient GetClientFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.Clients.Where(x => x.Id == clientId).Select(x => new BllClient
            {
                Id = x.Id,
                Email = x.Email,
                IsEmailVerified = x.IsEmailVerified,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                State = x.State,
                CategoryId = x.CategoryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                NickName = x.NickName,
                SecondName = x.SecondName,
                SecondSurname = x.SecondSurname,
                DocumentNumber = x.DocumentNumber,
                DocumentIssuedBy = x.DocumentIssuedBy,
                DocumentType = x.DocumentType,
                MobileNumber = x.MobileNumber,
                IsMobileNumberVerified = x.IsMobileNumberVerified,
                LanguageId = x.LanguageId,
                CreationTime = x.CreationTime,
                IsDocumentVerified = x.IsDocumentVerified,
                RegionId = x.RegionId,
                City = x.City,
                AffiliateReferralId = x.AffiliateReferralId,
                UserId = x.UserId,
                SendMail = x.SendMail,
                SendSms = x.SendSms,
                SendPromotions = x.SendPromotions,
                Info = x.Info,
                ZipCode = x.ZipCode,
                Address = x.Address,
                PasswordHash = x.PasswordHash,
                Salt = x.Salt,
                LastSessionId = x.LastSessionId,
                Citizenship = x.Citizenship,
                JobArea = x.JobArea,               
                Apartment = x.Apartment,
                BuildingNumber = x.BuildingNumber,
            }).FirstOrDefault();
            if (client != null)
            {
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                    client.State = Convert.ToInt32(parentState.NumericValue.Value);
            }
            return client;
        }

        private static BllClient GetClientFromDb(int partnerId, string userName)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.Clients.Where(x => x.PartnerId == partnerId && x.UserName == userName).Select(x => new BllClient
            {
                Id = x.Id,
                Email = x.Email,
                IsEmailVerified = x.IsEmailVerified,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                State = x.State,
                CategoryId = x.CategoryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                NickName = x.NickName,
                SecondName = x.SecondName,
                SecondSurname = x.SecondSurname,
                DocumentType = x.DocumentType,
                DocumentNumber = x.DocumentNumber,
                MobileNumber = x.MobileNumber,
                IsMobileNumberVerified = x.IsMobileNumberVerified,
                LanguageId = x.LanguageId,
                CreationTime = x.CreationTime,
                IsDocumentVerified = x.IsDocumentVerified,
                RegionId = x.RegionId,
                PasswordHash = x.PasswordHash,
                Salt = x.Salt,
                LastSessionId = x.LastSessionId,
                Citizenship = x.Citizenship,
                JobArea = x.JobArea,
                ZipCode = x.ZipCode,
                Apartment = x.Apartment,
                BuildingNumber = x.BuildingNumber,
            }).FirstOrDefault();
            if (client != null)
            {
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                    client.State = Convert.ToInt32(parentState.NumericValue.Value);
            }
            return client;
        }

        private static BllClient GetClientByNickNameFromDb(int partnerId, string nickName)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.Clients.Where(x => x.PartnerId == partnerId && x.NickName == nickName).Select(x => new BllClient
            {
                Id = x.Id,
                Email = x.Email,
                IsEmailVerified = x.IsEmailVerified,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                State = x.State,
                CategoryId = x.CategoryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                NickName = x.NickName,
                SecondName = x.SecondName,
                SecondSurname = x.SecondSurname,
                DocumentType = x.DocumentType,
                DocumentNumber = x.DocumentNumber,
                MobileNumber = x.MobileNumber,
                IsMobileNumberVerified = x.IsMobileNumberVerified,
                LanguageId = x.LanguageId,
                CreationTime = x.CreationTime,
                IsDocumentVerified = x.IsDocumentVerified,
                RegionId = x.RegionId,
                PasswordHash = x.PasswordHash,
                Salt = x.Salt,
                LastSessionId = x.LastSessionId,
                Citizenship = x.Citizenship,
                JobArea = x.JobArea,
                ZipCode = x.ZipCode,
                Apartment = x.Apartment,
                BuildingNumber = x.BuildingNumber,
            }).FirstOrDefault();
            if (client != null)
            {
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                    client.State = Convert.ToInt32(parentState.NumericValue.Value);
            }
            return client;
        }

        private static BllClient GetClientByMobileNumberFromDb(int partnerId, string mobileNumber)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.Clients.Where(x => x.PartnerId == partnerId && x.MobileNumber == mobileNumber).Select(x => new BllClient
            {
                Id = x.Id,
                Email = x.Email,
                IsEmailVerified = x.IsEmailVerified,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                State = x.State,
                CategoryId = x.CategoryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                NickName = x.NickName,
                DocumentType = x.DocumentType,
                DocumentNumber = x.DocumentNumber,
                MobileNumber = x.MobileNumber,
                IsMobileNumberVerified = x.IsMobileNumberVerified,
                LanguageId = x.LanguageId,
                CreationTime = x.CreationTime,
                IsDocumentVerified = x.IsDocumentVerified,
                RegionId = x.RegionId,
                PasswordHash = x.PasswordHash,
                Salt = x.Salt,
                LastSessionId = x.LastSessionId,
                Citizenship = x.Citizenship,
                JobArea = x.JobArea,
                ZipCode = x.ZipCode,
                Apartment = x.Apartment,
                BuildingNumber = x.BuildingNumber,
            }).FirstOrDefault();
            if (client != null)
            {
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                    client.State = Convert.ToInt32(parentState.NumericValue.Value);
            }
            return client;
        }

        private static BllClient GetClientByEmailFromDb(int partnerId, string email)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.Clients.Where(x => x.PartnerId == partnerId && x.Email == email).Select(x => new BllClient
            {
                Id = x.Id,
                Email = x.Email,
                IsEmailVerified = x.IsEmailVerified,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                PartnerId = x.PartnerId,
                Gender = x.Gender,
                BirthDate = x.BirthDate,
                State = x.State,
                CategoryId = x.CategoryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                NickName = x.NickName,
                DocumentType = x.DocumentType,
                DocumentNumber = x.DocumentNumber,
                MobileNumber = x.MobileNumber,
                IsMobileNumberVerified = x.IsMobileNumberVerified,
                LanguageId = x.LanguageId,
                CreationTime = x.CreationTime,
                IsDocumentVerified = x.IsDocumentVerified,
                RegionId = x.RegionId,
                PasswordHash = x.PasswordHash,
                Salt = x.Salt,
                LastSessionId = x.LastSessionId,
                Citizenship = x.Citizenship,
                JobArea = x.JobArea,
                ZipCode = x.ZipCode,
                Apartment = x.Apartment,
                BuildingNumber = x.BuildingNumber,
            }).FirstOrDefault();
            if (client != null)
            {
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                    client.State = Convert.ToInt32(parentState.NumericValue.Value);
            }
            return client;
        }     


        public static BllAsianCommissionPlan GetClientCommissionPlan(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientsCommissionPlan, clientId);
            var oldValue = MemcachedCache.Get<BllAsianCommissionPlan>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetClientCommissionPlanFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));

            return newValue;
        }

        private static BllAsianCommissionPlan GetClientCommissionPlanFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var client = db.AgentCommissions.FirstOrDefault(x => x.ClientId == clientId);
            if (client == null || string.IsNullOrEmpty(client.TurnoverPercent))
                return new BllAsianCommissionPlan();

            return JsonConvert.DeserializeObject<BllAsianCommissionPlan>(client.TurnoverPercent);
        }

        public static List<BllClientProductCommission> GetClientProductCommissionTree(int clientId, int productId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientProductCommissionTree, clientId, productId);
            var oldValue = MemcachedCache.Get<List<BllClientProductCommission>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetClientProductCommissionTreeFromDb(clientId, productId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));

            return newValue;
        }
        private static List<BllClientProductCommission> GetClientProductCommissionTreeFromDb(int clientId, int productId)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                return db.fn_ClientProductCommission(productId, clientId)
                                              .Select(x => new BllClientProductCommission
                                              {
                                                  Percent = x.Percent,
                                                  TurnoverPercent = x.TurnoverPercent,
                                                  ClientId = x.ClientId,
                                                  AgentId = x.AgentId
                                              }).ToList();
            }
        }
        public static List<BllGameProviderSetting> GetGameProviderSettings(int objectTypeId, int objectId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.GameProviderSettings, objectTypeId, objectId);
            var oldValue = MemcachedCache.Get<List<BllGameProviderSetting>>(key);
            if (oldValue != null)
                return oldValue;
            var newValue = GetGameProviderSettingsFromDb(objectTypeId, objectId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));

            return newValue;
        }

        private static List<BllGameProviderSetting> GetGameProviderSettingsFromDb(int objectTypeId, int objectId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.GameProviderSettings.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId)
                                          .Select(x => new BllGameProviderSetting
                                          {
                                              Id = x.Id,
                                              ObjectId = x.ObjectId,
                                              ObjectTypeId = x.ObjectTypeId,
                                              GameProviderId = x.GameProviderId,
                                              State = x.State,
                                              Order = x.Order,
                                              CreationTime = x.CreationTime,
                                              LastUpdateTime = x.LastUpdateTime
                                          }).ToList();
        }

        public static List<BllFavoriteProduct> GetClientFavoriteProducts(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientFavoriteProducts, clientId);
            var oldValue = MemcachedCache.Get<List<BllFavoriteProduct>>(key);
            if (oldValue != null)
                return oldValue ;
            var newValue = GetClientFavoriteProductsFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));

            return newValue;
        }

        public static void RemoveClientFavoriteProducts(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientFavoriteProducts, clientId);
            MemcachedCache.Remove(key);
        }

        private static List<BllFavoriteProduct> GetClientFavoriteProductsFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientFavoriteProducts.Where(x => x.ClientId == clientId)
                                            .Select(x => new BllFavoriteProduct
                                            {
                                                ProductId = x.ProductId
                                            }).ToList();
        }

        #endregion

        #region Session

        public static BllClientLastIp GetClientLastLoginIp(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.LastIps, clientId);
            var oldValue = MemcachedCache.Get<BllClientLastIp>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetClientLastLoginIpFromDb(clientId);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static void UpdateClientLastLoginIp(int clientId, string ip)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.LastIps, clientId);
            var newValue = new BllClientLastIp
            {
                ClientId = clientId,
                Ip = ip
            };
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
        }

        private static BllClientLastIp GetClientLastLoginIpFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientSessions.Where(x => x.ClientId == clientId && x.ProductId == Constants.PlatformProductId)
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new BllClientLastIp
                                    {
                                        ClientId = x.ClientId,
                                        Ip = x.Ip
                                    }).FirstOrDefault();
        }

        public static BllClientSession GetClientPlatformSessionById(long sessionId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientInactiveSessions, sessionId);
            var oldValue = MemcachedCache.Get<BllClientSession>(key);
            if (oldValue == null)
                oldValue = GetClientPlatformSessionByIdFromDb(sessionId);
            MemcachedCache.Store(StoreMode.Set, key, oldValue, TimeSpan.FromHours(3));
            return oldValue;
        }

        private static BllClientSession GetClientPlatformSessionByIdFromDb(long sessionId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientSessions.Where(x => x.Id == sessionId).Select(x => new BllClientSession
            {
                Id = x.Id,
                ClientId = x.ClientId,
                LanguageId = x.LanguageId,
                Ip = x.Ip,
                Country = x.Country,
                Token = x.Token,
                ProductId = x.ProductId,
                DeviceType = x.DeviceType,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                LastUpdateTime = x.LastUpdateTime ?? x.StartTime,
                State = x.State,
                CurrentPage = x.CurrentPage,
                ParentId = x.ParentId,
                CurrencyId = x.Client.CurrencyId,
                ExternalToken = x.ExternalToken
            }).OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public static BllClientSession GetClientPlatformSession(int clientId, long? sessionId, bool extend = true)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, clientId);
            var oldValue = MemcachedCache.Get<BllClientSession>(key);
            if (oldValue == null)
                oldValue = GetClientPlatformSessionFromDb(clientId);

            if (oldValue != null && extend && (sessionId == null || sessionId == oldValue.Id))
                oldValue.LastUpdateTime = DateTime.UtcNow;

            MemcachedCache.Store(StoreMode.Set, key, oldValue, TimeSpan.FromHours(3));
            return oldValue;
        }

        private static BllClientSession GetClientPlatformSessionFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientSessions.Where(x => x.ClientId == clientId && x.State == (int)SessionStates.Active && x.ProductId == Constants.PlatformProductId).Select(x => new BllClientSession
            {
                Id = x.Id,
                ClientId = x.ClientId,
                LanguageId = x.LanguageId,
                Ip = x.Ip,
                Country = x.Country,
                Token = x.Token,
                ProductId = x.ProductId,
                DeviceType = x.DeviceType,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                LastUpdateTime = x.LastUpdateTime ?? x.StartTime,
                State = x.State,
                CurrentPage = x.CurrentPage,
                ParentId = x.ParentId,
                CurrencyId = x.Client.CurrencyId,
                ExternalToken = x.ExternalToken,
                LogoutType = x.LogoutType
            }).OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public static void RemoveClientPlatformSession(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, clientId);
            MemcachedCache.Remove(key);
        }

        public static BllClientSession GetClientSessionByToken(string token, int? productId, bool extend = true)
        {
            var keyp = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, token, productId == null ? "0" : productId.Value.ToString());
            var key0 = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, token, "0");

            var oldValue0 = MemcachedCache.Get<BllClientSession>(key0);
            BllClientSession oldValuep = productId == null ? null : MemcachedCache.Get<BllClientSession>(keyp);

            var newValue = oldValue0 ?? (oldValuep ?? GetClientProductSessionFromDb(token, productId));
            if (newValue == null)
                return null;
            var lat = newValue.LastUpdateTime;
            if (oldValue0 != null && oldValue0.LastUpdateTime > lat)
                lat = oldValue0.LastUpdateTime;
            if (oldValuep != null && oldValuep.LastUpdateTime > lat)
                lat = oldValuep.LastUpdateTime;
            newValue.LastUpdateTime = lat;

            if (newValue != null && extend)
                newValue.LastUpdateTime = DateTime.UtcNow;

            MemcachedCache.Store(StoreMode.Set, key0, newValue, TimeSpan.FromHours(3));
            if (productId != null)
                MemcachedCache.Store(StoreMode.Set, keyp, newValue, TimeSpan.FromHours(3));

            return newValue;
        }

        private static BllClientSession GetClientProductSessionFromDb(string token, int? productId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var query = db.ClientSessions.Where(x => x.Token == token);
            if (productId != null)
                query = query.Where(x => x.ProductId == productId.Value);

            return query.Select(x => new BllClientSession
            {
                Id = x.Id,
                ClientId = x.ClientId,
                LanguageId = x.LanguageId,
                Ip = x.Ip,
                Country = x.Country,
                Token = x.Token,
                ProductId = x.ProductId,
                DeviceType = x.DeviceType,
                StartTime = x.StartTime,
                LastUpdateTime = x.LastUpdateTime ?? x.StartTime,
                State = x.State,
                CurrentPage = x.CurrentPage,
                ParentId = x.ParentId,
                ExternalToken = x.ExternalToken,
                LogoutType = x.LogoutType,
                CurrencyId = x.Client.CurrencyId
            }).FirstOrDefault();
        }

        public static void RemoveClientProductSession(string token, int? productId)
        {
            var key = string.Format("{0}_{1}_0", Constants.CacheItems.ClientSessions, token);
            MemcachedCache.Remove(key);
            if (productId != null)
            {
                key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, token, productId.Value);
                MemcachedCache.Remove(key);
            }
        }

        #endregion

        #region Integration

        public static string GetLiveGamesLobbyItems(int providerId, string operatorId, string currencyId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.LiveGamesLobbyItems, providerId, currencyId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;

            var newValue = "";//Integration.Products.Helpers.EzugiHelpers.GetMessagesFromProviderServer(operatorId, currencyId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromMinutes(1));
            return newValue;
        }

        #endregion

        #region Bonus

        public static BllClientBonus GetActiveWageringBonus(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientId);
            var oldValue = MemcachedCache.Get<BllClientBonus>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetActiveWageringBonusFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static BllClientBonus GetActiveWageringBonusFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var bonus = db.ClientBonus.Where(x => x.ClientId == clientId && x.Status == (int)BonusStatuses.Active &&
                        (x.Bonus.BonusType == (int)BonusTypes.CampaignWagerCasino || x.Bonus.BonusType == (int)BonusTypes.CampaignWagerSport) && x.TurnoverAmountLeft > 0).
                         Select(x => new BllClientBonus
                         {
                             Id = x.Id,
                             BonusId = x.BonusId,
                             ClientId = x.ClientId,
                             Status = x.Status,
                             BonusPrize = x.BonusPrize,
                             CreationTime = x.CreationTime,
                             TurnoverAmountLeft = x.TurnoverAmountLeft,
                             FinalAmount = x.FinalAmount,
                             CalculationTime = x.CalculationTime,
                             ReuseNumber = x.ReuseNumber
                         }).FirstOrDefault();
            if (bonus == null)
                bonus = new BllClientBonus { Id = 0 };
            return bonus;
        }

        public static void RemoveClientActiveBonus(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.ActiveBonusId, clientId);
            MemcachedCache.Remove(key);
        }

        public static BllClientBonus GetClientBonusById(int clientId, int bonusId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientId, bonusId);
            var oldValue = MemcachedCache.Get<BllClientBonus>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientBonusFromDb(clientId, bonusId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static BllClientBonus GetClientBonusFromDb(int clientId, int bonusId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var bonus = db.ClientBonus.Where(x => x.ClientId == clientId && x.BonusId == bonusId).
                         Select(x => new BllClientBonus
                         {
                             Id = x.Id,
                             BonusId = x.BonusId,
                             ClientId = x.ClientId,
                             Status = x.Status,
                             BonusPrize = x.BonusPrize,
                             CreationTime = x.CreationTime,
                             TurnoverAmountLeft = x.TurnoverAmountLeft,
                             FinalAmount = x.FinalAmount,
                             CalculationTime = x.CalculationTime
                         }).FirstOrDefault();
            if (bonus == null)
                bonus = new BllClientBonus { Id = 0 };
            return bonus;
        }

        public static void RemoveClientBonus(int clientId, int bonusId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientBonus, clientId, bonusId);
            MemcachedCache.Remove(key);
        }

        public static void RemoveComplimentaryPointRate(int partnerId, int productId, string currencyId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.ComplimantaryPointRates, partnerId, productId, currencyId);
            MemcachedCache.Remove(key);
            using var db = new IqSoftCorePlatformEntities();
            var product = db.Products.Where(x => x.Id == productId).FirstOrDefault();
            var prodTreeIds = product.Traverse(x => db.Products.Where(y => y.ParentId == x.Id)).Select(x => x.Id).ToList();
            prodTreeIds.ForEach(x =>
            {
                key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.ComplimantaryPointRates, partnerId, x, currencyId);
                MemcachedCache.Remove(key);
            });
        }

        public static BllComplimentaryPointRate GetComplimentaryPointRate(int partnerId, int productId, string currencyId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.ComplimantaryPointRates, partnerId, productId, currencyId);
            var oldValue = MemcachedCache.Get<BllComplimentaryPointRate>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetComplimentaryPointRateFromDb(partnerId, productId, currencyId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static BllComplimentaryPointRate GetComplimentaryPointRateFromDb(int partnerId, int productId, string currencyId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var complimentaryPoint = db.ComplimentaryPointRates.Where(x => x.PartnerId == partnerId && x.ProductId == productId &&
                                                                  x.CurrencyId == currencyId).
                         Select(x => new BllComplimentaryPointRate
                         {
                             Id = x.Id,
                             PartnerId = x.PartnerId,
                             ProductId = x.ProductId,
                             CurrencyId = x.CurrencyId,
                             Rate = x.Rate,
                             CreationDate = x.CreationDate,
                             LastUpdateDate = x.LastUpdateDate
                         }).FirstOrDefault();
            if (complimentaryPoint == null)
            {
                var productCategories = db.Products.Where(x => x.Level < 4).ToList();
                var product = db.Products.FirstOrDefault(x => x.Id == productId);
                var prodTreeIds = product.Traverse(x => productCategories.Where(y => y.Id == x.ParentId)).Select(x => x.Id).ToList();
                complimentaryPoint = db.ComplimentaryPointRates.Where(x => x.PartnerId == partnerId && prodTreeIds.Contains(x.ProductId) &&
                                                                           x.CurrencyId == currencyId)
                                                               .OrderByDescending(x => x.Product.Level)
                                                               .Select(x => new BllComplimentaryPointRate
                                                               {
                                                                   Id = x.Id,
                                                                   PartnerId = x.PartnerId,
                                                                   ProductId = productId,
                                                                   CurrencyId = x.CurrencyId,
                                                                   Rate = x.Rate,
                                                                   CreationDate = x.CreationDate,
                                                                   LastUpdateDate = x.LastUpdateDate
                                                               }).FirstOrDefault();

            }
            if (complimentaryPoint == null)
                complimentaryPoint = new BllComplimentaryPointRate();
            return complimentaryPoint;
        }


        public static List<ClientBonusProduct> GetBonusProducts(int bonusId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BonusProducts, bonusId);
            var oldValue = MemcachedCache.Get<List<ClientBonusProduct>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetBonusProductsFromDb(bonusId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        public static void RemoveBonusProducts(int bonusId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BonusProducts, bonusId);
            MemcachedCache.Remove(key);
        }

        private static List<ClientBonusProduct> GetBonusProductsFromDb(int bonusId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.BonusProducts.Where(x => x.BonusId == bonusId)
                                   .Select(x => new ClientBonusProduct
                                   {
                                       Id = x.Id,
                                       BonusId = x.BonusId,
                                       ProductId = x.ProductId,
                                       Percent = x.CashBackPercent
                                   }).ToList();
        }

        public static List<ClientBonusInfo> GetClientNotAwardedCampaigns(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, clientId);
            var oldValue = MemcachedCache.Get<List<ClientBonusInfo>>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetClientNotAwardedCampaignsFromDb(clientId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static List<ClientBonusInfo> GetClientNotAwardedCampaignsFromDb(int clientId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientBonus.Where(x => x.Status == (int)BonusStatuses.NotAwarded && x.ClientId == clientId).Select(x => new ClientBonusInfo
            { BonusId = x.BonusId, ReuseNumber = x.ReuseNumber ?? 1 }).ToList();
        }

        public static void RemoveClientNotAwardedCampaigns(int clientId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, clientId);
            MemcachedCache.Remove(key);
        }

        public static BonusInfo GetBonusById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, id);
            var oldValue = MemcachedCache.Get<BonusInfo>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetBonusInfoFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static BonusInfo GetBonusInfoFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            return db.Bonus.Where(x => x.Id == id)
                           .Select(x => new BonusInfo
                           {
                               Id = x.Id,
                               Name = x.Name,
                               PartnerId = x.PartnerId,
                               AccountTypeId = x.AccountTypeId,
                               Status = x.Status,
                               StartTime = x.StartTime,
                               FinishTime = x.FinishTime,
                               Period = x.Period,
                               BonusType = x.BonusType,
                               TurnoverCount = x.TurnoverCount,
                               Info = x.Info,
                               MinAmount = x.MinAmount,
                               MaxAmount = x.MaxAmount,
                               Priority = x.Priority,
                               ValidForAwarding = x.ValidForAwarding,
                               ValidForSpending = x.ValidForSpending,
                               Sequence = x.Sequence,
                               Condition = x.Condition,
                               ReusingMaxCount = x.ReusingMaxCount ?? 1,
                               PaymentSystems = x.BonusPaymentSystemSettings.Select(y => y.PaymentSystemId).ToList(),
                               FreezeBonusBalance = x.FreezeBonusBalance,
                               TriggerGroups = x.TriggerGroups.Select(y => new TriggerGroupInfo
                               {
                                   Id = y.Id,
                                   Name = y.Name,
                                   Type = y.Type,
                                   Priority = y.Priority,
                                   TriggerGroupSettings = y.TriggerGroupSettings.Select(z => new TriggerGroupSettingInfo
                                   {
                                       Id = z.Id,
                                       SettingId = z.SettingId,
                                       Order = z.Order,
                                   }).OrderBy(z => z.Order).ToList()
                               }).OrderBy(y => y.Priority).ToList()
                           }).FirstOrDefault();
        }

        public static void RemoveBonus(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.BonusInfo, id);
            MemcachedCache.Remove(key);
        }

        public static TriggerSettingInfo GetTriggerSettingById(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.TriggerSettings, id);
            var oldValue = MemcachedCache.Get<TriggerSettingInfo>(key);
            if (oldValue != null)
                return oldValue ;
            var newValue = GetTriggerSettingFromDb(id);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(1));
            return newValue;
        }

        private static TriggerSettingInfo GetTriggerSettingFromDb(int id)
        {
            using var db = new IqSoftCorePlatformEntities();
            var currentTime = DateTime.UtcNow;
            return db.TriggerSettings.Where(x => x.Id == id).Select(x => new TriggerSettingInfo
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                TranslationId = x.TranslationId,
                Type = x.Type,
                StartTime = x.StartTime,
                FinishTime = x.FinishTime,
                Percent = x.Percent,
                BonusSettingCodes = x.BonusSettingCodes,
                PartnerId = x.PartnerId,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
                MinAmount = x.MinAmount,
                MaxAmount = x.MaxAmount,
                MinBetCount = x.MinBetCount,
                Condition = x.Condition,
                SegmentId = x.SegmentId,
                DayOfWeek = x.DayOfWeek,
                UpToAmount = x.UpToAmount,
                PaymentSystemIds = x.BonusPaymentSystemSettings.Select(y => y.PaymentSystemId).ToList(),
                ProductSettings = x.TriggerProductSettings.Select(y => new TriggerProductInfo
                {
                    Id = y.Id,
                    ProductId = y.ProductId,
                    Percent = y.Percent
                }).ToList()
            }).FirstOrDefault();
        }

        public static void RemoveTriggerSetting(int id)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.TriggerSettings, id);
            MemcachedCache.Remove(key);
        }

        #endregion

        #region Banners

        public static List<BllBanner> GetBanners(int partnerId, int type, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Banners, partnerId, type, languageId);
            var oldValue = MemcachedCache.Get<List<BllBanner>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetBannersFromDb(partnerId, type, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemoveBanners(int partnerId, int type)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Banners, partnerId, type);
            var languages = GetAvailableLanguages();
            foreach (var l in languages)
            {
                MemcachedCache.Remove(key + "_" + l.Id);
            }
        }

        public static void RemoveBanners(int partnerId, int type, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Banners, partnerId, type, languageId);

            MemcachedCache.Remove(key);
        }

        private static List<BllBanner> GetBannersFromDb(int partnerId, int type, string languageId)
        {
            var currentDate = DateTime.UtcNow;
            using var db = new IqSoftCorePlatformEntities();
            var resp = db.fn_Banner(languageId).Where(x => x.PartnerId == partnerId && x.Type == type && x.IsEnabled && x.StartDate <= currentDate && x.EndDate > currentDate).
                    Select(x => new BllBanner
                    {
                        Id = x.Id,
                        PartnerId = x.PartnerId,
                        Type = x.Type,
                        NickName = x.NickName,
                        Head = x.Head,
                        Body = x.Body,
                        Link = x.Link,
                        Order = x.Order,
                        Image = x.Image,
                        IsEnabled = x.IsEnabled,
                        ShowDescription = x.ShowDescription,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Visibility = string.IsNullOrEmpty(x.Visibility) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.Visibility),
                        ButtonType = x.ButtonType
                    }).ToList();
            var ids = resp.Select(x => x.Id).ToList();
            var segments = db.BannerSegmentSettings.Where(x => ids.Contains(x.BannerId)).ToList();
            foreach (var banner in resp)
            {
                var bSegments = segments.Where(x => x.BannerId == banner.Id).ToList();
                banner.Segments = new BllSetting
                { Type = bSegments.Any() ? bSegments[0].Type : (int)BonusSettingConditionTypes.InSet, Ids = bSegments.Select(x => x.SegmentId).ToList() };
            }
            return resp;
        }
        public static List<BllPromotion> GetPromotions(int partnerId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Promotions, partnerId, languageId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null) return oldValue as List<BllPromotion>;
            var newValue = GetBllPromotionsFromDb(partnerId, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }
        public static void RemovePromotions(int partnerId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Promotions, partnerId);
            var languages = GetAvailableLanguages();
            foreach (var l in languages)
            {
                MemcachedCache.Remove(key + "_" + l.Id);
            }
        }

        private static List<BllPromotion> GetBllPromotionsFromDb(int partnerId, string languageId)
        {
            var currentDate = DateTime.UtcNow;
            using (var db = new IqSoftCorePlatformEntities())
            {
                var resp = db.fn_Promotion(languageId).Where(x => x.PartnerId == partnerId && x.State == (int)BaseStates.Active &&
                                                                  x.StartDate <= currentDate && x.FinishDate > currentDate)
                                                      .Select(x => new BllPromotion
                                                      {
                                                          Id = x.Id,
                                                          PartnerId = x.PartnerId,
                                                          Type = x.Type.ToString(),
                                                          NickName = x.NickName,
                                                          Title = x.Title,
                                                          Description = x.Description,
                                                          ImageName = x.ImageName,
                                                          Order = x.Order,
                                                          Visibility = string.IsNullOrEmpty(x.Visibility) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.Visibility),
                                                      }).ToList();
                var ids = resp.Select(x => x.Id).ToList();
                var segments = db.PromotionSegmentSettings.Where(x => ids.Contains(x.PromotionId)).ToList();
                var languages = db.PromotionLanguageSettings.Where(x => ids.Contains(x.PromotionId)).ToList();
                foreach (var promotion in resp)
                {
                    var pSegments = segments.Where(x => x.PromotionId == promotion.Id).ToList();
                    promotion.Segments = new BllSetting { Type = pSegments.Any() ? pSegments[0].Type : 0, Ids = pSegments.Select(x => x.SegmentId).ToList() };

                    var pLanguages = languages.Where(x => x.PromotionId == promotion.Id).ToList();
                    promotion.Languages = new BllSetting { Type = pLanguages.Any() ? pLanguages[0].Type : 0, Names = pLanguages.Select(x => x.LanguageId).ToList() };
                }
                return resp;
            }
        }

        #endregion

        #region Settings

        public static long GetLastProcessedBetDocumentId()
        {
            var key = Constants.PartnerKeys.LastProcessedBetDocumentId;
            var oldValue = MemcachedCache.Get<long?>(key);
            if (oldValue != null) return oldValue.Value;
            var newValue = GetLastProcessedBetDocumentIdFromDb();
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static long GetLastProcessedBetDocumentIdFromDb()
        {
            using var db = new IqSoftCorePlatformEntities();
            var resp = db.PartnerKeys.Where(x => x.Name == Constants.PartnerKeys.LastProcessedBetDocumentId).Select(x => x.NumericValue).FirstOrDefault();
            if (resp == null)
                return -1;

            return resp.Value;
        }

        public static void RemoveLastProcessedBetDocumentId()
        {
            var key = Constants.PartnerKeys.LastProcessedBetDocumentId;
            MemcachedCache.Remove(key);
        }

        #endregion

        #region CMS

        public static BllRegion GetRegionById(int id, string language)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Region, id, language);
            var oldValue = MemcachedCache.Get<BllRegion>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetRegionFromDb(id, language);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromHours(6));
            return newValue;
        }

        private static BllRegion GetRegionFromDb(int id, string language)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.fn_Region(language).Where(x => x.Id == id)
                                         .Select(x => new BllRegion
                                         {
                                             Id = x.Id,
                                             ParentId = x.ParentId,
                                             TypeId = x.TypeId,
                                             NickName = x.NickName,
                                             Name = x.Name,
                                             IsoCode = x.IsoCode,
                                             IsoCode3 = x.IsoCode3,
                                             Path = x.Path,
                                             State = x.State,
                                             LanguageId = x.LanguageId
                                         }).FirstOrDefault();
        }

        #endregion

        public static void RemoveClientSetting(int clientId, string name)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, name);
            MemcachedCache.Remove(key);
        }

        public static BllClientSetting GetClientSettingByName(int clientId, string name)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, name);
            var oldValue = MemcachedCache.Get<BllClientSetting>(key);
            if (oldValue != null && !string.IsNullOrEmpty(oldValue.Name))
                return oldValue;
            var newValue = GetClientSettingFromDb(clientId, name);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static BllClientSetting GetClientSettingFromDb(int clientId, string name)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.ClientSettings.Where(x => x.ClientId == clientId && x.Name == name).Select(x => new BllClientSetting
            {
                Id = x.Id,
                ClientId = x.ClientId,
                Name = x.Name,
                NumericValue = x.NumericValue,
                StringValue = x.StringValue,
                DateValue = x.DateValue,
                UserId = x.UserId
            }).OrderByDescending(x => x.UserId.HasValue).FirstOrDefault() ?? new BllClientSetting();
        }

        public static List<BllPartnerCountrySetting> GetPartnerCountrySettings(int partnerId, int settingType)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerCountrySetting, partnerId, settingType);
            var oldValue = MemcachedCache.Get<List<BllPartnerCountrySetting>>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerCountrySettingsFromDb(partnerId, settingType);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static List<BllPartnerCountrySetting> GetPartnerCountrySettingsFromDb(int partnerId, int settingType)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.PartnerCountrySettings.Where(x => x.PartnerId == partnerId && x.Type == settingType).Select(x => new BllPartnerCountrySetting
            {
                Id = x.Id,
                PartnerId = x.PartnerId,
                RegionId = x.RegionId,
                Type = x.Type
            }).ToList();
        }

        #region User

        public static BllUser GetUserById(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.User, userId);
            var oldValue = MemcachedCache.Get<BllUser>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetUserFromDb(userId);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        public static BllUser GetUserByUserName(int partnerId, string userName)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.User, partnerId, userName);
            var oldValue = MemcachedCache.Get<BllUser>(key);
            if (oldValue != null) return oldValue ;
            var newValue = GetUserFromDb(partnerId, userName);
            if (newValue != null)
                MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1));
            return newValue;
        }

        private static BllUser GetUserFromDb(int partnerId, string userName)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Users.Where(x => x.UserName == userName && x.PartnerId == partnerId).Select(x => new BllUser
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                NickName = x.NickName,
                MobileNumber = x.MobileNumber,
                IsTwoFactorEnabled = x.IsTwoFactorEnabled,
                ParentId = x.ParentId,
                PartnerId = x.PartnerId,
                State = x.State,
                Type = x.Type,
                Path = x.Path,
                CreationTime = x.CreationTime,
                Gender = x.Gender,
                Level = x.Level,
                SecurityCode = x.SecurityCode,
            }).FirstOrDefault();
        }

        private static BllUser GetUserFromDb(int userId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Users.Where(x => x.Id == userId).Select(x => new BllUser
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                CurrencyId = x.CurrencyId,
                UserName = x.UserName,
                NickName = x.NickName,
                MobileNumber = x.MobileNumber,
                IsTwoFactorEnabled = x.IsTwoFactorEnabled,
                ParentId = x.ParentId,
                PartnerId = x.PartnerId,
                State = x.State,
                Type = x.Type,
                Path = x.Path,
                CreationTime = x.CreationTime,
                Level = x.Level,
                Gender = x.Gender,
                SecurityCode = x.SecurityCode,
            }).FirstOrDefault();
        }

        public static void RemoveUserFromCache(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.User, userId);
            MemcachedCache.Remove(key);
        }

        public static void RemoveUserFailedLoginCountFromCache(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserFailedLoginCount, userId);
            MemcachedCache.Remove(key);
        }
        public static void RemoveUserSecurityCodeCountFromCache(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserFailedSecurityCodeCount, userId);
            MemcachedCache.Remove(key);
        }

        public static int UpdateUserFailedLoginCount(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserFailedLoginCount, userId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
            {
                MemcachedCache.Increment(key, 1, 1);
                return Convert.ToInt32(oldValue.ToString()) + 1;
            }
            MemcachedCache.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(1));
            return 1;
        }

        public static int UpdateUserFailedSecurityCodeCount(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserFailedSecurityCodeCount, userId);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
            {
                MemcachedCache.Increment(key, 1, 1);
                return Convert.ToInt32(oldValue.ToString()) + 1;
            }
            MemcachedCache.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(1));
            return 1;
        }

        public static BllUserSetting GetUserSetting(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserSettings, userId);
            var cacheValue = MemcachedCache.Get<BllUserSetting>(key);
            if (cacheValue != null)
                return cacheValue;
            cacheValue = GetUserSettingFromDb(userId);
            if (cacheValue != null)
                MemcachedCache.Store(StoreMode.Set, key, cacheValue, TimeSpan.FromDays(1));
            return cacheValue;
        }

        public static void RemoveUserSetting(int userId)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.UserSettings, userId);
            MemcachedCache.Remove(key);
        }

        private static BllUserSetting GetUserSettingFromDb(int userId)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.UserSettings.Where(x => x.UserId == userId).Select(x => new BllUserSetting
            {
                Id = x.Id,
                UserId = x.UserId,
                ParentState = x.ParentState,
                AllowAutoPT = x.AllowAutoPT,
                AllowOutright = x.AllowOutright,
                AllowDoubleCommission = x.AllowDoubleCommission,
                CalculationPeriod = x.CalculationPeriod,
                IsCalculationPeriodBlocked = x.CalculationPeriod == "[-1]",
                AgentMaxCredit = x.AgentMaxCredit,
                OddsType = x.OddsType,
                LevelLimits = x.LevelLimits,
                CountLimits = x.CountLimits,
                CreationTime = x.CreationTime,
                LastUpdateTime = x.LastUpdateTime,
            }).FirstOrDefault();
        }
        #endregion

        public static string GetPartnerTicker(int partnerId, string languageId)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.Ticker, partnerId, languageId);
            var oldValue = MemcachedCache.Get<string>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetPartnerTickerFromDb(partnerId, languageId);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        public static void RemovePartnerTickerFromCache(int partnerId, string languageId)
        {
            MemcachedCache.Remove(string.Format("{0}_{1}_{2}", Constants.CacheItems.Ticker, partnerId, languageId));
        }

        private static string GetPartnerTickerFromDb(int partnerId, string languageId)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                return db.fn_Announcement(languageId).Where(x => x.PartnerId == partnerId && x.Type == (int)AnnouncementTypes.Ticker &&
                                                                 x.State == (int)BaseStates.Active)
                                                     .OrderByDescending(x => x.Id)
                                                     .Select(x => x.Message)
                                                     .FirstOrDefault();
            }
        }

        public static DAL.Action GetAction(string actionName)
        {
            var key = string.Format("{0}_{1}", Constants.CacheItems.Action, actionName);
            var oldValue = MemcachedCache.Get<DAL.Action>(key);
            if (oldValue != null) return oldValue;
            var newValue = GetActionFromDb(actionName);
            MemcachedCache.Store(StoreMode.Set, key, newValue, TimeSpan.FromDays(1d));
            return newValue;
        }

        private static DAL.Action GetActionFromDb(string actionName)
        {
            using var db = new IqSoftCorePlatformEntities();
            return db.Actions.Where(x => x.NickName == actionName).FirstOrDefault();
        }

        public static int GetCRMApiRequestsCount(int UserId, string methodName)
        {
            var key = string.Format("{0}_{1}_{2}", Constants.CacheItems.CRMApiRequestsCount, UserId, methodName);
            var oldValue = MemcachedCache.Get(key);
            if (oldValue != null)
            {
                MemcachedCache.Increment(key, 1, 1);
                return Convert.ToInt32(oldValue.ToString()) + 1;
            }

            MemcachedCache.Store(StoreMode.Set, key, "1", TimeSpan.FromHours(1));
            return 1;
        }
    }
}