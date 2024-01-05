using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.Flexepin;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using System.Linq;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class FlexepinController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.Flexepin);
        [HttpPost]
        [Route("api/Flexepin/ApiRequest")]
        public HttpResponseMessage RedeemVoucher(PaymentInput input)
        {
            var response = new BaseData();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.PaymentRequestId));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    if (client.Id.ToString() != input.CustomerId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (input.VoucherPins!= null && input.VoucherPins.Any())
                    {
                        var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                             JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                        foreach (var v in input.VoucherPins)
                            if (!parameters.ContainsKey(v.VoucherPin))
                                parameters.Add(v.VoucherPin, v.VoucherSerialNumber);
                        paymentRequest.ExternalTransactionId = input.TransactionNo;
                        paymentRequest.Amount = input.VoucherValue; // check amount
                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, string.Empty);
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                                                           $"Code: {input.ResultCode}, Desc: {input.ResultDescription}", notificationBl);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.ResultCode = ex.Detail.Id;
                response.ResultDescription = ex.Detail.Message;
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response.ResultCode = Constants.Errors.GeneralException;
                response.ResultDescription = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8) };
        }
    }
}