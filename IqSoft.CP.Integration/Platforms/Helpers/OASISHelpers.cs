using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Platforms.Models.OASIS;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class OASISHelpers
    {
        private enum Providers
        {
            Bet3000 = 1,
            Insic = 2
        }
        static class RequestTypes
        {
            public const string ShortTermLockDirect = "kurzzeitsperren";
            public const string ShortTermLock = "block_24h";
            public const string LongTermLock = "block_long";
            public const string PlayerStatus = "check";
        }
        private enum RequestResults
        {
            ApiError = 0,
            //Success = 15, ??
            NotBlockedPlayer = 19,
            PlayerBlocked1 = 18,
            PlayerBlocked2 = 23,
            PlayerBlocked3 = 24,
            SuccessfullyLocked = 7,
        }

        private static string SendRequestDirect(int partnerId, string input, string method)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.OASISApiUrl).StringValue;
            var identity = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.OASISIdentity).StringValue;
            var password = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.OASISPassword).StringValue;

            return CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.TextPlain,
                Accept = Constants.HttpContentTypes.TextPlain,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "Kennung", identity }, { "Passwort", password } },
                Url = $"{url}/{method}",
                PostData = input
            }, out _);
        }

        private static string SendRequest(int partnerId, object input)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.OASISApiUrl).StringValue;

            return CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(input)
            }, out _);
        }

        public static void ShortTermLockDirect(BllClient client, DateTime exclutionDate)
        {
            var region = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId);
            var input = new TemporaryBanInput
            {
                TimestampData = new Timestamp
                {
                    Date = exclutionDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    FormatPattern = "dd.MM.yyyy HH:mm:ss"
                },
                PlayerData = new Player
                {
                    FirstName = client.FirstName,
                    Surname = client.LastName,
                    BirthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                    BirthName = client.SecondName,
                    BirthPlace = region.Name,
                    Address = new AddressData
                    {
                        ZipCode = client.ZipCode,
                        City = client.City,
                        Street = client.Address,
                        HousNumber = client.BuildingNumber,
                        SupplementalAddress = client.Address,
                        CountryCode = region.IsoCode3
                    }
                }
            };
            var xml = SerializeAndDeserialize.SerializeToXml(input, "KURZZEITSPERRDATEN");

            var result = SendRequestDirect(client.PartnerId, xml, RequestTypes.ShortTermLock);
        }

        public static void CheckClientStatus(BllClient client, int? betshopId, string languageId, SessionIdentity sessionIdentity, ILog log)
        {
            int requestResult = 0;
            var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.OASISProviderId);
            if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int providerId))
            {
                switch (providerId)
                {
                    case (int)Providers.Bet3000:
                        requestResult = CheckBet3000ClientStatus(client, betshopId);
                        break;
                    case (int)Providers.Insic:
                        requestResult = CheckInsicClientStatus(client, log);
                        break;
                }
            }
            log.Debug($"OASIS Result: {(RequestResults)requestResult}");

            using (var clientBl = new ClientBll(sessionIdentity, log))
            {
                clientBl.SaveClientSetting(client.Id, Constants.ClientSettings.ExternalStatus, ((RequestResults)requestResult).ToString(), requestResult, DateTime.UtcNow);
            }
            if (requestResult == (int)RequestResults.PlayerBlocked1)
                throw BaseBll.CreateException(languageId, Constants.Errors.ClientBlocked);
        }

        private static int CheckInsicClientStatus(BllClient client, ILog log)
        {
            var user = InsicHelpers.GetUser(client.PartnerId, client.Email,(int)InsicHelpers.Services.Insic, log);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISApiUrl).StringValue;
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(client.PartnerId, Constants.PartnerKeys.InsicPartnerKey, (int)InsicHelpers.Services.Insic);
            var serviceId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISAuthToken).StringValue;

            var res = CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "token", InsicHelpers.GetAdminToken(client.PartnerId, InsicHelpers.Services.Insic, log) } },
                Url = $"{url}/{partnerKey}/user/{user.Id}/engine/{serviceId}/check"
            }, out _); ;

            var result = JsonConvert.DeserializeObject<InsicOutput>(res);
            if (!result.Success)
                throw new Exception(res);
            if (result.IsUserBanned)
                return (int)RequestResults.PlayerBlocked1;
            return (int)RequestResults.NotBlockedPlayer;
        }


        private static int CheckBet3000ClientStatus(BllClient client, int? betshopId)
        {
            var authToken = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISAuthToken).StringValue;
            var input = new PlayerInput
            {
                AuthToken = authToken,
                BranchId = betshopId ?? 0,
                RequestType = RequestTypes.PlayerStatus,
                FirstName = client.FirstName,
                LastName = client.LastName,
                BirthDate = client.BirthDate.ToString("yyyy-MM-dd")
            };
            var resp = SendRequest(client.PartnerId, input);
            var result = JsonConvert.DeserializeObject<BaseOutput>(resp);
            return result.ResultKey;
        }

        public static void ShortTermLock(BllClient client, int? betshopId, ILog log)
        {
            var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.OASISProviderId);
            if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int providerId))
            {
                switch (providerId)
                {
                    case (int)Providers.Bet3000:
                        Bet3000ShortTermLock(client, betshopId, log);
                        break;
                    case (int)Providers.Insic:
                        InsicLockUser(client, DateTime.UtcNow.AddHours(24), "temporary", log);
                        break;
                }
            }
        }

        public static void LongTermLock(BllClient client, int? betshopId, DateTime exclusionDate, string reason, ILog log)
        {
            var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.OASISProviderId);
            if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int providerId))
            {
                switch (providerId)
                {
                    case (int)Providers.Bet3000:
                        Bet3000LongTermLock(client, betshopId, exclusionDate, reason, log);
                        break;
                    case (int)Providers.Insic:
                        InsicLockUser(client, exclusionDate, reason, log);
                        break;
                }
            }
        }

        public static void Bet3000ShortTermLock(BllClient client, int? betshopId, ILog log)
        {
            try
            {
                var authToken = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISAuthToken).StringValue;
                var region = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId);

                var input = new ShortTermLockInput
                {
                    AuthToken = authToken,
                    BranchId = betshopId ?? 0,
                    RequestType = RequestTypes.ShortTermLock,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    BirthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                    BirthName = string.IsNullOrEmpty(client.SecondName) ? client.LastName : client.SecondName,
                    BirthPlace = client.Apartment ?? "-",
                    ZipCode = client.ZipCode.Trim(),
                    City = client.City ?? "-",
                    Street = client.Address ?? "-",
                    HouseNumber = client.BuildingNumber ?? "-",
                    CountryCode = region.IsoCode
                };
                log.Debug( "__ input: " + JsonConvert.SerializeObject(input));

                var resp = SendRequest(client.PartnerId, input);
                log.Debug("resp:" + resp );
                var result = JsonConvert.DeserializeObject<BaseOutput>(resp);
                if (result.ResultKey != (int)RequestResults.SuccessfullyLocked)
                    throw new Exception(resp);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void Bet3000LongTermLock(BllClient client, int? betshopId, DateTime exclusionDate, string reason, ILog log)
        {
            try
            {
                var authToken = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISAuthToken).StringValue;
                var region = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId);
                var lockDuration = (int)(DateTime.UtcNow - exclusionDate).TotalDays/30;
                var input = new LongTermLockInput
                {
                    AuthToken = authToken,
                    BranchId = betshopId ?? 0,
                    RequestType = RequestTypes.LongTermLock,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    BirthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                    BirthName = client.SecondName ?? "-",
                    BirthPlace = client.Apartment ?? "-",
                    ZipCode = client.ZipCode.Trim(),
                    City = client.City ?? string.Empty,
                    Street = client.Address ?? string.Empty,
                    HouseNumber = client.BuildingNumber ?? string.Empty,
                    CountryCode = region.IsoCode,
                    LockDuration = lockDuration > 12 ? "inf" : $"{lockDuration}M",
                    LockReason = reason,
                    Email = client.Email,
                    Username = client.UserName,
                    RegDate = client.CreationTime.ToString("yyyy-MM-dd hh:mm:ss")

                };
                var resp = SendRequest(client.PartnerId, input);
                var result = JsonConvert.DeserializeObject<BaseOutput>(resp);
                if (result.ResultKey != (int)RequestResults.SuccessfullyLocked)
                    throw new Exception(resp);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public static void InsicLockUser(BllClient client, DateTime exclusionDate, string reason, ILog log)
        {
            var user = InsicHelpers.GetUser(client.PartnerId, client.Email, (int)InsicHelpers.Services.Insic, log);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OASISApiUrl).StringValue;
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(client.PartnerId, Constants.PartnerKeys.InsicPartnerKey, (int)InsicHelpers.Services.Insic);

            CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "token", InsicHelpers.GetAdminToken(client.PartnerId, InsicHelpers.Services.Insic, log) } },
                Url = $"{url}/{partnerKey}/user/{user.Id}/block",
                PostData = JsonConvert.SerializeObject(new { reason = "temporary", endDateOfBan = exclusionDate .ToString("dd.MM.yyyy"), isEndDateSpecified  = true})
            }, out _); 
        }
    }
}