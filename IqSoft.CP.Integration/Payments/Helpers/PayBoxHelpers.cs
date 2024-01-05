using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Payments.Models.PayBox;
using System;
using System.IO;
using System.Xml.Serialization;
using IqSoft.CP.BLL.Services;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Web.Script.Serialization;
using IqSoft.CP.Integration.Payments.Models;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class PayBoxHelpers
    {
        #region Deposits

        public static string CallPayBoxApi(PaymentRequest input, int partnerId, string cashierPageUrl, SessionIdentity session, ILog log)
        {
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, input.PaymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var script = "init_payment.php";
                var merchantId = partnerPaymentSetting.UserName;
                var secKey = partnerPaymentSetting.Password;
                var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.PayBoxDepositUrl).StringValue +
                          Path.AltDirectorySeparatorChar + script;
                var salt = CommonFunctions.GetRandomString(16);
                var partner = CacheManager.GetPartnerById(partnerId);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var paymentRequestInput = new PaymentRequestInput
                {
                    pg_merchant_id = merchantId,
                    pg_order_id = input.Id.ToString(),
                    pg_amount = input.Amount.ToString(),
                    pg_currency = input.CurrencyId,
                    pg_description = partner.Name,
                    pg_salt = salt,
                    pg_user_id = input.ClientId.ToString(),
                    pg_payment_system = paymentSystem.Name == Constants.PaymentSystems.OnlineBanking ?
                        paymentSystemBl.GetBankInfoById(Convert.ToInt32(paymentInfo.BankId)).BankCode :
                     GetBankCode(paymentSystem.Name),
                    pg_payment_route = "frame",
                    pg_failure_url = cashierPageUrl,
                    pg_success_url= cashierPageUrl,
                    pg_recurring_start = 1,
                    pg_recurring_lifetime = 12,
                    pg_result_url = string.Format("{0}/{1}", paymentGateway, "api/PayBox/Result"),
                    pg_check_url = string.Format("{0}/{1}", paymentGateway, "api/PayBox/Check")
                };

                if (paymentSystem.Name == Constants.PaymentSystems.PayBoxMobile)
                {
                    if (string.IsNullOrEmpty(paymentInfo.MobileNumber))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidMobile);

                    paymentRequestInput.pg_abonent_phone = "+" + paymentInfo.MobileNumber.Replace("+", string.Empty);
                    var mobileInfo = GetMobileNumberInfo(paymentRequestInput.pg_abonent_phone, partnerPaymentSetting, session, log);
                    if (mobileInfo.pg_status.ToLower() == "ok")
                        paymentRequestInput.pg_payment_system = mobileInfo.pg_operator_name.ToUpper() + "_PAYTECH_HIGHRISK";
                    else
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidMobile);
                }
                string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                var defaultProtocol = ServicePointManager.SecurityProtocol;
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    })
                };
                var deserializer = new XmlSerializer(typeof(PaymentRequestOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                {
                    var output = (PaymentRequestOutput)deserializer.Deserialize(stream);
                    ServicePointManager.SecurityProtocol = defaultProtocol;
                    if (output.pg_error_code != null)
                    {
                        log.Error(JsonConvert.SerializeObject(output));
                        throw BaseBll.CreateException(session.LanguageId, GetErrorCode(Convert.ToInt32(output.pg_error_code)));
                    }
                    input.ExternalTransactionId = output.pg_payment_id;
                    paymentSystemBl.ChangePaymentRequestDetails(input);
                    return output.pg_redirect_url;
                }
            }
        }

        private static MNPOutput GetMobileNumberInfo(string mobile, BllPartnerPaymentSetting bllPartnerPaymentSetting, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBox);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);

                var merchantId = bllPartnerPaymentSetting.UserName;
                var secKey = bllPartnerPaymentSetting.Password;
                var url = string.Format("{0}/mfs/check_mnp", partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxDepositUrl));
                var salt = CommonFunctions.GetRandomString(16);
                var paymentRequestInput = new MNPInput
                {
                    pg_merchant_id = merchantId,
                    pg_abonent_phone = mobile,
                    pg_salt = salt,
                    pg_sig = string.Empty
                };

                string signature = string.Format("check_mnp;{0};{1}", CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                var defaultProtocol = ServicePointManager.SecurityProtocol;
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var deserializer = new XmlSerializer(typeof(MNPOutput), new XmlRootAttribute("response"));
                using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                {
                    var output = (MNPOutput)deserializer.Deserialize(stream);
                    ServicePointManager.SecurityProtocol = defaultProtocol;
                    return output;
                }
            }
        }

        public static PaymentResponse ApprovedMobileRequest(string code, int requestId, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
                    using (var clientBl = new ClientBll(partnerBl))
                    {
                        var paymentRequest = paymentSystemBl.GetPaymentRequestById(requestId);
                        if (paymentRequest == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentRequestNotFound);

                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBoxMobile);
                        if (paymentSystem == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, paymentRequest.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                        var merchantId = partnerPaymentSetting.UserName;
                        var secKey = partnerPaymentSetting.Password;
                        var url = string.Format("{0}/mfs/approve", partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxDepositUrl));
                        var salt = CommonFunctions.GetRandomString(16);
                        var paymentRequestInput = new ApproveRequestInput
                        {
                            pg_merchant_id = merchantId,
                            pg_payment_id = paymentRequest.ExternalTransactionId,
                            pg_approval_code = code,
                            pg_salt = salt,
                            pg_sig = string.Empty
                        };
                        string signature = string.Format("approve;{0};{1}", CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                        paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                        var defaultProtocol = ServicePointManager.SecurityProtocol;
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = Constants.HttpRequestMethods.Post,
                            Url = url,
                            PostData = JsonConvert.SerializeObject(paymentRequestInput)
                        };
                        var deserializer = new XmlSerializer(typeof(MNPOutput), new XmlRootAttribute("response"));
                        using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                        {
                            var output = (MNPOutput)deserializer.Deserialize(stream);
                            ServicePointManager.SecurityProtocol = defaultProtocol;

                            if (output.pg_status.ToLower() == "ok")
                            {
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                return new PaymentResponse
                                {
                                    Status = PaymentRequestStates.Confirmed,
                                };
                            }
                            return new PaymentResponse
                            {
                                Status = PaymentRequestStates.Failed,
                                Description = output.pg_status
                            };
                        }
                    }
                }
            }
        }

        #endregion

        #region

        public static PaymentResponse CreatePayment(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var clientBl = new ClientBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);

                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBox);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);

                    var merchantId = partnerPaymentSetting.UserName;
                    var secKey = partnerPaymentSetting.Password;
                    var wUrl = partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxWithdrawUrl);

                    var deserializer = new XmlSerializer(typeof(WithdrawRequestOutput), new XmlRootAttribute("response"));
                    var script = "reg2reg";
                    var url = string.Format("{0}/{1}/{2}", wUrl, "api", script);
                    var salt = CommonFunctions.GetRandomString(16);
                    var currentTime = DateTime.UtcNow;
                    var timeLimit = currentTime.AddDays(1);
                    var serializer = new JavaScriptSerializer();
                    var endPoint = partnerBl.GetPaymentValueByKey(partner.Id, paymentSystem.Id, Constants.PartnerKeys.PayBoxPayWithdrawResultEndPoint);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var paymentRequestInput = new WithdrawRequestInput
                    {
                        pg_merchant_id = merchantId,
                        pg_order_id = input.Id.ToString(),
                        pg_amount = amount.ToString("F"),
                        pg_user_id = input.ClientId.ToString(),
                        pg_description = partner.Name,
                        //pg_back_link = "https://" + partner.SiteUrl,
                        pg_post_link = endPoint,
                        pg_order_time_limit = timeLimit.Year.ToString() + "-" + timeLimit.Month.ToString() + "-" + timeLimit.Day.ToString() + " 12:00:00",
                        pg_salt = salt
                    };
                    var lastDep = clientBl.GetClientLastDepositWithParams(paymentSystem.Id, input.ClientId.Value);
                    if (lastDep != null && !string.IsNullOrEmpty(lastDep.Parameters))
                    {
                        var cardInfo = serializer.Deserialize<Dictionary<string, string>>(lastDep.Parameters);
                        if (cardInfo.ContainsKey("card_id"))
                            paymentRequestInput.pg_card_id_to = cardInfo["card_id"];
                    }

                    var signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                    paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                    
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = JsonConvert.SerializeObject(paymentRequestInput, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        })
                    };
                    log.Info("Paybox_" + JsonConvert.SerializeObject(httpRequestInput));
                    using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                    {
                        var output = (WithdrawRequestOutput)deserializer.Deserialize(stream);
                        var response = new PaymentResponse
                        {
                            Url = output.pg_redirect_url,
                            Status = output.pg_status.ToString().ToLower() == "ok" ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed,
                            Description = output.pg_error_description
                        };
                        log.Info(JsonConvert.SerializeObject(response));
                        return response;
                    }
                }
            }
        }
        /*
        private static PaymentResponse AddCard(string wUrl, int paymentSystemId, int partnerId, string endPoint, string siteUrl, PaymentRequest input, ILog log)
        {
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystemId, input.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

            var deserializer = new XmlSerializer(typeof(WithdrawRequestOutput), new XmlRootAttribute("response"));
            var script = "add";
            var url = wUrl + string.Format("/v1/merchant/{0}/cardstorage/add", partnerPaymentSetting.UserName);
            var salt = CommonFunctions.GetRandomString(16);
            var addCardInput = new AddCardInput
            {
                pg_merchant_id = partnerPaymentSetting.UserName,
                pg_order_id = input.Id,
                pg_user_id = input.ClientId,
                pg_back_link = "https://" + siteUrl,
                pg_post_link = endPoint,
                pg_salt = salt
            };
            string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(addCardInput, ";"), partnerPaymentSetting.Password);
            log.Info("signature_" + signature);
            addCardInput.pg_sig = CommonFunctions.ComputeMd5(signature);
            log.Info("add_" + JsonConvert.SerializeObject(addCardInput));

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(addCardInput)
            };

            using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, SecurityProtocolType.Tls12))
            {
                var output = (WithdrawRequestOutput)deserializer.Deserialize(stream);
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol;
                var response = new PaymentResponse
                {
                    Url = output.pg_redirect_url,
                    Status = output.pg_status.ToString().ToLower() == "ok" ? PaymentRequestStates.Active : PaymentRequestStates.Failed,
                    Description = output.pg_error_description
                };
                return response;
            }
        }
        */

        /*private static CheckBalanceOutput CheckBalance(string wUrl, string merchantId, string secKey, PaymentRequest input)
{
var script = "ps_list.php";
var url = string.Format("{0}/{1}", wUrl, script);
var salt = CommonFunctions.GetRandomString(16);
var paymentRequestInput = new BaseInput
{
    pg_merchant_id = merchantId,
    pg_salt = salt,
};

string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
var httpRequestInput = new HttpRequestInput
{
    ContentType = Constants.HttpContentTypes.ApplicationJson,
    RequestMethod = Constants.HttpRequestMethods.Post,
    Url = url,
    PostData = JsonConvert.SerializeObject(paymentRequestInput)
};
var deserializer = new XmlSerializer(typeof(CheckBalanceOutput), new XmlRootAttribute("response"));
using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, SecurityProtocolType.Tls12))
{
    var output = (CheckBalanceOutput)deserializer.Deserialize(stream);
    ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol;
    if (output.pg_status.ToLower() == "ok" && Convert.ToDecimal(output.pg_balance) <= input.Amount)
        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.LowBalance);
    return output;
}
}*/


        #endregion

        #region Withdraw_ATM

        public static PaymentResponse CreateATMPayoutRequest(PaymentRequest input, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
                {
                    var client = CacheManager.GetClientById(input.ClientId.Value);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);

                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBoxATM);
                    if (paymentSystem == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                    if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);

                    var script = "check";
                    var merchantId = partnerPaymentSetting.UserName;
                    var secKey = partnerPaymentSetting.Password;
                    var url = string.Format("{0}/{1}/{2}", partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxWithdrawUrl),
                                                           "api/cbc", script);
                    var resultEndpoint = string.Format("{0}/{1}", partnerBl.GetPaymentValueByKey(client.PartnerId, null, Constants.PartnerKeys.PaymentGateway), "api/Paybox/Check");
                    var salt = CommonFunctions.GetRandomString(16);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
                    if (string.IsNullOrEmpty(paymentInfo.MobileNumber))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                    var amount = input.Amount - (input.CommissionAmount ?? 0);
                    var paymentRequestInput = new WithdrawRequestInput
                    {
                        pg_merchant_id = merchantId,
                        pg_phone = paymentInfo.MobileNumber.Replace("+7", ""),
                        pg_order_id = input.Id.ToString(),
                        pg_amount = amount.ToString(),
                        pg_post_link = resultEndpoint,
                        pg_salt = salt
                    };
                    string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(paymentRequestInput, ";"), secKey);
                    paymentRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                    var defaultProtocol = ServicePointManager.SecurityProtocol;
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        Url = url,
                        PostData = JsonConvert.SerializeObject(paymentRequestInput,
                                                                new JsonSerializerSettings()
                                                                {
                                                                    NullValueHandling = NullValueHandling.Ignore
                                                                })
                    };
                    var deserializer = new XmlSerializer(typeof(WithdrawRequestOutput), new XmlRootAttribute("response"));
                    var withdrawRequestOutput = new WithdrawRequestOutput();
                    using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                    {
                        withdrawRequestOutput = (WithdrawRequestOutput)deserializer.Deserialize(stream);
                    }
                    if (withdrawRequestOutput.pg_status.ToLower() == "ok")
                    {
                        input.ExternalTransactionId = withdrawRequestOutput.pg_payment_id;
                        paymentSystemBl.ChangePaymentRequestDetails(input);
                        script = "exec";
                        url = string.Format("{0}/{1}/{2}", partnerBl.GetPaymentValueByKey(null, paymentSystem.Id, Constants.PartnerKeys.PayBoxWithdrawUrl),
                                                                                   "api/cbc", script);
                        var approvedRequestInput = new WithdrawRequestOutput
                        {
                            pg_merchant_id = merchantId,
                            pg_payment_id = withdrawRequestOutput.pg_payment_id,
                            pg_salt = salt
                        };
                        signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(approvedRequestInput, ";"), secKey);
                        approvedRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
                        httpRequestInput.Url = url;
                        httpRequestInput.PostData = JsonConvert.SerializeObject(approvedRequestInput,
                                                            new JsonSerializerSettings()
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });
                        using (Stream approvedStream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
                        {
                            withdrawRequestOutput = (WithdrawRequestOutput)deserializer.Deserialize(approvedStream);
                        }
                    }
                    var response = new PaymentResponse
                    {
                        Url = withdrawRequestOutput.pg_redirect_url,
                        Status = withdrawRequestOutput.pg_status.ToLower() == "ok" ? PaymentRequestStates.PayPanding : PaymentRequestStates.Failed,
                        Description = withdrawRequestOutput.pg_error_description
                    };

                    ServicePointManager.SecurityProtocol = defaultProtocol;
                    return response;
                }
            }
        }

        public static void GetPayoutRequestStatus(PaymentRequest input, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(input.ClientId.Value);
            if (client == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayBoxATM);
            if (paymentSystem == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, input.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            if (partner == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PartnerNotFound);

            var script = "status";
            var merchantId = partnerPaymentSetting.UserName;
            var secKey = partnerPaymentSetting.Password;
            var url = string.Format("{0}/{1}/{2}", CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.PayBoxWithdrawUrl),
                                                   "api/cbc", script);
            var salt = CommonFunctions.GetRandomString(16);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(input.Info);
            if (string.IsNullOrEmpty(paymentInfo.MobileNumber))
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MobileNumberCantBeEmpty);

            var statusRequestInput = new CheckStatusInput
            {
                pg_merchant_id = merchantId,
                pg_payment_id = input.ExternalTransactionId,
                pg_salt = salt
            };
            string signature = string.Format("{0};{1};{2}", script, CommonFunctions.GetSortedValuesAsString(statusRequestInput, ";"), secKey);
            statusRequestInput.pg_sig = CommonFunctions.ComputeMd5(signature);
            var defaultProtocol = ServicePointManager.SecurityProtocol;
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(statusRequestInput,
                                                        new JsonSerializerSettings()
                                                        {
                                                            NullValueHandling = NullValueHandling.Ignore
                                                        })
            };
            var deserializer = new XmlSerializer(typeof(WithdrawRequestOutput), new XmlRootAttribute("response"));
            var response = new BaseOutput();
            using (Stream stream = CommonFunctions.SendHttpRequestForStream(httpRequestInput, out _, SecurityProtocolType.Tls12))
            {
                response = (WithdrawRequestOutput)deserializer.Deserialize(stream);
            }
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    using (var notificationBl = new NotificationBll(clientBl))
                    {
                        if (response.pg_status.ToLower() == "success")
                        {
                            var resp = clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Approved, 
                                string.Empty, null, null, false, string.Empty, documentBl, notificationBl);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                        else if (response.pg_status.ToLower() != "process" && string.IsNullOrEmpty(response.pg_error_code))
                        {
                            clientBl.ChangeWithdrawRequestState(input.Id, PaymentRequestStates.Failed, "Expierd", 
                                null, null, false, string.Empty, documentBl, notificationBl);
                        }
                    }
                }
            }
            ServicePointManager.SecurityProtocol = defaultProtocol;
        }

        #endregion

        private enum ResponseCodes
        {
            LessAmount = 1380
        }

        private readonly static Dictionary<int, int> ResponseCodesMapping = new Dictionary<int, int>
        {
            {(int)ResponseCodes.LessAmount, Constants.Errors.WrongOperationAmount }
        };

        private static int GetErrorCode(int responseCode)
        {
            if (ResponseCodesMapping.ContainsKey(responseCode))
                return ResponseCodesMapping[responseCode];
            return Constants.Errors.GeneralException;
        }

        private readonly static List<string> MobileOperators = new List<string>
        {
            "Tele2",
            "Altel"
        };

        private readonly static Dictionary<string, string> PayboxPaymentSystem = new Dictionary<string, string>
        {
            {"KAZPOSTKZT", Constants.PaymentSystems.KazPost },
            {"EPAYWEBKZT", Constants.PaymentSystems.PayBox},
            {"ATF24KZT", Constants.PaymentSystems.OnlineBanking},
            {"HOMEBANKKZT", Constants.PaymentSystems.OnlineBanking},
            {"FORTEBANKKZT", Constants.PaymentSystems.OnlineBanking},
            {"ALFACLICKKZT", Constants.PaymentSystems.OnlineBanking},
            {"BANKRBK24KZT", Constants.PaymentSystems.OnlineBanking},
            {"SBERONLINEKZT", Constants.PaymentSystems.OnlineBanking},
            {"TELE2_PAYTECH_HIGHRISK", Constants.PaymentSystems.PayBoxMobile},
            {"ALTEL_PAYTECH_HIGHRISK", Constants.PaymentSystems.PayBoxMobile}
        };

        public static string GetBankCode(string paymentSystem)
        {
            if (PayboxPaymentSystem.ContainsValue(paymentSystem))
            {
                var pair = PayboxPaymentSystem.FirstOrDefault(x => x.Value == paymentSystem);
                if (pair.Equals(default(KeyValuePair<string, string>)))
                    return string.Empty;
                return pair.Key;
            }
            return string.Empty;
        }

        public static string GetPaymentSystem(string bankCode)
        {
            var paymentSystem = Constants.PaymentSystems.PayBox;
            if (PayboxPaymentSystem.ContainsKey(bankCode))
                return PayboxPaymentSystem[bankCode];
            return paymentSystem;
        }
    }
}