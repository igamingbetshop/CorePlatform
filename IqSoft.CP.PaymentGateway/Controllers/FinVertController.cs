using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.FinVert;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class FinVertController : ApiController

    {
        enum ResponseCodes
        {
            Fail,
            Success,
            Pending,
            Cancelled,
            ToBeConfirm,
            Blocked,
            Unathorized,
            Redirected
        }
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.FinVert);

        [HttpPost]
        [Route("api/FinVert/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("FinVertCallBack : "+ JsonConvert.SerializeObject(input));
               
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.Transaction.CustomerOrderId));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {                               
                                if (Enum.TryParse(input.ResponseCode, out ResponseCodes responseCode))
                                {
                                    if (responseCode == ResponseCodes.Success)
                                    {
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                        foreach (var uId in userIds)
                                        {
                                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                        }
                                    }
                                    else if (responseCode == ResponseCodes.Fail || responseCode == ResponseCodes.Blocked)
                                    {
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                                            string.Format("ErrorMessage: {0}", input.ResponseMessage), notificationBl);
                                    }
                                }                                                               
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