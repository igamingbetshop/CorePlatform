using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Help2Pay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class Help2PayController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "61.14.177.254",
            "61.64.48.194",
            "61.64.48.206",
            "209.9.49.214",
            "203.192.140.222",
            "218.189.20.38",
            "52.64.184.122",
            "13.236.99.26",
            "13.55.162.28",
            "3.105.171.93",
            "3.104.235.239"
        };

        [HttpPost]
        [Route("api/Help2Pay/ApiRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var partnerBl = new PartnerBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Reference));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(Convert.ToInt32(input.Customer));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || string.IsNullOrEmpty(partnerPaymentSetting.UserName))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                               input.Merchant, input.Reference,
                                               input.Customer, input.Amount,
                                               input.Currency, input.Status,
                                               partnerPaymentSetting.Password);
                signature = CommonFunctions.ComputeMd5(signature);
                if (signature.ToLower() != input.Key.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                if (input.Status == Help2PayHelpers.Statuses.Approved || input.Status == Help2PayHelpers.Statuses.Success)
                {
                    request.ExternalTransactionId = input.ID;
                    paymentSystemBl.ChangePaymentRequestDetails(request);

                    response = "State=OK";
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
                }
                if (input.Status == Help2PayHelpers.Statuses.Failed ||
                    input.Status == Help2PayHelpers.Statuses.Rejected ||
                    input.Status == Help2PayHelpers.Statuses.Canceled)
                {
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, input.Status.ToString(), notificationBl);
                    return Ok(new StringContent("State=OK", Encoding.UTF8));
                }
                else
                {
                    response = "State=RETRY";
                    return Conflict(new StringContent(response, Encoding.UTF8));
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "State=OK";
                    return Ok(new StringContent(response, Encoding.UTF8));
                }
                Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response = "State=RETRY&ErrorDescription=" + ex.Message;
                return Conflict(new StringContent(response, Encoding.UTF8));
            }
        }

        [HttpPost]
        [Route("api/Help2Pay/VerifyTransaction")]
        public  ActionResult PayoutVerificationRequest([FromQuery] CheckInput checkInput)
        {
            try
            {                
                using (var partnerBl = new PartnerBll(new SessionIdentity(), Program.DbLogger))
                {
                    var ip = string.Empty;
                    if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                        ip = header.ToString();
                    BaseBll.CheckIp(WhitelistedIps, ip);
                    using (var paymentSystemBl = new PaymentSystemBll(partnerBl))
					{
						var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Help2Pay);
						if (paymentSystem == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
						var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(checkInput.transId));
						if (request == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(request.ClientId);
						if (client == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
						if (partnerPaymentSetting == null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
						var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
						var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
													   partnerPaymentSetting.UserName, checkInput.transId,
													   client.Id, request.Amount.ToString("F"),
													   request.CurrencyId, request.LastUpdateTime.ToString("yyyyMMddHHmmss"),
													   paymentInfo.BankAccountNumber, partnerPaymentSetting.Password);
						signature = CommonFunctions.ComputeMd5(signature);

						//if (signature.ToLower() != checkInput.key.ToLower())
						//    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);
					
                        return Ok(new StringContent("true", Encoding.UTF8));
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(new Exception(ex.Detail.Message));
                return Ok(new StringContent("false", Encoding.UTF8));
              
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return Ok(new StringContent("false", Encoding.UTF8)); 
            }                          
        }

        [HttpPost]
        [Route("api/Help2Pay/PayoutResult")]
        public ActionResult PayoutRequest(PayoutResultInput input)
        {          
            var response = new ResultOutput
            {
                StatusCode = Help2PayHelpers.Statuses.Success,
                Message = "Succes"
            };
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);

                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Help2Pay);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentSystemNotFound);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionID));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                if (Convert.ToInt64(input.MemberCode) != client.Id)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(request.Info);
                var signature = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                               input.MerchantCode, input.TransactionID,
                                               input.MemberCode, Convert.ToDecimal(input.Amount).ToString("F"),
                                               input.CurrencyCode, input.Status, partnerPaymentSetting.Password);
                signature = CommonFunctions.ComputeMd5(signature);

                if (signature.ToLower() != input.Key.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                if (request.Amount != Convert.ToDecimal(input.Amount))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                request.ExternalTransactionId = input.ID;
                paymentSystemBl.ChangePaymentRequestDetails(request);
                if (input.Status == Help2PayHelpers.Statuses.Success)//success
                {
                    var req = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                  null, null, false, request.Parameters, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(req, documentBl, notificationBl);
                }
                else if (input.Status == Help2PayHelpers.Statuses.Failed && request.Status == (int)PaymentRequestStates.PayPanding)
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.Message, null, null, false,
                                                        request.Parameters, documentBl, notificationBl);
                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.StatusCode = Help2PayHelpers.Statuses.Failed;
                response.Message = ex.Detail.Message;
                Program.DbLogger.Error(new Exception(ex.Detail.Message));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.StatusCode = Help2PayHelpers.Statuses.Failed;
                response.Message = ex.Message;
            }
            var output =  CommonFunctions.ToXML(response);
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(output, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            // return resp;
            return Ok();//todo
        }
    }
}