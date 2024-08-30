using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.Omer;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using System.Text;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net;
using System.Net.Http.Headers;
using IqSoft.CP.PaymentGateway.Helpers;
using System.Web.Http.Cors;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using System.Text.RegularExpressions;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Helpers;
using Microsoft.AspNet.SignalR.Hubs;
using static System.Net.WebRequestMethods;
using System.Globalization;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class OmerController : ApiController
    {
        [HttpPost]
        [Route("api/Omer/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    WebApiApplication.DbLogger.Info("api/Omer/ProcessPaymentRequest " + " " + JsonConvert.SerializeObject(input));

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

                    var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;

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
                    var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, request.PaymentSystemId, Constants.PartnerKeys.OmerApiUrl);

                    var region = CacheManager.GetRegionById(client.RegionId, Constants.DefaultLanguageId);
                    var amount = request.Amount;
                    if (client.CurrencyId != Constants.Currencies.USADollar)
                    {
                        var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                         JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                        amount = Math.Round(Convert.ToDecimal(parameters["AppliedRate"]) * request.Amount, 2);
                    }

                    var requestInput = new
                    {
                        date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        journal = request.Id.ToString(),
                        first_name = holderName[0],
                        last_name = holderName[1],
                        address_1 = input.Address,
                        city = input.City,
                        region = region.Name,
                        postal_code = input.Zip,
                        country = input.Country,
                        email = client.Email,
                        phone = client.MobileNumber,
                        ip = paymentInfo.TransactionIp,
                        date_of_birth = client.BirthDate.ToString("yyyy-MM-dd"),
                        description = "Deposit",
                        currency = client.CurrencyId,
                        amount = request.Amount,
                        ResourcesUrl = input.RedirectUrl,
                        custom_return_url = input.RedirectUrl,
                        tax = 0,
                        cardholder_name = input.HolderName,
                        card_number = input.CardNumber,
                        card_exp_month = input.ExpiryMonth.ToString(),
                        card_exp_year = input.ExpiryYear.ToString(),
                        card_cvv = input.VerificationCode.ToString(),
                        approval_return_url = input.RedirectUrl,
                        decline_return_url = input.RedirectUrl,
                        cancel_return_url = input.RedirectUrl,
                        notification_url = $"{paymentGateway}/api/Omer/Apirequest"
                    };

                    var header = new Dictionary<string, string>
                    {
                       { "Authorization", $"Bearer {partnerPaymentSetting.UserName}" }
                    };


                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        RequestHeaders = header,
                        Url = url,
                        PostData = JsonConvert.SerializeObject(requestInput)
                    };
                    var authOutput = JsonConvert.DeserializeObject<PaymentInput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));


                    if (authOutput?.Message != null && authOutput.Message.ToLower() != "success")
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            result.StatusCode = Constants.Errors.GeneralException;
                            result.Description = authOutput.Message;
                            clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, authOutput.Message, notificationBl);
                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadRequest,
                                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8)
                            };
                        }
                    }

                    paymentSystemBl.ChangePaymentRequestDetails(request);

                    if (!string.IsNullOrEmpty(authOutput.RedirectUrl))
                        result.RedirectUrl = authOutput.RedirectUrl;

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
        [Route("api/Omer/Apirequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = string.Empty;

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Journal));
                            if (paymentRequest == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);


                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                                        paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);

                            string serverDatestr = input.ServerDate.ToString("yyyy-MM-dd HH:mm:ss");
                            var signServerDate = CommonFunctions.ComputeSha256($"{partnerPaymentSetting.Password}{serverDatestr}{input.UuId}{input.Currency}{input.Amount}{input.Status}");
                            if (signServerDate.ToLower() != input.Signature.ToLower())
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                            var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                            paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);


                            paymentRequest.ExternalTransactionId = input.UuId;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);

                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                if (input.Status.ToUpper() == "APPROVED")
                                {

                                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                    response = "OK";
                                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                                }


                                else if (input.Status.ToUpper() == "DECLINED")
                                {
                                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted,
                                        string.Format("ErrorMessage: {0}, Messages {1}", input.Message, input.Messages), notificationBl);
                                    response = "DECLINED";
                                }
                            }

                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                else
                {
                    response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                    httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }

            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

    }
}