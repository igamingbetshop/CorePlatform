using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Transact365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;

namespace IqSoft.CP.PaymentGateway.Controllers
{

    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class Transact365Controller : ApiController
    {

        [Route("api/Transact365/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));
            var dict = HttpUtility.ParseQueryString(inputString);
            var json = new JavaScriptSerializer().Serialize(dict.AllKeys.ToDictionary(k => k, k => dict[k]));
            var input = JsonConvert.DeserializeObject<PaymentInput>(json);
            var response = "OK";
            var userIds = new List<int>();
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                if (input.Key != partnerPaymentSetting.UserName)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                                var param = new
                                {
                                    amount = input.Amount,
                                    auth_key = input.Key,
                                    auth_timestamp = input.Timestamp,
                                    auth_version = input.Version,
                                    currency = input.Currency,
                                    order_id = input.OrderId,
                                    status = input.Status,
                                    statuscode = input.StatusCode,
                                    statusmessage = input.StatusMessage,
                                    trans_id = input.TransId
                                };
                                var data = CommonFunctions.GetUriDataFromObject(param);
                                var hash = string.Format("{0}{1}", "POST\npdapi\n", data);
                                var signature = CommonFunctions.ComputeHMACSha256(hash, partnerPaymentSetting.Password).ToLower();
                                if (input.Signature.ToLower() != signature)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                if (input.Status.ToLower() == "ok")
                                {
                                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                                    {
                                        clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds);
                                    }
                                    else
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, request.Parameters, documentBl, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    }
                                }
                                else if (input.Status.ToLower() == "notok")
                                {
                                    var statusDescription = $"Error: {input.Status} {input.StatusCode} {input.StatusMessage}";
                                    if (request.Type == (int)PaymentRequestTypes.Deposit)
                                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, statusDescription, notificationBl);
                                    else
                                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, statusDescription, null, null, false,
                                                                            request.Parameters, documentBl, notificationBl, out userIds);
                                }
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response = "OK";
                    else if (ex.Detail != null && ex.Detail.Id == Constants.Errors.WrongHash) //?? 
                    {
                        response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    }
                    }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
        }
    }
}