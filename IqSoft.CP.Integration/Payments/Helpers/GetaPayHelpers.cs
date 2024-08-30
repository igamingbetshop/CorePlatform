using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models;
using log4net;
using System;
using System.Collections.Generic;
using IqSoft.CP.Common.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IqSoft.CP.Common.Helpers;
using Org.BouncyCastle.Asn1.Ocsp;
using IqSoft.CP.Integration.Payments.Models.GetaPay;
using Microsoft.IdentityModel.Tokens;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class GetaPayHelpers
    {
        public static PaymentResponse CreatePayoutRequest(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);


            #region check
            if (string.IsNullOrEmpty(client.FirstName?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.LastName?.Trim()))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (string.IsNullOrEmpty(client.MobileNumber))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            #endregion

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                paymentRequest.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.GetaPayUrl).StringValue;
            var amount = paymentRequest.Amount - (paymentRequest.CommissionAmount ?? 0);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(paymentRequest.Info);
            var paymentSystem = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId);
            var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);

            if (paymentRequest.CurrencyId != Constants.Currencies.USADollar)
            {
                var rate = BaseBll.GetCurrenciesDifference(client.CurrencyId, Constants.Currencies.USADollar);
                amount = Math.Round(rate * amount, 2);
                parameters.Add("Currency", client.CurrencyId);
                parameters.Add("AppliedRate", rate.ToString("F"));
            }

            string stringAmount = amount.ToString("F");
            string callbackurl = $"{paymentGateway}/api/GetaPay/ApiRequest";
            string payout = "Payout";
            var elements = new List<string>
            {
               client.MobileNumber,
               stringAmount,
               paymentInfo.CardNumber,
               paymentRequest.Id.ToString(),
               payout,
               client.CurrencyId,
               partnerPaymentSetting.UserName,
               callbackurl,
               client.UserName,
               client.Email
            };

            elements.Sort(StringComparer.Ordinal);
            var nonNullElements = elements.Where(e => !string.IsNullOrEmpty(e)).ToList();
            var sortedValue = string.Join("|", nonNullElements);
            var signature = CommonFunctions.HashHMACHex(sortedValue, partnerPaymentSetting.Password).ToLower();
            var paymentInput = new
            {
                project = partnerPaymentSetting.UserName,
                user_contact_email = client.Email,
                user_phone = client.MobileNumber,
                user_name = client.UserName,
                amount = stringAmount,
                currency = client.CurrencyId,
                result_url = callbackurl,
                description = payout,
                destination_card = paymentInfo.CardNumber,
                order_id = paymentRequest.Id.ToString(),
                signature = signature,
            };

            log.Info("GetaPay _" + JsonConvert.SerializeObject(paymentInput));
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(paymentInput)
            };

            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(resp);
            if (paymentOutput.IsSuccess)
            {
                using (var paymentSystemBl = new PaymentSystemBll(session, log))
                {
                    paymentRequest.ExternalTransactionId = paymentOutput.PayoutId;
                    paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    return new PaymentResponse
                    {
                        Status = PaymentRequestStates.PayPanding,
                    };
                }
            }
            throw new Exception(paymentOutput.Message);
        }
    }
}
