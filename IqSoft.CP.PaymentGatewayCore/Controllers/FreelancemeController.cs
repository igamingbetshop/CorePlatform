using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Freelanceme;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class FreelancemeController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
        };

        [HttpPost]
        [Route("api/Freelanceme/ApiRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = new RequestOutput
            {
                Error = new ErrorType
                {
                    Code = 0,
                    Description = string.Empty
                }
            };
            using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
            using var clientBl = new ClientBll(paymentSystemBl);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            try
            {
                //  BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}",
                    partnerPaymentSetting.UserName, input.AuthorizationData.Salt, partnerPaymentSetting.Password));

                if (hash != input.AuthorizationData.Hash)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                if (input.Status == 1)
                {
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else if (input.Status != 2)
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)

                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                     ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                    {
                        ;
                    }
                    else
                    {
                        response.Error.Code = ex.Detail.Id;
                        response.Error.Description = ex.Detail.Message;
                    }
                }
                else
                {
                    response.Error.Code = Constants.Errors.GeneralException;
                    response.Error.Description = ex.Message;
                }
                Program.DbLogger.Error(ex);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.Error.Code = Constants.Errors.GeneralException;
                response.Error.Description = ex.Message;
            }

            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
        }

        [HttpPost]
        [Route("api/Freelanceme/PayoutRequest")]
        public ActionResult PayoutRequest(RequestResultInput input)
        {
            var response = new RequestOutput
            {
                Error = new ErrorType
                {
                    Code = 0,
                    Description = string.Empty
                }
            };
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(clientBl))
                        {
                            try
                            {
                                //  BaseBll.CheckIp(WhitelistedIps);
                                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionId));
                                if (request == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                                var client = CacheManager.GetClientById(request.ClientId);
                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}",
                                    partnerPaymentSetting.UserName, input.AuthorizationData.Salt, partnerPaymentSetting.Password));

                                if (hash != input.AuthorizationData.Hash)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                                if (input.Status == 1)
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                          null, null, false, string.Empty, documentBl, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId);
                                }
                                else if (input.Status != 2)
                                    clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, string.Empty, null, null, 
                                                                        false, string.Empty, documentBl, notificationBl);
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null)

                                {
                                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                     ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                                    {
                                        ;
                                    }
                                    else
                                    {
                                        response.Error.Code = ex.Detail.Id;
                                        response.Error.Description = ex.Detail.Message;
                                    }
                                }
                                else
                                {
                                    response.Error.Code = Constants.Errors.GeneralException;
                                    response.Error.Description = ex.Message;
                                }
                                Program.DbLogger.Error(ex);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex);
                                response.Error.Code = Constants.Errors.GeneralException;
                                response.Error.Description = ex.Message;
                            }
                           
                            return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8));
                        }
                    }
                }
            }
        }
    }
}
