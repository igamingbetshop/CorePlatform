﻿using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Cartipal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class CartipalController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "?"
        };

        [HttpGet]
        [Route("api/Cartipal/ApiRequest")]
        public ActionResult ApiRequest([FromQuery]int paymentRequestId)
        {
            var response = string.Empty;
            try
            {
                var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
                var inputString = string.Join("&", queryString.AllKeys.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x), HttpUtility.UrlEncode(queryString[x]))));
                Program.DbLogger.Info("QuerString" + JsonConvert.SerializeObject(inputString));

                //   BaseBll.CheckIp(WhitelistedIps);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var partnerBl = new PartnerBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                            request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var segment = clientBl.GetClientPaymentSegments(client.Id, partnerPaymentSetting.PaymentSystemId).OrderBy(x => x.Priority).FirstOrDefault();
                var url = segment == null ? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CartipalApiUrl).StringValue : segment.ApiUrl;

                var paymentRequestInput = new
                {
                    api_key = segment == null ? partnerPaymentSetting.Password : segment.ApiKey
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/invoice/check/{1}", url, request.ExternalTransactionId),
                    PostData = CommonFunctions.GetUriDataFromObject(paymentRequestInput)
                };
                var result = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (string.IsNullOrEmpty(result.ErrorDescription) && !string.IsNullOrEmpty(result.BankCode))
                {
                    var paymentInfo = new PaymentInfo
                    {
                        BankCode = result.BankCode,
                        CardNumber = result.CardNumber,
                        InvoiceId = result.InvoiceKey
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
                else if (result.InProcess == 0)
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, result.ErrorDescription, notificationBl);
                return Ok(new StringContent("OK", Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));

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
        [HttpGet]
        [Route("api/Cartipal/PayoutRequest")]
        public ActionResult PayoutRequest(PayoutInput input, int payment)
        {
            var response = string.Empty;
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            var inputString = string.Join("&", queryString.AllKeys.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x), HttpUtility.UrlEncode(queryString[x]))));
            Program.DbLogger.Info("QuerString" + JsonConvert.SerializeObject(inputString));
            Program.DbLogger.Info("Input" + JsonConvert.SerializeObject(input));

            //   BaseBll.CheckIp(WhitelistedIps);
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBll = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(clientBl);
                var request = paymentSystemBl.GetPaymentRequestById(input.PaymentRquestId);
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);

                if (input.WithdrawalId != request.ExternalTransactionId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.CartipalApiUrl).StringValue;

                var paymentRequestInput = new
                {
                    api_key = partnerPaymentSetting.Password,
                    withdrawal_id = request.ExternalTransactionId
                };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/invoice/request", url),
                    PostData = CommonFunctions.GetUriDataFromObject(paymentRequestInput)
                };
                var result = JsonConvert.DeserializeObject<PayoutInput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));

                if (result.Status == 1)
                {
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                   null, null, false, string.Empty, documentBll, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                }
                else if (result.Status == 0 && !string.IsNullOrEmpty(result.ErrorDescription))
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, result.ErrorDescription, notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                    return Ok(new StringContent("OK", Encoding.UTF8));
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
    }
}