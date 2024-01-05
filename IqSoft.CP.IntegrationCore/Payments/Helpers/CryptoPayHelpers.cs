using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.CryptoPay;
using IqSoft.CP.Integration.Payments.Models.Payment;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CryptoPayHelpers
    {
        private static Dictionary<string, KeyValuePair<string, string>> CryptoChannels { get; set; } = new Dictionary<string, KeyValuePair<string, string>>
        {
            { Constants.PaymentSystems.CryptoPayBTC, new KeyValuePair<string,string> ("BTC", "bitcoin" ) },
            { Constants.PaymentSystems.CryptoPayBCH, new KeyValuePair<string,string> ("BCH", "bitcoin_cash" ) },
            { Constants.PaymentSystems.CryptoPayETH, new KeyValuePair<string,string> ("ETH","ethereum") },
            { Constants.PaymentSystems.CryptoPayLTC, new KeyValuePair<string,string> ("LTC" ,"litecoin")},
            { Constants.PaymentSystems.CryptoPayUSDT, new KeyValuePair<string,string> ("USDT","tron") },
            { Constants.PaymentSystems.CryptoPayUSDTERC20, new KeyValuePair<string,string> ("USDT","ethereum") },
            { Constants.PaymentSystems.CryptoPayUSDC, new KeyValuePair<string,string> ("USDC" ,"ethereum")},
            { Constants.PaymentSystems.CryptoPayDAI, new KeyValuePair<string,string> ("DAI","ethereum") },
            { Constants.PaymentSystems.CryptoPayXRP, new KeyValuePair<string,string> ("XRP","ripple") },
            { Constants.PaymentSystems.CryptoPayXLM, new KeyValuePair<string,string> ("XLM" ,"stellar")},
            { Constants.PaymentSystems.CryptoPayADA, new KeyValuePair<string,string> ("ADA" ,"cardano")},
            { Constants.PaymentSystems.CryptoPaySHIB, new KeyValuePair<string,string> ("SHIB" ,"ethereum")},
            { Constants.PaymentSystems.CryptoPaySOL, new KeyValuePair<string,string> ("SOL" ,"solana")}
        };

        public static CryptoAddress CreateCryptoPayChannel(int clientId, int paymentSystemId, ILog log)
        {
            var cryptoAddress = new CryptoAddress();
            try
            {
                var client = CacheManager.GetClientById(clientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystemId, client.CurrencyId,
                                                                                   (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentSystemId);
                if (!CryptoChannels.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CryptoPayApiUrl).StringValue;
                var apiKey = partnerPaymentSetting.UserName;
                var apiSecret = partnerPaymentSetting.Password.Split(',')[0];
                var relativeUrl = "/api/channels";
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var channel = CryptoChannels[paymentSystem.Name];
                var channelRequestInput = new
                {
                    pay_currency = channel.Key,
                    network = channel.Value,
                    receiver_currency = client.CurrencyId,
                    name = partner.Name,
                    custom_id = string.Format("{0}_{1}_{2}", client.Id.ToString(), channel.Key, channel.Value)
                };
                var data = JsonConvert.SerializeObject(channelRequestInput);
                var currentDate = DateTime.Now.ToUniversalTime();
                var hashingValues = new List<string>
            {
                HttpMethod.Post.ToString(),
                GetMD5Hash(data),
                Constants.HttpContentTypes.ApplicationJson,
                currentDate.ToString("r"),
                relativeUrl
            };

                var signature = GenerateSigniture(apiSecret, string.Join("\n", hashingValues));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"HMAC {apiKey}:{signature}" } },
                    Date = currentDate,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}{1}", url, relativeUrl),
                    PostData = data
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);
                if (paymentOutput== null || string.IsNullOrEmpty(paymentOutput.Details.HostedPageUrl))
                    log.Info(string.Format("CryptoPay: ClientId = {0}, PaymentSystemId = {1}, Response: {2}", clientId, paymentSystemId, response));
                var address = paymentOutput.Details.Address.Split(new string[] { "?dt=" }, StringSplitOptions.None);
                cryptoAddress.Address = address[0];
                if (address.Length > 1)
                    cryptoAddress.DestinationTag = address[1];
            }
            catch (Exception ex)
            {
                log.Info(string.Format("CryptoPay: {0}", ex));
            }
            return cryptoAddress;
        }

        public static string GenerateSigniture(string secret, string signitureData)
        {
            var apiSecretBytes = Encoding.UTF8.GetBytes(secret);
            var signitureBytes = Encoding.UTF8.GetBytes(signitureData);
            using var hmacsha1 = new HMACSHA1();
            hmacsha1.Key = apiSecretBytes;
            byte[] resultBytes = hmacsha1.ComputeHash(signitureBytes);
            return Convert.ToBase64String(resultBytes);
        }

        private static string GetMD5Hash(string text)
        {
            var returnvalue = new StringBuilder();
            using var md5Hash = MD5.Create();
            var HashBytes = Encoding.UTF8.GetBytes(text);
            HashBytes = md5Hash.ComputeHash(HashBytes);
            foreach (byte bte in HashBytes)
                returnvalue.Append(bte.ToString("x2").ToLower());
            return returnvalue.ToString();
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (!CryptoChannels.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                    paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CryptoPayApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var apiKey = partnerPaymentSetting.UserName;
                var apiSecret = partnerPaymentSetting.Password.Split(',')[0];
                var relativeUrl = "/api/coin_withdrawals";
                var currentDate = DateTime.Now.ToUniversalTime();
                var channel = CryptoChannels[paymentSystem.Name];
                if (!string.IsNullOrEmpty(paymentInfo.AccountType))
                    paymentInfo.WalletNumber += "?dt=" + paymentInfo.AccountType;
                var payoutRequestInput = new
                {
                    address = paymentInfo.WalletNumber,
                    charged_currency = paymentRequest.CurrencyId,
                    received_currency = channel.Key,
                    network = channel.Value,
                    charged_amount = amount,
                    custom_id = paymentRequest.Id.ToString(),
                    force_commit = true
                };
                var data = JsonConvert.SerializeObject(payoutRequestInput);
                var hashingValues = new List<string>
                {
                    HttpMethod.Post.ToString(),
                    GetMD5Hash( data),
                    Constants.HttpContentTypes.ApplicationJson,
                    currentDate.ToString("r"),
                    relativeUrl
                };
                var signature = GenerateSigniture(apiSecret, string.Join("\n", hashingValues));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Date = currentDate,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"HMAC {apiKey}:{signature}" } },
                    Url = string.Format("{0}{1}", url, relativeUrl),
                    PostData = data
                };
                var payoutRequestOutput = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                paymentRequest.ExternalTransactionId = payoutRequestOutput.Details.Id;
                var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                parameters.Add("ExchangeDetails", JsonConvert.SerializeObject(payoutRequestOutput.Details.ExchangeDetails));
                paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                if (payoutRequestOutput.Details.Status == "failed" ||
                    payoutRequestOutput.Details.Status == "cancelled")
                    throw new Exception(payoutRequestOutput.Details.Status);

                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}