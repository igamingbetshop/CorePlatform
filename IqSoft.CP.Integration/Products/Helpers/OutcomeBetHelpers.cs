using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using OutcomeBet = IqSoft.CP.Integration.Products.Models.OutcomeBet;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using log4net;
using IqSoft.CP.Integration.Products.Models.OutcomeBet;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class OutcomeBetHelpers
    {
        private static readonly string CertificatePath = @"C:\Certificates\{0}\client_{1}.pfx";
        private static readonly string CertificatePass = "iqsoft";
        public static List<GameItem> GetGamesList(int partnerId, string providerName)
        {
            var requestInput = new RequestBase
            {
                JsonRpc = "2.0",
                Method = "Game.List",
                RequestId = CommonFunctions.GetRandomString(16)
            };
            var provider = CacheManager.GetGameProviderByName(providerName);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Accept = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };
            var certificate = new X509Certificate2(string.Format(CertificatePath, providerName.ToLower(), partnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
            return JsonConvert.DeserializeObject<OutcomeBet.Game>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate)).Result.Games;
        }

        public static string RegisterBankGroup(int partnerId, string currencyId, string providerName)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            var groupId = string.Format("{0}_{1}", partner.Name, currencyId);
            var requestInput = new RequestBase
            {
                JsonRpc = "2.0",
                Method = "BankGroup.Set",
                RequestId = CommonFunctions.GetRandomString(16),
                Params = new
                {
                    Id = groupId,
                    Currency = currencyId
                }
            };
            var provider = CacheManager.GetGameProviderByName(providerName);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Accept = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };
            var certificate = new X509Certificate2(string.Format(CertificatePath, providerName.ToLower(), partnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
            JsonConvert.DeserializeObject<OutcomeBet.ResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate));
            return groupId;
        }

        public static OutcomeBet.ResponseBase RegisterPlayer(int clientId, BllGameProvider provider)
        {
            var client = CacheManager.GetClientById(clientId);
            var groupId = RegisterBankGroup(client.PartnerId, client.CurrencyId, provider.Name);
            var requestInput = new RequestBase
            {
                JsonRpc = "2.0",
                Method = "Player.Set",
                RequestId = CommonFunctions.GetRandomString(16),
                Params = new
                {
                    Id = clientId.ToString(),
                    Nick = client.UserName,
                    BankGroupId = groupId
                }
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Accept = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(requestInput)
            };
            var certificate = new X509Certificate2(string.Format(CertificatePath, provider.Name.ToLower(), client.PartnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
            return JsonConvert.DeserializeObject<OutcomeBet.ResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate));
        }

        public static string CreateSession(int clientId, int productId, string languageId, string token, SessionIdentity identity, ILog log)
        {
            var product = CacheManager.GetProductById(productId);
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            if (provider == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var client = CacheManager.GetClientById(clientId);
            RegisterPlayer(clientId, provider);
            using (var clientBl = new ClientBll(identity, log))
            {
                var clienFreeSpinBonus = clientBl.GetClientFreeSpinBonus(clientId, productId);
                var requestInput = new RequestBase
                {
                    JsonRpc = "2.0",
                    Method = "Session.Create",
                    RequestId = CommonFunctions.GetRandomString(16),
                    Params = new
                    {
                        PlayerId = clientId.ToString(),
                        GameId = product.ExternalId,
                        Params = new { language = languageId },
                        RestorePolicy = "Restore",
                        AlternativeId = token,
                        BonusId = clienFreeSpinBonus != null ? clienFreeSpinBonus.BonusId.ToString() : string.Empty
                    }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    Accept = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = provider.GameLaunchUrl,
                    PostData = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                var certificate = new X509Certificate2(string.Format(CertificatePath, provider.Name.ToLower(), client.PartnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
                var res = JsonConvert.DeserializeObject<OutcomeBet.ResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate));
                if (res.Error != null && res.Error.Code != 0)
                    throw new Exception(res.Error.Message);
                var sessionUrl = CacheManager.GetGameProviderValueByKey(client.PartnerId, provider.Id, Constants.PartnerKeys.OutcomeBetSessionUrl);
                var gameSession = JsonConvert.DeserializeObject<GameSession>(JsonConvert.SerializeObject(res.Result));
                if (!string.IsNullOrEmpty(sessionUrl))
                    return string.Format(sessionUrl, gameSession.SessionId);
                return gameSession.SessionUrl;
            }
        }

        public static string CreateDemoSession(int productId, int partnerId, int clientId)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            var product = CacheManager.GetProductById(productId);
            var client = CacheManager.GetClientById(clientId);
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            if (provider == null)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
            var groupId = RegisterBankGroup(partnerId, client == null ? partner.CurrencyId : client.CurrencyId, provider.Name);
            var requestInput = new RequestBase
            {
                JsonRpc = "2.0",
                Method = "Session.CreateDemo",
                RequestId = CommonFunctions.GetRandomString(16),
                Params = new
                {
                    BankGroupId = groupId,
                    GameId = product.ExternalId
                }
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Accept = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(requestInput)
            };
            var certificate = new X509Certificate2(string.Format(CertificatePath, provider.Name.ToLower(), partnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
            var res = JsonConvert.DeserializeObject<OutcomeBet.ResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate));
            if (res.Error != null && res.Error.Code != 0)
                throw new Exception(res.Error.Message);
            var sessionUrl = CacheManager.GetGameProviderValueByKey(partnerId, provider.Id, Constants.PartnerKeys.OutcomeBetSessionUrl);
            var gameSession = JsonConvert.DeserializeObject<OutcomeBet.GameSession>(JsonConvert.SerializeObject(res.Result));
            if (!string.IsNullOrEmpty(sessionUrl))
                return string.Format(sessionUrl, gameSession.SessionId);
            return gameSession.SessionUrl;
        }

        public static bool CreateFreeSpinBonus(int partnerId, int bonusId, int productId, int freeSpinCount, SessionIdentity identity)
        {
            var product = CacheManager.GetProductById(productId);
            if (product == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ProductNotFound);
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            if (provider == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongProviderId);
            var productKey = new Dictionary<string, object> { { product.ExternalId, new { FsCount = freeSpinCount } } };
            var requestInput = new RequestBase
            {
                JsonRpc = "2.0",
                Method = "Bonus.Set",
                RequestId = CommonFunctions.GetRandomString(16),
                Params = new
                {
                    Id = bonusId.ToString(),
                    FsType = "original",
                    CounterType = "separate",
                    SeparateParams = new
                    {
                        Games = productKey
                    }
                }
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Accept = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = provider.GameLaunchUrl,
                PostData = JsonConvert.SerializeObject(requestInput, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };
            var certificate = new X509Certificate2(string.Format(CertificatePath, provider.Name.ToLower(), partnerId), CertificatePass, X509KeyStorageFlags.MachineKeySet);
            var res = JsonConvert.DeserializeObject<OutcomeBet.ResponseBase>(CommonFunctions.SendHttpRequest(httpRequestInput, out _, certificate: certificate));
            if (res.Error != null)
                return false;
            return true;
        }
    }
}