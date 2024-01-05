using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Capital;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class CapitalController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Capital);
        [HttpGet]
        [HttpPost]
        [Route("api/Capital/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            var responseString = string.Empty;
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var partnerBl = new PartnerBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var request = paymentSystemBl.GetPaymentRequestById(input.RequestId);
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
                                if (paymentSystem.Name.ToLower() != input.PaymentMethod.ToLower())
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                if (input.Status.ToUpper() == "APPROVED")
                                {
                                    request.ExternalTransactionId = input.OrderId;

                                    if (paymentSystem.Name == Constants.PaymentSystems.Capital)
                                    {
                                        if (client.CurrencyId != input.Currency)
                                            input.Amount = BaseBll.ConvertCurrency(input.Currency, client.CurrencyId, input.Amount);
                                        request.Amount = input.Amount;
                                    }
                                    paymentSystemBl.ChangePaymentRequestDetails(request);
                                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                }
                                else if (input.Status.ToUpper() == "DECLINED")
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.ProviderResponse, notificationBl);
                                responseString = JsonConvert.SerializeObject(new { status = "SUCCES" });
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
                WebApiApplication.DbLogger.Error(string.Format("Input: {0}, Error: {1}", JsonConvert.SerializeObject(input), exp));
                responseString = JsonConvert.SerializeObject(new { status = "FAILED", message = exp });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(string.Format("Input: {0}, Error: {1}", JsonConvert.SerializeObject(input), ex));
                responseString = JsonConvert.SerializeObject(new { status = "FAILED", message = ex.Message });
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(responseString, Encoding.UTF8) };
        }
    }
}