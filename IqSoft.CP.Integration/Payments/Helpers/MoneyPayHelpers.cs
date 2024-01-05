// Author: Varsik Harutyunyan


using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.MoneyPay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class MoneyPayHelpers
    {
        public static string CallMoneyPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var currencyBll = new CurrencyBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    if (paymentsystem == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                    var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");

                    var paymentProcessingInput = new
                    {
                        OrderId = input.Id,
                        RedirectUrl = cashierPageUrl,
                        ResponseUrl = string.Format("{0}/api/MoneyPay/ProcessPaymentRequest", paymentGatewayUrl),
                        CancelUrl = string.Format("{0}/api/MoneyPay/CancelPaymentRequest", paymentGatewayUrl),
                        Amount = input.Amount.ToString("F"),
                        Currency = input.CurrencyId,
                        BillingAddress = client.Address?.Trim(),
                        HolderName = string.Format("{0} {1}", client.FirstName, client.LastName),
                        PartnerDomain = session.Domain,
                        ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                        session.LanguageId,
                        CountryCode = paymentInfo.Country,
                        ZipCode = client.ZipCode?.Trim(),
                        paymentInfo.City,
                        client.PartnerId,
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
        }
      
        public static void GetPayinRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MoneyPayQueryUrl).StringValue;
                var merchantId = partnerPaymentSetting.UserName.Split(',');
                var merchant = merchantId[0];
                var count = CacheManager.GetClientDepositCount(client.Id);
                var merchantInfo = partnerPaymentSetting.Password.Split(',');
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                if (count > 0 && merchantId.Length > 1)
                    merchant = merchantId[1];
              
                string channel_id = "";
                string secret_key = "";
                if (paymentInfo.CardNumber.StartsWith("34") || paymentInfo.CardNumber.StartsWith("37"))
                {
                    channel_id = merchantInfo[1].Split('-')[0];
                    secret_key = merchantInfo[1].Split('-')[1];
                }
                else
                {
                    channel_id = merchantInfo[0].Split('-')[0];
                    secret_key = merchantInfo[0].Split('-')[1];

                }
                var signObj = new
                {
                    version = "31",
                    merchant_id = partnerPaymentSetting.UserName,
                    channel_id,
                    transaction_id = paymentRequest.Id.ToString(),
                    secret_key
                };
              
                var xmlText = CommonFunctions.GetUriDataFromObject(signObj);
                var sign = CommonFunctions.ComputeMd5(xmlText).ToUpper();
                var transaction = new
                {
                    version = "31",
                    transaction_id = paymentRequest.Id.ToString(),
                    merchant_id = partnerPaymentSetting.UserName,
                    channel_id,
                    sign
                };
                var xmlDoc = CommonFunctions.GetUriDataFromObject(transaction);

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = xmlDoc,
                };
                var responseData = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<TransactionInfo>(responseData);
                if (output.Result.ToUpper() == "SUCCESSFUL")
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty, notificationBl);
                        }
                    }
                }
                else if (output.Result.ToUpper() == "FAILED")
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(documentBl))
                        {
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, output.Error_info, notificationBl);
                        }
                    }
                }
            }
        }

    }
}
