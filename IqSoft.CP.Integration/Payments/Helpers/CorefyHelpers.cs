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
using IqSoft.CP.Common.Helpers;
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.Integration.Payments.Models.Corefy;
using IqSoft.CP.Integration.Payments.Models;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class CorefyHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CorefyCreditCard, "payment_card_{0}_hpp"}, //payment_card_usd_hpp
            { Constants.PaymentSystems.CorefyBankTransfer,"bank_transfer_{0}_hpp"},
            { Constants.PaymentSystems.CorefyHavale, "bank_transfer_{0}_hpp" },
            { Constants.PaymentSystems.CorefyPep, "pep_{0}_hpp"},
            { Constants.PaymentSystems.CorefyPayFix, "payfix_{0}_hpp" },
            { Constants.PaymentSystems.CorefyMefete, "mefete_{0}_hpp"},
            { Constants.PaymentSystems.CorefyParazula, "parazula_{0}_hpp"},
            { Constants.PaymentSystems.CorefyPapara, "papara_{0}_hpp" },
            { Constants.PaymentSystems.CorefyMaldoCrypto, "maldo_crypto_{0}_hpp" }
        };

        private static Dictionary<string, string> PayoutServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CorefyCreditCard, "payment_card_{0}"}, //payment_card_usd_hpp
            { Constants.PaymentSystems.CorefyBankTransfer,"bank_transfer_{0}"},
            { Constants.PaymentSystems.CorefyHavale, "bank_transfer_{0}" },
            { Constants.PaymentSystems.CorefyPep, "pep_{0}"},
            { Constants.PaymentSystems.CorefyPayFix, "payfix_{0}" },
            { Constants.PaymentSystems.CorefyMefete, "mefete_{0}"},
            { Constants.PaymentSystems.CorefyParazula, "parazula_{0}"},
            { Constants.PaymentSystems.CorefyPapara, "papara_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoBTC, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoETH, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoETHBEP20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoLTC, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoXRP, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoXLM, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoBCH, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoLINKERC20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCERC20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCBEP20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCTRC20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTERC20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTBEP20, "maldo_crypto_{0}" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTTRC20, "maldo_crypto_{0}" }
        };

        private static Dictionary<string, string> CryptoNetworks { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.CorefyMaldoCryptoBTC, "BTC|BTC" },
            { Constants.PaymentSystems.CorefyMaldoCryptoETH, "ETH|ETH" },
            { Constants.PaymentSystems.CorefyMaldoCryptoETHBEP20, "ETH|BSC" },
            { Constants.PaymentSystems.CorefyMaldoCryptoLTC, "LTC|LTC" },
            { Constants.PaymentSystems.CorefyMaldoCryptoXRP, "XRP|XRP" },
            { Constants.PaymentSystems.CorefyMaldoCryptoXLM, "XLM|XLM" },
            { Constants.PaymentSystems.CorefyMaldoCryptoBCH, "BCH|BCH" },
            { Constants.PaymentSystems.CorefyMaldoCryptoLINKERC20, "LINK|ETH" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCERC20, "USDC|ETH" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCBEP20, "USDC|BSC" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDCTRC20, "USDC|TRX" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTERC20, "USDT|ETH" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTBEP20, "USDT|BSC" },
            { Constants.PaymentSystems.CorefyMaldoCryptoUSDTTRC20, "USDT|TRX" }
        };


        public static string CallCorefyApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.FirstName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CorefyApiUrl);
                var isTest = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.CorefyIsTest);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
               
                var paymentInput = new PaymentInput
                {
                    Data = new InputDataModel
                    {
                        Type = "payment-invoices",
                        Attributes = new InputAttribute
                        {
                            Service = string.Format(PaymentServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            ReferenceId = input.Id.ToString(),
                            Currency = client.CurrencyId,
                            Amount = Math.Round(input.Amount, 2),
                            Customer = new CustomerModel
                            {
                                ReferenceId = client.Id.ToString(),
                                Name = $"{client.FirstName} {client.LastName}",
                                Email = client.Email,
                                Phone = client.MobileNumber,
                                DateOfBirth = "1987-08-27",
                                Address = new AddressModel
                                {
                                    FullAddress = client.Address,
                                    Country = session.Country,
                                    PostCode = "dummy",
                                    City = session.Country
                                }
                            },
                            TestMode = !string.IsNullOrEmpty(isTest) && isTest == "1",
                            ReturnUrl = cashierPageUrl,
                            CallbackUrl = $"{paymentGateway}/api/Corefy/ApiRequest",
                           
                        }
                    }
                };
                if(paymentSystem.Name == Constants.PaymentSystems.CorefyPapara)
                {
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    paymentInput.Data.Attributes.ServiceFields = new ServiceFields
                    {
                        IdentificationNumber = paymentInfo.Info != string.Empty ? paymentInfo.Info : null
                    };
                }    
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payment-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };
                log.Info("Corefay: " + JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    input.ExternalTransactionId =  paymentOutput.Data.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    if (paymentOutput.Data.Attributes.Resolution != "ok")
                        log.Error(paymentOutput.Data.Attributes.Resolution);
                    return paymentOutput.Data.Attributes.HppUrl;
                }
                throw new Exception(resp);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                   paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.CorefyApiUrl);
                var isTest = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.CorefyIsTest);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
                if (!PayoutServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);
                var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
                var accountNumber = paymentInfo.AccountNumber != string.Empty ? paymentInfo.AccountNumber : null;
                if (string.IsNullOrEmpty(client.FirstName))
                    BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (paymentSystem.Name == Constants.PaymentSystems.CorefyPapara || paymentSystem.Name == Constants.PaymentSystems.CorefyParazula ||
                    paymentSystem.Name == Constants.PaymentSystems.CorefyPep)
                    accountNumber = paymentInfo.WalletNumber != string.Empty ? paymentInfo.WalletNumber : null;
                else if (paymentSystem.Name == Constants.PaymentSystems.CorefyPep)
                    paymentInfo.BankBranchName =  $"{client.FirstName} {client.LastName}";
                else if(paymentSystem.Name == Constants.PaymentSystems.CorefyHavale)
                {
                    var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                    if (bankInfo == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                    paymentInfo.BankBranchName = bankInfo.BankCode;
                }
                string beneficiaryName = null;
                if (paymentSystem.Name == Constants.PaymentSystems.CorefyParazula || paymentSystem.Name == Constants.PaymentSystems.CorefyPayFix ||
                    paymentSystem.Name == Constants.PaymentSystems.CorefyPep ||  paymentSystem.Name == Constants.PaymentSystems.CorefyMefete ||
                    paymentSystem.Name == Constants.PaymentSystems.CorefyHavale)
                    {
                        beneficiaryName = $"{client.FirstName} {client.LastName}";
                    }

                if(paymentSystem.Name == Constants.PaymentSystems.CorefyBankTransfer)
                    beneficiaryName=paymentInfo.BeneficiaryName;
                else if (!string.IsNullOrEmpty(paymentInfo.BankAccountHolder))
                    beneficiaryName = paymentInfo.BankAccountHolder;
                var paymentInput = new PaymentInput
                {
                    Data = new InputDataModel
                    {
                        Type = "payout-invoice",
                        Attributes = new InputAttribute
                        {
                            Service = string.Format(PayoutServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            ReferenceId = paymentRequest.Id.ToString(),
                            Currency = client.CurrencyId,
                            Amount = amount,
                            Fields = new InputFields
                            {
                                CardNumber = paymentInfo.CardNumber != string.Empty ? paymentInfo.CardNumber : null,
                                AccountNumber = accountNumber,
                                DocumentId = paymentInfo.DocumentId != string.Empty ? paymentInfo.DocumentId : null,
                                // UserName = paymentInfo.BeneficiaryName != string.Empty ? paymentInfo.BeneficiaryName : null,
                                MaldoCrypto = paymentSystem.Name.StartsWith(Constants.PaymentSystems.CorefyMaldoCrypto) && paymentInfo.WalletNumber != string.Empty ? paymentInfo.WalletNumber : null,
                                PhoneNumber = paymentInfo.MobileNumber != string.Empty ? paymentInfo.MobileNumber : null,
                                BranchCode = paymentSystem.Name != Constants.PaymentSystems.CorefyHavale && paymentInfo.BankBranchName != string.Empty ? paymentInfo.BankBranchName : null,
                                BankBranchCode = paymentInfo.BankBranchName != string.Empty ? paymentInfo.BankBranchName : null,
                                BeneficiaryFullName = beneficiaryName,
                                BeneficiaryAccountNumber = paymentInfo.BankAccountNumber != string.Empty ? paymentInfo.BankAccountNumber : null
                            },
                            Customer = new CustomerModel
                            {
                                ReferenceId = client.Id.ToString(),
                                Name = $"{client.FirstName} {client.LastName}",
                                Email = client.Email,
                                Phone = client.MobileNumber,
                                DateOfBirth = "1987-08-27",
                                Address = new AddressModel
                                {
                                    FullAddress = client.Address,
                                    Country = paymentInfo.Country,
                                    PostCode = "dummy",
                                    City = paymentInfo.Country
                                }
                            },
                            TestMode = !string.IsNullOrEmpty(isTest) && isTest == "1",
                            CallbackUrl = $"{paymentGateway}/api/Corefy/ApiRequest"
                        }
                    }
                };
                if (paymentSystem.Name.StartsWith(Constants.PaymentSystems.CorefyMaldoCrypto))
                    paymentInput.Data.Attributes.MetadataFields = new Metadata
                    {
                        CryptoCoin = CryptoNetworks[paymentSystem.Name]
                    };
                else if(paymentSystem.Name.StartsWith(Constants.PaymentSystems.CorefyHavale))
                    paymentInput.Data.Attributes.MetadataFields = new Metadata
                    {
                        BankCode = paymentInfo.BankBranchName
                    };
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payout-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    paymentRequest.ExternalTransactionId =  paymentOutput.Data.Id;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    if (paymentOutput.Data.Attributes.Resolution == "ok")
                        return new PaymentResponse
                        {
                            Status = PaymentRequestStates.PayPanding,
                        };
                    throw new Exception(paymentOutput.Data.Attributes.Resolution);
                }
                throw new Exception(resp);
            }
        }
    }
}