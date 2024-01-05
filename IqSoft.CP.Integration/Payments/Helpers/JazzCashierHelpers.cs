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
using IqSoft.CP.Integration.Payments.Models.JazzCashier;
using IqSoft.CP.Common.Helpers;
using System.Text;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class JazzCashierHelpers
    {
        private enum TransactionTypes
        {
            Deposit = 1,
            Payout = 2
        }

        public static Dictionary<string, KeyValuePair<int,int>> PaymentWays { get; set; } = new Dictionary<string, KeyValuePair<int, int>>
        {
            { Constants.PaymentSystems.JazzCashierCreditCard, new KeyValuePair<int,int>(1,1) },
            { Constants.PaymentSystems.JazzCashierCreditCard3D, new KeyValuePair<int,int>(1,2) },

            { Constants.PaymentSystems.JazzCashierCrypto, new KeyValuePair<int,int>(2,0) },
            { Constants.PaymentSystems.JazzCashierBitcoins, new KeyValuePair<int,int>(2,3) },
            { Constants.PaymentSystems.JazzCashierEthereum,  new KeyValuePair<int,int>(2,4) },
            { Constants.PaymentSystems.JazzCashierLiteCoin,  new KeyValuePair<int,int>(2,5) },
            { Constants.PaymentSystems.JazzCashierBitCoinCash,  new KeyValuePair<int,int>(2,6) },

            { Constants.PaymentSystems.JazzCashierSoldana,  new KeyValuePair<int,int>(2,12) },
            { Constants.PaymentSystems.JazzCashierCardano,  new KeyValuePair<int,int>(2,13) },
            { Constants.PaymentSystems.JazzCashierDogecoin,  new KeyValuePair<int,int>(2,14) },
            { Constants.PaymentSystems.JazzCashierUSDT,  new KeyValuePair<int,int>(2,28) },
            { Constants.PaymentSystems.JazzCashierBinanceUSD,  new KeyValuePair<int,int>(2,29) },
            { Constants.PaymentSystems.JazzCashierWBTC,  new KeyValuePair<int,int>(2,30) },
            { Constants.PaymentSystems.JazzCashierUSDC,  new KeyValuePair<int,int>(2,31) },
            { Constants.PaymentSystems.JazzCashierBNB,  new KeyValuePair<int,int>(2,32) },
            { Constants.PaymentSystems.JazzCashierEUROC,  new KeyValuePair<int,int>(2,33) },
            { Constants.PaymentSystems.JazzCashierTRX,  new KeyValuePair<int,int>(2,34) },
            { Constants.PaymentSystems.JazzCashierCryptoCreditCard,  new KeyValuePair<int,int>(2,35) },
            { Constants.PaymentSystems.JazzCashierMoneyGram,  new KeyValuePair<int,int>(3,7) },
            { Constants.PaymentSystems.JazzCashierRia,  new KeyValuePair<int,int>(3,8)},
            { Constants.PaymentSystems.JazzCashierRemitly,  new KeyValuePair<int,int>(3,9)},
            { Constants.PaymentSystems.JazzCashierBoss,  new KeyValuePair<int,int>(3,10)},
            { Constants.PaymentSystems.JazzCashierZelle,  new KeyValuePair<int,int>(3,11)},
            { Constants.PaymentSystems.JazzCashierPagoefectivo,  new KeyValuePair<int,int>(4,15)},
            { Constants.PaymentSystems.JazzCashierAstropay,  new KeyValuePair<int,int>(4,16)},
            { Constants.PaymentSystems.JazzCashierBancoVenezuela,  new KeyValuePair<int,int>(4,17)},
            { Constants.PaymentSystems.JazzCashierBoleto,  new KeyValuePair<int,int>(4,18)},
            { Constants.PaymentSystems.JazzCashierJustPay,  new KeyValuePair<int,int>(4,19)},
            { Constants.PaymentSystems.JazzCashierKhipu,  new KeyValuePair<int,int>(4,20)},
            { Constants.PaymentSystems.JazzCashierTransbank,  new KeyValuePair<int,int>(4,21)},
            { Constants.PaymentSystems.JazzCashierMonnet,  new KeyValuePair<int,int>(4,22)},
            { Constants.PaymentSystems.JazzCashierPagadito,  new KeyValuePair<int,int>(4,23)},
            { Constants.PaymentSystems.JazzCashierPaycash,  new KeyValuePair<int,int>(4,24)},
            { Constants.PaymentSystems.JazzCashierPaycips,  new KeyValuePair<int,int>(4,25)},
            { Constants.PaymentSystems.JazzCashierPix,  new KeyValuePair<int,int>(4,26)},
            { Constants.PaymentSystems.JazzCashierMercadopago,  new KeyValuePair<int,int>(4,27)}
        };

        public static string CallJazzCashierApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentWays.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MethodNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JazzCashierApiUrl).StringValue;
                var sessionToken = RequestForToken(client.PartnerId, partnerPaymentSetting.UserName, partnerPaymentSetting.Password);
                var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + sessionToken } };
                var paymentRequestInput = new
                {
                    IdInternalTransaction = input.Id.ToString(),
                    IdCategory = paymentSystem.Name == Constants.PaymentSystems.JazzCashierCrypto ? 0 : PaymentWays[paymentSystem.Name].Key,
                    IdPaymentMethod = PaymentWays[paymentSystem.Name].Value == 0 ? (int?)null : PaymentWays[paymentSystem.Name].Value,
                    IdTransactionType = (int)TransactionTypes.Deposit,
                    Amount = Math.Round(input.Amount, 2),
                    Currency = input.CurrencyId,
                    Account = new
                    {
                        IdAccount = client.Id,
                        Username = client.UserName
                    }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}auth/open_cashier/v1", url),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (paymentRequestOutput.Code != 1)
                    throw new Exception($"Code: {paymentRequestOutput.Code}, Message: {paymentRequestOutput.Message}");
                input.ExternalTransactionId = paymentRequestOutput.Data.IdInternalTransaction;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return paymentRequestOutput.Data.PaymentSystemUrl;
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (!PaymentWays.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MethodNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                       paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var sessionToken = RequestForToken(client.PartnerId, partnerPaymentSetting.UserName, partnerPaymentSetting.Password);
                var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + sessionToken } };
    
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.JazzCashierApiUrl).StringValue;
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                
                var payoutRequestInput = new 
                {
                    IdInternalTransaction = paymentRequest.Id.ToString(),
                    IdTransactionType = (int)TransactionTypes.Payout,
                    //IdCategory = PaymentWays[paymentSystem.Name].Key,
                    IdPaymentMethod = PaymentWays[paymentSystem.Name].Value,
                    Amount = amount,
                    Currency = client.CurrencyId,
                    Account = new
                    {
                        IdAccount = client.Id,
                        Username = client.UserName,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Address = client.Address
                    },
                    Details = new
                    {
                        Address = paymentInfo.WalletNumber
                    }
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}transaction/send_payout/v1", url),
                    RequestHeaders = requestHeaders,
                    PostData = JsonConvert.SerializeObject(payoutRequestInput)
                };
                var payoutRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                if (payoutRequestOutput.Code != 1)
                    throw new Exception($"Code: {payoutRequestOutput.Code}, Message: {payoutRequestOutput.Message}");
                paymentRequest.ExternalTransactionId = payoutRequestOutput.Data.IdInternalTransaction;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }

        private static string RequestForToken(int partnerId, string integrationId, string apiKey)
        {
            var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.JazzCashierApiUrl).StringValue;
            var key = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", integrationId, apiKey)));
            var requestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + key } };       
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}auth/session_token/v1", url),
                RequestHeaders = requestHeaders
            };

            var response = JsonConvert.DeserializeObject<TokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (response.Code != 1)
                throw new Exception(response.Message);
            return response.Data.Token;

        }
    }
}
