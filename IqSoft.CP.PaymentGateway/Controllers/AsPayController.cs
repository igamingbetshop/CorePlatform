using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.AsPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class AsPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.AsPay);

        [HttpPost]
        [Route("api/AsPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var status = "success";
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {

                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.ProviderTransactionId)) ??
                                  throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               client.CurrencyId, paymentRequest.Type);
                            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);

                            var hash = $"{partnerPaymentSetting.UserName}{partnerPaymentSetting.Password}{paymentRequest.ExternalTransactionId}";
                            if (CommonFunctions.ComputeSha256(hash).ToLower() != input.UniqueIdentifier.ToLower() ||
                                CommonFunctions.ComputeSha256(hash+ input.IsSuccess.ToString().ToLower()).ToLower() != input.Hash.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            if (input.IsSuccess)
                            {
                                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.AsPayApiUrl);
                                var token = AsPayHelpers.GetToken(partnerPaymentSetting.UserName, partnerPaymentSetting.Password, url);
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = Constants.HttpRequestMethods.Get,
                                    RequestHeaders = new Dictionary<string, string> { { "Token", token }, { "PKey", partnerPaymentSetting.Password } },
                                    Url = $"{url}/api/payquery/{paymentRequest.Id}" 
                                };
                                var paymentStatus = JsonConvert.DeserializeObject<PaymentStatus>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                                if (paymentStatus.Amount != paymentRequest.Amount)
                                {
                                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);

                                    if (parameters.ContainsKey("InitialAmount"))
                                        parameters["InitialAmount"] = paymentRequest.Amount.ToString("F");
                                    else
                                        parameters.Add("InitialAmount", paymentRequest.Amount.ToString("F"));
                                    if (parameters.ContainsKey("UpdatedAmount"))
                                        parameters["UpdatedAmount"] = paymentStatus.Amount.ToString("F");
                                    else
                                        parameters.Add("UpdatedAmount", paymentStatus.Amount.ToString("F"));
                                    paymentRequest.Amount = paymentStatus.Amount;
                                    paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                }

                                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                {
                                    if (paymentStatus.Status == 4)
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                    else if (paymentStatus.Status == 5)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentStatus.Status.ToString(), notificationBl);
                                }
                                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                {

                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                    {
                                        if (paymentStatus.Status == 4)
                                        {

                                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                           null, null, false, string.Empty, documentBll, notificationBl);
                                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);

                                        }
                                        else if (paymentStatus.Status == 5)
                                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentStatus.Status.ToString(),
                                                                                null, null, false, string.Empty, documentBll, notificationBl);
                                    }
                                }
                                Helpers.PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                            else
                            {
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, string.Empty, notificationBl);
                                else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                                {
                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                    {
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, string.Empty,
                                                                            null, null, false, string.Empty, documentBll, notificationBl);
                                        Helpers.PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (!(ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)))
                {
                    var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                    WebApiApplication.DbLogger.Error(exp);
                    status = exp.Message;
                }
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new { status }), Encoding.UTF8)
            };
        }
    }
}