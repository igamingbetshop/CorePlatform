using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using IqSoft.CP.Integration.Products.Models.Tomhorn;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Linq;
using System.Text;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class TomHornHelpers
    {
        private static readonly BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.TomHorn);

        private static List<string> SubProvidersList = new List<string>
        {
            "VivoGaming",
            "BetIndustries",
            "Provision",
            "FortuneCrown"
        };
        public static string GetGameData(int partnerId, int clientId, int productId, bool isForMobile, bool isForDemo, SessionIdentity session, out string newSesstionId)
        {
            var product = CacheManager.GetProductById(productId);
            var partner = CacheManager.GetPartnerById(partnerId);
            string secretKey, operatorId;
            if (!product.SubProviderId.HasValue || product.SubProviderId == product.GameProviderId)
            {
                secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornSecretKey);
                operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornOperatorId);
            }
            else
            {
                secretKey = CacheManager.GetGameProviderValueByKey(partnerId, product.SubProviderId.Value, Constants.PartnerKeys.TomHorn3SecretKey);
                operatorId = CacheManager.GetGameProviderValueByKey(partnerId, product.SubProviderId.Value, Constants.PartnerKeys.TomHorn3OperatorId);
            }
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornApiUrl);
            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerKeyNotFound);
            var moduleParameters = string.Empty;
            long sessionId = 0;
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post
            };
            if (!isForDemo)
            {
                var client = CacheManager.GetClientById(clientId);

                var createIdentityInput = new
                {
                    partnerID = operatorId,
                    name = client.Id.ToString(),
                    displayName = client.Id.ToString(),
                    currency = client.CurrencyId,
                    sign = CommonFunctions.ComputeHMACSha256(string.Format("{0}{1}{1}{2}", operatorId, clientId, client.CurrencyId), secretKey)
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(createIdentityInput);
                httpRequestInput.Url = string.Format(apiUrl, "CreateIdentity");
                CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var createSessionInput = new
                {
                    partnerID = operatorId,
                    name = client.Id.ToString(),
                    sign = CommonFunctions.ComputeHMACSha256(string.Format("{0}{1}", operatorId, clientId), secretKey)
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(createSessionInput);
                httpRequestInput.Url = string.Format(apiUrl, "CreateSession");
                var sessionOutput = JsonConvert.DeserializeObject<SessionOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (sessionOutput.Code != 0)
                    throw new Exception(sessionOutput.MessageText);
                var closeSessionInput = new
                {
                    partnerID = operatorId,
                    sessionID = sessionOutput.Session.Id,
                    sign = CommonFunctions.ComputeHMACSha256(string.Format("{0}{1}", operatorId, sessionOutput.Session.Id), secretKey)
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(closeSessionInput);
                httpRequestInput.Url = string.Format(apiUrl, "CloseSession");
                CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                httpRequestInput.PostData = JsonConvert.SerializeObject(createSessionInput);
                httpRequestInput.Url = string.Format(apiUrl, "CreateSession");
                sessionOutput = JsonConvert.DeserializeObject<SessionOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (sessionOutput.Code != 0)
                    throw new Exception(sessionOutput.MessageText);

                var getModuleInfo = new
                {
                    partnerID = operatorId,
                    sessionID = sessionOutput.Session.Id,
                    module = product.ExternalId,
                    sign = CommonFunctions.ComputeHMACSha256(string.Format("{0}{1}{2}", operatorId, sessionOutput.Session.Id, product.ExternalId), secretKey)
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(getModuleInfo);
                httpRequestInput.Url = string.Format(apiUrl, "GetModuleInfo");
                moduleParameters = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                sessionId = sessionOutput.Session.Id;
            }
            else
            {
                var getModuleInfo = new
                {
                    partnerID = operatorId,
                    currency = partner.CurrencyId,
                    module = product.ExternalId,
                    sign = CommonFunctions.ComputeHMACSha256(string.Format("{0}{1}{2}", operatorId, product.ExternalId, partner.CurrencyId), secretKey)
                };
                httpRequestInput.PostData = JsonConvert.SerializeObject(getModuleInfo);
                httpRequestInput.Url = string.Format(apiUrl, "GetPlayMoneyModuleInfo");
                moduleParameters = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            }

            var moduleBytes = Encoding.UTF8.GetBytes(moduleParameters);

            var parameters = new
            {
                module = Convert.ToBase64String(moduleBytes),
                siteUrl = session.Domain,
                languageId = session.LanguageId,
                sessionId,
                productExternalId = product.ExternalId,
                isForMobile
            };
            newSesstionId = sessionId.ToString();
            return string.Format(Provider.GameLaunchUrl, session.Domain, GetUriDataFromObject(parameters));
        }

        public static string GetUriDataFromObject<T>(T obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             select p.Name + "=" +
                            (p.GetValue(obj, null) != null ? p.GetValue(obj, null).ToString() : string.Empty);

            var requestData = string.Join("&", properties.Where(x => !string.IsNullOrEmpty(x)));
            return requestData;
        }

        private static List<GameModule> GetGameModule(string apiUrl, string operatorId, string secretKey)
        {
            var input = new
            {
                partnerID = operatorId,
                sign = CommonFunctions.ComputeHMACSha256(operatorId, secretKey)
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url =  string.Format(apiUrl, "GetGameModules"),
                PostData = JsonConvert.SerializeObject(input)
            };
            var resp = JsonConvert.DeserializeObject<GamesOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Code != 0)
                throw new Exception(resp.Message);
            return resp.GameModules;
        }

        public static List<GameModule> GetGamesList(int partnerId)
        {
            var apiUrl = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornApiUrl);
            var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornOperatorId);
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.TomHornSecretKey);
            var gamesModules = GetGameModule(apiUrl, operatorId, secretKey).Select(x => { x.Provider = Provider.Id.ToString(); return x; }).ToList();

            foreach (var subProviderName in SubProvidersList)
            {
                var subProvider = CacheManager.GetGameProviderByName(subProviderName);
                if(subProvider != null)
                {
                    secretKey = CacheManager.GetGameProviderValueByKey(partnerId, subProvider.Id, Constants.PartnerKeys.TomHorn3SecretKey);
                    operatorId = CacheManager.GetGameProviderValueByKey(partnerId, subProvider.Id, Constants.PartnerKeys.TomHorn3OperatorId);
                    if (!string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(operatorId))
                        gamesModules.AddRange(GetGameModule(apiUrl, operatorId, secretKey).Select(x => { x.Provider = subProvider.Id.ToString(); return x; }));
                }
            }
            return gamesModules;
        }
    }
}