using System;
using System.Collections.Generic;
using System.Net.Http;
using IqSoft.CP.PaymentGateway.Models.IPS;
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
using System.Net.Http.Headers;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using IqSoft.CP.PaymentGateway.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class IPSController : ApiController
    {
        private static readonly List<string> WhitelistedIps = new List<string> //distribution
        {
           "171.22.255.23" //tgi
        };       

        [HttpPost]
        [Route("api/IPS/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
               WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                //BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var regionBl = new RegionBll(paymentSystemBl))
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {                                
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                if (request.Status != (int)PaymentRequestStates.Pending)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                                var clientSession = paymentSystemBl.GetClientSessionById(request.SessionId ?? 0);
                                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IPSApiUrl).StringValue;
                                var apiId = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.IPSApiId).StringValue;
                                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                                input.HolderName = Regex.Replace(input.HolderName, @"\s+", " ");
                                var holderName = input.HolderName.Trim().Split(' ');
                                var cardType = input.CardNumber.StartsWith("4") ? "VISA" : input.CardNumber.StartsWith("5") ? "MC" :
                                               input.CardNumber.StartsWith("3") ? "AMEX" : "undefined";
                                var firstDigits = input.CardNumber.Substring(0, 6);
                                var lastDigits = input.CardNumber.Substring(input.CardNumber.Length - 4, 4);
                                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(!string.IsNullOrEmpty(request.Info) ? request.Info : "{}");
                                paymentInfo.CardNumber = string.Concat(firstDigits, new String('*', input.CardNumber.Length - firstDigits.Length - lastDigits.Length), lastDigits);
                                paymentInfo.CardHolderName = input.HolderName;
                                paymentInfo.CardType = cardType;
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
                                var amount = request.Amount;
                                if (client.CurrencyId != Constants.Currencies.Euro)
                                {
                                    var parameters = string.IsNullOrEmpty(request.Parameters) ? new Dictionary<string, string>() :
                                                     JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                                    amount = Math.Round(Convert.ToDecimal(parameters["AppliedRate"]) * request.Amount, 2);
                                }
                                var requestParameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                                var distributionUrlKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.DistributionUrl);
                                if (distributionUrlKey == null || distributionUrlKey.Id == 0)
                                    distributionUrlKey = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.DistributionUrl);

                                var distributionUrl = string.Format(distributionUrlKey.StringValue, requestParameters["Domain"]);

                                var paymentStatusPageUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentStatusPageUrl).StringValue;
                                if (string.IsNullOrEmpty(paymentStatusPageUrl))
                                    paymentStatusPageUrl = string.Format("https://{0}/", requestParameters["Domain"]);
                                else
                                    paymentStatusPageUrl = string.Format(paymentStatusPageUrl, requestParameters["Domain"]);
                                var redirectUrl = string.Format("{0}/redirect/RedirectRequest?redirectUrl={1}", distributionUrl, paymentStatusPageUrl);
                                var processPaymentInput = new
                                {
                                    Id = apiId,
                                    Session = partnerPaymentSetting.Password,
                                    OrderId = request.Id.ToString(),
                                    Amount = amount.ToString("F"),
                                    CurrencyCode = Constants.Currencies.Euro,
                                    CCVC = input.VerificationCode,
                                    CCExpiryMonth = input.ExpiryMonth,
                                    CCExpiryYear = input.ExpiryYear,
                                    CCNameSurname = input.HolderName,
                                    CCNumber = input.CardNumber,
                                    CCType = cardType,
                                    ClientAddress = input.Address,
                                    ClientZip = input.Zip,
                                    ClientCity = paymentInfo.City,
                                    ClientDOB = client.BirthDate.ToString("dd/MM/yyyy"),
                                    ClientEmail = client.Email,
                                    ClientExternalIdentifier = client.Id.ToString(),
                                    ClientIP = clientSession.Ip,
                                    ClientName = holderName[0],
                                    ClientSurname = holderName[1],
                                    ClientCountryCode = paymentInfo.Country,
                                    ClientPhone = client.MobileNumber,
                                    ClientDevice = input.ClientDevice,
                                    LanguageCode = clientSession.LanguageId,
                                    WCRedirectUrl = redirectUrl,
                                    WCResponseUrl = string.Format("{0}/api/IPS/ApiRequest", paymentGateway)
                                };

                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = url,
                                    PostData = CommonFunctions.GetUriEndocingFromObject(processPaymentInput)
                                };
                                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                try
                                {
                                    var serializer = new XmlSerializer(typeof(procccresult), new XmlRootAttribute("proc-cc-result"));
                                    var transactionResult = (procccresult)serializer.Deserialize(new StringReader(resp));
                                    switch (transactionResult.procccerror.proccccode)
                                    {
                                        case 0:
                                            request.ExternalTransactionId = transactionResult.proccccontainer.internalorderid.ToString();
                                            paymentSystemBl.ChangePaymentRequestDetails(request);
                                            clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, transactionResult.procccerror.procccmessage, notificationBl);
                                            throw new Exception(transactionResult.procccerror.procccmessage);
                                        case 1:
                                            request.ExternalTransactionId = transactionResult.proccccontainer.internalorderid.ToString();
                                            paymentSystemBl.ChangePaymentRequestDetails(request);
                                            clientBl.ApproveDepositFromPaymentSystem(request, false, comment: transactionResult.procccerror.procccmessage);
                                            PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                            BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                            break;
                                        case 5:
                                            request.ExternalTransactionId = transactionResult.proccccontainer.internalorderid.ToString();
                                            paymentSystemBl.ChangePaymentRequestDetails(request);
                                            if (transactionResult.proccccontainer.redirection.url.type == "form")
                                            {
                                                var formUrl = transactionResult.proccccontainer.redirection.url.Value;
                                                var formParameters = new StringBuilder();
                                                foreach (var p in transactionResult.proccccontainer.redirection.vars)
                                                {
                                                    formParameters.Append(String.Format("&{0}={1}", p.name, Uri.EscapeDataString(p.Value)));
                                                }
                                                result.RedirectUrl = string.Format("{0}/paymentform/paymentrequest?apiUrl={1}{2}", distributionUrl, formUrl, formParameters.ToString());
                                            }
                                            else
                                            {
                                                result.RedirectUrl = transactionResult.proccccontainer.redirection.url.Value;
                                            }
                                            break;
                                        default: break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex);
                                    using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                                    {
                                        clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, resp, notificationBl);
                                        throw new Exception(resp);
                                    }
                                }
                            }
                        }
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
        [Route("api/IPS/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = "SUCCESS";
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info(inputString);

                var serializer = new XmlSerializer(typeof(procccresult), new XmlRootAttribute("proc-cc-result"));
                var transactionResult = (procccresult)serializer.Deserialize(new StringReader(inputString));

                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var documentBll = new DocumentBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                var paymentRequest = paymentSystemBl.GetPaymentRequestById(transactionResult.proccccontainer.orderid);
                                if (paymentRequest == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                                if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                                {
                                    if (transactionResult.proccccontainer.approved == 1)
                                    {
                                        clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);

                                    }
                                    else
                                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, transactionResult.procccerror.procccmessage, notificationBl);
                                }
                                else
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
                            }
                        }
                    }
                }
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