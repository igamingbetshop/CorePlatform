using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Models.GatewayPay;
using IqSoft.CP.Common.Models;
using System.IO;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class GatewayPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Pay4Fun);
        [HttpPost]
        [Route("api/GatewayPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            var inputString = bodyStream.ReadToEnd();
            WebApiApplication.DbLogger.Info("inputString: " + inputString);
            try
            {
              //  BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.Transaction.CustomerOrderId)) ??
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(request.Info) ? request.Info : "{}");
                            paymentInfo.CardNumber = input.Data.Card.CardNo;
                            paymentInfo.ExpiryDate = $"{input.Data.Card.ExpiryMonth}/{input.Data.Card.ExpiryYear}";
                            request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            if(input.Data.Transaction.Amount != request.Amount)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);

                            if (input.ResponseCode == 1)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false, comment: input.ResponseMessage);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.ResponseCode == 0 || input.ResponseCode == 3)
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.ResponseMessage, notificationBl);
                            else
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Pending, input.ResponseMessage, notificationBl);
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
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}