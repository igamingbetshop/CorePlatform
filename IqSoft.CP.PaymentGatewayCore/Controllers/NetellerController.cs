using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Models.Neteller;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net.Http;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class NetellerController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "88.99.175.216",
            "135.181.78.212"
        };

        [HttpGet]
        [Route("api/Neteller/ApiRequest")]
        public ActionResult ApiRequest([FromQuery] int paymentRequestId)
        {
            try
            {
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                using var documentBl = new DocumentBll(paymentSystemBl);
                using var notificationBl = new NotificationBll(paymentSystemBl);

                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(paymentRequestId));
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                var netellerApiUrl = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.NetellerApiUrl).StringValue;

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                var requestHeaders = new Dictionary<string, string>
                            {
                                { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(
                                string.Format("{0}:{1}", partnerPaymentSetting.UserName, partnerPaymentSetting.Password ))) }
                            };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = HttpMethod.Get,
                    Url = string.Format("{0}/paymenthandles/{1}", netellerApiUrl, request.ExternalTransactionId),
                    RequestHeaders = requestHeaders
                };
                var response = JsonConvert.DeserializeObject<PaymentRequestResult>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                if (response.Status.ToUpper() == "COMPLETED")
                {
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                }
                else if (response.Status.ToUpper() == "FAILED" || response.Status.ToUpper() == "EXPIRED")
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Failed, response.Status, notificationBl);
                return Ok();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail != null &&
                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                {
                    return Ok();
                }
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Program.DbLogger.Error(exp);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
            }
            return Conflict();
        }
    }
}