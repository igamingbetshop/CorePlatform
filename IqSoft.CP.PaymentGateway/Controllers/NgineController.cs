using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Ngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Cors;
using TransferInput = IqSoft.CP.PaymentGateway.Models.Ngine.TransferInput;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NgineController : ApiController
    {
        [HttpPost]
        [Route("api/Ngine/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var regionBl = new RegionBll(paymentSystemBl))
                    {
                        using (var clientBl = new ClientBll(paymentSystemBl))
                        {
                            using (var notificationBl = new NotificationBll(paymentSystemBl))
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                                if (request.Status != (int)PaymentRequestStates.Pending)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                                var clientSession = paymentSystemBl.GetClientSessionById(request.SessionId ?? 0);
                                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NgineApiUrl).StringValue;
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
                                request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                });
                                request.CardNumber = paymentInfo.CardNumber;
                                paymentSystemBl.ChangePaymentRequestDetails(request);

                                var postData = JsonConvert.SerializeObject(new
                                {
                                    UserLogin = input.OrderId,
                                    UserPassword = client.Id.ToString(),
                                    InstanceID = 1
                                });
                                var httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = string.Format(url, "api/Authentication/GenerateToken"),
                                    PostData = postData,
                                };
                                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                var output = JsonConvert.DeserializeObject<AuthenticationOutput>(response);

                                postData = JsonConvert.SerializeObject(new
                                {
                                    Token = output.Authentication.Token
                                });
                                httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = string.Format(url, "api/Deposits/GetDepositLimits"),
                                    PostData = postData,
                                };
                                response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                WebApiApplication.DbLogger.Info($"GetDepositLimits: {postData} , {response}");
                                var limitsOutput = JsonConvert.DeserializeObject<GetDepositLimitsOutput>(response);

                                postData = JsonConvert.SerializeObject(new
                                {
                                    Token = output.Authentication.Token,
                                    TransactionID = input.OrderId,
                                    CreditCardHolder = input.HolderName,
                                    CreditCardNumber = input.CardNumber,
                                    CreditCardExpirationMonth = input.ExpiryMonth,
                                    CreditCardExpirationYear = input.ExpiryYear,
                                    CreditCardType = limitsOutput.Authentication.FirstOrDefault().CreditCardTypeID,
                                    CreditCardCVV = input.VerificationCode,
                                    Amount = request.Amount,
                                    CurrencyCode = client.CurrencyId,
                                    IPv4Address = clientSession.Ip,
                                    InstanceID = 1,
                                    Source = "Mobile",
                                    BonusList = ""
                                });
                                httpRequestInput = new HttpRequestInput
                                {
                                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                                    RequestMethod = Constants.HttpRequestMethods.Post,
                                    Url = string.Format(url, "api/Deposits/CreditCardDeposit"),
                                    PostData = postData,
                                };
                                response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                                WebApiApplication.DbLogger.Info($"CreditCardDeposit: {postData} , {response}");
                                var depositOutput = JsonConvert.DeserializeObject<CreditCardDepositOutput>(response);

                                if (depositOutput.Authentication.Status != "Approved")
                                {
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, depositOutput.Authentication.ErrorDescription, notificationBl);
                                    throw new Exception(depositOutput.Authentication.ErrorDescription);
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
        [Route("api/Ngine/ApiRequest")]
        public HttpResponseMessage ApiRequest(JObject input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            var action = input.First.Path;
            var userIds = new List<int>();
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            var response = string.Empty;
            BllClient client;
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                switch (action)
                                {
                                    case NgineHelpers.Methods.GetCustomerInfo:
                                        var customerInfoInput = JsonConvert.DeserializeObject<CustomerInfoInput>(JsonConvert.SerializeObject(input));
                                        client = CacheManager.GetClientById(Convert.ToInt32(customerInfoInput.GetCustomerInfo.ClientId));
                                        var regionPath = CacheManager.GetRegionPathById(client.RegionId);
                                        var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.IsoCode;
                                        var state = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City)?.IsoCode;
                                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(customerInfoInput.GetCustomerInfo.RequestId));
                                        var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                                        var data = new
                                        {
                                            Name = client.FirstName,
                                            LastName = client.LastName,
                                            SSN = 1111,
                                            Phone = client.MobileNumber,
                                            Email = client.Email,
                                            City = paymentInfo.City,
                                            Address = client.Address,
                                            ZipCode = client.ZipCode?.Trim(),
                                            DOB = client.BirthDate,
                                            Country = country,
                                            State = state,
                                            CurrencyCode = client.CurrencyId,
                                            Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2)
                                        };
                                        response = JsonConvert.SerializeObject(data);
                                        break;
                                    case NgineHelpers.Methods.GetBalance:
                                        var balanceInput = JsonConvert.DeserializeObject<GetBalanceInput>(JsonConvert.SerializeObject(input));
                                        client = CacheManager.GetClientById(Convert.ToInt32(balanceInput.GetBalance.ClientId));
                                        if (client == null)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                        var balanceOutput = new BalanceOutput()
                                        {
                                            Balance = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance, 2),
                                            Status = "OK"
                                        };
                                        response = JsonConvert.SerializeObject(balanceOutput);
                                        break;
                                    case NgineHelpers.Methods.Transfer:
                                        var transferInput = JsonConvert.DeserializeObject<TransferInput>(JsonConvert.SerializeObject(input));
                                        request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(transferInput.Transfer.CustPIN));
                                        if (request == null)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                        if (Convert.ToInt64(transferInput.Transfer.CustPassword) != request.ClientId)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                        client = CacheManager.GetClientById(request.ClientId.Value);

                                        request.ExternalTransactionId = transferInput.Transfer.TransactionID.ToString();
                                        paymentSystemBl.ChangePaymentRequestDetails(request);
                                        if (transferInput.Transfer.TransType == "Deposit")
                                        {
                                            if (transferInput.Transfer.ErrorCode == null)
                                            {
                                                clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds);
                                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                            }
                                            else
                                            {
                                                clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, transferInput.Transfer.ErrorDescription, notificationBl);
                                            }
                                        }
                                        else if (transferInput.Transfer.TransType == "Payout")
                                        {
                                            if (transferInput.Transfer.ErrorCode == null)
                                            {
                                                var resp = clientBl.ChangeWithdrawRequestState(Convert.ToInt64(transferInput.Transfer.CustPIN),
                                                                            PaymentRequestStates.Approved, string.Empty, null, null, false,
                                                                            string.Empty, documentBll, notificationBl, out userIds);
                                                clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                            }
                                            else
                                            {
                                                clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed,
                                                transferInput.Transfer.ErrorDescription, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                            }
                                        }
                                        response = JsonConvert.SerializeObject(new TransferOutput
                                        {
                                            TransactionID = request.Id
                                        });
                                        break;
                                    case NgineHelpers.Methods.PayoutRequest:
                                        var payoutInput = JsonConvert.DeserializeObject<PayoutInput>(JsonConvert.SerializeObject(input));
                                        request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(payoutInput.PayoutRequest.CustPIN));
                                        if (request == null)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                        response = JsonConvert.SerializeObject(new TransferOutput
                                        {
                                            TransactionID = request.Id
                                        });
                                        break;
                                    case NgineHelpers.Methods.PayoutCanceled:
                                        var payoutCanceled = JsonConvert.DeserializeObject<PayoutCanceledInput>(JsonConvert.SerializeObject(input));
                                        request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(payoutCanceled.PayoutCanceled.CustPIN));
                                        if (request == null)
                                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                                        clientBl.ChangeWithdrawRequestState(Convert.ToInt64(payoutCanceled.PayoutCanceled.CustPIN),
                                                                        PaymentRequestStates.CanceledByUser, string.Empty, null, null, false,
                                                                        string.Empty, documentBll, notificationBl, out userIds);
                                        response = JsonConvert.SerializeObject(new TransferOutput
                                        {
                                            TransactionID = request.Id
                                        });
                                        break;
                                    default: break;
                                }
                            }
                        }
                    }
                }
                foreach (var uId in userIds)
                {
                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
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