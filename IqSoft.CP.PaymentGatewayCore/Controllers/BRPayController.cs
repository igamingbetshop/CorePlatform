using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.BRPay;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace IqSoft.CP.PaymentGatewayCore.Controllers
{

    [ApiController]
    public class BRPayController : ControllerBase
    {

        [Route("api/BRPay/ApiRequest")]
        public ActionResult ApiRequest(HttpRequestMessage httpRequestMessage)
        {
            var response = new PaymentResponse();
            try
            {
                var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
                Program.DbLogger.Info(JsonConvert.SerializeObject(inputString));
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                var decode = HttpUtility.UrlDecode(inputString);
                var json = decode.Replace("sp_json=", "");
                var list = JsonConvert.DeserializeObject<List<PaymentInput>>(json);
                var input = list.FirstOrDefault();
                var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderId));
                if (paymentRequest == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(paymentRequest.ClientId);
                var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BRPay);
                if (paymentSystem == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentSystemNotFound);
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                var orderdParams = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", input.Amount, input.Currency,
                                           input.NetAmount, input.OrderId, input.PaymentId, input.PaymentSystem, input.PsAmount,
                                           input.Result, input.Salt);
                var sign = CommonFunctions.ComputeMd5("ApiRequest;" + orderdParams + ";" + partnerPaymentSetting.Password);
                if (sign != input.Signature)
                {
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                }

                paymentRequest.Info = JsonConvert.SerializeObject(input);
                paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
                if (input.Result == 1)
                {
                    clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
                    PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
                    //BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
                }
                else
                {
                    clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, null, notificationBl);
                }
                response.Status = "ok";
                response.Salt = input.Salt;
                response.Signature = input.Signature;

                var res = SerializeAndDeserialize.SerializeToXml(response, "response");
                return Ok(new StringContent(res, Encoding.UTF8, Constants.HttpContentTypes.TextXml));
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null)
                {
                    if (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists || ex.Detail.Id == Constants.Errors.RequestAlreadyPayed)
                        response.Status = "ok";
                    else if (ex.Detail.Id == Constants.Errors.WrongHash)
                    {
                        response.ErrorDescription = ex.Detail.Id + " " + ex.Detail.NickName;
                    }
                }
                else
                {
                    response.ErrorDescription = ex.Message;
                }
                var res = SerializeAndDeserialize.SerializeToXml(response, "response");
                Program.DbLogger.Error(JsonConvert.SerializeObject(res));
                return BadRequest(res);
            }
            catch (Exception ex)
            {
                response.ErrorDescription = ex.Message;
                var res = SerializeAndDeserialize.SerializeToXml(response, "response");
                Program.DbLogger.Error(JsonConvert.SerializeObject(res));
                return BadRequest(res);
            }
        }
    }
}