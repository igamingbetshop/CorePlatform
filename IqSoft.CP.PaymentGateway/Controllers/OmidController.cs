using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.Omid;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Text;
using IqSoft.CP.DAL.Models.Cache;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class OmidController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "?"//cup distribution ip
        };

        [HttpGet]
        [Route("api/Omid/ApiRequest")]
        public HttpResponseMessage ApiRequest(int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                //   BaseBll.CheckIp(WhitelistedIps);
                var userIds = new List<int>();
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var partnerBl = new PartnerBll(paymentSystemBl))
                        {
                            using (var documentBl = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(documentBl))
                                {
                                    var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                                    if (request == null)
                                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                    var client = CacheManager.GetClientById(request.ClientId.Value);
                                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                                request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.OmidApiUrl).StringValue;
                                    var amount = request.Amount - (request.CommissionAmount ?? 0);
                                    var paymentRequestInput = new
                                    {
                                        mid = partnerPaymentSetting.UserName,
                                        amount = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.IranianRial, Convert.ToInt32(amount))),
                                        authority = request.ExternalTransactionId
                                    };
                                    var httpRequestInput = new HttpRequestInput
                                    {
                                        RequestMethod = Constants.HttpRequestMethods.Get,
                                        Url = string.Format("{0}/trs/webservice/verifyRequest?params={1}", url, JsonConvert.SerializeObject(paymentRequestInput))
                                    };
                                    var result = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                                    if (result.Error && result.Result == request.ExternalTransactionId)
                                    {
                                        clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds);
                                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                    }
                                    else
                                    {
                                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                            result.Message, null, null, false, string.Empty, documentBl, notificationBl, out userIds);
                                    }
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
                                }
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
