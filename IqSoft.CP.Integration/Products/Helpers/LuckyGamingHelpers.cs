using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Products.Models.LuckyGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public class LuckyGamingHelpers
    {
        private readonly static BllGameProvider Provider = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyGaming);

        public static string GetUrl(int partnerId, int clientId, int productId)
        {
            var aesKey = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.LuckyGamingAESKey);
            var md5Key = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.LuckyGamingMD5Key);
            var adentId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.LuckyGamingAgentID);
            var product = CacheManager.GetProductById(productId);
            var gamePlatformId = CacheManager.GetGameProviderValueByKey(partnerId, Provider.Id, Constants.PartnerKeys.LuckyGamingGamePlatformID);
            var accountName = GetAccount(clientId, aesKey, md5Key, adentId);
            var apiUrl = string.Format(Provider.GameLaunchUrl, "launchH5");
            var input = new
            {
                AgentID = adentId,
                AccountName = accountName,
                GamePlatformID = gamePlatformId,
                GameID = product.ExternalId
            };
            var response = ApiRequest(apiUrl, input, aesKey, md5Key);
            var data = JsonConvert.DeserializeObject<LaunchH5Output>(response);
            if(data.ErrorCode != 0)
            {
                throw new Exception($"Error: {data.ErrorCode} ");
            }
            return data.Url;
        }

        private static string GetAccount(int clientId, string aesKey, string md5Key, string agentId)
        {
            var client = CacheManager.GetClientById(clientId);
            var input = new AccountInput()
            {
                AgentID = agentId,
                AccountName = client.Id + "lucky",
                AccountPW = client.Id.ToString() + "Lucky",
                AccountDisplay = client.UserName + "lucky",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            var apiUrl = string.Format(Provider.GameLaunchUrl, "accountcreate");
            var response = ApiRequest(apiUrl, input, aesKey, md5Key);

            var data = JsonConvert.DeserializeObject<AccountOutput>(response);
            if(data.ErrorCode == 0)
                return data.AccountName;
            if (data.ErrorCode == 903)
                return client.Id + "lucky";
            else
            {
                throw new Exception($"Error: {data.ErrorCode} ");
            }
        }
        

        private static string ApiRequest(string apiUrl, object input, string aesKey, string md5Key)
        {
            var inputString = JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var aes = AESEncryptHelper.Encryption(inputString, aesKey);
            var hash = CommonFunctions.ComputeMd5(aes + md5Key);
            var requestHeaders = new Dictionary<string, string>
            {
                { "AES-ENCODE", hash }           
            };
            
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson, 
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = apiUrl,
                RequestHeaders = requestHeaders,
                PostData = inputString
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            return response;
        }
    }
}
