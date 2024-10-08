using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.PaymentGateway.Models.FugaPay;
using IqSoft.CP.Common;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using System.ServiceModel;
using System;
using System.Net;
using System.Text;
using IqSoft.CP.PaymentGateway.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class FugaPayController : ApiController
    {
        [HttpPost]
        [Route("api/FugaPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            var userIds = new List<int>();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                WebApiApplication.DbLogger.Info("inputString: " + inputString);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var clientBl = new ClientBll(paymentSystemBl))
                using (var notificationBl = new NotificationBll(paymentSystemBl))
                using (var documentBl = new DocumentBll(paymentSystemBl))
                {
                    var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.TransactionDetails.OrderID));
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                    var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
                                                                                       client.CurrencyId, paymentRequest.Type);
                    var merchantData = partnerPaymentSetting.UserName.Split(',');
                    var sign = CommonFunctions.ComputeMd5($"{input.TransactionDetails.TrnRequestId}.{paymentRequest.Id}." +
                                                          $"{input.TransactionDetails.Status}.{input.TransactionDetails.Amount:F}.{merchantData[1]}");
                    if (sign.ToLower() != input.Signature)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);

                    if (paymentRequest.Amount != input.TransactionDetails.Amount)
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
                        var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
                                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
                        if (!parameters.ContainsKey("RequestedAmount"))
                            parameters.Add("RequestedAmount", paymentRequest.Amount.ToString());
                        else
                            parameters["RequestedAmount"] =  paymentRequest.Amount.ToString();
                        if (!parameters.ContainsKey("UpdatedAmount"))
                            parameters.Add("UpdatedAmount", input.TransactionDetails.Amount.ToString());
                        else
                            parameters["UpdatedAmount"] =  input.TransactionDetails.Amount.ToString();
                        paymentRequest.Amount = input.TransactionDetails.Amount;
                        paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
                        paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                    }
                    var merchantKeys = partnerPaymentSetting.Password.Split(',');

                    if (input.TransactionDetails.Status.ToUpper() == "CMD")
                    {
                        if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                        {
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds, comment: input.Desc);
                        }
                        else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                        {

                            var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.TransactionDetails.Status,
                                                                           null, null, false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);
                            clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
                    }
                    else if(input.TransactionDetails.Status.ToUpper() == "RJT" || input.TransactionDetails.Status.ToUpper() == "CNL")
                        if(paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
                        clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Desc, notificationBl);
                    else if(paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
                            clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Desc, null, null, 
                                                                false, paymentRequest.Parameters, documentBl, notificationBl, out userIds);

                    foreach (var uId in userIds)
                    {
                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                    }
                    response = "OK";
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    response = "OK";
                }
                response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Detail.Id + ", " + ex.Detail.Message);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_Error: " + ex.Message);
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}