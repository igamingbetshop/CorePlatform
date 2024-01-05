using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Ngine;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class NgineHelpers
    {
        public static string CallNgineApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.FirstName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.ZipCode))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ZipCodeCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.MobileNumber))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);
                var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                 JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                parameters.Add("Domain", session.Domain);
                input.Parameters = JsonConvert.SerializeObject(parameters);
                paymentSystemBl.ChangePaymentRequestDetails(input);
                var city = string.Empty;
                using (var regionBl = new RegionBll(session, log))
                {
                    var regionPath = regionBl.GetRegionPath(client.RegionId);
                    var cityPath = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                    if (cityPath != null)
                        city = CacheManager.GetRegionById(cityPath.Id ?? 0, client.LanguageId)?.Name;
                }
                if (string.IsNullOrWhiteSpace(city))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var paymentGatewayUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var resourcesUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResourcesUrl);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);

                var paymentProcessingInput = new
                {
                    OrderId = input.Id,
                    RedirectUrl = cashierPageUrl,
                    ResponseUrl = string.Format("{0}/api/Ngine/ProcessPaymentRequest", paymentGatewayUrl),
                    CancelUrl = string.Format("{0}/api/Ngine/CancelPaymentRequest", paymentGatewayUrl),
                    input.Amount,
                    Currency = input.CurrencyId,
                    BillingAddress = client.Address?.Trim(),
                    HolderName = string.Format("{0} {1}", client.FirstName, client.LastName),
                    PartnerDomain = session.Domain,
                    ResourcesUrl = (resourcesUrlKey == null || resourcesUrlKey.Id == 0 ? ("https://resources." + session.Domain) : resourcesUrlKey.StringValue),
                    session.LanguageId,
                    CountryCode = paymentInfo.Country,
                    ZipCode = client.ZipCode?.Trim(),
                    City = paymentInfo.City,
                    client.PartnerId,
                    PaymentSystemName = input.PaymentSystemName.ToLower()
                };
                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                var distributionUrl = string.Format(distributionUrlKey.StringValue, session.Domain);
                var data = AESEncryptHelper.EncryptDistributionString(JsonConvert.SerializeObject(paymentProcessingInput));
                return string.Format("{0}/paymentform/paymentprocessing?data={1}", distributionUrl, data);
            }
        }


        public static string CallNgineZelleApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.NgineZelle);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                           input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NgineApiUrl).StringValue;
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                        var amount = input.Amount - (input.CommissionAmount ?? 0);
                        var postData = JsonConvert.SerializeObject(new
                        {
                            UserLogin = input.Id.ToString(), 
                            UserPassword = client.Id.ToString(),
                            InstanceID = 1
                        });
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "api/Authentication/GenerateToken"),
                            PostData = postData,
                        };
                        var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"GenerateToken: {postData} , {response}");
                        var output = JsonConvert.DeserializeObject<AuthenticationOutput>(response);  

                        postData = JsonConvert.SerializeObject(new
                        {
                            Token = output.Authentication.Token,
                            URL = partnerPaymentSetting.Password,
                            ProcessorID = partnerPaymentSetting.UserName,

                        });
                        httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "/api/Quicker/QuickerBankAccounts"),
                            PostData = postData,
                        };
                        response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"QuickerBankAccounts: {postData} , {response}");
                        var bankAccounts = JsonConvert.DeserializeObject<QuickerBankAccounts>(response);

                        postData = JsonConvert.SerializeObject(new
                        {
                            Token = output.Authentication.Token,
                            Amount = input.Amount,
                            ProcessorID = partnerPaymentSetting.UserName,
                            BankAccountID = bankAccounts.Authentication.BankAccounts.FirstOrDefault(x => x.BankName == "Zelle").BankAccountID,
                            SendInstructions = true,
                            isDirectDeposit = true
                        });
                        httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "/api/quicker/QuickerInitiateTransaction"),
                            PostData = postData,
                        };
                        response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"QuickerTransaction: {postData} , {response}");
                        var transaction = JsonConvert.DeserializeObject<DepositOutput>(response);

                        if (transaction.Authentication.Status == "REQUESTED")
                        {
                            input.ExternalTransactionId = transaction.Authentication.TransactionID;
                            input.Info = response;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            return "ConfirmationCode";
                        }
                        else
                            throw new Exception(string.Format("Status: {0} ErrorDescription: {1}", transaction.Authentication.Status, transaction.Authentication.ErrorDescription ?? 
                                                                                                                                      transaction.Authentication.HtmlResponse));                        
                    }
                }
            }
        }
        public static string CallNgineApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NgineUrl).StringValue;
                return $"{url}?CustomerPIN={input.Id}&Password={client.Id}";
            }
        }

        public static PaymentResponse PayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        if (string.IsNullOrEmpty(client.FirstName))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                        if (string.IsNullOrEmpty(client.LastName))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Ngine);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NgineApiUrl).StringValue;
                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                        var amount = input.Amount - (input.CommissionAmount ?? 0);
                        var postData = JsonConvert.SerializeObject(new 
                        {
                            UserLogin = input.Id.ToString(),
                            UserPassword = client.Id.ToString(),
                            InstanceID = 1
                        });
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "api/Authentication/GenerateToken"),
                            PostData = postData,
                        };
                        var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"GenerateToken: {postData} , {response}");
                        var output = JsonConvert.DeserializeObject<AuthenticationOutput>(response);


                        postData = JsonConvert.SerializeObject(new
                        {
                            Token = output.Authentication.Token,
                            Currency = client.CurrencyId,
                            Amount = amount
                        });
                        httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "api/Payouts/GetPayoutLimits"),
                            PostData = postData,
                        };
                        response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"GetPayoutLimits: {postData} , {response}");
                        var limitsOutput = JsonConvert.DeserializeObject<GetPayoutLimitsOutput>(response);

                        postData = JsonConvert.SerializeObject(new 
                        {
                            Token = output.Authentication.Token,
                            CurrencyCode = client.CurrencyId,
                            Amount = amount,
                            Identifier = paymentInfo.WalletNumber,
                            FirstName = client.FirstName,
                            LastName = client.LastName,
                            BonusList = "",
                            ProcessorID = limitsOutput?.Authentication?.FirstOrDefault()?.ProcessorID,

                        });
                        httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format(url, "api/Payouts/GenericPayout"),
                            PostData = postData,
                        };
                        response =  CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                        log.Info($"GenericPayout: {postData} , {response}");
                        var payoutOutput = JsonConvert.DeserializeObject<GenericPayoutOutput>(response);

                        if (payoutOutput.Authentication.Status == "REQUESTED")
                        {
                            input.ExternalTransactionId = payoutOutput.Authentication.TransactionID;
                            input.Info = response;
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.PayPanding,
                            };
                        }
                        else
                            throw new Exception(string.Format("Status: {0} ErrorDescription: {1}", payoutOutput.Authentication.Status, payoutOutput.Authentication.ErrorDescription));
                    }
                }
            }
        }
    }    
}
