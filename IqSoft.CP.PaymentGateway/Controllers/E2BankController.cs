using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Pix;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class PixController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pix).Id;

        [Route("api/E2Bank/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = string.Empty;
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));               
                using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
                            paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(depositeInput.ConciliationId.Substring(14)));
                        }
                        else
                        {
                            withdrawInput = JsonConvert.DeserializeObject<PaymentWithdrawInput>(inputString);
                            paymentRequest = paymentSystemBl.GetPaymentRequest(withdrawInput.UuId, ProviderId, (int)PaymentRequestTypes.Withdraw);
                        }

                        if (paymentRequest == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                        var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
                        var sign = HttpContext.Current.Request.Headers.Get("Signature");
                        var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.PixSecretKey).StringValue;
                        var jsonString = isDeposite ? JsonConvert.SerializeObject(depositeInput) : JsonConvert.SerializeObject(withdrawInput);
                        var signature = CommonFunctions.ComputeHMACSha256(jsonString, secretKey).ToLower();
                        if (sign.ToLower() != signature.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        if (depositeInput.Status == "PAYED")
                            clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                        else if (withdrawInput.Status == "DONE")
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
                        else if (withdrawInput.Status == "ERROR" || withdrawInput.Status == "CANCELED" || withdrawInput.Status == "UNDONE")
                        {
                            using (var documentBll = new DocumentBll(paymentSystemBl))
                            {
                                using (var notificationBl = new NotificationBll(paymentSystemBl))
                                {
                                    clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, 
                                        withdrawInput.Status, null, null, false, string.Empty, documentBll, notificationBl);
                                }
                            }
                        }
                        response = "OK";
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
                        BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent(response, Encoding.UTF8) };
                    }
                }
                WebApiApplication.DbLogger.Error(response);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                WebApiApplication.DbLogger.Error(response);
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response, Encoding.UTF8)
            };
        }
    }
}