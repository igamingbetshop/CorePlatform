using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.Integration.Payments.Models.Cartipal;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class CartipalHelpers
    {
        public static string CallCartipalApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var partnerBl = new PartnerBll(paymentSystemBl))
                {
                    using (var clientBl = new ClientBll(partnerBl))
                    {
                        var client = CacheManager.GetClientById(input.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var segment = clientBl.GetClientPaymentSegments(input.ClientId.Value, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                        var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                        var url = segment == null ? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CartipalApiUrl).StringValue : segment.ApiUrl;
                        var pass = segment == null ? partnerPaymentSetting.Password : segment.ApiKey;

                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var amount = input.Amount;
                        if (input.CurrencyId != Constants.Currencies.IranianRial)
                        {
                            var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.IranianRial, partnerPaymentSetting);
                            amount = Math.Round(rate * input.Amount, 2);
                            var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                             JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                            parameters.Add("Currency", Constants.Currencies.IranianRial);
                            parameters.Add("AppliedRate", rate.ToString("F"));
                            input.Parameters = JsonConvert.SerializeObject(parameters);
                            paymentSystemBl.ChangePaymentRequestDetails(input);
                        }
                        var notifyUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                        var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                        if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                            distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                        var distUrl = string.Format(distributionUrlKey.StringValue, session.Domain) +
                                string.Format("/notify/NotifyResult?orderId={0}&returnUrl={1}&domain={2}&providerName=Cartipal&methodName=ApiRequest", input.Id, cashierPageUrl, notifyUrl);

                        var paymentRequestInput = new
                        {
                            api_key = pass,
                            amount = Convert.ToInt32(amount),
                            return_url = distUrl,
                            description = partner.Name,
                            website_name = partner.Name,
                            user_id = client.Id,
                            reference = input.Id,
                            payment_type = paymentSystem.Name == Constants.PaymentSystems.Cartipay ? 1 : 2
                        };
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = string.Format("{0}/invoice/request", url),
                            PostData = CommonFunctions.GetUriEndocingFromObject(paymentRequestInput)
                        };
                        var response = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        if (response.Status == 0)
                            throw new Exception(response.ErrorDescription);
                        input.ExternalTransactionId = response.InvoiceKey;

                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        return string.Format("{0}/invoice/pay/{1}", url, response.InvoiceKey);
                    }
                }
            }
        }

        public static PaymentResponse CreatePayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CartipalApiUrl).StringValue;
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var amount = input.Amount - (input.CommissionAmount ?? 0);
                var callbackUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);

                if (input.CurrencyId != Constants.Currencies.IranianRial)
                {
                    var rate = BaseBll.GetPaymentCurrenciesDifference(client.CurrencyId, Constants.Currencies.IranianRial, partnerPaymentSetting);
                    amount = Math.Round(rate * input.Amount, 2);
                    var parameters = string.IsNullOrEmpty(input.Parameters) ? new Dictionary<string, string>() :
                                            JsonConvert.DeserializeObject<Dictionary<string, string>>(input.Parameters);
                    parameters.Add("Currency", Constants.Currencies.IranianRial);
                    parameters.Add("AppliedRate", rate.ToString("F"));
                    input.Parameters = JsonConvert.SerializeObject(parameters);
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                var paymentRequestInput = new
                {
                    api_key = partnerPaymentSetting.Password,
                    amount = amount,
                    first_name = client.UserName,
                    last_name = client.UserName,
                    account_number = paymentInfo.BankAccountNumber,
                    card_number = paymentInfo.CardNumber,
                    iban = paymentInfo.BankACH,
                    user_id = client.Id,
                    return_url = string.Format("{0}/api/Cartipal/PayoutRequest?payment={1}", callbackUrl, input.Id),
                    website_name =  string.Format("https//{0}", session.Domain),
                    reference = input.Id,
                    payment_type = paymentSystem.Name == Constants.PaymentSystems.Cartipay ? 1 : 2
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = string.Format("{0}/withdraw/request", url),
                    PostData = CommonFunctions.GetUriDataFromObject(paymentRequestInput)
                };
                var response = JsonConvert.DeserializeObject<PayoutOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Status == 0)
                {
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.Failed,
                        Description = response.ErrorDescription
                    };
                }
                input.ExternalTransactionId = response.WithdrawalId;
                paymentSystemBl.ChangePaymentRequestDetails(input);
                return new PaymentResponse
                {
                    Status = PaymentRequestStates.PayPanding,
                };
            }
        }
    }
}
