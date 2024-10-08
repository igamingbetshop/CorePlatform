using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.EZeeWallet;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class EZeeWalletHelpers
    {
        public static string CallEZeeWalletApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                 JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Domain", session.Domain);
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                var paymentProcessingInput = new
                {
                    OrderId = input.Id,
                    RedirectUrl = cashierPageUrl,
                    ResponseUrl = string.Format("{0}/api/EZeeWallet/ProcessPaymentRequest", paymentGatewayUrl),
                    CancelUrl = string.Format("{0}/api/Payment/CancelPaymentRequest", paymentGatewayUrl),
                    input.Amount,
                    Currency = input.CurrencyId,
                    PartnerDomain = session.Domain,
                    ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                    session.LanguageId,
                    client.PartnerId,
                    PaymentSystemName = input.PaymentSystemName.ToLower(),
                    partnerPaymentSetting.MinAmount,
                    partnerPaymentSetting.MaxAmount
                };
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log, out List<int> userIds)
        {
            userIds = new List<int>();
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(documentBl))
                        {
                            var client = CacheManager.GetClientById(input.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.EZeeWalletUrl).StringValue;
                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                            if (string.IsNullOrEmpty(paymentInfo.WalletNumber))
                                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                            clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.PayPanding, string.Empty,
                                                                null, null, true, input.Parameters, documentBl, notificationBl, out userIds, false);
                            var paymentRequestInput = new
                            {
                                email = paymentInfo.WalletNumber,
                                amount = (int)((input.Amount - (input.CommissionAmount ?? 0))*100),
                                currency = client.CurrencyId,
                                merchant_reference = input.Id.ToString()
                            };
                            var byteArray = Encoding.Default.GetBytes(string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password));
                            var headers = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationJson,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}/single_payouts", url),
                                RequestHeaders = headers,
                                PostData = JsonConvert.SerializeObject(paymentRequestInput)
                            };
                            var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            var response = JsonConvert.DeserializeObject<PayoutOutput>(r);
                            if (!string.IsNullOrEmpty(response.ExternalTransactionId))
                            {
                                input.ExternalTransactionId = response.ExternalTransactionId;
                                paymentSystemBl.ChangePaymentRequestDetails(input);
                            }
                            if (response.Status.ToLower() == "succeeded")
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Approved
                                };
                            if (!string.IsNullOrEmpty(response.Code))
                            {
                                clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Failed,
                                    response.Message, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Failed,
                                };
                            }
                        }
                    }
                }
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed
                };
            }
        }
    }
}