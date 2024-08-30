using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.GetaPay;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using System.Net;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class GetaPayController : ApiController
    {
        [HttpPost]
        [Route("api/GetaPay/ApiRequest")]
        public HttpResponseMessage PayoutRequest(HttpRequestMessage httpRequestMessage/*PayoutInput input*/)
        {
            var response = "OK";
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info("GetaPay inputString: " + inputString);
                var input = JsonConvert.DeserializeObject<PayoutInput>(inputString);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(clientBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId)) ??
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                                   client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                              
                                var elements = new List<string>
                                {
                                   input.Amount, 
                                   input.FullAmount, 
                                   input.AmountGate,
                                   input.PayoutId,
                                   input.StatusCode, 
                                   input.OrderId, 
                                   input.CurrencyGate,
                                   input.StatusDescription, 
                                   input.PayoutStatus, 
                                   input.Success
                                 };

                                elements.Sort(StringComparer.Ordinal);
                                var nonNullElements = elements.Where(e => !string.IsNullOrEmpty(e)).ToList();
                                var sortedValue = string.Join("|", nonNullElements);

                                var sign = CommonFunctions.HashHMACHex(sortedValue, partnerPaymentSetting.Password).ToLower();

                                if (sign.ToLower() != input.Signature.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                                if (input.PayoutStatus.ToLower() == "payout_completed")
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, input.PayoutStatus,
                                         null, null, false, request.Parameters, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                                else if (input.PayoutStatus.ToLower() == "payout_failed")
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                        input.PayoutStatus, null, null, false, request.Parameters, documentBll, notificationBl);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
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
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = ex.Message;
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}
