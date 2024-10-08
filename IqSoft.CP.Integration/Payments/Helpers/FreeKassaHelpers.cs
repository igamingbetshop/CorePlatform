 using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.FreeKassa;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class FreeKassaHelpers
    {
        public static Dictionary<string, int> DepositPaymentWays { get; set; } = new Dictionary<string, int>
        {
            { Constants.PaymentSystems.FreeKassaWallet+"RUB", 1 },
            //{ Constants.PaymentSystems.FreeKassaWallet+"USD", 2 },
            //{ Constants.PaymentSystems.FreeKassaWallet+"EUR", 3 },
            { Constants.PaymentSystems.FreeKassaCard+"RUB", 4 },
            { Constants.PaymentSystems.FreeKassaCard+"UAH", 7 },
            //{ Constants.PaymentSystems.FreeKassaCard+"USD", 32 },
            { Constants.PaymentSystems.FreeKassaMasterCard+"RUB", 8 },
            { Constants.PaymentSystems.FreeKassaMasterCard+"UAH", 9 },
            { Constants.PaymentSystems.FreeKassaYoomoney+"RUB", 6 },
            { Constants.PaymentSystems.FreeKassaQIWI+"RUB", 10 },
            { Constants.PaymentSystems.FreeKassaBitcoinCash+"RUB", 16 },
            { Constants.PaymentSystems.FreeKassaOnlineBank+"RUB", 13 },
            { Constants.PaymentSystems.FreeKassaSteamPay+"RUB", 27 },
            { Constants.PaymentSystems.FreeKassaPerfectMoney+"USD", 33 },
            { Constants.PaymentSystems.FreeKassaRipple+"RUB", 23 },
            { Constants.PaymentSystems.FreeKassaDash+"RUB", 18 },
            { Constants.PaymentSystems.FreeKassaLitecoin+"RUB", 25 },
            { Constants.PaymentSystems.FreeKassaEthereum+"RUB", 26 },
            { Constants.PaymentSystems.FreeKassaBitcoin+"RUB", 24 },
            { Constants.PaymentSystems.FreeKassaERC20+"USDT", 14 },
            { Constants.PaymentSystems.FreeKassaTRC20+"USDT", 15 },
            { Constants.PaymentSystems.FreeKassaMir+"RUB", 12 },
        };

        public static Dictionary<string, int> WithdrawPaymentWays { get; set; } = new Dictionary<string, int>
        {
            { Constants.PaymentSystems.FreeKassaQIWI+"RUB", 63 },
            { Constants.PaymentSystems.FreeKassaQIWI+"USD", 123 },
            { Constants.PaymentSystems.FreeKassaYandex+"RUB", 45 },
            { Constants.PaymentSystems.FreeKassaPayPal+"RUB", 70 },
            { Constants.PaymentSystems.FreeKassaBeeline+"RUB", 83 },
            { Constants.PaymentSystems.FreeKassaMTS+"RUB", 84 },
            { Constants.PaymentSystems.FreeKassaMegafon+"RUB", 82 },
            { Constants.PaymentSystems.FreeKassaCard+"RUB", 94 },
            { Constants.PaymentSystems.FreeKassaPerfectMoney+"USD", 64 },
            { Constants.PaymentSystems.FreeKassaPerfectMoney+"EUR", 69 },
            { Constants.PaymentSystems.FreeKassaCard+"KZT", 186 },
            { Constants.PaymentSystems.FreeKassaCard+"UAH", 67 },
            { Constants.PaymentSystems.FreeKassaTele2+"RUB", 132 },
            { Constants.PaymentSystems.FreeKassaPayeer+"RUB", 114 },
            { Constants.PaymentSystems.FreeKassaWallet+"RUB", 133 },
            { Constants.PaymentSystems.FreeKassaWebMoney+"RUB", 2 },
            { Constants.PaymentSystems.FreeKassaAdnvCash+"USD", 136 },
            { Constants.PaymentSystems.FreeKassaAdnvCash+"EUR", 183 },
            { Constants.PaymentSystems.FreeKassaAdnvCash+"RUB", 150 },
            { Constants.PaymentSystems.FreeKassaAdnvCash+"KZT", 184 },
            { Constants.PaymentSystems.FreeKassaQIWI+"EUR", 161 },
            { Constants.PaymentSystems.FreeKassaQIWI+"KZT", 162 },
            { Constants.PaymentSystems.FreeKassaExmo+"USD", 162 } 
        };

        public static string CallFreeKassaApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.FreeKassaApiUrl);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var currency = client.CurrencyId;
                var amount = input.Amount;
                if (!DepositPaymentWays.ContainsKey(paymentsystem.Name + client.CurrencyId))
                {
                    currency = Constants.Currencies.RussianRuble;
                    if (!DepositPaymentWays.ContainsKey(paymentsystem.Name + currency))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.RussianRuble);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.RussianRuble);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);             
                }
                var a = amount.ToString("0.##");
                var paymentRequestInput = new
                {
                    m = partnerPaymentSetting.UserName,
                    oa = a,
                    o = input.Id,
                    currency,
                    lang = session.LanguageId,
                    s = CommonFunctions.ComputeMd5(string.Format("{0}:{1}:{2}:{3}:{4}", partnerPaymentSetting.UserName, a,
                                                                                   partnerPaymentSetting.Password.Split('/')[0], currency, input.Id)),

                    i = DepositPaymentWays[paymentsystem.Name + currency]
                };

                return string.Format("{0}?{1}", url, CommonFunctions.GetUriEndocingFromObject(paymentRequestInput));
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                    input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.FreeKassaWithdrawApiUrl);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var purseData = string.Empty;
                switch (paymentsystem.Name)
                {
                    case Constants.PaymentSystems.FreeKassaWallet:
                    case Constants.PaymentSystems.FreeKassaPayeer:
                    case Constants.PaymentSystems.FreeKassaAdnvCash:
                    case Constants.PaymentSystems.FreeKassaExmo:
                        purseData = paymentInfo.WalletNumber;
                        break;
                    case Constants.PaymentSystems.FreeKassaCard:
                        purseData = paymentInfo.CardNumber;
                        break;
                    case Constants.PaymentSystems.FreeKassaQIWI:
                    case Constants.PaymentSystems.FreeKassaMTS:
                    case Constants.PaymentSystems.FreeKassaBeeline:
                    case Constants.PaymentSystems.FreeKassaTele2:
                        purseData = paymentInfo.MobileNumber;
                        break;
                    case Constants.PaymentSystems.FreeKassaAlfaBank:
                    case Constants.PaymentSystems.FreeKassaSberBank:
                        purseData = paymentInfo.BankAccountNumber;
                        break;
                    default:
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                };
                var currency = client.CurrencyId;
                var requestAmount = input.Amount - (input.CommissionAmount ?? 0);
                if (!WithdrawPaymentWays.ContainsKey(paymentsystem.Name + client.CurrencyId))
                {
                    currency = Constants.Currencies.RussianRuble;
                    if (!WithdrawPaymentWays.ContainsKey(paymentsystem.Name + currency))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.RussianRuble);
                    requestAmount *= rate;
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.RussianRuble);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
               
                var amount = requestAmount.ToString("0.##");
                var paymentWay = WithdrawPaymentWays[paymentsystem.Name + currency];
                var payoutRequestInput = new
                {
                    wallet_id = partnerPaymentSetting.UserName,
                    purse = purseData,
                    currency = paymentWay,
                    amount,
                    desc = partner.Name,
                    order_id = input.Id,
                    disable_exchange = 1,
                    check_duplicate = 1,
                    sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}", partnerPaymentSetting.UserName, paymentWay,
                    amount, purseData, partnerPaymentSetting.Password)),
                    action = "cashout"
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = CommonFunctions.GetUriDataFromObject(payoutRequestInput)
                };
                var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<PayoutOutput>(res);
                if (output.Status.ToLower() == "error")
                    throw new Exception(output.Description);
                var data = JsonConvert.DeserializeObject<PaymentData>(output.Data.ToString());
                input.ExternalTransactionId = data.PaymentId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                    Description = output.Description
                };
            }
        }

        public static List<int> GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var response = new List<int>();
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var notificationBl = new NotificationBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                        paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var url = partnerBl.GetPaymentValueByKey(null, null, Constants.PartnerKeys.FreeKassaWithdrawApiUrl);

                    var statusRequestInput = new
                    {
                        wallet_id = partnerPaymentSetting.UserName,
                        payment_id = paymentRequest.ExternalTransactionId,
                        sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", partnerPaymentSetting.UserName, paymentRequest.ExternalTransactionId, partnerPaymentSetting.Password)),
                        action = "get_payment_status"
                    };
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = CommonFunctions.GetUriDataFromObject(statusRequestInput)
                    };
                    var res = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var output = JsonConvert.DeserializeObject<PayoutOutput>(res);
                    if (output.Status.ToLower() == "error")
                        throw new Exception(output.Description);
                    var data = JsonConvert.DeserializeObject<PaymentData>(output.Data.ToString());
                    if (data.Status.ToLower() == "completed")
                    {
                        using (var clientBl = new ClientBll(partnerBl))
                        using (var documentBl = new DocumentBll(partnerBl))
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, 
                                output.Status, null, null, false, string.Empty, documentBl, notificationBl, out response);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                    }
                    else if (data.Status.ToLower() == "canceled")
                    {
                        using (var clientBl = new ClientBll(partnerBl))
                        using (var documentBl = new DocumentBll(clientBl))
                        {
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                            output.Description, null, null, false, string.Empty, documentBl, notificationBl, out response);
                        }
                    }
                }
            }
            return response;
        }
    }
}