using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.Integration.Platforms.Models.Insic;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class InsicHelpers
    {
        public enum Services
        {
            Insic = 1,
            Lugas = 2,
            Shop = 3,
            SOW = 4
        }

        private static readonly Dictionary<string, int> VerificationServiceNames = new Dictionary<string, int>
        {
            {"schufa.identity", 1 },
            {"schufa.bankAccount", 2 } ,
            {"ebics.1cent", 3 },
            {"face.recognition", 4 },
            {"schufa.credit", 5 },
            {"finapi-service", 6 }
            //"document.reader",
            //"yes-identity"
        };

        public static string GetAdminToken(int partnerId, Services service, ILog log)
        {
            var token = CacheManager.GetPartnerExternalToken(partnerId, (int)service);
            if(string.IsNullOrEmpty( token))
            {
                token = LoginAdminUser(partnerId, service, log);
                CacheManager.UpdatePartnerExternalToken(partnerId, (int)service, token);
            }
            return token;
        }

        private static string LoginAdminUser(int partnerId, Services service, ILog log)
        {
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPartnerKey, (int)service);
            var url = string.Format(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.InsicApiUrl).StringValue, $"api/{partnerKey}");
            var username = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicUsername, (int)service);
            var pass = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPassword, (int)service);
            var input = new LoginInput
            {
                Email = username,
                Password = pass
            };
            var resp = CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = $"{url}/auth/login",
                PostData = JsonConvert.SerializeObject(input)
            }, out _);
            log.Debug("Login Output: "  + resp);
            var output = JsonConvert.DeserializeObject<LoginOutput>(resp);
            if (!string.IsNullOrEmpty(output.Token))
                return output.Token;
            throw new Exception(resp);
        }

        public static void CreateClientOnAllPlatforms(Client client, bool isBetshopUser, DAL.Models.SessionIdentity sessionIdentity, ILog log)
        {
            using (var clientBl = new ClientBll(sessionIdentity, log))
            {
                foreach (var service in VerificationServiceNames)
                {
                    clientBl.SaveClientSetting(client.Id, $"{Constants.ClientSettings.VerificationServiceName}_{service.Value}", service.Key, 1, DateTime.UtcNow);
                }
            }
            if (isBetshopUser)
                CreateUser(client, Services.Shop, log);
            else
                CreateUser(client, Services.Insic, log);

            CreateUser(client, Services.SOW, log);
            CreateUser(client, Services.Lugas, log);
            PlayerRegistration(client, log);
            
        }

        private static void CreateUser(Client client, Services service, ILog log) //+
        {
            try
            {
                var partnerKey = CacheManager.GetNotificationServiceValueByKey(client.PartnerId, Constants.PartnerKeys.InsicPartnerKey, (int)service);
                var url = string.Format(CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InsicApiUrl).StringValue, $"api/{partnerKey}");
                if (string.IsNullOrEmpty(partnerKey))
                    return;
                var input = new UserInput
                {
                    Email = client.Email,
                    Password = $"User_{client.Id}",
                    Profile = new UserProfile
                    {
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Birthday = client.BirthDate.ToString("yyyy-MM-dd"),
                        ZipCode = client.ZipCode?.Trim(),
                        Street = client.Address,
                        HouseNumber = client.BuildingNumber,
                        MobileNumber = client.MobileNumber,
                        City = client.City,
                        PlaceOfBirth = client.Apartment
                    }
                };
                var resp = CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "token", GetAdminToken(client.PartnerId, Services.Insic, log) } },
                    Url = $"{url}/user",
                    PostData = JsonConvert.SerializeObject(input)
                }, out _);
                //   var output = JsonConvert.DeserializeObject<UserOutput>(resp);
                log.Debug(resp);
            }
            catch (Exception ex)
            {
                log.Debug("CreateUser: " + ex);
                throw;
            }
        }

        public static string GetVerificationWidget(int verificationPlatformId, int partnerId, string email, string domain, string lang, ILog log)
        {            
            var verificationServiceType = verificationPlatformId == (int)VerificationPlatforms.Insic ? (int)Services.Insic : (int)Services.SOW;
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPartnerKey, verificationServiceType);
            var mode = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicApiMode, verificationServiceType);
            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
            var redirectUrl = string.Format(distributionUrlKey.StringValue, domain);
            var userToken = GetUserToken(partnerId, email, verificationServiceType, log);
            var input = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(new
            {
                Mode = mode,
                Lang = lang,
                PartnerId = partnerKey,
                UserToken = userToken,
                Frontend = verificationPlatformId == (int)VerificationPlatforms.Insic ? 3 : 2,
            }));

          return string.Format("{0}/insic/widget?rd={1}", redirectUrl, input);
        }

        private static string GetUserToken(int partnerId, string email, int verificationPlatformId, ILog log)
        {
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPartnerKey, verificationPlatformId);
            var url = string.Format(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.InsicApiUrl).StringValue, $"api/{partnerKey}");
            var input = new UserInput
            {
                Email = email
            };
            var resp = CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = new Dictionary<string, string> { { "token", GetAdminToken(partnerId, (Services)verificationPlatformId, log) } },
                Url = $"{url}/user/getToken",
                PostData = JsonConvert.SerializeObject(input)
            }, out _);
            var output = JsonConvert.DeserializeObject<LoginOutput>(resp);
            if (!string.IsNullOrEmpty(output.Token))
                return output.Token;
            throw new Exception(resp);
        }

        public static UserModel GetUser(int partnerId, string email, int verificationPlatformId, ILog log)
        {
            var partnerKey = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPartnerKey, verificationPlatformId);
            var url = string.Format(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.InsicApiUrl).StringValue, $"api/{partnerKey}");

            var resp = CommonFunctions.SendHttpRequest(new Common.Models.HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                RequestHeaders = new Dictionary<string, string> { { "token", GetAdminToken(partnerId, (Services)verificationPlatformId, log) } },
                Url = $"{url}/user?withRelated[]=profile&query[filter][email]={email}"
            }, out _);
            var output = JsonConvert.DeserializeObject<DataModel>(resp);
            if (output.Data == null || !output.Data.Any())
                throw new Exception(resp);
            return output.Data[0];
           
        }

        #region Lugas
        private static class Events
        {
            public const string PlayerRegistration = "PlayerRegistration";//+
            public const string PlayerStatus = "PlayerStatus";// 1+ 2 Verific -
            public const string PlayerUpdate = "PlayerUpdate";//-
            public const string PlayerLogin = "PlayerLogin";//+
            public const string PlayerTimeout = "PlayerTimeout";//+
            public const string PlayerDataApproved = "PlayerDataApproved"; // verification
            public const string PlayerSuspension = "PlayerSuspension"; //+
            public const string PlayerResumption = "PlayerResumption"; //+
            public const string LocalLimitSet = "LocalLimitSet"; //+
            public const string LocalLimitUnset = "LocalLimitUnset"; //+
            public const string PlayerStatistics = "PlayerStatistics"; //-

            public const string PaymentModalityRegistration = "PaymentModalityRegistration"; //+
            public const string PaymentModalityCancelation = "PaymentModalityCancelation"; //-
            public const string Deposit = "Deposit"; //+
            public const string Payout = "Payout"; //++

        }

        public enum PlayerStatuses
        {
            VerificationPending,
            VerificationFailed,
            Verified,
            AccountClosed
        }

        private enum LimitScores
        {
            Deposit,
            Stake,
            Loss
        }

        private enum TimeUnits
        {
            Day,
            Week,
            Month
        }

        private static void AddEvent(int partnerId, object input, ILog log)
        {
            try
            {
                var partnerKey = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.InsicPartnerKey, (int)Services.Lugas);
                if (string.IsNullOrEmpty(partnerKey))
                    return;
                var url = string.Format(CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.InsicApiUrl).StringValue, $"safe2/{partnerKey}");
                var i = new Common.Models.HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "token", GetAdminToken(partnerId, Services.Lugas, log) } },
                    Url = $"{url}/lugas",
                    PostData = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                log.Info($"i: {JsonConvert.SerializeObject(i)}");
                var resp = CommonFunctions.SendHttpRequest(i, out _);
                log.Info($"Output: {resp}");
                var output = JsonConvert.DeserializeObject<ResponseBase>(resp); // handle error 
                log.Info($"Input: {JsonConvert.SerializeObject(input)}");
                if (!output.Success)
                {
                    log.Error($"Input: {JsonConvert.SerializeObject(input)}, Output: {resp}");
                    throw new Exception(resp);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw new Exception(ex.Message);
            }
        }
       
        private static void PlayerRegistration(Client client, ILog log) //+
        {
            var currentTime = DateTime.UtcNow;
            var region = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId);
            var input = new PlayerData
            {
                EventName = Events.PlayerRegistration,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = client.Id.ToString(),
                EventId = currentTime.Ticks.ToString(),
                PersonalPlayerData = new PersonalPlayerData
                {
                    GivenName = client.FirstName,
                    LastName = client.LastName,
                    Street = client.Address,
                    Number = client.BuildingNumber,
                    PostCode = client.ZipCode?.Trim(),
                    Place = client.City,
                    Area = client.Address,
                    CountryAlpha2Code = region.IsoCode,
                    BirthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                    BirthPlace = client.Apartment,
                    BirthName = string.IsNullOrEmpty(client.SecondName) ? client.LastName : client.SecondName
                }
            };
            AddEvent(client.PartnerId, new List<PlayerData> { input }, log);
        }

        public static void PlayerLogin(int partnerId, int clientId, ILog log) //+
        {
            var currentTime = DateTime.UtcNow;
            var input = new RequestBase
            {
                EventId =  CommonFunctions.GetRandomString(5) + "_" + currentTime.Ticks.ToString(),
                EventName = Events.PlayerLogin,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString()
            };
            AddEvent(partnerId, new List<RequestBase> { input }, log);
        }

        public static void PlayerLogout(int partnerId, int clientId, ILog log) //+
        {
            var currentTime = DateTime.UtcNow;
            var input = new RequestBase
            {
                EventId =  currentTime.Ticks.ToString(),
                EventName = Events.PlayerTimeout,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString()
            };
            AddEvent(partnerId, new List<RequestBase> { input }, log);
        }

        public static void CloseAccount(int partnerId, int clientId, string reason, ILog log)
        {
            PlayerStatus(partnerId, clientId, PlayerStatuses.AccountClosed, reason, log);
        }

        private static void PlayerStatus(int partnerId, int clientId, PlayerStatuses status, string reason, ILog log) 
        {
            var currentTime = DateTime.UtcNow;
            var input = new PlayerStatusInput
            {
                EventName = Events.PlayerStatus,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                Status = status.ToString(),
                Intent = reason
            };
            AddEvent(partnerId, new List<PlayerStatusInput> { input }, log);
        }

        public static void PlayerUpdate(Client client, ILog log) 
        {
            var currentTime = DateTime.UtcNow;
            var region = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId);

            var input = new PlayerData
            {
                EventName = Events.PlayerUpdate,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = client.Id.ToString(),
                EventId = currentTime.Ticks.ToString(),
                PersonalPlayerData = new PersonalPlayerData
                {
                    GivenName = client.FirstName,
                    LastName = client.LastName,
                    Street = client.Address,
                    Number = client.BuildingNumber,
                    PostCode = client.ZipCode?.Trim(),
                    Place = client.City,
                    Area = client.Address,
                    CountryAlpha2Code = region.IsoCode,
                    BirthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                    BirthPlace = client.Apartment,
                    BirthName = string.IsNullOrEmpty(client.SecondName) ? client.LastName : client.SecondName
                }
            };
            AddEvent(client.PartnerId, new List<PlayerData> { input }, log);
        }

        public static void PlayerExcluded(int partnerId, int clientId, string suspensionReason, DateTime suspensionEndsAt, ILog log) //+
        {
            var currentTime = DateTime.UtcNow;
            var input = new SuspensionInput
            {
                EventName = Events.PlayerSuspension,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                SuspensionReason = /*!string.IsNullOrEmpty(suspensionReason)  ? suspensionReason :*/ "Voluntary",
               // SuspensionEndsAt = suspensionEndsAt.ToString("yyyy-MM-dd")
            };
            AddEvent(partnerId, new List<SuspensionInput> { input }, log);
        }

        public static void PlayerUnexcluded(int partnerId, int clientId, string suspensionReason, ILog log) //+-
        {
            var currentTime = DateTime.UtcNow;
            var input = new SuspensionInput
            {
                EventName = Events.PlayerResumption,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                SuspensionReason = !string.IsNullOrEmpty(suspensionReason) ? suspensionReason : "Voluntary",
            };
            AddEvent(partnerId, new List<SuspensionInput> { input }, log);
        }

        public static void UpdatePlayerLimit(int partnerId, int clientId, string limitSettingName, decimal? amount, ILog log) //+
        {
            var limitScore = limitSettingName.Contains(LimitScores.Deposit.ToString()) ? LimitScores.Deposit :
                            (limitSettingName.Contains(LimitScores.Loss.ToString()) ? LimitScores.Loss :
                          (limitSettingName.Contains("Bet") ? LimitScores.Stake : 0));
            if (limitScore == 0) return;
            var timeUnit = limitSettingName.Contains(TimeUnits.Month.ToString()) ? TimeUnits.Month :
                          (limitSettingName.Contains(TimeUnits.Week.ToString()) ? TimeUnits.Week : TimeUnits.Day);
            if (amount.HasValue)
                SetLimit(partnerId, clientId, limitScore, timeUnit, amount==0 ? 0.1m : amount.Value, log);
            else
                UnsetLimit(partnerId, clientId, limitScore, timeUnit, log);
        }

        private static void SetLimit(int partnerId, int clientId, LimitScores limitScore, TimeUnits timeUnit, decimal amount, ILog log)
        {
            var currentTime = DateTime.UtcNow;
            var input = new LimitInput
            {
                EventName = Events.LocalLimitSet,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                LimitScope = limitScore.ToString(),
                TimeUnit = timeUnit.ToString(),
                Amount = amount
            };
            AddEvent(partnerId, new List<LimitInput> { input }, log);
        }

        private static void UnsetLimit(int partnerId, int clientId, LimitScores limitScore, TimeUnits timeUnit, ILog log)
        {
            var currentTime = DateTime.UtcNow;
            var input = new LimitInput
            {
                EventName = Events.LocalLimitUnset,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                LimitScope = limitScore.ToString(),
                TimeUnit = timeUnit.ToString()
            };
            AddEvent(partnerId, new List<LimitInput> { input }, log);
        }

        public static void PaymentModalityRegistration(int partnerId, int clientId, int paymentSystemId, DAL.Models.SessionIdentity session, ILog log)//+
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var currentTime = DateTime.UtcNow;
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentSystemId);
                var paymentSystems = clientBl.GetClientPaymentSystems(clientId);
                var index = 0;
                if (paymentSystems!= null)
                {
                    if (paymentSystems.Contains(paymentSystemId))
                        index = paymentSystems.FindIndex(x => x == paymentSystemId);
                    else
                        index = paymentSystems.Count() + 1;
                }
                var input = new PaymentModalityInput
                {
                    EventName = Events.PaymentModalityRegistration,
                    EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                    PlayerId = clientId.ToString(),
                    EventId = currentTime.Ticks.ToString(),
                    PaymentModalityId = paymentSystemId.ToString(),
                    MethodName = paymentSystem.Name,
                    IdData =  Constants.CapitalLetters[Math.Max(0, index - 1)].ToString()
                };
                AddEvent(partnerId, new List<PaymentModalityInput> { input }, log);
            }
        }

        public static void PaymentRequest(int partnerId, int clientId, long paymentRequestId, int paymentRequestType,  decimal paymentAmount, ILog log) //+
        {
            var currentTime = DateTime.UtcNow;
            var input = new PaymentInput
            {
                EventName = paymentRequestType == (int)PaymentRequestTypes.Deposit ? Events.Deposit : Events.Payout,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                Amount = paymentAmount,
                DepositId = paymentRequestType == (int)PaymentRequestTypes.Deposit  ? paymentRequestId.ToString() : null,
                PayoutId = paymentRequestType == (int)PaymentRequestTypes.Deposit ? null : paymentRequestId.ToString(),
                Automatic =  paymentRequestType == (int)PaymentRequestTypes.Deposit ? (bool?)null : false
            };
            AddEvent(partnerId, new List<PaymentInput> { input }, log);
        }

        public static void PlayerStatistics(int partnerId, int clientId, decimal stake, decimal profit, decimal loss, ILog log) //-
        {
            var currentTime = DateTime.UtcNow;
            var input = new StatisticsInput
            {
                EventName = Events.PaymentModalityCancelation,
                EventTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                PlayerId = clientId.ToString(),
                EventId = currentTime.Ticks.ToString(),
                Stake = stake,
                Profit = profit,
                Loss = loss
            };
            AddEvent(partnerId, new List<StatisticsInput> { input }, log);
        }

        #endregion
    }
}
