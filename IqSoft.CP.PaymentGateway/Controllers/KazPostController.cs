using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.KazPost;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class KazPostController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.KazPost);

        [HttpGet]
        [Route("api/KazPost/ApiRequest/orderId")]
        public HttpResponseMessage ApiRequest(string orderId)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        WebApiApplication.DbLogger.Info("orderId=" + orderId);

                        BaseBll.CheckIp(WhitelistedIps);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(orderId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KazPostToken).StringValue;
                        var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.KazPostUrl).StringValue;
                        url = string.Format("{0}{1}{2}", url, " /api/v0/orders/payment/", request.ExternalTransactionId);
                        var requestHeaders = new Dictionary<string, string> { { "Authorization", "Token " + secretKey } };
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Get,
                            Url = url
                        };
                        var checkResponse = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        WebApiApplication.DbLogger.Info("checkResponse=" + JsonConvert.SerializeObject(checkResponse));
                        if (checkResponse.Success)
                        {
                            if (checkResponse.Result.Status == (int)KazPostHelpers.Statuses.Paid ||
                                checkResponse.Result.Status == (int)KazPostHelpers.Statuses.Confirmed)
                            {
								clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                        }
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(response, Encoding.UTF8)
                        };

                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new StringContent(response, Encoding.UTF8)
                            };
                        }
                        WebApiApplication.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Conflict,
                            Content = new StringContent(ex.Message, Encoding.UTF8)
                        };
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error(ex);
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Conflict,
                            Content = new StringContent(ex.Message, Encoding.UTF8)
                        };
                    }
                }
            }
        }
    }
}
