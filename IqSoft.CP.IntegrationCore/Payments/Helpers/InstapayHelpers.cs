using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Instapay;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class InstapayHelpers
    {
        private readonly static Dictionary<string, string> InstaMethods = new Dictionary<string, string>
        {
            {Constants.PaymentSystems.InstaMFT, "deposit" },
            {Constants.PaymentSystems.InstaPapara, "paparaDeposit" },
            {Constants.PaymentSystems.InstaPeple, "pepDeposit" },
            {Constants.PaymentSystems.InstaKK, "deposit" }
        };
        private readonly static Dictionary<string, string> InstaWithdrawMethods = new Dictionary<string, string>
        {
            {Constants.PaymentSystems.InstaMFT, "withdraw" },
            {Constants.PaymentSystems.InstaPapara, "paparaWithdraw" },
            {Constants.PaymentSystems.InstaPeple, "pepWithdraw" }
        };
        public static string CallInstapayApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                        var url = CacheManager.GetPartnerSettingByKey(client.PartnerId,
                            paymentSystem.Name == Constants.PaymentSystems.InstaMFT ? Constants.PartnerKeys.InstaMFTUrl : Constants.PartnerKeys.InstapayUrl).StringValue;
                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var methodName = InstaMethods[paymentSystem.Name];
                        var paymentRequestInput = new
                        {
                            method = methodName,
                            transaction_id = input.Id,
                            user_id = client.Id,
                            name_surname = string.Format("{0} {1}", client.FirstName, client.LastName),
                            note = partner.Name,
                            hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", methodName != "deposit" ? methodName : string.Empty,
                                                                                            input.Id, client.Id, partnerPaymentSetting.UserName))
                        };

                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = System.Net.Http.HttpMethod.Post,
                            Url = string.Format("{0}?api_token={1}", url, partnerPaymentSetting.Password),
                            PostData = JsonConvert.SerializeObject(paymentRequestInput)
                        };
                        var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        if (response.Status.ToLower() == "success")
                            return response.RedirectUrl;
                        return response.Message;
                    }
                }
            }
        }

        public static string CallInstaApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InstaKKUrl).StringValue;
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentRequestInput = new
                {
                    method = InstaMethods[Constants.PaymentSystems.InstaKK],
                    transaction_id = input.Id,
                    user_id = client.Id,
                    first_name = client.Id,
                    last_name = client.UserName,
                    birth_date = client.BirthDate.ToString("yyyy-MM-dd"),
                    email = client.Email,
                    note = partner.Name,
                    hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", input.Id, client.Id, partnerPaymentSetting.UserName))
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = string.Format("{0}?api_token={1}", url, partnerPaymentSetting.Password),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Status.ToLower() == "success")
                    return response.RedirectUrl;
                return response.Message;
            }
        }

        public static string CallInstaCepbank(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InstaCepbankUrl).StringValue;
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var cashierPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashierPageUrl).StringValue;
                if (string.IsNullOrEmpty(cashierPageUrl))
                    cashierPageUrl = string.Format("https://{0}/user/1/deposit/", session.Domain);
                else
                    cashierPageUrl = string.Format(cashierPageUrl, session.Domain);

                var paymentRequestInput = new
                {
                    request_type = "deposit",
                    method = "cepbank",
                    merchant_ref = input.Id.ToString(),
                    eps_tran_ref = input.Id.ToString(),
                    amount = (int)input.Amount,
                    customer_ref = client.Id.ToString(),
                    firstname = client.FirstName,
                    lastname = client.LastName,
                    email = client.Email,
                    back_redirect_url = cashierPageUrl,
                    requestor_ip = session.LoginIp,
                    signature = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}{5}{6}", "deposit", input.Id, client.Id,
                            client.FirstName, client.LastName, (int)input.Amount, partnerPaymentSetting.Password))
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = string.Format("{0}deposit?api_token={1}", url, partnerPaymentSetting.UserName),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<CepbankOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Datas == null)
                    throw new Exception(response.Message);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}", partnerPaymentSetting.Password, response.Datas.Params.Id)).ToLower();
                if (hash!= response.Datas.Params.Hash.ToLower())
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongHash);
                var sign = CommonFunctions.ComputeSha256(string.Format("{0}{1}{2}{3}{4}{5}", partnerPaymentSetting.Password,
                    response.Datas.Method, hash, response.Datas.Params.Id, response.Datas.Params.Id, "/deposit/"));
                if (sign.ToLower()!= response.Signature)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongHash);

                input.ExternalTransactionId = response.Datas.Params.Id.ToString();
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return string.Format("{0}?hash={1}&id={2}", response.Datas.RedirectUrl, response.Datas.Params.Hash, response.Datas.Params.Id);
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId,
                    paymentSystem.Name == Constants.PaymentSystems.InstaMFT ? Constants.PartnerKeys.InstaMFTUrl : Constants.PartnerKeys.InstapayUrl).StringValue;

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.TurkishLira)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.TurkishLira, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.TurkishLira);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var methodName = InstaWithdrawMethods[paymentSystem.Name];
                var paymentRequestInput = new
                {
                    method = methodName,
                    transaction_id = input.Id,
                    user_id = client.Id,
                    name_surname = client.UserName,
                    username = client.UserName,
                    note = partner.Name,
                    amount,
                    account_number = paymentInfo.WalletNumber,
                    papara_no = paymentInfo.WalletNumber,
                    pep_no = paymentInfo.WalletNumber,
                    hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", methodName != "withdraw" ? methodName : string.Empty,
                                           input.Id, client.Id, partnerPaymentSetting.UserName))
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}?api_token={1}", url, partnerPaymentSetting.Password),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Status.ToLower() == "success")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = response.Message
                };
            }
        }
        public static PaymentResponse CreateCepbankPayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.InstaCepbankUrl).StringValue;

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                if (input.CurrencyId != Constants.Currencies.TurkishLira)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.TurkishLira, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                  JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.TurkishLira);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                if (!Int64.TryParse(paymentInfo.NationalId, out long nationalId))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

                var paymentRequestInput = new
                {
                    request_type = "withdraw",
                    bank_id = bankInfo.BankCode,
                    eps_tran_ref = input.Id,
                    customer_ref = client.Id,
                    firstname = client.FirstName,
                    lastname = client.LastName,
                    note = partner.Name,
                    amount = (int)amount,
                    receiver_tc = nationalId,
                    branch_number = paymentInfo.BankBranchName,
                    account_number = paymentInfo.BankAccountNumber,
                    iban_number = paymentInfo.BankIBAN,
                    signature = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "withdraw", bankInfo.BankCode, input.Id, client.Id,
                    (int)amount, paymentInfo.BankBranchName, paymentInfo.BankAccountNumber, paymentInfo.BankIBAN, partnerPaymentSetting.Password))
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url = string.Format("{0}process?api_token={1}", url, partnerPaymentSetting.UserName),
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Status.ToLower() == "success")
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.Failed,
                    Description = response.Message
                };
            }
        }
    }
}