using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.PaymentGateway.Models.CartToCard;
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
using IqSoft.CP.PaymentGateway.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    public class CardToCardController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.CardToCard);

        [HttpGet]
        [Route("api/CardToCard/ApiRequest")]
        public HttpResponseMessage ApiRequest(int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var partnerBl = new PartnerBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                            if (request == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CardToCard);
                            var url = partnerBl.GetPaymentValueByKey(client.PartnerId, paymentSystem.Id, Constants.PartnerKeys.CardToCardApiUrl);
                            var verifyInput = new
                            {
                                payment_id = request.Id
                            };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}/c2c/verifyPay", url),
                                PostData = CommonFunctions.GetUriEndocingFromObject(verifyInput)
                            };
                            var res = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            if (res.Code == 0)
                            {
                                request.ExternalTransactionId = string.Format("{0}_{0}", request.Id);
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else
                                throw new Exception(res.Message);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("OK", Encoding.UTF8) };
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
