using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.FreeKassa;
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
    public class FreeKassaController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.FreeKassa);
        [HttpPost]
        [Route("api/FreeKassa/ApiRequest")]
        public HttpResponseMessage ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        BaseBll.CheckIp(WhitelistedIps);
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                        var request = paymentSystemBl.GetPaymentRequestById(input.MERCHANT_ORDER_ID);
                        if (request == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var signature = CommonFunctions.ComputeMd5(string.Format("{0}:{1}:{2}:{3}", partnerPaymentSetting.UserName,
                            input.AMOUNT.ToString(".##"), partnerPaymentSetting.Password.Split('/')[1], request.Id));
                        if (signature.ToLower() != input.SIGN.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                        request.ExternalTransactionId = input.intid.ToString();
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
