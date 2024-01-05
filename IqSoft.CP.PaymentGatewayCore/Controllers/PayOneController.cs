using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.PayOne;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PayOneController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "95.216.139.17",
            "213.175.208.152",
            "5.77.32.84",
            "5.77.32.87",
            "176.9.151.27",
            "88.99.175.216",
            "109.75.47.208",
            "196.18.159.11",

            "18.193.194.86"
        };

        [HttpGet]
        [Route("api/PayOne/ApiRequest")]
        public ActionResult ApiRequest(int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                Program.DbLogger.Info(paymentRequestId);
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayOne);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var segment = clientBl.GetClientPaymentSegments(request.ClientId, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                var verifyInput = new
                {
                    MerchantId = segment == null ? partnerPaymentSetting.UserName : segment.ApiKey,
                    InvoiceId = request.ExternalTransactionId
                };
                var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PayOpApiUrl).StringValue;
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/api/v1/invoice/verify", url),
                    PostData = JsonConvert.SerializeObject(verifyInput)
                };
                var res = JsonConvert.DeserializeObject<PaymentRequestOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (res.Status == 1)
                {
                    var paymentInfo = new PaymentInfo
                    {
                        TrackingNumber = res.TrackingNumber,
                        CardNumber = res.CardNumber,
                        CardHolderName = res.HolderName
                    };
                    request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    request.CardNumber = paymentInfo.CardNumber;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else
                {
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, res.Description, notificationBl);
                }
                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))

                    return Conflict(new StringContent("OK", Encoding.UTF8));
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = ex.Message;
            }
            return Conflict(new StringContent(response, Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/PayOne/GetWithdrawal")]
        public ActionResult GetWaitingWithdrawal(WithdrawalRequestInput input)
        {
            var response = string.Empty;
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayOne);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(input.PartnerId,
                                   paymentSystem.Id, Constants.Currencies.IranianTuman, (int)PaymentRequestTypes.Withdraw);
                if (CommonFunctions.ComputeMd5(string.Format("{0}:{1}", input.PartnerId, partnerPaymentSetting.UserName)) != input.Sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    var currentDate = DateTime.UtcNow;
                    var toDate = currentDate.AddDays(1);
                    var fromDate = currentDate.AddMonths(-1);
                    var fDate = fromDate.Year * 1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
                    var tDate = toDate.Year * 1000000 + toDate.Month * 10000 + toDate.Day * 100 + toDate.Hour;

                    var filter = new FilterfnPaymentRequest
                    {
                        ToDate = tDate,
                        FromDate = fDate,
                        States = new FiltersOperation
                        {
                            IsAnd = true,
                            OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = (int) PaymentRequestStates.PayPanding
                                    }
                                }
                        },
                        PartnerId = partnerPaymentSetting.PartnerId,
                        PaymentSystemIds = new FiltersOperation
                        {
                            IsAnd = true,
                            OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = paymentSystem.Id
                                    }
                                }
                        }
                    };
                    var result = paymentSystemBl.GetPaymentRequests(filter, false).Select(x => new
                    {
                        UserId = x.ClientId,
                        x.FirstName,
                        x.LastName,
                        WithdrawalId = x.Id,
                        Amount = Convert.ToInt32(BaseBll.ConvertCurrency(x.CurrencyId, Constants.Currencies.IranianTuman, x.Amount)),
                        BankDetails = JsonConvert.DeserializeObject<PaymentInfo>(x.Info, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        })
                    });

                    return Ok(new StringContent(JsonConvert.SerializeObject(result, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }), Encoding.UTF8));
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = ex.Message;
            }

            return Ok(new StringContent(response, Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/PayOne/PayPaymentRequest")]
        public ActionResult PayPaymentRequest(PayPaymentRequestInput input)
        {
            var response = string.Empty;
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                using var paymentSystemBl = new PaymentSystemBll(clientBl);
                using var documentBl = new DocumentBll(clientBl);
                using var notificationBl = new NotificationBll(clientBl);
                var request = paymentSystemBl.GetPaymentRequestById(input.OrderId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.PayOne);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(input.PartnerId,
                                   paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var segment = clientBl.GetClientPaymentSegments(request.ClientId, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                if (CommonFunctions.ComputeMd5(string.Format("{0}:{1}", client.PartnerId, segment == null ? partnerPaymentSetting.UserName : segment.ApiKey)) != input.Sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                if (input.State == 1)
                {
                    request.ExternalTransactionId = input.MerchantOrderId;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, input.BankTransactionId + " " + input.Description,
                        null, null, false, string.Empty, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                }
                else
                {
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Description, null, null, false,
                                                       string.Empty, documentBl, notificationBl);
                }

                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                response = exp.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = ex.Message;
            }

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Ok(new StringContent(response, Encoding.UTF8));
           
        }
    }
}