using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.PaymentGateway.Models.Runpay;
using System.Web.Http;
using IqSoft.CP.Common.Models;
using System.Security.Cryptography;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class RunpayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };
        [HttpPost]
        [Route("api/Runpay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //   BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.LMI_PAYMENT_NO));
                        if (paymentRequest == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                           client.CurrencyId, paymentRequest.Type);
                        var hash = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}", input.LMI_MERCHANT_ID, input.LMI_PAYMENT_NO, input.LMI_SYS_PAYMENT_ID,
                            input.LMI_SYS_PAYMENT_DATE, input.LMI_PAYMENT_AMOUNT, input.LMI_CURRENCY, input.LMI_PAID_AMOUNT, input.LMI_PAID_CURRENCY,
                            input.LMI_PAYMENT_SYSTEM, input.LMI_SIM_MODE, partnerPaymentSetting.Password);
                        var bytesToSign = Encoding.UTF8.GetBytes(hash);
                        using (var hashAlgorithm = SHA256.Create())
                        {
                            byte[] hashBytes = hashAlgorithm.ComputeHash(bytesToSign);
                            hash = Convert.ToBase64String(hashBytes);
                        }
                        if (hash.ToLower() != input.LMI_HASH.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.NotAllowed);
                        if (input.LMI_MERCHANT_ID != input.LMI_MERCHANT_ID)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                        paymentInfo.WalletNumber = input.LMI_PAYER_IDENTIFIER;
                        paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
                        paymentRequest.ExternalTransactionId = input.LMI_SYS_PAYMENT_ID;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                        response = JsonConvert.SerializeObject(new { status = "OK" });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = JsonConvert.SerializeObject(new { status = "OK" });
                }
                response = JsonConvert.SerializeObject(new { status = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.Message });

                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = JsonConvert.SerializeObject(new { status = ex.Message });
                WebApiApplication.DbLogger.Error(response);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}