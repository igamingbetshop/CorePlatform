using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.Piastrix;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class PiastrixController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "87.98.145.206",
            "51.68.53.104",
            "51.68.53.105",
            "51.68.53.106",
            "51.68.53.107",
            "91.121.216.63",
            "37.48.108.180",
            "37.48.108.181"
        };

        [HttpPost]
        [Route("api/Piastrix/ApiRequest")]
        public ActionResult ApiRequest(RequestResultInput input)
        {
            var response = string.Empty;
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.shop_order_id));
                if (request == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                var signature = CommonFunctions.GetSortedValuesAsString(input, ":") +
                                                                     partnerPaymentSetting.Password;
                signature = CommonFunctions.ComputeSha256(signature);
                if (signature.ToLower() != input.sign.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongPaymentRequest);

                if (input.status.ToLower() == "success")
                {
                    request.ExternalTransactionId = input.payment_id.ToString();
                    if (!string.IsNullOrEmpty(input.payer_id))
                    {
                        var inp = !string.IsNullOrEmpty(request.Parameters) ?
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters) : new Dictionary<string, string>();
                        inp.Add("payer_id", input.payer_id);
                        request.Parameters = JsonConvert.SerializeObject(inp);
                    }
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    response = "OK";
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                    return Ok(new StringContent(response, Encoding.UTF8));

                }
                else if (input.status.ToLower() == "rejected")
                {
                    request.ExternalTransactionId = input.payment_id.ToString();
                    paymentSystemBl.ChangePaymentRequestDetails(request);
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                else
                {
                    response = "Error";
                    return Conflict(new StringContent(response, Encoding.UTF8));

                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    return Ok(new StringContent("OK", Encoding.UTF8));
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
                return Conflict(new StringContent(exp.Message, Encoding.UTF8));

            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return Conflict(new StringContent(ex.Message, Encoding.UTF8));
            }
        }
    }
}