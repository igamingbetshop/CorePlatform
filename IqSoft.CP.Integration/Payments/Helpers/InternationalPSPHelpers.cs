using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using log4net;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using IqSoft.CP.Common.Models;
using System;
using IqSoft.CP.Common.Enums;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.InternationalPSP;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class InternationalPSPHelpers
    {
        public static string CallPSPApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                              JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Domain", session.Domain);
                var amount = input.Amount;
                if (input.CurrencyId != Constants.Currencies.USADollar)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar);
                    amount = Math.Round(rate * input.Amount, 2);
                    parameters.Add("Currency", Constants.Currencies.USADollar);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                }
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);
                var paymentProcessingInput = new
                {
                    OrderId = input.Id,
                    RedirectUrl = cashierPageUrl,
                    ResponseUrl = string.Format("{0}/api/InternationalPSP/ProcessPaymentRequest", paymentGatewayUrl),
                    CancelUrl = string.Format("{0}/api/InternationalPSP/CancelPaymentRequest", paymentGatewayUrl),
                    Amount = amount,
                    Currency = Constants.Currencies.USADollar,
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
                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }

        public static void CheckPaymentRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            using (var paymentSystemBl = new PaymentSystemBll(clientBl))
            using (var notificationBl = new NotificationBll(clientBl))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InternationalPSPApiUrl).StringValue;
                var authToken = Convert.ToBase64String(Encoding.Default.GetBytes(partnerPaymentSetting.Password));

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {authToken}" } },
                    Url = $"{url}/status",
                    PostData = JsonConvert.SerializeObject(new { transaction_uuid = paymentRequest.ExternalTransactionId })
                };
                var statusOutput = JsonConvert.DeserializeObject<StatusOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                if (statusOutput.Status.ToLower() == "approved")
                {
                    if (paymentRequest.Amount != statusOutput.AmountCaptured)
                    {
                        paymentRequest.Amount = statusOutput.AmountCaptured;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                }
                else if (statusOutput.Status.ToLower() == "declined" || statusOutput.Status.ToLower() == "rejected")
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, $"Code: {statusOutput.Code}, Msg: {statusOutput.Status}", notificationBl);

            }
        }
    }
}
