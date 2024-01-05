using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.PaymentGateway.Models.Corefy;
using System.Linq;
using System.Security.Cryptography;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class CorefyController : ApiController
    {
        [HttpPost]
        [Route("api/Corefy/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                WebApiApplication.DbLogger.Info("inputString: " + inputString);

                if (!Request.Headers.Contains("X-Signature"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("X-Signature").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                WebApiApplication.DbLogger.Info("signature: " + signature);
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
                                var sign = ComputeSha1($"{partnerPaymentSetting.Password}{inputString}{partnerPaymentSetting.Password}");
                                if (sign.ToLower() != signature.ToLower())
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);

                                if (input.Data.Attributes.Currency != paymentRequest.CurrencyId ||
                                 (paymentRequest.ExternalTransactionId != null && input.Data.Id != paymentRequest.ExternalTransactionId.ToString()))
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                                var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                                var originalId = input.Data.Attributes.OriginalData?.OriginalId ?? input.Data.Attributes.OriginalId;
                                if (!parameters.ContainsKey("OriginalTransactionId"))
                                    parameters.Add("OriginalTransactionId", originalId);
                                else
                                    parameters["OriginalTransactionId"] = originalId;

                                if (input.Data.Attributes.Status.ToLower() == "processed")
                                {
                                    if (paymentRequest.Type== (int)PaymentRequestTypes.Deposit)
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
                                       string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    }
                                }
                                else if (input.Data.Attributes.Status.ToLower() == "expired" ||
                                         input.Data.Attributes.Status.ToLower() == "process_failed" ||
                                         input.Data.Attributes.Status.ToLower() == "terminated" ||
                                         input.Data.Attributes.Status.ToLower() == "canceled" ||
                                         input.Data.Attributes.Status.ToLower() == "refunded")
                                {
                                    if (paymentRequest.Type== (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Data.Attributes.Resolution, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Data.Attributes.Resolution,
                                                                            null, null, false, string.Empty, documentBl, notificationBl);
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
            return httpResponseMessage;
        }

        private static string ComputeSha1(string rawData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] bytes = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}