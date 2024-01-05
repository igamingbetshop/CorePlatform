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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IqSoft.CP.Integration.Payments.Models.FinVert;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public class FinVertHelpers
    {
        enum ResponseCodes
        {
            Fail,
            Success,
            Pending,
            Cancelled,
            ToBeConfirm,
            Blocked,
            Unathorized,
            Redirected
        }
        public static string CallFinVertApi(PaymentRequest input, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(input.ClientId.Value);
                if (string.IsNullOrEmpty(client.FirstName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.FirstNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.LastName?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.LastNameCantBeEmpty);
                if (string.IsNullOrEmpty(client.Email?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
                if (string.IsNullOrEmpty(client.CurrencyId?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.CurrencyNotExists);
                if (string.IsNullOrEmpty(cashierPageUrl?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var clientIp = paymentInfo.TransactionIp;
                if (string.IsNullOrEmpty(clientIp?.Trim()))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);

                if (string.IsNullOrEmpty(paymentInfo.Country) || string.IsNullOrEmpty(paymentInfo.City))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.RegionNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId,
                                                                                   input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.FinVertUrl).StringValue;
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var amount = input.Amount;

                var paymentInput = JsonConvert.SerializeObject(new
                {

                    first_name = client.FirstName,
                    last_name = client.LastName,
                    email = client.Email,
                    amount = amount.ToString("F"),
                    currency = client.CurrencyId,
                    ip_address = clientIp,
                    address = client.Address,
                    country = paymentInfo.Country,
                    state = paymentInfo.City,
                    city = paymentInfo.City,
                    zip = client.ZipCode.Trim(),
                    phone_no = client.MobileNumber,
                    customer_order_id = input.Id.ToString(),
                    response_url = cashierPageUrl,
                    webhook_url = string.Format("{0}/api/FinVert/ApiRequest", paymentGateway)

                });

                var headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer " + partnerPaymentSetting.UserName }
                };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    RequestHeaders = headers,
                    Date = DateTime.Now.ToUniversalTime(),
                    Url = url,
                    PostData = paymentInput
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var paymentOutput = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                log.Info(JsonConvert.SerializeObject(paymentOutput));
                if (!string.IsNullOrEmpty(paymentOutput?.Data?.Transaction?.OrderId))
                {
                    input.ExternalTransactionId = paymentOutput.Data.Transaction.OrderId;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                }
                ResponseCodes responseCode;
                if (Enum.TryParse(paymentOutput.ResponseCode, out responseCode) && responseCode == ResponseCodes.Redirected)
                    return paymentOutput.ThreeDomainSequreUrl;
                throw new Exception(string.Format("ErrorCode: {0}, ErrorMessage: {1}", paymentOutput.ResponseCode, paymentOutput.ResponseMessage));
            }
        }
    }
}
