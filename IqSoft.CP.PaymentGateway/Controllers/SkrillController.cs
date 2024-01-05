using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.PaymentGateway.Models.Skrill;
using System.Collections.Generic;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using System.Net.Http;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.Text;
using System.Net;
using System;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class SkrillController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            ""
        };
        [HttpPost]
         [Route("api/Skrill/ApiRequest")]
         public HttpResponseMessage ApiRequest(SkrillRequestStatus input)
         {
            var response = string.Empty;
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            //BaseBll.CheckIp(WhitelistedIps);
                            var request = paymentSystemBl.GetPaymentRequestById(input.transaction_id);
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);

                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var key = CommonFunctions.ComputeMd5(CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.SkrillSecurKey).StringValue).ToUpper();
                            var signString = string.Format("{0}{1}{2}{3}{4}{5}", input.merchant_id, input.transaction_id, key, input.mb_amount, input.mb_currency, input.Status);
                            signString = CommonFunctions.ComputeMd5(signString);
                            if (signString.ToLower() != input.Md5Sig.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            if (input.Status == SkrillHelpers.StatusCodes.Confirmed)
                            {
                                request.ExternalTransactionId = input.mb_transaction_id;
                                paymentSystemBl.ChangePaymentRequestDetails(request);
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                response = "OK";
                            }
                            else if (input.Status == SkrillHelpers.StatusCodes.CanceledByClient || input.Status == SkrillHelpers.StatusCodes.Failed)
                            {
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, input.failed_reason_code, notificationBl);
                                response = "Failed";
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
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("OK", Encoding.UTF8) };

                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response = ex.Message;
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}
