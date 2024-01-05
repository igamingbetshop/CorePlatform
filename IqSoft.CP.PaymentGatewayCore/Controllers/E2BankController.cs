using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGatewayCore.Models.Pix;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ServiceModel;

namespace IqSoft.CP.PaymentGatewayCore.Controllers
{
    [ApiController]
    public class PixController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pix).Id;

        [HttpPost]
        [Route("api/E2Bank/ApiRequest")]
        public IActionResult ApiRequest([FromBody] JToken jsonbody)
        {
            var response = string.Empty;
            try
            {
                var inputString = jsonbody.ToString();
                Program.DbLogger.Info(JsonConvert.SerializeObject(inputString));               
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var depositeInput = new PaymentDepositInput();
                        var withdrawInput = new PaymentWithdrawInput();
                        bool isDeposite = false;
                        PaymentRequest paymentRequest;
                        if (inputString.Contains("conciliation_id"))
                        {
                            depositeInput = JsonConvert.DeserializeObject<PaymentDepositInput>(inputString);
                            isDeposite = true;
                            paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(depositeInput.conciliation_id.Substring(14)));
                        }
                        else
                        {
                            withdrawInput = JsonConvert.DeserializeObject<PaymentWithdrawInput>(inputString);
                            paymentRequest = paymentSystemBl.GetPaymentRequest(withdrawInput.uuid, ProviderId, (int)PaymentRequestTypes.Withdraw);
                        }

                        if (paymentRequest == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId);
                        var sign = Request.Headers["Signature"];
                        var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PixSecretKey).StringValue;
                        var jsonString = isDeposite ? JsonConvert.SerializeObject(depositeInput) : JsonConvert.SerializeObject(withdrawInput);
                        var signature = CommonFunctions.ComputeHMACSha256(jsonString, secretKey).ToLower();
                        if (sign.ToString().ToLower() != signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        if (depositeInput.status == "PAYED")
                        {
                            paymentRequest.ExternalTransactionId = depositeInput.conciliation_id;
                            paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                        }
                        else if (withdrawInput.status == "DONE")
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                               null, null, false, string.Empty, documentBll, notificationBl);
                                    clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                }
                            }
                        }
                        else if (withdrawInput.status == "ERROR" || withdrawInput.status == "CANCELED" || withdrawInput.status == "UNDONE")
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, withdrawInput.status, null, null, false, string.Empty, documentBll, notificationBl);
                                }
                            }
                        }
                        response = "OK";
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response = "OK";
                    else if (ex.Detail != null && ex.Detail.Id == Constants.Errors.WrongHash)
                    {
                        response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                        return Unauthorized(response);
                    }
                }
                Program.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            return Ok(response);
        }
    }
}