using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.BLL.Services;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using System.Net;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Text.RegularExpressions;
using IqSoft.CP.PaymentGateway.Models.InternationalPSP;
using IqSoft.CP.PaymentGateway.Helpers;
using System.IO;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InternationalPSPController : ApiController
    {
        [HttpPost]
        [Route("api/InternationalPSP/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId)) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(request.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                        request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                    if (request.Status != (int)PaymentRequestStates.Pending)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                    input.HolderName = Regex.Replace(input.HolderName, @"\s+", " ");
                    var holderName = input.HolderName.Trim().Split(' ');
                    if (holderName.Length <= 1)
                        throw BaseBll.CreateException(null, Constants.Errors.UserNotFound);
                    var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(request.Info) ? request.Info : "{}");
                    var firstDigits = input.CardNumber.Substring(0, 6);
                    var lastDigits = input.CardNumber.Substring(input.CardNumber.Length - 4, 4);
                    paymentInfo.CardNumber = string.Concat(firstDigits, new String('*', input.CardNumber.Length - firstDigits.Length - lastDigits.Length), lastDigits);
                    paymentInfo.CardHolderName = input.HolderName;
                    if (!string.IsNullOrEmpty(input.Country))
                        paymentInfo.Country = input.Country;
                    if (!string.IsNullOrEmpty(input.City))
                        paymentInfo.City = input.City;
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    request.CardNumber = paymentInfo.CardNumber;
                    request.CountryCode = paymentInfo.Country;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    var partner = CacheManager.GetPartnerById(client.PartnerId);
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, request.PaymentSystemId, Constants.PartnerKeys.InternationalPSPApiUrl);
                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

                    var autorizationInput = new
                    {
                        date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        journal = partnerPaymentSetting.UserName,
                        type = "sale",
                        first_name = holderName[0],
                        last_name = holderName[1],
                        address_1 = input.Address,
                        city = input.City,
                        region = input.Region,
                        postal_code = input.Zip,
                        country = paymentInfo.Country,
                        email = client.Email,
                        phone = client.MobileNumber,
                        custom_return_url = input.RedirectUrl,
                        custom_decline_return_url = input.RedirectUrl,
                        custom_cancel_return_url = input.RedirectUrl,
                        custom_postback_url = $"{paymentGateway}/api/InternationalPSP/ApiRequest",
                        browser_ip = paymentInfo.TransactionIp,
                        description = partner.Name,
                        currency = client.CurrencyId,
                        amount = request.Amount,
                        tax = 0,
                        cardholder_name = input.HolderName,
                        card_number = input.CardNumber,
                        card_exp_month = input.ExpiryMonth,
                        card_exp_year = input.ExpiryYear,
                        card_cvv = input.VerificationCode,

                    };
                    var authToken = Convert.ToBase64String(Encoding.Default.GetBytes(partnerPaymentSetting.Password));
                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        RequestHeaders = new Dictionary<string, string> { { "Authorization", $"Basic {authToken}" } },
                        Url = $"{url}/authorization",
                        PostData = JsonConvert.SerializeObject(autorizationInput)
                    };
                    var authOutput = JsonConvert.DeserializeObject<AuthorizationOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                    request.ExternalTransactionId = authOutput.TransactionUuid;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    var sign = CommonFunctions.ComputeSha256($"{partnerPaymentSetting.Password.Split(':')[1]}{authOutput.TransactionUuid}{authOutput.OperationUuid}{authOutput.Status}");
                    if (sign.ToLower() != authOutput.Signature.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    if (!string.IsNullOrEmpty(authOutput.RedirectUrl))
                        result.RedirectUrl = authOutput.RedirectUrl;
                    else
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                            if (authOutput.Status.ToLower() == "approved")
                            {
                                if (request.Amount != authOutput.AmountCaptured)
                                {
                                    request.Amount = authOutput.AmountCaptured;
                                    paymentSystemBl.ChangePaymentRequestDetails(request);
                                }
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (authOutput.Status.ToLower() == "declined" || authOutput.Status.ToLower() == "rejected")
                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, authOutput.Message, notificationBl);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                WebApiApplication.DbLogger.Error(exp);

                result.StatusCode = ex.Detail.Id;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                result.StatusCode = Constants.Errors.GeneralException;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/InternationalPSP/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)// for log
        {
            var response = "SUCCESS";
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info(inputString);

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(new Exception(response));
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(ex);
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}