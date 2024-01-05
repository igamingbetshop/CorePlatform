using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.EZeeWallet;
using IqSoft.CP.PaymentGateway.Models.PaymentProcessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class EZeeWalletController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string> //distribution
        {
           "88.99.175.216" //iqsoft
        };

        [HttpPost]
        [Route("api/EZeeWallet/ProcessPaymentRequest")]
        public HttpResponseMessage ProcessPaymentRequest(PaymentProcessingInput input)
        {
            var result = new ResultOutput();
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (request.Status != (int)PaymentRequestStates.Pending)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestAlreadyPayed);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var paymentGateway = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PaymentGateway).StringValue;
                var url = CacheManager.GetPartnerPaymentSystemByKey(client.PartnerId, request.PaymentSystemId, Constants.PartnerKeys.EZeeWalletUrl);
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                var cashierPageUrl = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeys.CashierPageUrl).StringValue;
                if (string.IsNullOrEmpty(cashierPageUrl))
                    cashierPageUrl = string.Format("https://{0}/user/1/deposit/", parameters["Domain"]);
                else
                    cashierPageUrl = string.Format(cashierPageUrl, parameters["Domain"]);
                result.RedirectUrl = cashierPageUrl;
                var paymentRequestInput = new
                {
                    transaction_id = "Merchant_" + request.Id.ToString(),
                    usage = partner.Name,
                    amount = Convert.ToInt32(request.Amount*100),
                    currency = client.CurrencyId,
                    source_wallet_id = input.WalletNumber,
                    source_wallet_pwd = input.WalletPassword,
                    return_success_url = cashierPageUrl,
                    return_failure_url = cashierPageUrl,
                    notification_url = string.Format("{0}/{1}", paymentGateway, "api/EZeeWallet/ApiRequest"),
                    merchant_website = cashierPageUrl
                };
                var byteArray = Encoding.Default.GetBytes(string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password));
                var headers = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(byteArray) } };

                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Post,
                    Url = string.Format("{0}/transfers", url),
                    RequestHeaders = headers,
                    PostData = JsonConvert.SerializeObject(paymentRequestInput)
                };
                var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                var paymentRequestOutput = JsonConvert.DeserializeObject<PaymentOutput>(response);
                request.ExternalTransactionId = paymentRequestOutput.TransferDetails.MerchantTransactionId;
                paymentSystemBl.ChangePaymentRequestDetails(request);
                if (paymentRequestOutput.TransferDetails.Status.ToLower() == "approved")
                {
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    //BaseHelpers.BroadcastBalance(request.ClientId);
                }
                else
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, response, notificationBl);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                result.StatusCode = ex.Detail.Id;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                result.StatusCode = Constants.Errors.GeneralException;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("api/EZeeWallet/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {

                        Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                        var request = paymentSystemBl.GetPaymentRequestById(input.MerchantTransactionId);
                        if (request == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                        var client = CacheManager.GetClientById(request.ClientId);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                            request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var sign = CommonFunctions.ComputeSha1(input.MerchantTransactionId.ToString() + partnerPaymentSetting.UserName);
                        if (sign.ToLower() != input.Signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        if (input.Status.ToLower() == "approved")
                        {
                            request.ExternalTransactionId = string.Format("{0}_{0}", request.Id);
                            paymentSystemBl.ChangePaymentRequestDetails(request);
                            clientBl.ApproveDepositFromPaymentSystem(request, false);
                          //  BaseHelpers.BroadcastBalance(request.ClientId);
                        }
                        //else if (input.CODE == 9)
                        //    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty);
                        httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new { unique_id = request.Id.ToString() }), Encoding.UTF8);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    Program.DbLogger.Error(JsonConvert.SerializeObject(ex.Detail));
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        Code = ex.Detail.Id,
                        ex.Detail.Message
                    }), Encoding.UTF8);
                }
                else
                {
                    Program.DbLogger.Error(JsonConvert.SerializeObject(ex.Detail));
                    httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        Code = Constants.Errors.GeneralException,
                        ex.Message
                    }), Encoding.UTF8);
                }
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(ex));

                httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    Code = Constants.Errors.GeneralException,
                    ex.Message
                }), Encoding.UTF8);
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponseMessage;
        }
    }
}