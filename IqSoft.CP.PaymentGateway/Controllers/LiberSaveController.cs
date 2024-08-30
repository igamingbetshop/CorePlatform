using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.LiberSave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class LiberSaveController : ApiController
    {
        [HttpPost]
        [Route("api/nixxe/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentEncryptedInput encryptedInput)
        {
            var response = "SUCCESS";
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);

                var byteArray = Convert.FromBase64String(encryptedInput.Data);
                var data = Encoding.UTF8.GetString(byteArray);
                var input = JsonConvert.DeserializeObject<PaymentInput>(data);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId)) ??
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                      client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password);
                    if (sign.ToLower() != encryptedInput.Sign.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    if (input.Amount != paymentRequest.Amount)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                    if (input.Currency != paymentRequest.CurrencyId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(paymentRequest.Info) ? paymentRequest.Info : "{}");
                    paymentInfo.WalletNumber = input.Email;
                    paymentRequest.Info = JsonConvert.SerializeObject(paymentInfo);
                    paymentRequest.ExternalTransactionId =  input.OrderId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (input.Status.ToUpper() == "SUCCESS")
                    {
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else if (input.Status.ToUpper() == "CANCELED")
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status, notificationBl);
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
                else
                {
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}