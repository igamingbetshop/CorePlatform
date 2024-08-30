using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.PaymentGateway.Models.Omno;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Text;
using System.Net.Http.Headers;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OmnoController : ApiController
    {
        [HttpPost]
        [Route("api/Omno/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info("api/Omno/ApiRequest "  + inputString);
            var response = "OK";
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var partnerBl = new PartnerBll(paymentSystemBl))
                    {
                        var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                        var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId)) ??
                           throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                           client.CurrencyId, paymentRequest.Type);
                        paymentRequest.ExternalTransactionId = input.Id;
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                        using (var clientBl = new ClientBll(paymentSystemBl))
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            if (input.Status.ToUpper() == "SUCCESS")
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                            else if (input.Status.ToUpper() == "DECLINED" || input.Status.ToUpper() == "TIMEOUT" || input.Status.ToUpper() == "PENDING3DS")
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Status, notificationBl);
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                            BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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