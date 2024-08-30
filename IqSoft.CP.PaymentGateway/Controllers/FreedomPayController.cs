using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Models.FreedomPay;
using System.Linq;
using System.IO;
using System.Web;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class FreedomPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.FreedomPay);

        [HttpPost]
        [Route("api/FreedomPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            try
            {
                // BaseBll.CheckIp(WhitelistedIps); ??
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info(inputString);
                if (!Request.Headers.Contains("X-Notification-Secret"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.order_id)) ??
                               throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                 if (Request.Headers.GetValues("X-Notification-Secret").FirstOrDefault() != partnerPaymentSetting.Password)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(request.Info) ? request.Info : "{}");
                    paymentInfo.BankBranchName = input.card_brand;
                    paymentInfo.CardNumber = input.card_number;
                    paymentInfo.ExpiryDate = $"{input.expiry?.month}/{input.expiry.year}";
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    request.ExternalTransactionId = input.order_id; // ?????????
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    var amount = input.amount;
                    if (client.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                         JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                        amount = Math.Round(Convert.ToDecimal(parameters["AppliedRate"]) * input.amount, 2);
                    }

                    if (request.Amount != amount)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestInValidAmount);

                    if (input.code == "00" && input.state.Replace(" ", string.Empty).ToUpper() == "TRANSACTIONSUCCESS")
                    {
                        clientBl.ApproveDepositFromPaymentSystem(request, false);
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                    }
                    else if(input.state.Replace(" ", string.Empty).ToUpper() == "TRANSACTIONFAILED")
                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.failure, notificationBl);
                    response = "OK";
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}