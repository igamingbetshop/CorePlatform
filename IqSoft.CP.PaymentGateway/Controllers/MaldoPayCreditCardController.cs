using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using IqSoft.CP.PaymentGateway.Models.MaldoPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MaldoPayCreditCardController : ApiController
    {
        [HttpPost]
        [Route("api/MaldoPayCreditCard/ProcessPaymentRequest")]
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
                                var merchant = partnerPaymentSetting.UserName.Split(',');
                                var mid = merchant[0];
                                var brandId = merchant[1];
                                var integrationId = merchant[2];
                                var keys = partnerPaymentSetting.Password.Split(',');
                                var encryptionKey = keys[0];
                                var apiKey = keys[1];
                                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MaldoPayApiUrl).StringValue;
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
                                var paymentJson = new
                                {
                                    transaction = new
                                    {
                                        clientId = mid,
                                        brandId,
                                        integrationId,
                                        landingPages = new
                                        {
                                            landingSuccess = input.RedirectUrl,
                                            landingPending = input.RedirectUrl,
                                            landingDeclined = input.RedirectUrl,
                                            landingFailed = input.RedirectUrl
                                        },
                                        request = new
                                        {
                                            serviceId = 2116,
                                            currencyCode = client.CurrencyId,
                                            amount = (int)input.Amount,
                                            referenceOrderId = request.Id.ToString(),
                                            serviceData = new
                                            {
                                                serviceData1 = input.CardNumber,
                                                serviceData2 = input.ExpiryYear,
                                                serviceData3 = input.ExpiryMonth,
                                                serviceData4 = input.VerificationCode,//CCV of the card, ?
                                                serviceData5 = $"{holderName[0]} {holderName[1]}",//Card Holder ?
                                                serviceData6 = input.WalletNumber,// ???? to check! User agent or TC Kimlik
                                                serviceData7 = client.CreationTime//?//User account createn date (YYYY-MM-DD)                                                                                            
                                            }
                                        },
                                        user = new
                                        {
                                            firstName = client.FirstName,
                                            lastName = client.LastName,
                                            address = client.Address,
                                            birthDate = client.BirthDate.ToString("yyyy-MM-dd"),
                                            countryCode = paymentInfo.Country,
                                            city = paymentInfo.City,
                                            playerId = client.Id.ToString(),
                                            postCode = client.ZipCode.Trim(),
                                            languageCode = client.LanguageId,
                                            emailAddress = client.Email,
                                            phone = client.MobileNumber,
                                            ipAddr = clientSession.Ip

                                        }
                                    }
                                };

                                var paymentInput = new
                                {
                                    json = JsonConvert.SerializeObject(paymentJson),
                                    checksum = CommonFunctions.ComputeHMACSha256(JsonConvert.SerializeObject(paymentJson), encryptionKey).ToLower(),
                                    apiKey
                                };
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                    Accept = $"{Constants.HttpContentTypes.ApplicationJson}; version=2.1",
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = url,
                                    PostData = CommonFunctions.GetUriDataFromObject(paymentInput)
                                };
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(httpRequestInput));
                                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(resp));
                                var transactionResult = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                                try
                                {
                                    request.ExternalTransactionId = transactionResult.Transaction.TransactionId.ToString();
                                    paymentSystemBl.ChangePaymentRequestDetails(request);
                                    if (!string.IsNullOrEmpty(transactionResult.Redirect))
                                    {
                                        result.RedirectUrl = transactionResult.Redirect;
                                    }
                                    else
                                    {
                                        throw new Exception(resp);
                                    }

                                    /*  var transactionResult = JsonConvert.DeserializeObject<PaymentOutput>(resp);
                                      request.ExternalTransactionId = transactionResult.Transaction.TransactionId.ToString();
                                      paymentSystemBl.ChangePaymentRequestDetails(request);
                                      switch (transactionResult.Transaction.CodeId)
                                      {
                                          //300 Transaction Payment pending
                                          //311 Redirect to payment gateway.
                                          //319 Pending Transaction. Waiting SMS form customer
                                          case 400://Transaction Failure
                                          case 491://Transaction Failure, api credentials for brand missing
                                          case 500://Declined

                                              clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, transactionResult.Transaction.CodeMessage, notificationBl);
                                              throw new Exception(transactionResult.Transaction.CodeMessage);
                                          case 200:                                           
                                              clientBl.ApproveDepositFromPaymentSystem(request, false, comment: transactionResult.Transaction.CodeMessage);
                                              PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                              BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                              break;
                                          case 311:                                        

                                              if (!string.IsNullOrEmpty(transactionResult.Redirect))
                                              {
                                                  result.RedirectUrl = transactionResult.Redirect;
                                              }
                                              else
                                                  throw new Exception(resp);
                                              break;
                                          default: 
                                              break;

                                      }*/
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
    }
}