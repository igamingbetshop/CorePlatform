using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.MaldoPay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class MaldoPayController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.MaldoPay);

        [HttpPost]
        [Route("api/MaldoPay/ApiRequest")]
        public HttpResponseMessage ApiRequest(PaymentInput input)
        {
            var response = "Failed";
            var userIds = new List<int>();
            WebApiApplication.DbLogger.Info("input: " + JsonConvert.SerializeObject(input));
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(paymentSystemBl))
                    {
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.ReferenceOrderId)) ??
                             throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                            var client = CacheManager.GetClientById(request.ClientId.Value);
                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId,
                                                                                               client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            //var encryptionKey = partnerPaymentSetting.Password.Split(',')[0];
                            //if (!string.IsNullOrEmpty(inputString))
                            //{
                            //    var checkSum = input.Checksum;
                            //    input.Checksum = null;
                            //    var responseRaw = (JObject)JsonConvert.DeserializeObject(inputString);
                            //    responseRaw.Remove("checksum");
                            //    var payload = string.Join("|", responseRaw.Properties().Select(y => y.First().ToObject<string>()));
                            //    var ss = CommonFunctions.ComputeHMACSha256(payload, encryptionKey).ToLower();
                            //    WebApiApplication.DbLogger.Info("calculatedCheckSum: " + ss);
                            //}
                            //if (CommonFunctions.ComputeHMACSha256(ss, encryptionKey).ToLower() != checkSum)
                            //    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                            if (request.Amount != input.Amount && request.Type == (int)PaymentRequestTypes.Withdraw)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
                            if (input.Result.ToUpper() == "CONFIRMED")
                            {
                                if (request.Type == (int)PaymentRequestTypes.Deposit)
                                {
                                    var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);

                                    if (parameters.ContainsKey("InitialAmount"))
                                        parameters["InitialAmount"] = request.Amount.ToString("F");
                                    else
                                        parameters.Add("InitialAmount", request.Amount.ToString("F"));
                                    if (parameters.ContainsKey("UpdatedAmount"))
                                        parameters["UpdatedAmount"] = input.Amount.ToString("F");
                                    else
                                        parameters.Add("UpdatedAmount", input.Amount.ToString("F"));
                                    request.Amount = input.Amount;
                                    request.Parameters = JsonConvert.SerializeObject(parameters);
                                    paymentSystemBl.ChangePaymentRequestDetails(request);
                                    clientBl.ApproveDepositFromPaymentSystem(request, false, out userIds, comment: input.Reason);
                                }
                                else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                                {
                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                    {
                                        var resp = clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Approved, string.Empty,
                                                                                       null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                                        clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
                                    }
                                }
                                PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                BaseHelpers.BroadcastBalance(request.ClientId.Value);
                            }
                            else if (input.Result.ToUpper() == "CANCELED" || input.Result.ToUpper() == "DECLINED")
                            {
                                var desc = $"Code: {input.CodeId} Reason: {input.Reason}";
                                if (request.Type == (int)PaymentRequestTypes.Deposit)
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, desc, notificationBl);
                                else if (request.Type == (int)PaymentRequestTypes.Withdraw)
                                    using (var documentBll = new DocumentBll(paymentSystemBl))
                                        clientBl.ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Failed, desc, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
                            }
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            response = "OK";
                        }
                    }
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
                WebApiApplication.DbLogger.Error($"InputString: {JsonConvert.SerializeObject(input)} _Error: {ex.Detail.Id}, {ex.Detail.Message}");
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error($"InputString: {JsonConvert.SerializeObject(input)} _Error: {ex.Message}");
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(response, Encoding.UTF8) };
        }
    }
}