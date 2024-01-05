using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.KazPost;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class KazPostHelpers
    {
        public static string CallKazPostApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.KazPost);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                    input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KazPostToken).StringValue;
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KazPostUrl).StringValue;
                url = string.Format("{0}{1}", url, " /api/v0/orders/payment/");
                var requestInput = new PaymentRequestInput
                {
                    Amount = input.Amount,
                    BackUrl = cashierPageUrl,
                    NotifyUrl = string.Format("{0}/{1}/{2}", paymentGateway, "api/KazPost/ApiRequest", input.Id),
                    Currency = input.CurrencyId
                };
                var requestHeaders = new Dictionary<string, string> { { "Authorization", "Token " + secretKey } };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(requestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Result != null)
                {
                    input.ExternalTransactionId = response.Result.TransactionId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return response.Result.Url;
                }
                return response.Errors.Description;
            }
        }


        /*

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Help2Pay);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerNotFound);
                    var now = DateTime.UtcNow.AddHours(8);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if (bankInfo == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.BankIsUnavailable);
                    //var bankCode = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.Help2PayWithdrawBankCode);
                    var secretKey = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.Help2PaySecretKey);
                    var url = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.Help2PayWithdrawUrl);

                    var requestInput = new PayoutRequestInput
                    {
                        ClientIp = session.LoginIp,
                        ReturnURI = partnerBl.GetPaymentValueByKey(null, null, Constants.PartnerKeys.PaymentGateway) + "/api/Help2Pay/PayoutResult",
                        MerchantCode = partnerPaymentSetting.UserName,
                        TransactionID = input.Id.ToString(),
                        CurrencyCode = input.CurrencyId,
                        MemberCode = client.Id.ToString(),
                        Amount = input.Amount.ToString("F"),
                        TransactionDateTime = now.ToString("yyyy-MM-dd hh:mm:sstt"),
                        BankCode = bankInfo.BankCode,
                        toBankAccountName = paymentInfo.ToBankName,
                        toBankAccountNumber = paymentInfo.ToBankNumber
                    };

                    var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                                      requestInput.MerchantCode, requestInput.TransactionID,
                                                      requestInput.MemberCode, requestInput.Amount,
                                                      requestInput.CurrencyCode, now.ToString("yyyyMMddHHmmss"),
                                                      requestInput.toBankAccountNumber, secretKey);

                    requestInput.Key = CommonFunctions.ComputeMd5(signature);


                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = CommonFunctions.GetUriEndocingFromObject(requestInput)
                    };
                    var deserializer = new XmlSerializer(typeof(PayoutOutput), new XmlRootAttribute("Payout"));
                    using (var stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput))
                    {
                        var output = (PayoutOutput)deserializer.Deserialize(stream);

                        var response = new PaymentResponse
                        {
                            Status = output.StatusCode == "000" ? PaymentRequestStates.Active : PaymentRequestStates.Failed
                        };
                        //response.Description = output.Message;
                        return response;
                    }
                }
            }
        }*/
    }
}
