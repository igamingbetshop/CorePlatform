using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Interkassa;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class InterkassaController : ApiController
    {
        [Route("api/Interkassa/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));
                var dict = HttpUtility.ParseQueryString(inputString);
                var x = dict.AllKeys.ToDictionary(k => k, k => dict[k]);
                
                var input = JsonConvert.DeserializeObject<PaymentInput>(JsonConvert.SerializeObject(x));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.ik_pm_no));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            if (partnerPaymentSetting == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                            var orderdParams = CommonFunctions.GetSortedValuesAsString(input, ":");

                            using (SHA256 sha256Hash = SHA256.Create())
                            {
                                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(orderdParams + ":" + partnerPaymentSetting.Password));
                                var sign = Convert.ToBase64String(bytes);
                                if (sign != input.ik_sign.Trim())
                                {
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                }
                            }
                            paymentRequest.Info = JsonConvert.SerializeObject(input);
                            paymentRequest.ExternalTransactionId = input.ik_inv_id;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            if (input.ik_inv_st == "success")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                            }
                            else
                            {
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, null, notificationBl);
                            }
                            response = "OK";
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                          ex.Detail.Id == Constants.Errors.RequestAlreadyPayed || ex.Detail.Id == Constants.Errors.CanNotCancelPayedRequest))
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
            return httpResponseMessage;
        }
    }
}