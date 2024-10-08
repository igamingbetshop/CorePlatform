using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using System.Net;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Models.CryptonPay;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Net.Http.Headers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class CryptonPayController : ApiController
    {
        [HttpPost]
        [Route("api/CryptonPay/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (paymentRequest.Status != (int)PaymentRequestStates.Pending)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.CryptonPayApiUrl);
                    var processPaymentInput = new
                    {
                        merchantId = partnerPaymentSetting.UserName,
                        merchantTransactionId = paymentRequest.Id.ToString(),
                        merchantUserId = client.Id.ToString(),
                        merchantUserEmail  = client.Email,
                        merchantUserMobile = client.MobileNumber,
                        amount = (int)(paymentRequest.Amount * 100),
                        currency = client.CurrencyId,
                        pan = input.CardNumber,
                        expirydate = new DateTime(Convert.ToInt32(input.ExpiryYear), Convert.ToInt32(input.ExpiryMonth), 1).ToString("MM/yyyy"),
                        securitycode = input.VerificationCode,
                        redirectUrlSuccess = input.RedirectUrl,
                        redirectUrlFail= input.RedirectUrl
                    };

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        RequestHeaders = new Dictionary<string, string> { { "api-key", partnerPaymentSetting.Password } },
                        Url = url,
                        PostData = CommonFunctions.GetUriDataFromObject(processPaymentInput)
                    };
                    WebApiApplication.DbLogger.Info("httpRequestInput: " +  JsonConvert.SerializeObject(httpRequestInput));

                    try
                    {
                        var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        result.RedirectUrl = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _)).TransactionUrl;
                        if (string.IsNullOrEmpty(result.RedirectUrl))
                            throw new Exception(res);
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                        {
                            clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, ex.Message, notificationBl);                            
                            throw;
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);

                result.StatusCode = ex.Detail.Id;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                result.StatusCode = Constants.Errors.GeneralException;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8)
            };
        }


        [HttpPost]
        [Route("api/CryptonPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "SUCCESS";
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("Input: " +  JsonConvert.SerializeObject(input));

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantTransactionId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {                                
                                if (input.Status.Code.ToUpper() == "SUCCESS")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds, input.Status.Message);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                }
                                else
                                {
                                    var comment = $"Status: {input.Status.Code}, Message: {input.Status.Message}";
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, comment, notificationBl);
                                }
                            }
                            else
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(new Exception(response));
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
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