using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.CoinsPaid;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;

namespace IqSoft.CP.PaymentGateway.Controllers
{

    [ApiController]
    public class CoinsPaidController : ControllerBase
    {

        [Route("api/CoinsPaid/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                Program.DbLogger.Info(JsonConvert.SerializeObject(inputString));
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
                        var isDeposit = input.Type == "deposit_exchange";
                        var prId = isDeposit ? input.CryptoAddress.ForeignId : input.ForeignId;
                        var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(prId));
                        if (paymentRequest == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var signature = Request.Headers["X-Processing-Signature"];
                        var client = CacheManager.GetClientById(paymentRequest.ClientId);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.CoinsPaid); 
                        if (paymentSystem == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        if (partnerPaymentSetting == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                        var sign = CommonFunctions.ComputeHMACSha512(inputString, partnerPaymentSetting.Password).ToLower();
                        Program.DbLogger.Info(JsonConvert.SerializeObject($"SIGN {sign} SIGNATURE {signature}"));
                        if (sign != signature)
                        {
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        }
                        if(isDeposit)
                           paymentRequest.Amount = Math.Round(Convert.ToDecimal(input.CurrencyReceived.AmountMinusFee));
                        paymentRequest.ExternalTransactionId = input.Transactions.FirstOrDefault().Txid;
                        paymentRequest.Info = JsonConvert.SerializeObject(input);
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                        if (input.Status == "confirmed")
                        {
                            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                            {
                                clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                            }
                            else
                            {
                                using (var documentBll = new DocumentBll(paymentSystemBl))
                                {
                                    using (var notificationBl = new NotificationBll(clientBl))
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
                                         null, null, false, paymentRequest.Parameters, documentBll, notificationBl);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                            //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                        }
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
                    }
                }
                Program.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                Program.DbLogger.Error(response);
            }
            var a = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
            Program.DbLogger.Info(JsonConvert.SerializeObject(a));
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
        }
    }
}