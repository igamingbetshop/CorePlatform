using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Integration.Payments.Models.Mifinity;
using System.Collections.Generic;
using System.Linq;
using System;
using IqSoft.CP.Integration.Payments.Models;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class MifinityHelpers
    {
        public static string CallMifinityApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                if (string.IsNullOrEmpty(client.MobileNumber) || string.IsNullOrEmpty(client.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MifinityApiUrl);
                var environment = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.MifinityEnvironment);
                if (string.IsNullOrEmpty(environment))
                    environment = "secure";
                var paymentGtw = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var accountData = partnerPaymentSetting.UserName.Split(',');
                var timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                var paymentRequestInput = new
                {
                    brandId = accountData[0],
                    validationKey = $"mifinity_validationKey_{timestamp}_{timestamp}",
                    traceId = input.Id.ToString(),
                    clientReference = client.Id.ToString(),
                    client = new
                    {
                        dob = client.BirthDate.ToString("yyyy-MM-dd"),
                        firstName = client.FirstName,
                        lastName = client.LastName,
                        phone = client.MobileNumber,
                        dialingCode = client.MobileNumber.Replace("+", string.Empty).Substring(0, 3),
                        nationality = paymentInfo.Country,
                        emailAddress = paymentInfo.Info
                    },
                    address = new
                    {
                        addressLine1 = client.Address,
                        countryCode = paymentInfo.Country,
                        city = paymentInfo.City
                    },
                    money = new
                    {
                        amount = input.Amount,
                        currency = client.CurrencyId
                    },
                    returnUrl = $"{paymentGtw}/api/Mifinity/PayRequest",
                    errorUrl = $"{paymentGtw}/api/Mifinity/RejectRequest",
                    description = input.Id.ToString(),
                    destinationAccountNumber = accountData[1],
                    languagePreference = session.LanguageId.ToUpper()
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    RequestHeaders = new Dictionary<string, string> { { "x-api-version", "1" }, { "key", partnerPaymentSetting.Password } },
                    Url = $"{url}/pegasus-ci/api/gateway/init-iframe",
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var iframeResult = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (iframeResult.Payload != null && iframeResult.Payload.Any())
                {
                    var distributionUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                    return $"{distributionUrl}/mifinity/init?it={iframeResult.Payload[0].InitializationToken}&e={environment}&d={session.Domain}";
                }
                throw new Exception(resp);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MifinityApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var partner = CacheManager.GetPartnerById(client.PartnerId);

                var payoutRequestInput = new
                {
                    sourceAccount = partnerPaymentSetting.UserName,
                    destinationAccount = paymentInfo.WalletNumber,
                    money = new { amount, currency = client.CurrencyId },
                    description = partner.Name,
                    destinationCurrency = client.CurrencyId,
                    traceId = paymentRequest.Id
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/api/payments/acct2acct", url),
                    RequestHeaders = new Dictionary<string, string> { { "x-api-version", "1" }, { "key", partnerPaymentSetting.Password } },
                    PostData = JsonConvert.SerializeObject(payoutRequestInput)
                };
                try
                {
                    var paymentRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (paymentRequestOutput.Payload != null && paymentRequestOutput.Payload.Any())
                    {
                        if (paymentRequestOutput.Payload[0].TotalFees != null)
                        {
                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            parameters.Add("Amount", paymentRequestOutput.Payload[0].TotalFees.Amount);
                            parameters.Add("ExchangeRate", paymentRequestOutput.Payload[0].ExchangeRate);
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        }
                        paymentRequest.ExternalTransactionId = paymentRequestOutput.Payload[0].TransactionReference;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    }
                }
                catch (Exception ex)
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    using (var documentBll = new DocumentBll(paymentSystemBl))
                    {
                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, ex.Message,
                                                            null, null, false, string.Empty, documentBll, notificationBl);
                    }
                    throw;
                }
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                };
            }
        }
    }
}