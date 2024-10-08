using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Neteller;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class NetellerHelpers
    {
        public static string CallNetellerApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NetellerApiUrl).StringValue;
                var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
                              string.Format("{0}?requestId={1}&redirectUrl={2}&notifyUrl={3}", "/Neteller/NotifyResult", input.Id, cashierPageUrl, notifyUrl);
                var paymentInput = new
                {
                    merchantRefNum = input.Id.ToString(),
                    transactionType = "PAYMENT",
                    paymentType = "NETELLER",
                    amount = Convert.ToInt32(input.Amount),
                    currencyCode = client.CurrencyId,
                    neteller = new
                    {
                        consumerId = client.Email,
                        consumerIdLocked = true
                    },
                    returnLinks = new List<object>
                        {
                            new {rel = "on_completed",href = distUrl },
                            new {rel = "on_failed",href = distUrl},
                            new  {rel = "default",href = distUrl}
                        }
                };
                var requestHeaders = new Dictionary<string, string> {
                    { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(
                     string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password ))) } };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}paymenthandles", url),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                var response = JsonConvert.DeserializeObject<OrderOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = response.ExternalTransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return response.Links[0].RedirectUrl;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NetellerApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var payoutRequestInput = new
                {
                    merchantRefNum = input.Id.ToString(),
                    transactionType = "STANDALONE_CREDIT",
                    paymentType = "NETELLER",
                    amount = Convert.ToInt32(amount),
                    currencyCode = client.CurrencyId,
                    neteller = new
                    {
                        consumerId = client.Email //paymentInfo.WalletNumber
                    },
                };

                var requestHeaders = new Dictionary<string, string> {
                    { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(
                     string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password ))) } };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/paymenthandles", url),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(payoutRequestInput)
                };
                var response = JsonConvert.DeserializeObject<OrderOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                input.ExternalTransactionId = response.ExternalTransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(input);

                if (response.Status.ToUpper() == "COMPLETED")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                    };
                else if (response.Status.ToUpper() == "CANCELLED" || response.Status.ToUpper() == "FAILED"
                    || response.Status.ToUpper() == "EXPIRED")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed
                    };

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding
                };
            }
        }

        public static List<int> GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var userIds = new List<int>();
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                        paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NetellerApiUrl).StringValue;
                    var requestHeaders = new Dictionary<string, string> {
                    { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(
                     string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password ))) } };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Get,
                        Url = string.Format("{0}/paymenthandles?merchantRefNum=135", url, paymentRequest.Id),
                        RequestHeaders = requestHeaders
                    };

                    var output = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    if (output.PaymentHandles.Any())
                    { 
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            if (output.PaymentHandles[0].Status.ToUpper() == "COMPLETED")
                            {
                                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                    output.PaymentHandles[0].StatusReason, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                            }
                            else if (output.PaymentHandles[0].Status.ToUpper() == "CANCELLED" ||
                                     output.PaymentHandles[0].Status.ToUpper() == "FAILED" ||
                                     output.PaymentHandles[0].Status.ToUpper() == "EXPIRED")
                            {

                                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                output.PaymentHandles[0].StatusReason, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                            }
                        }
                    }
                }
            }
            return userIds;
        }
    }
}