using IqSoft.CP.Common.Helpers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.Help2Pay;
using log4net;
using System;
using IqSoft.CP.Integration.Payments.Models;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    class Help2PayHelpers
    {
        public static string CallHelp2PayApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrWhiteSpace(input.Info))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Help2PayApiUrl).StringValue;
                var now = DateTime.UtcNow.AddHours(8);//UTC(+8)
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
                if (bankInfo == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var requestInput = new PaymentRequestInput
                {
                    Merchant = partnerPaymentSetting.UserName,
                    Currency = input.CurrencyId,
                    Customer = client.Id.ToString(),
                    Reference = input.Id.ToString(),
                    Amount = input.Amount.ToString("F"),
                    Note = partner.Name,
                    Datetime = now.ToString("yyyy-MM-dd hh:mm:sstt"),
                    FrontURI = cashierPageUrl,
                    BackURI = string.Format("{0}/api/Help2Pay/ApiRequest", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                    Bank = bankInfo.BankCode,
                    Language = CommonHelpers.LanguageISO5646Codes[session.LanguageId],
                    ClientIP = session.LoginIp
                };

                var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                               requestInput.Merchant, requestInput.Reference,
                                               requestInput.Customer, requestInput.Amount,
                                               requestInput.Currency, now.ToString("yyyyMMddHHmmss"),
                                               partnerPaymentSetting.Password, requestInput.ClientIP);
                requestInput.Key = CommonFunctions.ComputeMd5(signature);

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = System.Net.Http.HttpMethod.Post,
                    Url =string.Format("https://api.{0}/MerchantTransfer", url),
                    PostData = CommonFunctions.GetUriEndocingFromObject(requestInput)
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
             
                var returnUrl = string.Format(CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl).StringValue, session.Domain);
                var parameters = ParseHtmlResponse(resp);
                if (parameters != null && parameters.Count != 0)
                {
                    var regEx = new Regex(" action=\"(.*)\"");
                    var action = regEx.Matches(resp)[0].Groups[1].Value;
                    return string.Format("{0}/help2pay/PaymentRequest?apiName={1}&{2}",
                           returnUrl, action, string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))));
                }
                
                return string.Format("{0}/help2pay/errorRequest?params={1}", returnUrl, resp);
            }
        }

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using var paymentSystemBl = new PaymentSystemBll(session, log);
            var client = CacheManager.GetClientById(input.ClientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var now = DateTime.UtcNow.AddHours(8);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            var bankInfo = paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId));
            if (bankInfo == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BankIsUnavailable);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.Help2PayApiUrl).StringValue;
            var amount = input.Amount - (input.CommissionAmount ?? 0);
            var requestInput = new PayoutRequestInput
            {
                ClientIp = session.LoginIp,
                ReturnURI = string.Format("{0}/api/Help2Pay/PayoutResult", CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue),
                MerchantCode = partnerPaymentSetting.UserName,
                TransactionID = input.Id.ToString(),
                CurrencyCode = input.CurrencyId,
                MemberCode = client.Id.ToString(),
                Amount = amount.ToString("F"),
                TransactionDateTime = now.ToString("yyyy-MM-dd hh:mm:sstt"),
                BankCode = bankInfo.BankCode,
                toBankAccountName = paymentInfo.BankAccountHolder,
                toBankAccountNumber = paymentInfo.BankAccountNumber
            };
            var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                              requestInput.MerchantCode, requestInput.TransactionID,
                                              requestInput.MemberCode, amount.ToString("F"),
                                              requestInput.CurrencyCode, now.ToString("yyyyMMddHHmmss"),
                                              requestInput.toBankAccountNumber, partnerPaymentSetting.Password);
            requestInput.Key = CommonFunctions.ComputeMd5(signature);

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Url = string.Format("https://app.{0}/merchantpayout/{1}", url, partnerPaymentSetting.UserName),
                PostData = CommonFunctions.GetUriEndocingFromObject(requestInput)
            };
            var deserializer = new XmlSerializer(typeof(PayoutOutput), new XmlRootAttribute("Payout"));
            using var stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12);
            var output = (PayoutOutput)deserializer.Deserialize(stream);
            return new PaymentResponse
            {
                Status = output.StatusCode == "000" ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed,
                Description = output.Message
            };
        }

        private static Dictionary<string, string> ParseHtmlResponse(string source)
        {
            if (source == null) 
                return null;
            var regEx = new Regex("input type='hidden' name='(.*)' value='(.*)'");

            var matches = regEx.Matches(source);
            var results = new Dictionary<string, string>();
            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value, match.Groups[2].Value);
            }
            return results;
        }
    }
}