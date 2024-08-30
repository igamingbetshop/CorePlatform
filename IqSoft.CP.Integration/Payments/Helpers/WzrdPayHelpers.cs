using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Payments.Models.WzrdPay;
using IqSoft.CP.Common.Models;
using IqSoft.CP.BLL.Caching;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Integration.Payments.Models;
using System.Runtime.InteropServices;
using static IqSoft.CP.Integration.Platforms.Helpers.InsicHelpers;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class WzrdPayHelpers
    {
        private static Dictionary<string, string> PaymentServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.WzrdPayCreditCard, "payment_card_{0}_hpp"}, //payment_card_usd_hpp
        };

        private static Dictionary<string, string> PayoutServices { get; set; } = new Dictionary<string, string>
        {
            { Constants.PaymentSystems.WzrdPayCreditCard, "payment_card_{0}"}, //payment_card_usd_hpp   
        };

        public static string CallWZRDPayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(input.Info) ? input.Info : "{}");

                #region Check
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.FirstName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(client.MobileNumber))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                if (string.IsNullOrEmpty(client.MobileNumber))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                #endregion


                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.WzrdPayUrl);
                var isTest = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.WzrdPayIsTest);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                if (!PaymentServices.ContainsKey(paymentSystem.Name))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPaymentRequest);

                var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);
                var paymentInput = new PaymentInput
                {
                    Data = new InputDataModel
                    {
                        Type = "payment-invoices",
                        Attributes = new InputAttribute
                        {
                            Service = string.Format(PaymentServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            ReferenceId = input.Id.ToString(),
                            Amount = Math.Round(input.Amount, 2),
                            Description = "Invoice Example",
                            Currency = client.CurrencyId,
                            TestMode= string.IsNullOrEmpty(isTest) && isTest == "1",
                            Customer = new CustomerModel
                            {
                                ReferenceId = client.Id.ToString(),
                                Name = $"{client.FirstName} {client.LastName}",
                                Email = client.Email,
                                Phone = client.MobileNumber,
                                DateOfBirth = "1987-08-27",
                                Address = new AddressModel
                                {
                                    Country = paymentInfo.Country,
                                    Region = region?.IsoCode ?? region?.IsoCode3,
                                    City = paymentInfo.City,
                                    FullAddress = client.Address,
                                    PostCode = "dummy",
                                }
                            },
                            ReturnUrl = cashierPageUrl,
                            ReturnUrls = new Urls
                            {
                                Success = cashierPageUrl,
                                Pending = cashierPageUrl,
                                Fail = cashierPageUrl
                            },
                            CallbackUrl = $"{paymentGateway}/api/WzrdPay/ApiRequest",
                        }
                    }
                };

                log.Info("WzrdPay paymentInput: " + JsonConvert.SerializeObject(paymentInput));
                var PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payment-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };

                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info("WzrdPay: resp: " + JsonConvert.SerializeObject(resp));
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    input.ExternalTransactionId = paymentOutput.Data.Id;
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
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WzrdPayUrl);
                var isTest = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, paymentRequest.PaymentSystemId, Constants.PartnerKeys.WzrdPayIsTest);
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
                if (string.IsNullOrWhiteSpace(client.Address))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AddressCantBeEmpty);
                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);

                var region = CacheManager.GetRegionById(client.RegionId, session.LanguageId);
                var paymentInput = new PaymentInput
                {
                    Data = new InputDataModel
                    {
                        Type = "payout-services",
                        Attributes = new InputAttribute
                        {
                            Service = string.Format(PayoutServices[paymentSystem.Name], client.CurrencyId.ToLower()),
                            Currency = client.CurrencyId,
                            Amount = amount,
                            ReferenceId = paymentRequest.Id.ToString(),
                            TestMode = !string.IsNullOrEmpty(isTest) && isTest == "1",
                            Fields = new InputFields
                            {
                                CardNumber = paymentInfo.CardNumber != string.Empty ? paymentInfo.CardNumber : null,
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
                                    Country = paymentInfo.Country,
                                    Region = region?.IsoCode ?? region?.IsoCode3,
                                    City = paymentInfo.City,
                                    FullAddress = client.Address,
                                    PostCode = "dummy"
                                }
                            },
                            CallbackUrl = $"{paymentGateway}/api/WzrdPay/ApiRequest",
                            Options = new Options
                            {
                                AutoProcess = true,
                            }
                        },
                    }
                };

                log.Info("WZRDPayout _" + JsonConvert.SerializeObject(paymentInput));
                var basicAuth = Convert.ToBase64String(Encoding.Default.GetBytes($"{partnerPaymentSetting.UserName}"));
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {basicAuth}" } },
                    Url = $"{url}/payout-invoices",
                    PostData = JsonConvert.SerializeObject(paymentInput, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info("WZRDPayout   response _" + JsonConvert.SerializeObject(resp));
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                if (paymentOutput.Data != null && paymentOutput.Data.Attributes != null)
                {
                    paymentRequest.ExternalTransactionId = paymentOutput.Data.Id;
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
