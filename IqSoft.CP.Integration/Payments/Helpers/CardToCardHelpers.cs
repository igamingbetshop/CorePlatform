using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.CardToCard;
using log4net;
using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CardToCardHelpers
    {
        private static string GenerateSignature(object inputData)
        {
            var privateKey = "h4FSJ6T2HAYfStwX43DyvXnCAQoqB909SiDfK7zJiWLH4VCGE4uo_9HqsfF3twWWEtI0i8mtlt";
            var authKey = "sNRsufibwerpi4e";
            var sss = JsonConvert.SerializeObject(inputData);
            var base64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(inputData)));
            var checkSum = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(base64String) + privateKey);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(base64String + checkSum + authKey));
        }

        public static string CallCardToCardApiNew(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBll = new PaymentSystemBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(input.Amount)));
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var requestInput = new
                    {
                        desCardNo = paymentInfo.BankAccountNumber,
                        Amount = amount,
                        currency = Constants.Currencies.IranianRial,
                        customData1 = "0",
                        customData2 = "0",
                        callBackUrl = string.Format("{0}/{1}", partnerBl.GetPaymentValueByKey(client.PartnerId, null, Constants.PartnerKeys.PaymentGateway), "api/CardToCard/ApiRequest"),
                        cancelUrl = string.Format("{0}/user/1/deposit", session.Domain)
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = "http://c2cdirectpayment.nl/c2c/pool/ipg/",
                        PostData = "d="+GenerateSignature(requestInput)
                    };
                    var output = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    return "";
                }
            }
        }

        public static string CallCardToCardApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBll = new PaymentSystemBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var url = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CardToCardApiUrl);
                    var amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(input.Amount)));
                    var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                    if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                        distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                    var distUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
                    string.Format("{0}?orderId={1}&url={2}", "/CardToCard/NotifyResult", input.Id, cashierPageUrl);

                    var paymentRequestInput = new
                    {
                        token = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(input.Id.ToString()) + "BTF") + amount.ToString()),
                        amount,
                        callback_url = distUrl,
                        payment_id = input.Id
                    };
                    return string.Format("{0}/c2c?{1}", url, CommonFunctions.GetUriEndocingFromObject(paymentRequestInput));
                }
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
                    using (var currencyBl = new CurrencyBll(partnerBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        var url = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CardToCardApiUrl);
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                        var amount = input.Amount - (input.CommissionAmount ?? 0);
                        var payoutRequestInput = new PayoutRequestInput
                        {
                            Amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(amount))),
                            PaymentId = input.Id,
                            BankCardNumber = paymentInfo.BankAccountNumber,
                            BankACH = paymentInfo.BankACH,
                            BankAccountHolder = paymentInfo.BankAccountHolder
                        };
                        payoutRequestInput.Hash = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(
                            payoutRequestInput.BankCardNumber) + payoutRequestInput.Amount) + payoutRequestInput.PaymentId));
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format("{0}/pay/add2CheckoutList", url),
                            PostData = JsonConvert.SerializeObject(payoutRequestInput)
                        };
                        var result = JsonConvert.DeserializeObject<PayoutRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        if (result.Code == 0)
                        {
                            input.ExternalTransactionId = result.Data.ExternalId;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.PayPanding,
                            };
                        }
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Failed,
                            Description = result.Message
                        };
                    }
                }
            }
        }

        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var partnerBl = new PartnerBll(clientBl))
                {
                    using (var notificationBl = new NotificationBll(clientBl))
                    {
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.CardToCardApiUrl);
                        var date = DateTime.UtcNow;
                        var dateFormat = string.Format("{0}{0}-{1}{1}-{2}{2}", date.Year.ToString("d4"), date.Month.ToString("d2"), date.Day.ToString("d2"));
                        var checkStatusInput = new
                        {
                            verify_hash = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(
                                CommonFunctions.ComputeMd5(paymentRequest.Id.ToString()) + dateFormat) + paymentRequest.Id)),
                            ref_id = paymentRequest.Id
                        };
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format("{0}/pay/trackCheckoutRequest", url),
                            PostData = JsonConvert.SerializeObject(checkStatusInput)
                        };
                        var output = JsonConvert.DeserializeObject<PayoutRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        if (output.Data.Status == "0")
                        {
                            using (var documentBl = new DocumentBll(clientBl))
                            {
                                var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty, 
                                    null, null, false, string.Empty, documentBl, notificationBl);
                                clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                            }
                        }
                    }
                }
            }
        }
    }
}
