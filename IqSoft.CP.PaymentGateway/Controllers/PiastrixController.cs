using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Piastrix;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PiastrixController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Piastrix);

        [HttpPost]
        [Route("api/Piastrix/ApiRequest")]
        public HttpResponseMessage ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var notificationBl = new NotificationBll(paymentSystemBl))
                    {
                        try
                        {
                            BaseBll.CheckIp(WhitelistedIps);
                            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.shop_order_id));
                            if (request == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            if (client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var signature = CommonFunctions.GetSortedValuesAsString(input, ":") +
                                                                                 partnerPaymentSetting.Password;
                            signature = CommonFunctions.ComputeSha256(signature);
                            if (signature.ToLower() != input.sign.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                            if (input.status.ToLower() == "success")
                            {
                                request.ExternalTransactionId = input.payment_id.ToString();
                                if (!string.IsNullOrEmpty(input.payer_id))
                                {
                                    var inp = !string.IsNullOrEmpty(request.Parameters) ?
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters) : new Dictionary<string, string>();
                                    inp.Add("payer_id", input.payer_id);
                                    request.Parameters = JsonConvert.SerializeObject(inp);
                                }
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                response = "OK";
                                clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                            }
                            else if (input.status.ToLower() == "rejected")
                            {
                                request.ExternalTransactionId = input.payment_id.ToString();
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                response = "OK";
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);

                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                            }
                            else
                            {
                                response = "Error";
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
                            }
                        }
                        catch (FaultException<BllFnErrorType> ex)
                        {
                            if (ex.Detail != null &&
                                (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                            {
                                response = "OK";
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
                            }
                            var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                            WebApiApplication.DbLogger.Error(exp);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(ex.Message, Encoding.UTF8) };
                        }
                    }
                }
            }
        }
    }
}