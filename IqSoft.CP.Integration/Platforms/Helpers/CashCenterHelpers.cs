using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Platforms.Models.CashCenter;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Platforms.Helpers
{
    public static class CashCenterHelpers
    {
        public static UserModel LoginUser(int partnerId, string code, SessionIdentity session, ILog log)
        {
            try
            {
                var token = UserAuthentication(partnerId, code, session);
                var userModel = GetUserData(partnerId, token);
                var walletInfo = GetBalance(partnerId, userModel.UserId);
                userModel.Token = token;
                userModel.CurrencyId = walletInfo.CurrencyId;
                userModel.Balance = walletInfo.AvailableBalance;
                return userModel;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return null;
            }
        }

        public static string UserAuthentication(int partnerId, string code, SessionIdentity session)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterApiUrl).StringValue;
            var clientId = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterClientId).StringValue;
            var clientSecret = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterClientSecret).StringValue;
            var codeVerifier = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterCodeVerifier).StringValue;
            var distributionUrlKey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.DistributionUrl);
            if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

            var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
            var input = new
            {
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                code,
                code_verifier = codeVerifier,
                redirect_uri = Uri.EscapeDataString(string.Format("{0}/cashcenter/redirectrequest", distributionUrl))
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                PostData = CommonFunctions.GetUriDataFromObject(input),
                Url = string.Format("{0}/token", url)
            };
            return JsonConvert.DeserializeObject<AuthenticationOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).AccessToken;
        }

        public static UserModel GetUserData(int partnerId, string userToken)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterApiUrl).StringValue;
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + userToken } },
                Url = string.Format("{0}/userinfo", url)
            };
            return JsonConvert.DeserializeObject<UserModel>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
        }

        public static string AppAuthentication(int partnerId)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterApiUrl).StringValue;
            var clientId = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterAppClientId).StringValue;
            var clientSecret = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterAppClientSecret).StringValue;
            var input = new
            {
                grant_type = "client_credentials",
                client_id = clientId,
                client_secret = clientSecret
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                PostData = CommonFunctions.GetUriDataFromObject(input),
                Url = string.Format("{0}/token", url)
            };
            return JsonConvert.DeserializeObject<AuthenticationOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).AccessToken;
        }

        public static UserWallet GetBalance(int partnerId, string userId)
        {
            userId = userId.Replace(Constants.ExternalClientPrefix, string.Empty);
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CashCenterAppApiUrl).StringValue;
            var appToken = AppAuthentication(partnerId);
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Get,
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + appToken } },
                Url = string.Format("{0}/balance/id/{1}", url, userId)
            };
            var result = JsonConvert.DeserializeObject<WalletResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (result.Errors != null && result.Errors.Any())
                throw new Exception(result.Errors[0]);

            return result.UserWalletInfo;
        }

        public static WalletResult Credit(int clientId, string transactionId, decimal amount)
        {
            var client = CacheManager.GetClientById(clientId);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashCenterAppApiUrl).StringValue;
            var refId = CommonFunctions.ComputeMd5(transactionId.ToString());
            var appToken = AppAuthentication(client.PartnerId);

            var reservationInput = new TransactionInput
            {
                UserId = client.UserName.Replace(Constants.ExternalClientPrefix, string.Empty),
                TransactionId = refId,
                Amount = amount,
                CurrencyId = client.CurrencyId,
                Description = "Credit"
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + appToken } },
                PostData = JsonConvert.SerializeObject(reservationInput),
                Url = string.Format("{0}/reservation", url)
            };
            var result = JsonConvert.DeserializeObject<WalletResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (result.Errors!=null && result.Errors.Any())
                throw new Exception(result.Errors[0]);
            var releaseInput = new ReleaseInput
            {
                Confirmed = true,
                TransactionId = refId
            };
            httpRequestInput.Url = string.Format("{0}/release", url);
            httpRequestInput.PostData  = JsonConvert.SerializeObject(releaseInput);
            result = JsonConvert.DeserializeObject<WalletResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (result.Errors!=null && result.Errors.Any())
                throw new Exception(result.Errors[0]);

            return result;
        }

        public static WalletResult Debit(int clientId, string transactionId, decimal amount, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashCenterAppApiUrl).StringValue;
            var refId = CommonFunctions.ComputeMd5(transactionId.ToString());
            var appToken = AppAuthentication(client.PartnerId);
            var depositInput = new TransactionInput
            {
                UserId = client.UserName.Replace(Constants.ExternalClientPrefix, string.Empty),
                TransactionId = refId,
                Amount = amount,
                CurrencyId = client.CurrencyId,
                Description = "Debit"
            };
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + appToken } },
                PostData = JsonConvert.SerializeObject(depositInput),
                Url = string.Format("{0}/deposit", url)
            };
            
            var result = JsonConvert.DeserializeObject<WalletResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (log != null)
                log.Info("CashCenter_Debit_" + JsonConvert.SerializeObject(httpRequestInput) + "_" + JsonConvert.SerializeObject(result));
            if (result.Errors!=null && result.Errors.Any())
                throw new Exception(result.Errors[0]);

            return result;
        }

        public static void Rollback(int clientId, string transactionId, int transactionType, decimal amount, ILog log)
        {
            if (transactionType == (int)OperationTypes.WinRollback)
                Credit(clientId, transactionId, amount);
            else if (transactionType == (int)OperationTypes.BetRollback)
                Debit(clientId, transactionId, amount, log);
        }
    }
}
