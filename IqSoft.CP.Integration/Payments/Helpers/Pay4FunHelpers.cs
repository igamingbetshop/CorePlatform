using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Pay4Fun;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class Pay4FunHelpers
    {
        public static string CallPay4FunApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay4FunApiUrl).StringValue;
            var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var paymentRequestInput = new
            {
                amount = Math.Round(input.Amount, 2),
                merchantInvoiceId = input.Id,
                language = CommonHelpers.LanguageISOCodes[session.LanguageId],
                currency = client.CurrencyId,
                okUrl = cashierPageUrl,
                notOkUrl = cashierPageUrl,
                confirmationUrl = string.Format("{0}/api/Pay4Fun/ApiRequest", paymentGatewayUrl)
            };
            var merchantKeys = partnerPaymentSetting.Password.Split(',');
            if (merchantKeys.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidSecretKey);
            var merchantKey = merchantKeys[0];
            var merchantSecret = merchantKeys[1];
            var hash = string.Format("{0}{1}{2}{3}", partnerPaymentSetting.UserName, paymentRequestInput.amount * 100, input.Id, merchantSecret);
            var headers = new Dictionary<string, string>
                {
                    {"merchantId", partnerPaymentSetting.UserName },
                    {"hash", CommonFunctions.ComputeHMACSha256(hash, merchantKey) }
                };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = string.Format("{0}/1.0/wallet/process/", url),
                PostData = JsonConvert.SerializeObject(paymentRequestInput)
            };
            var resp = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (resp.Code != 201)
                throw new Exception(resp.Message);
            return resp.Url;
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay4FunApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var merchantKeys = partnerPaymentSetting.Password.Split(',');
                if (merchantKeys.Length != 2)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidSecretKey);
                var merchantKey = merchantKeys[0];
                var merchantSecret = merchantKeys[1];
                var hash = string.Format("{0}1{1}", partnerPaymentSetting.UserName, merchantSecret);
                var sign = string.Format("{0}{1}{2}{3}{4}", partnerPaymentSetting.UserName, Math.Truncate(amount * 100), client.CurrencyId,
                                                            paymentInfo.WalletNumber, merchantSecret);

                var payoutRequestInput = new
                {
                    amount,
                    currency = client.CurrencyId,
                    targetCustomerEmail = paymentInfo.WalletNumber,
                    merchantInvoiceId = paymentRequest.Id,
                    sign = CommonFunctions.ComputeHMACSha256(sign, merchantKey)
                };

                var headers = new Dictionary<string, string>
                {
                    {"merchantId", partnerPaymentSetting.UserName },
                    {"hash", CommonFunctions.ComputeHMACSha256(hash, merchantKey) }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/api/1.0/payout/payout/", url),
                    RequestHeaders = headers,
                    PostData = JsonConvert.SerializeObject(new List<object> { payoutRequestInput })
                };
                var paymentRequestOutput = JsonConvert.DeserializeObject<List<PayoutOutput>>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (paymentRequestOutput.Any())
                {
                    paymentRequest.ExternalTransactionId = paymentRequestOutput[0].TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                    if (paymentRequestOutput[0].Status.ToLower() == "confirmed")
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    throw new Exception(string.Format("Code: {0}, Error: {1}", paymentRequestOutput[0].Status, paymentRequestOutput[0].Message));
                }
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                };
            }
        }

        public static List<int> GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var userIds = new List<int>();
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Pay4FunApiUrl).StringValue;
            var merchantKeys = partnerPaymentSetting.Password.Split(',');
            if (merchantKeys.Length != 2)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidSecretKey);
            var merchantKey = merchantKeys[0];
            var merchantSecret = merchantKeys[1];
            var hash = string.Format("{0}{1}", partnerPaymentSetting.UserName, merchantSecret);

            var headers = new Dictionary<string, string>
                        {
                            {"merchantId", partnerPaymentSetting.UserName },
                            {"hash", CommonFunctions.ComputeHMACSha256(hash, merchantKey) }
                        };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/api/1.0/payout/transaction/", url),
                RequestHeaders = headers,
                PostData = JsonConvert.SerializeObject(new List<long> { paymentRequest.Id })
            };
            var res = JsonConvert.DeserializeObject<List<PayoutOutput>>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            var payoutOutput = res.FirstOrDefault(x => x.MerchantInvoiceId == paymentRequest.Id);
            if (payoutOutput != null)
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            if (payoutOutput.Status.ToLower() == "verified")
                            {
                                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                    string.Empty, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                            }
                            else if (payoutOutput.Status.ToLower() != "confirmed")
                            {
                                clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                                string.Format("Code: {0}, Message: {1}", payoutOutput.Status, payoutOutput.Message), null, null, 
                                false, string.Empty, documentBl, notificationBl, out userIds);
                            }
                        }
                    }
                }
            }
            return userIds;
        }
    }
}