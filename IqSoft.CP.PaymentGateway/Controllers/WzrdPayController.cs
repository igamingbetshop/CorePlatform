using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.WzrdPay;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net.Http.Headers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WzrdPayController : ApiController
    {


        [HttpPost]
        [Route("api/WzrdPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = "OK";
            #region Comm
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);


                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            using (var documentBl = new DocumentBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.Attributes.ReferenceId));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                                   client.CurrencyId, paymentRequest.Type);

                                if (input.Data.Attributes.Currency != paymentRequest.CurrencyId ||
                                 (paymentRequest.ExternalTransactionId != null && input.Data.Id != paymentRequest.ExternalTransactionId.ToString()))
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                                var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                                var originalId = input.Data.Attributes.ReferenceId;
                                if (!parameters.ContainsKey("OriginalTransactionId"))
                                    parameters.Add("OriginalTransactionId", originalId);
                                else
                                    parameters["OriginalTransactionId"] = originalId;

                                if (input.Data.Attributes.Status.ToLower() == "processed")
                                {
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                    {
                                        if (input.Data.Attributes.ProcessedAmount != paymentRequest.Amount)
                                        {
                                            if (!parameters.ContainsKey("ProcessedAmount"))
                                                parameters.Add("ProcessedAmount", input.Data.Attributes.ProcessedAmount.ToString());
                                            else
                                                parameters["ProcessedAmount"] = input.Data.Attributes.ProcessedAmount.ToString();
                                            if (!parameters.ContainsKey("InitialAmount"))
                                                parameters.Add("InitialAmount", paymentRequest.Amount.ToString());
                                            else
                                                parameters["InitialAmount"] = paymentRequest.Amount.ToString();
                                            paymentRequest.Amount = input.Data.Attributes.ProcessedAmount ?? paymentRequest.Amount;
                                        }
                                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                    }
                                    else
                                    {
                                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                                       string.Empty, null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    }
                                }
                                else if (input.Data.Attributes.Status.ToLower() == "expired" ||
                                         input.Data.Attributes.Status.ToLower() == "process_failed" ||
                                         input.Data.Attributes.Status.ToLower() == "canceled" ||
                                         input.Data.Attributes.Status.ToLower() == "refunded")
                                {
                                    if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Data.Attributes.Resolution, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Data.Attributes.Resolution,
                                                                            null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                response = "OK";
                            }
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
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            #endregion
            return httpResponseMessage;
        }
    }
}