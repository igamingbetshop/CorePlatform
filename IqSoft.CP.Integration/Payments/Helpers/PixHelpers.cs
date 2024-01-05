using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Pix;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class PixHelpers
    {
        public static string CreateSaleByQRCode(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pix);
                var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.PixUrl);
                var pixKey = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.PixKey);
                var prefix = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.PixPrefix);
                string conciliationId = prefix != null ? string.Format(prefix, DateTime.Now.ToString("yyyyMMdd")) : prefix;
                var token = GetAccessToken(session, client, paymentSystem, url, (int)PaymentRequestTypes.Deposit);
                var action = string.Format(url, "purchase");
                var requestHeaders = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {token}"}
                    };
                var httpRequestInput = new HttpRequestInput
                {
                    Accept = Constants.HttpContentTypes.ApplicationJson,
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = action,
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(new { amount = input.Amount, pix_key = pixKey, conciliation_id = conciliationId + input.Id })
                };
                var encodedData = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var encodedValue = JsonConvert.DeserializeObject<DepositResponse>(encodedData).EncodedValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);

                string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue));
                var paymentProcessingInput = new
                {
                    PayAddress = decodedData,
                    Amount = input.Amount,
                    Currency = input.CurrencyId,
                    PartnerDomain = session.Domain,
                    ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                    PartnerId = client.PartnerId,
                    LanguageId = session.LanguageId,
                    PaymentSystemName = input.PaymentSystemName.ToLower()
                };

                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pix);
                    var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.PixUrl);
                    var action = string.Format(url, "pix/cash-out");
                    var destinationPixKey = JsonConvert.DeserializeObject<PaymentInfo>(input.Info).WalletNumber;
                    var token = GetAccessToken(session, client, paymentSystem, url, (int)PaymentRequestTypes.Withdraw);
                    var requestHeaders = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {token}"}
                    };
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var httpRequestInput = new HttpRequestInput
                    {
                        Accept = Constants.HttpContentTypes.ApplicationJson,
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = action,
                        RequestHeaders = requestHeaders,
                        PostData = JsonConvert.SerializeObject(new { amount, pix_key = destinationPixKey })
                    };
                    var responce = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var data = JsonConvert.DeserializeObject<WithdrawResponse>(responce);
                    input.ExternalTransactionId = data.UuId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    log.Info(JsonConvert.SerializeObject(responce));
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding
                    };
                }
            }
        }

        private static string GetAccessToken(SessionIdentity session, BllClient client, BllPaymentSystem paymentSystem, string url, int type)
        {
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, type);
            if (partnerPaymentSetting == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

            var client_id = partnerPaymentSetting.UserName;
            var client_secret = partnerPaymentSetting.Password;
            var loginUrl = string.Format(url, "login");

            var loginResponse = CommonFunctions.SendHttpRequest(new HttpRequestInput
            {
                Accept = Constants.HttpContentTypes.ApplicationJson,
                Url = loginUrl,
                ContentType = "application/json",
                RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(new { client_id, client_secret })
            }, out _);
            var token = JsonConvert.DeserializeObject<LoginResponse>(loginResponse).AccessToken;
            return token;
        }
    }
}
