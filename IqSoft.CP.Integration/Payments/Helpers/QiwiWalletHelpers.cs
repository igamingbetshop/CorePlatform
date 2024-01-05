using System;
using System.Text;
using System.Collections.Generic;
using log4net;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Integration.Payments.Models;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using System.Web.Script.Serialization;
using IqSoft.CP.Integration.Payments.Models.QiwiWallet;
using System.Linq;
namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class QiwiWalletHelpers
    {
        public static string CallQiwiWalletApi(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {               
                var url = partnerBl.GetPaymentValueByKey(null, input.PaymentSystemId, Constants.PartnerKeys.QiwiWalletDepositUrl);
                var client = CacheManager.GetClientById(input.ClientId.Value);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var API_ID = partnerPaymentSetting.UserName;
                var API_password = partnerPaymentSetting.Password;
                var prvId = partnerBl.GetPaymentValueByKey(client.PartnerId, input.PaymentSystemId, Constants.PartnerKeys.QiwiWalletProviderId);
                var partner = CacheManager.GetPartnerById(client.PartnerId);               
               
                var requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(API_ID + ":" + API_password)));
                var requestId = input.Id.ToString();
                var transactionId = requestId + requestId + requestId;

                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var mobile = paymentInfo.Info;
                if (string.IsNullOrEmpty(mobile))
                    mobile = paymentInfo.MobileNumber;
                if (!BaseBll.IsMobileNumber(mobile))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

                var createRequest = new CreateRequestInput
                {
                    User = "tel:" + mobile,
                    Amount = input.Amount.ToString(),
                    Account = input.ClientId.ToString(),
                    Currency = client.CurrencyId,
                    Comment = "",
                    LifeTime = DateTime.UtcNow.AddHours(4).ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                    ProviderName = partner.Name,
                    Extras = transactionId
                };
                var postData = ToQueryString(createRequest);
                byte[] bytes = Encoding.Default.GetBytes(postData);
                postData = Encoding.UTF8.GetString(bytes);
                log.Info("PostData = " + postData);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    Accept = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Put,
                    Url = url.Replace("{prv_id}", prvId).Replace("{bill_id}", transactionId),
                    RequestHeaders = requestHeaders,
                    PostData = postData
                };
                var serializer = new JavaScriptSerializer();
                var response = serializer.Deserialize<DepositRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                log.Info(JsonConvert.SerializeObject(response));
                if (response.response.result_code == ResponseCodes.Success)
                {
                    var depositUrlInput = new DepositUrlInput
                    {
                        shop = prvId,
                        transaction = response.response.bill.bill_id
                    };
                    var actionUrl = partnerBl.GetPaymentValueByKey(null, input.PaymentSystemId, Constants.PartnerKeys.QiwiWalletActionUrl);
                    return actionUrl + "?" + CommonFunctions.GetUriEndocingFromObject(depositUrlInput);
                }
                throw BaseBll.CreateException(session.LanguageId, GetErrorCode(response.response.result_code));
            }
        }

        private static class ResponseCodes
        {
            public const int Success = 0;
            public const int WrongParameterFormat = 5;
            public const int InvalidOperation = 78;
            public const int AuthorisationError = 150;
            public const int RequestNotFound = 210;
            public const int PaymentRequestAlreadyExist = 215;
            public const int AmountIsLess = 241;
            public const int AmountIsHigh = 242;
            public const int WalletNotFound = 298;
            public const int WrongMobileNumber = 303;
            public const int NotAllowedAction = 319;
            public const int ExceededLimits = 700;
            public const int WalletIsBlocked = 774;
            public const int ForbiddenCurrency = 1001;
        }

        private readonly static Dictionary<int, int> ResponseCodesMapping = new Dictionary<int, int>
        {
            {ResponseCodes.WrongParameterFormat, Constants.Errors.WrongParameters},
            {ResponseCodes.InvalidOperation, Constants.Errors.NotAllowed},
            {ResponseCodes.AuthorisationError, Constants.Errors.PartnerPaymentSettingNotFound},//?
            {ResponseCodes.RequestNotFound,  Constants.Errors.WrongPaymentRequest},
            {ResponseCodes.PaymentRequestAlreadyExist, Constants.Errors.PaymentRequestNotAllowed},//?
            {ResponseCodes.AmountIsLess, Constants.Errors.PaymentRequestInValidAmount},
            {ResponseCodes.AmountIsHigh, Constants.Errors.PaymentRequestInValidAmount},
            {ResponseCodes.WalletNotFound, Constants.Errors.PartnerPaymentSettingNotFound},//??
            {ResponseCodes.WrongMobileNumber, Constants.Errors.InvalidMobile},
            {ResponseCodes.NotAllowedAction, Constants.Errors.NotAllowed},
            {ResponseCodes.ExceededLimits, Constants.Errors.PartnerProductLimitExceeded},
            {ResponseCodes.ForbiddenCurrency, Constants.Errors.CurrencyNotExists},
            {ResponseCodes.WalletIsBlocked, Constants.Errors.PartnerPaymentSettingBlocked}
        };

        public static int GetErrorCode(int ourErrorCode)
        {
            if (ResponseCodesMapping.ContainsKey(ourErrorCode))
                return ResponseCodesMapping[ourErrorCode];
            return Constants.Errors.GeneralException;
        }

        public static string ToQueryString(CreateRequestInput request, string separator = ",")
        {
            if (request == null)
                throw new ArgumentNullException("request");
            // Get all properties on the object
            var properties = request.GetType().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(request, null) != null)
                .ToDictionary(
                x => x.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Cast<JsonPropertyAttribute>().
                Select(jp => jp.PropertyName != null ? jp.PropertyName : x.Name).First(),
                x => x.GetValue(request, null));

            // Concat all key/value pairs into a string separated by ampersand
            return string.Join("&", properties
                .Select(x => string.Concat(
                    Uri.EscapeDataString(x.Key), "=",
                    Uri.EscapeDataString(x.Value.ToString()))));
        }
    }
}

