using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Zippy;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class ZippyController : ControllerBase
    {
        [HttpPost]
        [Route("api/Zippy/WebpayRequest")]
        [Route("api/Zippy/CashInRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = new ResultResponse
            {
                Code = 0,
                Description = string.Empty
            };
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MERCHANTREQUESTID));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", input.MERCHANTREQUESTID, Convert.ToInt32(input.AMOUNT), input.CODE, partnerPaymentSetting.Password));
                if (sign != input.SIGN)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.CODE == 0)
                {
                    request.ExternalTransactionId = string.IsNullOrEmpty(input.ZIPPYID) ? string.Format("{0}_{0}", request.Id) : input.ZIPPYID;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else if (input.CODE == 9 || input.CODE == 12)
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    ;
                }
                response.Code = ex.Detail.Id;
                response.Description = ex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Code = Constants.Errors.GeneralException;
                response.Description = ex.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }

        [Route("api/Zippy/PayInRequest")]
        public ActionResult PayInRequest(RequestResultInput input)
        {
            var response = new ResultResponse
            {
                Code = 0,
                Description = string.Empty
            };
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MERCHANTREQUESTID));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", input.MERCHANTREQUESTID, input.AMOUNT, input.CODE, partnerPaymentSetting.Password));
                if (sign != input.SIGN)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.CODE == 0)
                {
                    request.ExternalTransactionId = string.IsNullOrEmpty(input.ZIPPYID) ? string.Format("{0}_{0}", request.Id) : input.ZIPPYID;
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else if (input.CODE == 9 || input.CODE == 12)
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    ;
                }
                response.Code = ex.Detail.Id;
                response.Description = ex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Code = Constants.Errors.GeneralException;
                response.Description = ex.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/Zippy/CashOutRequest")]
        public ActionResult PayoutRequest(RequestResultInput input)
        {
            var response = new ResultResponse
            {
                Code = 0,
                Description = string.Empty
            };
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(clientBl);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MERCHANTREQUESTID));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
                var sign = string.Empty;
                if (paymentSystem.Name == Constants.PaymentSystems.ZippyCashGenerator)
                    sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", request.Id, input.AMOUNT, input.CODE, partnerPaymentSetting.Password));
                else
                    sign = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", request.Id, Convert.ToInt32(request.Amount), input.CODE, partnerPaymentSetting.Password));

                if (sign != input.SIGN)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.CODE == 0)
                {
                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                   null, null, false, string.Empty, documentBl, notificationBl);
                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                }
                else
                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, input.MESSAGE, null, null, false, 
                                                        string.Empty, documentBl, notificationBl);

                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    ;
                }
                response.Code = ex.Detail.Id;
                response.Description = ex.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                response.Code = Constants.Errors.GeneralException;
                response.Description = ex.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            return Conflict(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }
    }
}