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
using IqSoft.CP.Integration.Payments.Models.PayStage;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PayStageHelpers
    {
        private static Dictionary<string, KeyValuePair<string, string>> PaymentMethods { get; set; } = new Dictionary<string, KeyValuePair<string, string>>
        {
            { Constants.PaymentSystems.PayStageCard, new KeyValuePair<string, string> ("credit_debit_card", string.Empty ) },//++
            { Constants.PaymentSystems.PayStageWallet, new KeyValuePair<string, string> ("e_wallet", "oriental_wallet" ) },//+
            { Constants.PaymentSystems.PayStageGCash, new KeyValuePair<string, string> ("e_wallet", "gcash" ) },//+
            { Constants.PaymentSystems.PayStageGrabPay, new KeyValuePair<string, string> ("e_wallet", "grabpay" ) },//+
            { Constants.PaymentSystems.PayStageUnionBank, new KeyValuePair<string, string> ("online_banking", "unionbank" )},//+
            { Constants.PaymentSystems.PayStageBankTransfer, new KeyValuePair<string, string> ("local_bank_transfer", string.Empty )},//++
            { Constants.PaymentSystems.PayStagePalawan, new KeyValuePair<string, string> ("cash_payment", "palawan_pawnshop" )},//++
            { Constants.PaymentSystems.PayStageCebuana, new KeyValuePair<string, string> ("cash_payment", "cebuana" )},//++
            { Constants.PaymentSystems.PayStagePaygate, new KeyValuePair<string, string> ("payment_provider", "paygate" )},//+
            { Constants.PaymentSystems.PayStageInstapay, new KeyValuePair<string, string> ("payment_provider", "instapay" )},//++
            { Constants.PaymentSystems.PayStageQR, new KeyValuePair<string, string> ("payment_provider", "qrph" )},
            { Constants.PaymentSystems.PayStagePesonet, new KeyValuePair<string, string> ("payment_provider", "pesonet" )},
        };

        public static string CallPayStageApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.ZipCode?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            if (string.IsNullOrEmpty(paymentInfo.City) || string.IsNullOrEmpty(paymentInfo.Country))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);

            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayStageApiUrl).StringValue;
            var paymentRequestInput = new
            {
                customer = new
                {
                    address_line_1 = client.Address,
                    city = paymentInfo.City,
                    country = paymentInfo.Country,
                    email = client.Email,
                    first_name = client.FirstName,
                    last_name = client.LastName,
                    mobile = client.MobileNumber,
                    state = paymentInfo.City,
                    zip = client.ZipCode.Trim()
                },
                details = new
                {
                    reference_no = input.Id.ToString(),
                    method = PaymentMethods[paymentSystem.Name].Key,
                    solution = PaymentMethods[paymentSystem.Name].Value,
                    currency = paymentSystem.Name == Constants.PaymentSystems.PayStageBankTransfer ? Constants.Currencies.JapaneseYen : client.CurrencyId,
                    receiving_currency = client.CurrencyId,
                    amount = (int)input.Amount,
                    redirect_url = cashierPageUrl
                }
            };
            var headers = new Dictionary<string, string>
            {
                { "X-GATEWAY-KEY", partnerPaymentSetting.UserName }, //Public Key 
                { "X-GATEWAY-SECRET", CommonFunctions.ComputeHMACSha256($"{partnerPaymentSetting.UserName}{input.Id}", partnerPaymentSetting.Password).ToLower()  }  //Secret Key
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = $"{url}/deposit/intent",
                PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };

            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var result = JsonConvert.DeserializeObject<PaymentOutput>(resp);
            if (result.Data == null)
                throw new Exception(resp);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                input.ExternalTransactionId =  result.Data.TransactionNumber;
                input.Amount =  (int)input.Amount;
                paymentSystemBl.ChangePaymentRequestDetails(input);
            }
            return result.Data.CheckoutUrl;
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            if (string.IsNullOrEmpty(client.FirstName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Address))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
            if (string.IsNullOrWhiteSpace(client.ZipCode?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            if (string.IsNullOrEmpty(paymentInfo.City) || string.IsNullOrEmpty(paymentInfo.Country))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayStageApiUrl).StringValue;
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                               client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var paymentRequestInput = new
            {
                customer = new
                {
                    address_line_1 = client.Address,
                    city = paymentInfo.City,
                    country = paymentInfo.Country,
                    email = client.Email,
                    first_name = client.FirstName,
                    last_name = client.LastName,
                    mobile = client.MobileNumber,
                    state = paymentInfo.City,
                    zip = client.ZipCode.Trim()
                },
                details = new
                {
                    reference_no = paymentRequest.Id.ToString(),
                    method = PaymentMethods[paymentSystem.Name].Key,
                    solution = PaymentMethods[paymentSystem.Name].Value,
                    currency = client.CurrencyId,
                    debit_currency = client.CurrencyId,
                    amount = (int)paymentRequest.Amount
                }
            };
            var headers = new Dictionary<string, string>
            {
                {"X-GATEWAY-KEY", partnerPaymentSetting.UserName }, //Public Key 
                {"X-GATEWAY-SECRET", CommonFunctions.ComputeHMACSha256($"{partnerPaymentSetting.UserName}{paymentRequest.Id}", partnerPaymentSetting.Password).ToLower()  }  //Secret Key
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = $"{url}/withdraw/intent",
                PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })
            };

            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var result = JsonConvert.DeserializeObject<PaymentOutput>(resp);
            if (result.Data == null || !result.Success)
                throw new Exception(resp);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                paymentRequest.ExternalTransactionId =  result.Data.TransactionNumber;
                paymentRequest.Amount =  (int)paymentRequest.Amount;
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                var postData = string.Empty;
                switch (paymentSystem.Name)
                {
                    case Constants.PaymentSystems.PayStageWallet: //03
                        postData = JsonConvert.SerializeObject(new { receiver_individual_account = paymentInfo.WalletNumber });
                        break;
                    case Constants.PaymentSystems.PayStageBankTransfer://15
                        var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                        if (bankInfo == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                        postData = JsonConvert.SerializeObject(new
                        {
                            account_name_katakana = paymentInfo.BankAccountHolder,
                            account_number = paymentInfo.BankAccountNumber,
                            bank_branch_name = paymentInfo.BankBranchName,
                            bank_code = bankInfo.BankCode,
                            bank_branch_code = bankInfo.AccountNumber,
                            bank_name = bankInfo.BankName,
                            auto = true
                        });
                        break;
                    case Constants.PaymentSystems.PayStageUnionBank: //15
                        bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                        if (bankInfo == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                        postData = JsonConvert.SerializeObject(new
                        {
                            sender_ref_id = paymentRequest.Id.ToString(),
                            account_number = paymentInfo.BankAccountNumber,
                            bank_code = bankInfo.BankCode,
                            name = paymentInfo.BankAccountHolder,
                            line1 = client.Address,
                            line2 = client.Address,
                            city = paymentInfo.City,
                            province = paymentInfo.City,
                            zip_code = client.ZipCode.Trim(),
                            country = paymentInfo.Country
                        });
                        break;
                    case Constants.PaymentSystems.PayStageInstapay://03
                    case Constants.PaymentSystems.PayStagePesonet:
                        postData = JsonConvert.SerializeObject(new
                        {
                            sender_ref_id = paymentRequest.Id.ToString(),
                            account_number = paymentInfo.WalletNumber,
                            recipient = new
                            {
                                name = paymentInfo.BankAccountHolder,
                                address = new
                                {
                                    line1 = client.Address,
                                    line2 = client.Address,
                                    city = paymentInfo.City,
                                    province = paymentInfo.City,
                                    zip_code = client.ZipCode.Trim(),
                                    country = paymentInfo.Country
                                }
                            }
                        });
                        break;
                    default: break;
                }
                httpRequestInput.Url = $"{url}/withdraw/{paymentRequest.ExternalTransactionId}";
                httpRequestInput.PostData = postData;
                resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }

        public class BankItem
        {
            public string name { get; set; }
            public string code { get; set; }
        }

        public class BankList
        {
            public List<BankItem> data { get; set; }
        }
        public static void GetBanks()
        {
            var headers = new Dictionary<string, string>
            {
                {"X-GATEWAY-KEY", "public Key" },
                {"X-GATEWAY-SECRET", CommonFunctions.ComputeHMACSha256("public Key", "Secret Key")  }
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                RequestHeaders = headers,
                Url = " https://api-staging.paystage.net/bank-codes/jpay"
            };

            var resp = JsonConvert.DeserializeObject<BankList>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            foreach (var dd in resp.data)
            {
                httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    RequestHeaders = headers,
                    Url = " https://api-staging.paystage.net/bank-codes/jpay/" + dd.code
                };
                var r = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            }
        }
    }
}