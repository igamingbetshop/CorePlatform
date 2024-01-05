using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.PremierCashier;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PremierCashierHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PremierCashierWallet, "wallet"},
            { Constants.PaymentSystems.PremierCashierCard, "card"},
            { Constants.PaymentSystems.PremierCashierCrypto, "crypto"},
            { Constants.PaymentSystems.PremierCashierECheck, "echeck"},
            { Constants.PaymentSystems.PremierCashierMoneyTransfer,   "moneytransfer"},
            { Constants.PaymentSystems.PremierCashierManual, "manual"},
        };

        public static string CallPremierCashierApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partner = CacheManager.GetPartnerById(client.Id);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, input.Type);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PremierCashierApiUrl).StringValue;
                var frontendTokens = partnerPaymentSetting.UserName.Split(',');
                var payoutCRM = new { order_id = input.Id.ToString() };
                var depositCRM = new
                {
                    order_id = input.Id.ToString(),
                    amount = (int)input.Amount
                };
                var inputJson = JsonConvert.SerializeObject(new
                {
                    frontend_id = Convert.ToInt32(frontendTokens[0]),
                    action = input.Type == (int)PaymentRequestTypes.Withdraw ? "PAYOUT" : "DEPOSIT",
                    pin = client.Id.ToString(),
                    lang = session.LanguageId,
                    customer = input.Type == (int)PaymentRequestTypes.Withdraw ? null : new
                    {
                        custname = $"{client.FirstName} {client.LastName}" ?? "NA",
                        street = client.Address  ?? "NA",
                        city = client.City ?? "NA",
                        country = session.Country,
                        phone = client.MobileNumber ?? "NA",
                        email = client.Email,
                        dob = client.BirthDate.ToString("MM-dd-yyyy"),
                        gender = "U"
                    },
                    crm = new
                    {
                        order_id = input.Id.ToString(),
                        amount = input.Type == (int)PaymentRequestTypes.Withdraw ? null : (int?)input.Amount
                    },
                    timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    tokenname = frontendTokens[1]
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var signature = CommonFunctions.ComputeHMACSha384(inputJson, partnerPaymentSetting.Password).ToLower();
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Cashier-Signature", signature } },
                    Url = $"{url}/createcashiersession",
                    PostData = inputJson
                };
                log.Debug(JsonConvert.SerializeObject(httpRequestInput));
                var resp = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (resp.Code != 0)
                    throw new Exception(resp.Message);
                return resp.Data.Url;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log, bool approve)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                using (var notificationBl = new NotificationBll(clientBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                        paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PremierCashierApiUrl).StringValue;
                    var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                    var frontendTokens = partnerPaymentSetting.UserName.Split(',');

                    var inputJson = JsonConvert.SerializeObject(new
                    {
                        frontend_id = Convert.ToInt32(frontendTokens[0]),
                        traceid = paymentRequest.ExternalTransactionId,
                        action = approve ? "approve" : "reject",
                        timestamp = (Int64)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                        tokenname = frontendTokens[1]
                    });
                    var signature = CommonFunctions.ComputeHMACSha384(inputJson, partnerPaymentSetting.Password).ToLower();
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        RequestHeaders = new Dictionary<string, string> { { "Cashier-Signature", signature } },
                        Url = $"{url}/authorizepayout",
                        PostData = inputJson
                    };
                    log.Info(JsonConvert.SerializeObject(httpRequestInput));

                    var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    log.Info(r);

                    var resp = JsonConvert.DeserializeObject<PaymentOutput>(r);


                    if (resp.Code != 0)
                        throw new Exception(resp.Message);
                    if (approve)
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding
                        };
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.CancelPending
                    };
                }
            }
        }
    }
}