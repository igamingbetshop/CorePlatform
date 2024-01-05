using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Neteller;
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
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class NetellerController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Neteller);
        [HttpGet]
        [Route("api/Neteller/ApiRequest")]
        public HttpResponseMessage ApiRequest([FromUri]int paymentRequestId)
        {
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            try
                            {
                                BaseBll.CheckIp(WhitelistedIps);
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(paymentRequestId));
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var netellerApiUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NetellerApiUrl).StringValue;

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                                var requestHeaders = new Dictionary<string, string>
                                {
                                    { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(
                                                        string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password ))) }
                                };
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = Constants.HttpRequestMethods.Get,
                                    Url = string.Format("{0}/paymenthandles/{1}", netellerApiUrl, request.ExternalTransactionId),
                                    RequestHeaders = requestHeaders
                                };
                                var response = JsonConvert.DeserializeObject<PaymentRequestResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                                if (response.Status.ToUpper() == "COMPLETED")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                }
                                else if (response.Status.ToUpper() == "FAILED" || response.Status.ToUpper() == "EXPIRED")
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, response.Status, notificationBl);
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                                }
                                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                                WebApiApplication.DbLogger.Error(exp);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                            }
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict };
                        }
                    }
                }
            }
        }
    }
}
