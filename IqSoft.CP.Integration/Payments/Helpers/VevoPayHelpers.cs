using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.VevoPay;
using System.Text;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Integration.Payments.Models;
using System.Linq;
using System.Globalization;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class VevoPayHelpers
    {
        private static Dictionary<string, string> PaymentMethods { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.VevoPayPapara, "papara" },
            { Constants.PaymentSystems.VevoPayHavale, "havale" },
            { Constants.PaymentSystems.VevoPayMefete, "mefete" },
            { Constants.PaymentSystems.VevoPayKreditCard, "kredikarti" },
            { Constants.PaymentSystems.VevoPayPayfix, "payfix" },
            { Constants.PaymentSystems.VevoPayParazula, "parazula" },
            { Constants.PaymentSystems.VevoPayFups, "fups" },
            { Constants.PaymentSystems.VevoPayCmt, "cmt" },
            { Constants.PaymentSystems.VevoPayPep, "pep" }
        };
       
        public static string CallVevoPayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentMethods.ContainsKey(paymentSystem.Name))
                    BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);

                var client = CacheManager.GetClientById(input.ClientId.Value);
                var url =  CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentSettingTypes.Deposit).Password.Split(',')[1];
                var apiKey = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,input.CurrencyId, (int)PaymentSettingTypes.Deposit).UserName;
                var paymentInput = JsonConvert.SerializeObject(new
                {
                    islem = "iframeolustur",
                    firma_key = apiKey,
                    kullanici_isim = $"{client.FirstName} {client.LastName}",
                    kullanici_id = client.Id,
                    referans = input.Id,
                    yontem = PaymentMethods[paymentSystem.Name]
                });

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Date = DateTime.Now.ToUniversalTime(),
                    Url = url,
                    PostData = paymentInput
                };
                log.Info($"PostData: {httpRequestInput.PostData}");
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                log.Info($"Response: {paymentOutput}");               
                if (paymentOutput.ApiQuery.ToUpper() == "HATALI")
                    throw new Exception(string.Format("ApiQuery: {0}, ErrorMessage: {1}", paymentOutput.ApiQuery,
                                         paymentOutput.ErrorMessage));
                return paymentOutput.IframeInfo.RedirectUrl;
            }
        }
        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    if (!PaymentMethods.ContainsKey(paymentSystem.Name))
                        BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);

                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                       input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var url = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentSettingTypes.Withdraw).Password.Split(',')[1];
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var apiKey = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentSettingTypes.Withdraw).UserName;
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var payoutInput = JsonConvert.SerializeObject(new
                    {
                        Process = "Withdrawal",
                        firma_key = apiKey,
                        UserID = client.Id,
                        NameSurname = $"{client.FirstName} {client.LastName}",                        
                        BankAccountNo = paymentInfo.Info,
                        Amount = amount.ToString(CultureInfo.InvariantCulture),
                        Method = PaymentMethods[paymentSystem.Name],
                        Reference = input.Id
                    });
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = payoutInput
                    };
                    log.Info($"PostData: {httpRequestInput.PostData}");
                    var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    log.Info($"Response: {res}");
                    var output = JsonConvert.DeserializeObject<PayoutOutput>(res);
                    if (output.ApiStatus.ToUpper() == "ERROR")
                    {
                        throw new Exception(string.Format("ApiStatus: {0}, Message: {1}", output.ApiStatus,
                                         output.Message.En));
                    }
                   
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                        Description = output.Message.En
                    };
                }
            }
        }
    }
}
