using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.PaymentGateway.Models.Eway;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class EwayController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??" //distribution 
        };

        [HttpGet]
        [Route("api/Eway/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]int transactionId)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(transactionId);
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                            if (paymentSystem.Name != Constants.PaymentSystems.Eway)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.EwayApiUrl);
                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            if (!parameters.ContainsKey("AccessCode"))
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
                            var byteArray = Encoding.Default.GetBytes($"{partnerPaymentSetting.Password}:{partnerPaymentSetting.UserName}");
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = Constants.HttpRequestMethods.Get,
                                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } },
                                Url = string.Format("{0}/AccessCode/{1}", url, parameters["AccessCode"])
                            };
                            var result = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            WebApiApplication.DbLogger.Info("Payment result: " + result);
                            var paymentResultOutput = JsonConvert.DeserializeObject<PaymentResultOutput>(result);
                            if (string.IsNullOrEmpty(paymentResultOutput.TransactionID) || paymentResultOutput.ResponseCode != "00")
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, paymentResultOutput.ResponseMessage, notificationBl);
                            else if (paymentResultOutput.TransactionStatus)
                            {
                                paymentRequest.ExternalTransactionId = paymentResultOutput.TransactionID;
                                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            response = "OK";
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}