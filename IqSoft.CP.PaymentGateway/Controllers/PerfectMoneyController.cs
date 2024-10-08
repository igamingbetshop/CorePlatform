using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.PerfectMoney;
using IqSoft.CP.Common.Helpers;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.Common.Models;
using System.Linq;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PerfectMoneyController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.PerfectMoney);

        [HttpPost]
        [Route("api/PerfectMoney/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentRequestInput input)
        {
            var response = string.Empty;
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                        BaseBll.CheckIp(WhitelistedIps);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.PAYMENT_ID));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(request.ClientId.Value);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var segment = clientBl.GetClientPaymentSegments(request.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                        var passphrase = segment == null ? partnerPaymentSetting.Password : segment.ApiKey.Split('/')[1];
                        if (!string.IsNullOrEmpty(passphrase))
                        {
                            var sign = input.V2_HASH.ToLower();
                            input.V2_HASH = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}",
                                input.PAYMENT_ID, input.PAYEE_ACCOUNT, input.PAYMENT_AMOUNT.ToString("0.##"),
                                input.PAYMENT_UNITS, input.PAYMENT_BATCH_NUM, input.PAYER_ACCOUNT,
                                CommonFunctions.ComputeMd5(passphrase).ToUpper(), input.TIMESTAMPGMT);

                            input.V2_HASH = CommonFunctions.ComputeMd5(input.V2_HASH);
                            if (sign != input.V2_HASH.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        }
                        request.ExternalTransactionId = input.PAYMENT_BATCH_NUM;
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                        paymentInfo.PayerAccount = input.PAYER_ACCOUNT;
                        paymentInfo.PayeeAccount = input.PAYEE_ACCOUNT;
                        paymentInfo.BatchNumber = input.PAYMENT_BATCH_NUM;

                        request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });
                        paymentSystemBl.ChangePaymentRequestDetails(request);
                        var pInfo = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
                        {
                            Type = (int)ClientPaymentInfoTypes.Wallet,
                            WalletNumber = input.PAYER_ACCOUNT,
                            PartnerPaymentSystemId = request.PartnerPaymentSettingId,
                            CreationTime = request.CreationTime,
                            LastUpdateTime = request.LastUpdateTime,
                            ClientId = request.ClientId.Value,
                            AccountNickName = Constants.PaymentSystems.PerfectMoneyWallet
                        });
                        clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds, string.Empty, pInfo);
                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                        BaseHelpers.BroadcastBalance(request.ClientId.Value);
                        response = "OK";

                        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
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
