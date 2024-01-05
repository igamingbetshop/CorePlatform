using System;
using System.IO;
using System.Xml.Serialization;
using log4net;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Payments.Models.Wooppay;
using IqSoft.CP.Integration.WooppayCustomer;
using System.Net;
using Newtonsoft.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class Methods
    {
        public const string StartTransaction = "startTransaction";
    }
    public static class ErrorCodes
    {
        public const int WrongFormat = 1;
        public const int WrongCommand = 2;
        public const int InernalError = 3;
        public const int AccessDenied = 4;
        public const int Login = 5;
        public const int WrongOperation = 11;
        public const int WrongCredentials = 101;
        public const int WrongStartBalance = 157;
        public const int Duplicated = 158;
        public const int WrongService = 213;
        public const int ExceededLimits = 214;
        public const int WrongAmount = 215;
        public const int DuplicateOperation = 221;
        public const int WrongCode = 223;
        public const int ExceededAttempts = 224;
        public const int WrongNumber = 225;
        public const int InsufficientBalance = 226;
        public const int IpDenied = 308;
    }
   
    public static class WooppayHelpers
    {
        private static int PaymentSystemId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.WooppayMobile).Id;

        private static Dictionary<int, int> Errors { get; set; } = new Dictionary<int, int>
        {
            {ErrorCodes.Login,  Constants.Errors.WrongLoginParameters},
            {ErrorCodes.Duplicated,  Constants.Errors.TransactionAlreadyExists},
            {ErrorCodes.ExceededLimits,  Constants.Errors.ClientMaxLimitExceeded},
            {ErrorCodes.WrongAmount,  Constants.Errors.WrongOperationAmount},
            {ErrorCodes.DuplicateOperation,  Constants.Errors.TransactionAlreadyExists},
            {ErrorCodes.WrongCode,  Constants.Errors.WrongVerificationKey},
            {ErrorCodes.WrongNumber,  Constants.Errors.WrongInputParameters},
            {ErrorCodes.InsufficientBalance,  Constants.Errors.LowBalance}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return Constants.Errors.GeneralException;
        }
        public static string CallWooppayApi(PaymentRequest input, int partnerId, SessionIdentity session, ILog log)
		{
			using (var partnerBl = new PartnerBll(session, log))
			{
				var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId,
					input.CurrencyId, (int)PaymentRequestTypes.Deposit);

				var loginRequest = new CoreLoginRequest
				{
					username = partnerPaymentSetting.UserName,
					password = partnerPaymentSetting.Password
				};
				var xmlControllerPortTypeClient = new XmlControllerPortTypeClient();
				var loginResponse = xmlControllerPortTypeClient.core_login(loginRequest);

                if (loginResponse.error_code != 0)
                    throw BaseBll.CreateException(session.LanguageId, GetErrorCode(loginResponse.error_code));
				using (OperationContextScope scope = new OperationContextScope(xmlControllerPortTypeClient.InnerChannel))
				{
					/*MessageHeader header = MessageHeader.CreateHeader(
											"Cookie",
											"",
											"session=" + loginResponse.response.session
										);
					OperationContext.Current.OutgoingMessageHeaders.Add(header);
					*/
					var prop = new HttpRequestMessageProperty();
					prop.Headers.Add(HttpRequestHeader.Cookie, "session=" + loginResponse.response.session);
					OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, prop);
					
					var partner = CacheManager.GetPartnerById(partnerId);
					var expDate = DateTime.UtcNow.AddHours(12);
					var createPaymentRequest = new CashCreateInvoiceRequest
					{
						referenceId = input.Id.ToString(),
						backUrl = "https://" + session.Domain,
						requestUrl = string.Format("{0}/api/Wooppay/ApiRequest?OrderId={1}&Amount={2}",
                        partnerBl.GetPaymentValueByKey(partnerId, null, Constants.PartnerKeys.PaymentGateway), input.Id, input.Amount),
						addInfo = partner.Name,
						amount = (float)input.Amount,
						deathDate = expDate.ToString("yyyy-MM-dd HH:mm:ss"),
						description = string.Empty
					};
					log.Info(JsonConvert.SerializeObject(createPaymentRequest));
					var response = xmlControllerPortTypeClient.cash_createInvoice(createPaymentRequest);
					log.Info(JsonConvert.SerializeObject(response));

					if (response.response == null)
                        throw BaseBll.CreateException(session.LanguageId, GetErrorCode(response.error_code));

					using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
					{
						input.ExternalTransactionId = response.response.operationId.ToString();
						paymentSystemBl.ChangePaymentRequestDetails(input);
					}

					return response.response.operationUrl;
				}
			}
        }


        public static void SendSMSCodeWooppayApi(int clientId, string mobileNumber, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystemId,
                    client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var loginRequest = new CoreLoginRequest
                {
                    username = partnerPaymentSetting.UserName,
                    password = partnerPaymentSetting.Password
                };
                var xmlControllerPortTypeClient = new XmlControllerPortTypeClient();
                var loginResponse = xmlControllerPortTypeClient.core_login(loginRequest);

                if (loginResponse.error_code != 0)
                {
                    log.Error("loginResponse_" + JsonConvert.SerializeObject(loginResponse));
                    throw BaseBll.CreateException(session.LanguageId, GetErrorCode(loginResponse.error_code));
                }
                using (OperationContextScope scope = new OperationContextScope(xmlControllerPortTypeClient.InnerChannel))
                {
                    var prop = new HttpRequestMessageProperty();
                    prop.Headers.Add(HttpRequestHeader.Cookie, "session=" + loginResponse.response.session);
                    OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, prop);

                    var core_requestConfirmationCode = new CoreRequestConfirmationCodeRequest
                    {
                        phone = mobileNumber.Replace("+7", string.Empty)
                    };

                    var response = xmlControllerPortTypeClient.core_requestConfirmationCode(core_requestConfirmationCode);

                    if (response.error_code != 0)
                    {
                        log.Error("response_" + JsonConvert.SerializeObject(response));
                        throw BaseBll.CreateException(session.LanguageId, GetErrorCode(response.error_code));
                    }
                }
            }
        }

        public static string CallWooppayMobileApi(PaymentRequest input, int partnerId, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {                
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId,
                    input.CurrencyId, (int)PaymentRequestTypes.Deposit);

                var loginRequest = new CoreLoginRequest
                {
                    username = partnerPaymentSetting.UserName,
                    password = partnerPaymentSetting.Password
                };
                var xmlControllerPortTypeClient = new XmlControllerPortTypeClient();
                var loginResponse = xmlControllerPortTypeClient.core_login(loginRequest);

                if (loginResponse.error_code != 0)
                    throw new ArgumentNullException(loginResponse.error_code.ToString());

                using (OperationContextScope scope = new OperationContextScope(xmlControllerPortTypeClient.InnerChannel))
                {
                    var prop = new HttpRequestMessageProperty();
                    prop.Headers.Add(HttpRequestHeader.Cookie, "session=" + loginResponse.response.session);
                    OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, prop);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    var expDate = DateTime.UtcNow.AddHours(12);
                    var mobileNumber = paymentInfo.MobileNumber.Replace("+7", string.Empty);
                    var createPaymentRequest = new CashCreateInvoiceExtendedRequest
                    {
                        referenceId = input.Id.ToString(),
                        backUrl = "https://" + session.Domain,
                        requestUrl = string.Format("{0}/api/Wooppay/ApiRequest?OrderId={1}&Amount={2}&MobileNumber={3}",
                        partnerBl.GetPaymentValueByKey(partnerId, null, Constants.PartnerKeys.PaymentGateway), input.Id, input.Amount, mobileNumber),
                        addInfo = paymentInfo.SMSCode,
                        amount = (float)input.Amount,
                        deathDate = expDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        description = string.Empty,
                        serviceType=2,
                        userPhone = mobileNumber
                    };
                    var response = xmlControllerPortTypeClient.cash_createInvoiceExtended(createPaymentRequest);
                    if (response.response == null)
                        throw BaseBll.CreateException(session.LanguageId, GetErrorCode(response.error_code));

                    using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                    {
                        input.ExternalTransactionId = response.response.operationId.ToString();
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                    }

                    return response.response.operationUrl;
                }
            }
        }


        public static PaymentRequestStates SendPaymentRequestToWooppay(PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var resState = PaymentRequestStates.Failed;
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var currencyBll = new CurrencyBll(session, log))
                {
                    return resState;
                }
            }
        }

        public static CompleteCashOutTransactionOutput CompleteWooppayCashOutToRegisteredCard(CompleteCashOutTransactionInput input, int partnerId, int paymentSystemId, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var url = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.WooppayUrl);
                var xml = SerializeAndDeserialize.SerializeToXml(input, "request");
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationXml,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = xml
                };
                var deserializer = new XmlSerializer(typeof(CompleteCashOutTransactionOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                {
                   return (CompleteCashOutTransactionOutput)deserializer.Deserialize(stream);
                }
            }
        }

        public static StartCashOutTransactionOutput StartWooppayCashOutToRegisteredCard(StartCashOutTransactionInput input, int partnerId, int paymentSystemId, SessionIdentity session, ILog log)
        {

            using (var partnerBl = new PartnerBll(session, log))
            {
                var url = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.WooppayUrl);

                var xml = SerializeAndDeserialize.SerializeToXml(input, "request");
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationXml,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = xml
                };
                var deserializer = new XmlSerializer(typeof(StartCashOutTransactionOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                {
                     return (StartCashOutTransactionOutput)deserializer.Deserialize(stream);
                }
            }
        }
    }
}