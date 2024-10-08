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
using IqSoft.CP.PaymentGateway.Models.PaymentAsia;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Net.Http.Headers;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class PaymentAsiaController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
           "??"
        };

        [HttpPost]
        [Route("api/PaymentAsia/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //   BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.merchant_reference));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                      paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
                            var hash = CommonFunctions.GetSortedParamWithValuesAsString(input, "&");
                            hash = CommonFunctions.ComputeSha512(hash + partnerPaymentSetting.Password);
                            if (hash.ToLower() != input.sign.ToLower())
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                            if (input.amount != paymentRequest.Amount || input.currency != paymentRequest.CurrencyId ||
                                input.merchant_reference != paymentRequest.Id.ToString())
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                            paymentRequest.ExternalTransactionId = input.request_reference;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (input.status == 1)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
                            else if (input.status == 2)
                            {
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.status.ToString(), notificationBl);
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
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("api/PaymentAsia/PayoutRequest")]
        public HttpResponseMessage PayoutRequest(PayoutRequestInput input)
        {
            var response = "Ok";
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var userIds = new List<int>();
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBl = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.request_reference));
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,paymentRequest.PaymentSystemId,
                                                                                                   client.CurrencyId, paymentRequest.Type);
                                var hash = GetSortedParamWithValuesAsString(input, "&");
                                hash = CommonFunctions.ComputeSha512(hash + partnerPaymentSetting.Password);
                                if (hash.ToLower() !=  input.sign.ToLower())
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                                if (Convert.ToDecimal(input.order_amount) != paymentRequest.Amount || input.order_currency!= paymentRequest.CurrencyId )
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongInputParameters);
                              
                                if (input.status == 1)
                                {
                                    var req = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                  null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                                }
                                else if (input.status == 2 )
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.status.ToString(), null, null, false,
                                                                        paymentRequest.Parameters, documentBl, notificationBl, out userIds);

                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail.Message;
                WebApiApplication.DbLogger.Error(new Exception(ex.Detail.Message));
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationUrlEncoded);
            return resp;
        }

        public static string GetSortedParamWithValuesAsString(object paymentRequest, string delimiter = "")
        {
            var sortedParams = new SortedDictionary<string, string>();
            var properties = paymentRequest.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(paymentRequest, null);
                if ( field.Name.ToLower().Contains("sign") || (value !=  null && value.ToString() == string.Empty) )
                    continue;
                sortedParams.Add(field.Name, value == null ? string.Empty : value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + HttpUtility.UrlEncode(par.Value) + delimiter);

            return string.IsNullOrEmpty(result) ? result : result.Remove(result.LastIndexOf(delimiter), delimiter.Length);
        }

    }
}