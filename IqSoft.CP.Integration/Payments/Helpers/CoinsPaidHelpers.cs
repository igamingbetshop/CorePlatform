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
using IqSoft.CP.Integration.Payments.Models.CoinsPaid;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Payment;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CoinsPaidHelpers
    {
        private static Dictionary<string, string> CryptoCurrencies { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CoinsPaidBTC, "BTC" },
            { Constants.PaymentSystems.CoinsPaidETH, "ETH" },
            { Constants.PaymentSystems.CoinsPaidUSDTT, "USDTT" }, // USDTRC20
            { Constants.PaymentSystems.CoinsPaidDOGE, "DOGE" },
            { Constants.PaymentSystems.CoinsPaidCPD, "CPD" },
            { Constants.PaymentSystems.CoinsPaidBSC, "BSC" },  // BEP20
            { Constants.PaymentSystems.CoinsPaidBCH, "BCH" },  // BTCASH
            { Constants.PaymentSystems.CoinsPaidADA, "ADA" },  // CARDANO
            { Constants.PaymentSystems.CoinsPaidBNB, "BNB" },  // BEP2
            { Constants.PaymentSystems.CoinsPaidUSDC, "USDC" },  
            { Constants.PaymentSystems.CoinsPaidTRX, "TRX" }
        };

        //public static string PaymentRequest(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        //{
        //    var client = CacheManager.GetClientById(input.ClientId);
        //    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
        //    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, client.CurrencyId, 
        //                                                                       (int)PaymentRequestTypes.Deposit);
        //    var id = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidId).NumericValue;
        //    var postData = JsonConvert.SerializeObject(new
        //    {
        //        client_id = id,
        //        currency = paymentInfo.AccountType,
        //        convert_to = client.CurrencyId,
        //        foreign_id = input.Id.ToString(),
        //        url_back = cashierPageUrl
        //    });
        //    var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(postData));
        //    var signature = CommonFunctions.ComputeHMACSha512(postData, partnerPaymentSetting.Password).ToLower();
        //    var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidTerminalUrl).StringValue;
        //   return string.Format(url, data, signature);
        //}

        public static CryptoAddress PaymentRequest(int clientId, int paymentSystemId, ILog log)
        {
            var cryptoAddress = new CryptoAddress();
            try
            {
                var client = CacheManager.GetClientById(clientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystemId, client.CurrencyId,
                                                                                   (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentSystemId);
                if (!CryptoCurrencies.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var convertCurrency = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidConvertCurrency).StringValue;
				var postData = new
                {
                    currency = CryptoCurrencies[paymentSystem.Name],
                    convert_to = !string.IsNullOrWhiteSpace(convertCurrency) ? convertCurrency : Constants.Currencies.Euro, //client.CurrencyId,
                    foreign_id = clientId.ToString()
                };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidUrl).StringValue;
                var json = JsonConvert.SerializeObject(postData);
                var signature = CommonFunctions.ComputeHMACSha512(json, partnerPaymentSetting.Password).ToLower();

                var requestHeaders = new Dictionary<string, string>
                {
                   { "X-Processing-Key", partnerPaymentSetting.UserName},
                   { "X-Processing-Signature", signature}
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = String.Format(url, "addresses/take"),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(postData)
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<PaymentOutput>(response);
                cryptoAddress.Address = paymentSystem.Name == Constants.PaymentSystems.CoinsPaidBCH ? output.Data.Address.Replace("bitcoincash:", "") :  output.Data.Address;
                cryptoAddress.DestinationTag = output.Data.Tag;
            }
            catch (Exception ex)
            {
                log.Info(string.Format("CoinsPaid: {0}", ex));
            }
            return cryptoAddress;
        }
        public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!CryptoCurrencies.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.Euro)
                {
                    var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.Euro);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    if (parameters.ContainsKey("Currency"))
                        parameters["Currency"] = Constants.Currencies.Euro;
                    else
                        parameters.Add("Currency", Constants.Currencies.Euro);
                    if (parameters.ContainsKey("AppliedRate"))
                        parameters["AppliedRate"] = rate.ToString("F");
                    else
                        parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }

                var postData = new
                {
                    foreign_id = input.Id.ToString(),
                    amount,
                    currency = Constants.Currencies.Euro,
                    convert_to = CryptoCurrencies[paymentSystem.Name], 
                    address = paymentInfo.WalletNumber,
                    tag = string.IsNullOrWhiteSpace(paymentInfo.AccountType) ? null : paymentInfo.AccountType,
                };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CoinsPaidUrl).StringValue;
                var json = JsonConvert.SerializeObject(postData, new JsonSerializerSettings()
                {
						NullValueHandling = NullValueHandling.Ignore
				});
                var signature = CommonFunctions.ComputeHMACSha512(json, partnerPaymentSetting.Password).ToLower();

                var requestHeaders = new Dictionary<string, string>
                {
                   { "X-Processing-Key", partnerPaymentSetting.UserName},
                   { "X-Processing-Signature", signature}
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = String.Format(url, "withdrawal/crypto"),
                    RequestHeaders = requestHeaders,
                    PostData = json
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var output = JsonConvert.DeserializeObject<PayoutOutput>(response);
                input.ExternalTransactionId = output.Data.Id.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(input);

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }

        public static void GetSupportedCurrencies(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var postData = new
            {
                visible = true
            };
            var signature = CommonFunctions.ComputeHMACSha512("", "sySWEUrBNLYKJwQkCeamFuJ7grDVhheAZPHMDff497LKMTpRPFLmPt5EzPjLsrv2").ToLower();
            var url = CacheManager.GetPartnerSettingByKey(1, Constants.PartnerKeys.CoinsPaidUrl).StringValue;
            var requestHeaders = new Dictionary<string, string>
                {
                   { "X-Processing-Key", "Lmpzg8ZWlvV5jAzJXTY4yXsyCXwpR1hg"},
                   { "X-Processing-Signature", signature}
                };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = String.Format(url, "currencies/list"),
                RequestHeaders = requestHeaders,
                PostData = JsonConvert.SerializeObject(postData)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
    }
}

