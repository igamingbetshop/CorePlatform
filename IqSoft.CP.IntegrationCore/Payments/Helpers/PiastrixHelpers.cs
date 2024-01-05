using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Piastrix;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IqSoft.CP.Integration.Payments.Models.PerfectMoney;
using System.Linq;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class PiastrixHelpers
    {
        public static Dictionary<string, string> PaymentWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PiastrixWallet + "RUB", "piastrix_rub" },
            { Constants.PaymentSystems.PiastrixWallet + "KZT", "piastrix_kzt" },
            { Constants.PaymentSystems.PiastrixWallet + "USD", "piastrix_usd" },
            { Constants.PaymentSystems.PiastrixWallet + "EUR", "piastrix_eur" },
            { Constants.PaymentSystems.PiastrixWallet + "UAH", "piastrix_usd" },

            { Constants.PaymentSystems.PiastrixVisaMaster + "RUB", "card_rub" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "UAH", "card_uah" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "USD", "card_usd" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "EUR", "card_rub" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "KZT", "card_kzt" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "INR", "card_rub" },
            { Constants.PaymentSystems.PiastrixVisaMaster + "PLN", "card_rub" },

            { Constants.PaymentSystems.PiastrixQiwi + "RUB", "qiwi_rub" },
            { Constants.PaymentSystems.PiastrixQiwi + "USD", "qiwi_usd" },
            { Constants.PaymentSystems.PiastrixQiwi + "EUR", "qiwi_eur" },
            { Constants.PaymentSystems.PiastrixQiwi + "UAH", "qiwi_usd" },
            { Constants.PaymentSystems.PiastrixQiwi + "KZT", "qiwi_kzt" },

            { Constants.PaymentSystems.PiastrixYandex + "RUB", "yamoney_rub" },
            { Constants.PaymentSystems.PiastrixYandex + "EUR", "yamoney_rub" },
            { Constants.PaymentSystems.PiastrixYandex + "USD", "yamoney_rub" },
            { Constants.PaymentSystems.PiastrixYandex + "UAH", "yamoney_rub" },
                                                        
            { Constants.PaymentSystems.PiastrixPayeer + "RUB", "payeer_rub" },
            { Constants.PaymentSystems.PiastrixPayeer + "USD", "payeer_usd" },
            { Constants.PaymentSystems.PiastrixPayeer + "EUR", "payeer_eur" },
            { Constants.PaymentSystems.PiastrixPayeer + "UAH", "payeer_usd" },

            { Constants.PaymentSystems.PiastrixPerfectMoney + "USD", "perfectmoney_usd" },
            { Constants.PaymentSystems.PiastrixPerfectMoney + "EUR", "perfectmoney_eur" },
            { Constants.PaymentSystems.PiastrixPerfectMoney + "RUB", "perfectmoney_usd" },
            { Constants.PaymentSystems.PiastrixPerfectMoney + "UAH", "perfectmoney_usd" },

            { Constants.PaymentSystems.PiastrixBeeline + "RUB", "beeline_rub" },
            { Constants.PaymentSystems.PiastrixBeeline + "USD", "beeline_rub" },
            { Constants.PaymentSystems.PiastrixBeeline + "EUR", "beeline_rub" },
            { Constants.PaymentSystems.PiastrixBeeline + "UAH", "beeline_rub" },

            { Constants.PaymentSystems.PiastrixMTS + "RUB", "mts_rub" },
            { Constants.PaymentSystems.PiastrixMTS + "EUR", "mts_rub" },
            { Constants.PaymentSystems.PiastrixMTS + "USD", "mts_rub" },
            { Constants.PaymentSystems.PiastrixMTS + "UAH", "mts_rub" },

            { Constants.PaymentSystems.PiastrixMegafon + "RUB", "megafon_rub" },
            { Constants.PaymentSystems.PiastrixMegafon + "EUR", "megafon_rub" },
            { Constants.PaymentSystems.PiastrixMegafon + "USD", "megafon_rub" },
            { Constants.PaymentSystems.PiastrixMegafon + "UAH", "megafon_rub" },

            { Constants.PaymentSystems.PiastrixTele2 + "RUB", "tele2_rub" },
            { Constants.PaymentSystems.PiastrixTele2 + "EUR", "tele2_rub" },
            { Constants.PaymentSystems.PiastrixTele2 + "USD", "tele2_rub" },
            { Constants.PaymentSystems.PiastrixTele2 + "UAH", "tele2_rub" },

            { Constants.PaymentSystems.PiastrixAlfaclick + "RUB", "alfaclick_rub" },
            { Constants.PaymentSystems.PiastrixAlfaclick + "EUR", "alfaclick_rub" },
            { Constants.PaymentSystems.PiastrixAlfaclick + "USD", "alfaclick_rub" },
            { Constants.PaymentSystems.PiastrixAlfaclick + "UAH", "alfaclick_rub" },

            { Constants.PaymentSystems.PiastrixBTC + "RUB", "btc_usd" },
            { Constants.PaymentSystems.PiastrixBTC + "USD", "btc_usd" },
            { Constants.PaymentSystems.PiastrixBTC + "EUR", "btc_usd" },
            { Constants.PaymentSystems.PiastrixBTC + "KZT", "btc_usd" },
            { Constants.PaymentSystems.PiastrixBTC + "INR", "btc_usd" },
            { Constants.PaymentSystems.PiastrixBTC + "PLN", "btc_usd" },
            { Constants.PaymentSystems.PiastrixETH + "RUB", "eth_usd" },
            { Constants.PaymentSystems.PiastrixETH + "USD", "eth_usd" },
            { Constants.PaymentSystems.PiastrixETH + "EUR", "eth_usd" },
            { Constants.PaymentSystems.PiastrixETH + "KZT", "eth_usd" },
            { Constants.PaymentSystems.PiastrixETH + "INR", "eth_usd" },
            { Constants.PaymentSystems.PiastrixETH + "PLN", "eth_usd" },
            { Constants.PaymentSystems.PiastrixTerminal + "RUB", "terminal_rub" },
            { Constants.PaymentSystems.PiastrixTether + "RUB", "tether_usd" },
            { Constants.PaymentSystems.PiastrixTether + "USD", "tether_usd" },
            { Constants.PaymentSystems.PiastrixTether + "EUR", "tether_usd" },
            { Constants.PaymentSystems.PiastrixTinkoff + "RUB", "tinkoff_rub" },
            { Constants.PaymentSystems.PiastrixTinkoff + "USD", "tether_usd" }
        };

        private static Dictionary<string, string> PaymentOfflineWays { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.PiastrixBeeline+"RUB", "beeline_rub" },
            { Constants.PaymentSystems.PiastrixMTS+"RUB", "mts_rub" },
            { Constants.PaymentSystems.PiastrixMegafon+"RUB", "megafon_rub" },
            { Constants.PaymentSystems.PiastrixTele2+"RUB", "tele2_rub" }
        };

        public static string CallPiastrixBillApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using var currencyBll = new CurrencyBll(session, log);
            using var paymentSystemBl = new PaymentSystemBll(currencyBll);
            var client = CacheManager.GetClientById(input.ClientId);
            var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            var lang = session.LanguageId ?? Constants.Languages.English;
            var url = string.Format(CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PiastrixDepositUrl).StringValue, lang);
            var currency = !string.IsNullOrEmpty(partnerPaymentSetting.Info) && currencyBll.GetCurrencyById(partnerPaymentSetting.Info) != null
                            ? partnerPaymentSetting.Info : input.CurrencyId;
            if (!PaymentWays.ContainsKey(paymentsystem.Name + currency))
                currency = Constants.Currencies.RussianRuble;
            var currencyCode = currencyBll.GetCurrencyById(currency).Code;
            var merchantId = partnerPaymentSetting.UserName.Split(',');
            var merchant = merchantId[0];
            var count = CacheManager.GetClientDepositCount(client.Id);
            if (count > 0 && merchantId.Length > 1)
                merchant = merchantId[1];
            var amount = input.Amount;
            if (input.CurrencyId != currency)
            {
                var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, currency);
                amount *= rate;
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Currency", currency);
                parameters.Add("AppliedRate", rate.ToString("F"));
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            var paymentRequestInput = new PaymentInput
            {
                amount = amount.ToString("F"),
                currency = currencyCode,
                shop_id = merchant,
                shop_order_id = input.Id
            };
            paymentRequestInput.sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ":") +
                                                                     partnerPaymentSetting.Password);
            paymentRequestInput.success_url = cashierPageUrl;
            paymentRequestInput.failed_url = cashierPageUrl;
            paymentRequestInput.description = cashierPageUrl;
            paymentRequestInput.payway = PaymentWays.ContainsKey(paymentsystem.Name + currency) ? PaymentWays[paymentsystem.Name + currency] : null;
            return url + "?" + CommonFunctions.GetUriEndocingFromObject(paymentRequestInput);
        }

        public static string CallPiastrixInvoiceApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var currencyBl = new CurrencyBll(session, log))
            {
                using (var clientBl = new ClientBll(currencyBl))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(currencyBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId);
                        var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var merchantId = partnerPaymentSetting.UserName.Split(',');
                        var merchant = merchantId[0];
                        var count = CacheManager.GetClientDepositCount(client.Id);
                        if (count > 0 && merchantId.Length > 1)
                            merchant = merchantId[1];
                        var lang = session.LanguageId ?? Constants.Languages.English;
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PiastrixWithdrawalUrl).StringValue;
                        var currency = !string.IsNullOrEmpty(partnerPaymentSetting.Info) && currencyBl.GetCurrencyById(partnerPaymentSetting.Info) != null
                                   ? partnerPaymentSetting.Info : input.CurrencyId;
                        if (!PaymentWays.ContainsKey(paymentsystem.Name + currency))
                            currency = Constants.Currencies.RussianRuble;
                        var amount = input.Amount;
                        if (input.CurrencyId != currency)
                        {
                            var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, currency);
                            amount *= rate;
                            var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                            parameters.Add("Currency", currency);
                            parameters.Add("AppliedRate", rate.ToString("F") );
                            input.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                        }
                        string currencyCode = currencyBl.GetCurrencyById(currency).Code;
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                        var paymentRequestInput = new PaymentInput
                        {
                            amount = amount.ToString("F"),
                            currency = currencyCode,
                            shop_id = merchant,
                            shop_order_id = input.Id,
                            payway = PaymentWays.ContainsKey(paymentsystem.Name + currency) ? PaymentWays[paymentsystem.Name + currency] : null
                        };
                        paymentRequestInput.sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ":") +
                                                                                 partnerPaymentSetting.Password);
                        paymentRequestInput.success_url = cashierPageUrl;
                        paymentRequestInput.failed_url = cashierPageUrl;
                        var lastDep = clientBl.GetClientLastDeposit(paymentsystem.Id, client.Id);
                        if (lastDep != null && !string.IsNullOrEmpty(lastDep.Parameters))
                        {
                            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(lastDep.Parameters);
                            if (parameters.ContainsKey("payer_id"))
                                paymentRequestInput.payer_id = parameters["payer_id"];
                        }
                        if (!string.IsNullOrEmpty(paymentInfo.CardNumber))
                        {
                            paymentRequestInput.phone = paymentInfo.CardNumber;
                            paymentRequestInput.phone = paymentRequestInput.phone.Replace("+", string.Empty);
                            if (paymentsystem.Name != "PiastrixQiwi")
                            {
                                paymentRequestInput.phone = "+" + paymentRequestInput.phone;
                                paymentRequestInput.phone = paymentRequestInput.phone.Replace("+7", string.Empty);
                            }
                        }
                        else if (!string.IsNullOrEmpty(paymentInfo.MobileNumber))
                        {
                            paymentRequestInput.phone = paymentInfo.MobileNumber;
                            paymentRequestInput.phone = paymentRequestInput.phone.Replace("+", string.Empty);
                            if (paymentsystem.Name != "PiastrixQiwi")
                            {
                                paymentRequestInput.phone = "+" + paymentRequestInput.phone;
                                paymentRequestInput.phone = paymentRequestInput.phone.Replace("+7", string.Empty);
                            }
                        }
                        paymentRequestInput.description = paymentRequestInput.success_url;
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = System.Net.Http.HttpMethod.Post,
                            Url = string.Format("{0}{1}", url, "invoice/try"),
                            PostData = JsonConvert.SerializeObject(paymentRequestInput,
                                                                   new JsonSerializerSettings()
                                                                   {
                                                                       NullValueHandling = NullValueHandling.Ignore
                                                                   })
                        };
                        log.Info("Input_" + JsonConvert.SerializeObject(httpRequestInput));
                        var otp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info("Output_" + otp);
                        var output = JsonConvert.DeserializeObject<PaymentOutput>(otp);
                        if (output.Result)
                        {
                            httpRequestInput.Url = string.Format("{0}{1}", url, "invoice/create");
                            output = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            if (output.Result)
                            {
                                if (PaymentOfflineWays.ContainsKey(paymentsystem.Name + currency))
                                    if (session.LanguageId.ToLower() == "ru")
                                        return output.Data.RequestData.ru;
                                    else
                                        return output.Data.RequestData.en;
                                httpRequestInput.RequestMethod = new HttpMethod(output.Data.Method);
                                if (paymentsystem.Name == Constants.PaymentSystems.PiastrixPerfectMoney)
                                {
                                    var distributionUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                                    var paymentInput = new PaymentRequestInput
                                    {
                                        MerchantId = output.Data.RequestData.PAYEE_ACCOUNT,
                                        MerchantName = output.Data.RequestData.PAYEE_NAME,
                                        Amount = output.Data.RequestData.PAYMENT_AMOUNT.Value,
                                        CurrencyId = output.Data.RequestData.PAYMENT_UNITS,
                                        PaymentRequestId = output.Data.RequestData.PAYMENT_ID,
                                        StatusUrl = output.Data.RequestData.STATUS_URL,
                                        PaymentUrl = output.Data.RequestData.PAYMENT_URL,
                                        PaymentMethod = "account",
                                        Language = CommonHelpers.LanguageISOCodes[session.LanguageId]
                                    };
                                    var properties = from p in paymentInput.GetType().GetProperties()
                                                     select (p.GetValue(paymentInput, null) != null ? p.GetValue(paymentInput, null).ToString() : string.Empty);
                                    paymentInput.InputData = CommonFunctions.ComputeMd5(CommonFunctions.ComputeMd5(string.Join(":", properties.ToArray().
                                                                    Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='))));
                                    return string.Format("{0}/PerfectMoney/PaymentRequest?{1}", distributionUrl, CommonFunctions.GetUriEndocingFromObject(paymentInput));
                                }
                                var resp = string.Format("{0}?{1}", output.Data.Url,
                                    CommonFunctions.GetSortedParamWithValuesAsString(output.Data.RequestData, "&"));
                                return resp;
                            }
                        }
                        throw BaseBll.CreateException(session.LanguageId, GetErrorCode(output.error_code));
                    }
                }
            }
        }

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using var currencyBl = new CurrencyBll(session, log);
            using var paymentSystemBl = new PaymentSystemBll(currencyBl);
            var client = CacheManager.GetClientById(input.ClientId);
            var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PiastrixWithdrawalUrl).StringValue;
            var currency = !string.IsNullOrEmpty(partnerPaymentSetting.Info) && currencyBl.GetCurrencyById(partnerPaymentSetting.Info) != null
                         ? partnerPaymentSetting.Info : input.CurrencyId;
            if (!PaymentWays.ContainsKey(paymentsystem.Name + currency))
                currency = Constants.Currencies.RussianRuble;
            string currencyCode = currencyBl.GetCurrencyById(currency).Code;
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var account = paymentInfo.WalletNumber;
            if (string.IsNullOrEmpty(account) || string.IsNullOrWhiteSpace(account))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

            var merchantId = partnerPaymentSetting.UserName.Split(',');
            var merchant = merchantId[0];
            var count = CacheManager.GetClientDepositCount(client.Id);
            if (count > 0 && merchantId.Length > 1)
                merchant = merchantId[1];
            var amount = input.Amount - (input.CommissionAmount ?? 0);
            if (input.CurrencyId != currency)
            {
                var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, currency);
                amount *= rate;
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Currency", currency);
                parameters.Add("AppliedRate", rate.ToString("F"));
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            var requestInput = new PayoutInput
            {
                Amount = amount.ToString("F"),
                AmountType = "writeoff_amount",//"receive_amount"
                PayeeAccount = account.Trim(),
                PayeeCurrency = currencyCode,
                ShopCurrency = currencyCode,
                ShopId = Convert.ToInt32(merchant),
                ShopPaymentId = input.Id.ToString()
            };
            requestInput.Sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(requestInput, ":") +
                                             partnerPaymentSetting.Password);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = url + "transfer/create",
                PostData = JsonConvert.SerializeObject(requestInput)
            };

            var output = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (output.Result && output.Data != null && output.Data.ExternalTransactionId > 0 && output.Data.Status == 5)
            {
                input.ExternalTransactionId = output.Data.ExternalTransactionId.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Approved,
                };
            }
            else if (output.Data != null && WaitingStatuses.Contains(output.Data.Status))
            {
                input.ExternalTransactionId = output.Data.ExternalTransactionId.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
            else if (output.error_code == 6)
            {
                var checkStatusInput = new CheckStatusInput
                {
                    CurrentDataTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RequestId = merchant,
                    WithdrawId = input.ExternalTransactionId
                };
                checkStatusInput.Sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(checkStatusInput, ":") +
                                             partnerPaymentSetting.Password);
                httpRequestInput.Url = url + "withdraw/status";
                httpRequestInput.PostData = JsonConvert.SerializeObject(checkStatusInput);
                output = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (output.Data.Status == 5)
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Approved,
                    };
            }
            return new PaymentResponse
            {
                Status = PaymentRequestStates.Failed,
                Description = output.Message
            };
        }
        public static PaymentResponse CreateVisaCardPayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var currencyBl = new CurrencyBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    var paymentsystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PiastrixWithdrawalUrl).StringValue;
                    var currency = !string.IsNullOrEmpty(partnerPaymentSetting.Info) && currencyBl.GetCurrencyById(partnerPaymentSetting.Info) != null
                                  ? partnerPaymentSetting.Info : input.CurrencyId;
                    if (!PaymentWays.ContainsKey(paymentsystem.Name + currency))
                        currency = Constants.Currencies.RussianRuble;

                    string currencyCode = currencyBl.GetCurrencyById(input.CurrencyId).Code;
                    if (string.IsNullOrWhiteSpace(input.Info))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                    if (!PaymentWays.ContainsKey(paymentsystem.Name + currency))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    if (string.IsNullOrEmpty(paymentInfo.CardNumber.Trim()))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                    var merchantId = partnerPaymentSetting.UserName.Split(',');
                    var merchant = merchantId[0];
                    var count = CacheManager.GetClientDepositCount(client.Id);
                    if (count > 0 && merchantId.Length > 1)
                        merchant = merchantId[1];
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    if (input.CurrencyId != currency)
                    {
                        var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.RussianRuble);
                        amount = rate * input.Amount;
                        var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                        parameters.Add("Currency", Constants.Currencies.RussianRuble);
                        parameters.Add("AppliedRate", rate.ToString("F"));
                        input.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                    }
                    var requestInput = new PayoutInput
                    {
                        Account = paymentInfo.CardNumber.Trim().Replace("+", string.Empty),
                        PayWay = (currency == Constants.Currencies.PolandZloty && PaymentWays[paymentsystem.Name + currency] == "card_rub") ?
                        "card_eur" : PaymentWays[paymentsystem.Name + currency],
                        Amount = amount.ToString("F"),
                        AmountType = "shop_amount", //"ps_amount"
                                                    //PayeeCurrency = currencyCode,
                        ShopCurrency = currencyCode,
                        ShopId = Convert.ToInt32(merchant),
                        ShopPaymentId = input.Id.ToString()
                    };
                    requestInput.Sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(requestInput, ":") +
                                                     partnerPaymentSetting.Password);
                    requestInput.PayeeCurrency = currencyCode;
                    log.Info("CreateWithdrawRequest_" + JsonConvert.SerializeObject(requestInput));
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
                        Url = url + "withdraw/create",
                        PostData = JsonConvert.SerializeObject(
                            requestInput,
                            new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            })
                    };
                    var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    log.Info("CreateWithdrawResponse_" + JsonConvert.SerializeObject(resp));
                    var output = JsonConvert.DeserializeObject<PayoutOutput>(resp);
                    if (output.Result && output.Data != null && output.Data.ExternalTransactionId > 0 && output.Data.Status == 5)
                    {
                        input.ExternalTransactionId = output.Data.ExternalTransactionId.ToString();
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.Approved,
                        };
                    }
                    else if (output.Data != null && WaitingStatuses.Contains(output.Data.Status))
                    {
                        input.ExternalTransactionId = output.Data.ExternalTransactionId.ToString();
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    }
                    else if (output.error_code == 6)
                    {
                        var checkStatusInput = new CheckStatusInput
                        {
                            CurrentDataTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            RequestId = merchant,
                            WithdrawId = input.ExternalTransactionId
                        };
                        checkStatusInput.Sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(checkStatusInput, ":") +
                                                     partnerPaymentSetting.Password);
                        httpRequestInput.Url = url + "withdraw/status";
                        log.Info("WithdrawStatusRequest_" + JsonConvert.SerializeObject(requestInput));
                        httpRequestInput.PostData = JsonConvert.SerializeObject(checkStatusInput);
                        resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info("WithdrawStatusResponse_" + JsonConvert.SerializeObject(resp));
                        output = JsonConvert.DeserializeObject<PayoutOutput>(resp);
                        if (output.Data != null)
                        {
                            if (output.Data.Status == 5)
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Approved,
                                };
                            else if (output.Data.Status == 6)
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Failed,
                                    Description = "Wallet Not Found"
                                };
                        }
                    }
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = output.Message
                    };
                }
            }
        }

        public static void GetPayoutRequestStatus(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PiastrixWithdrawalUrl).StringValue;
                var merchantId = partnerPaymentSetting.UserName.Split(',');
                var merchant = merchantId[0];
                var count = CacheManager.GetClientDepositCount(client.Id);
                if (count > 0 && merchantId.Length > 1)
                    merchant = merchantId[1];
                var checkStatusInput = new CheckStatusInput
                {
                    CurrentDataTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    RequestId = merchant,
                    WithdrawId = paymentRequest.ExternalTransactionId
                };
                checkStatusInput.Sign = CommonFunctions.ComputeSha256(CommonFunctions.GetSortedValuesAsString(checkStatusInput, ":") +
                                             partnerPaymentSetting.Password);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = url + "withdraw/status",
                    PostData = JsonConvert.SerializeObject(
                                   checkStatusInput,
                                   new JsonSerializerSettings()
                                   {
                                       NullValueHandling = NullValueHandling.Ignore
                                   })
                };
                var output = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                using var documentBl = new DocumentBll(clientBl);
                using var notificationBl = new NotificationBll(clientBl);
                if (output.Data.Status == 5)
                {
                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved,
                        string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);

                }
                else if (!WaitingStatuses.Contains(output.Data.Status))
                {
                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
                    output.Message, null, null, false, string.Empty, documentBl, notificationBl);
                }
            }
        }

        private static readonly List<int> WaitingStatuses = new List<int> { 1, 2, 3, 7 };

        private enum ResponseCodes
        {
            PaywayNotFound = 1,
            PaywayNotUsed = 2,
            PaywayNotAvailable = 3,
            AmountTooSmall = 4,
            AmountTooLarge = 5,
            OperationNotUnique = 6,
            OperationNotFound = 7,
            OperationIsProcessing = 8,
            InsufficientBalance = 9,
            IncorrectRequestParam = 10,
            ShopNotFound = 11,
            ShopNotActive = 12,
            AccountNotFound = 13,
            IncorrectAccountStatus = 14,
            RequestIpDenied = 15,
            InvalidCurrencyExchange = 16,
            InvalidShopContract = 17,
            IncorrectAccountType = 18,
            ShopAggregatorRequire = 19,
            InvalidProject = 20,
            ProjectNotActive = 21,
            IncorrectOperationStatus = 100,
            OtherExcetion = 2000
        }

        private readonly static Dictionary<int, int> ResponseCodesMapping = new Dictionary<int, int>
        {
            {(int)ResponseCodes.PaywayNotFound, Constants.Errors.MethodNotFound },
            {(int)ResponseCodes.PaywayNotUsed, Constants.Errors.MethodNotFound },
            {(int)ResponseCodes.PaywayNotAvailable, Constants.Errors.MethodNotFound },
            {(int)ResponseCodes.AmountTooSmall, Constants.Errors.PaymentRequestInValidAmount },
            {(int)ResponseCodes.AmountTooLarge, Constants.Errors.PaymentRequestInValidAmount },
            {(int)ResponseCodes.OperationNotUnique, Constants.Errors.PaymentRequestAlreadyExists },
            {(int)ResponseCodes.OperationNotFound, Constants.Errors.PaymentRequestNotFound },
            {(int)ResponseCodes.InsufficientBalance, Constants.Errors.LowBalance },
            {(int)ResponseCodes.IncorrectRequestParam, Constants.Errors.WrongInputParameters },
            {(int)ResponseCodes.AccountNotFound, Constants.Errors.AccountNotFound },
            {(int)ResponseCodes.RequestIpDenied, Constants.Errors. CanNotPayFailedRequest},
            {(int)ResponseCodes.OtherExcetion, Constants.Errors. GeneralException}
        };

        private static int GetErrorCode(int responseCode)
        {
            if (ResponseCodesMapping.ContainsKey(responseCode))
                return ResponseCodesMapping[responseCode];
            return Constants.Errors.GeneralException;
        }
    }
}