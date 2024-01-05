using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.KazPost;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.DAL.Models;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [ApiController]
    public class KazPostController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "61.14.177.254"//change
        };

        [HttpGet]
        [Route("api/KazPost/ApiRequest/orderId")]
        public ActionResult ApiRequest(string orderId)
        {
            var response = string.Empty;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    try
                    {
                        Program.DbLogger.Info("orderId=" + orderId);

                        var ip = string.Empty;
                        if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                            ip = header.ToString();
                        BaseBll.CheckIp(WhitelistedIps, ip);
                        var request = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(orderId));
                        if (request == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                        var client = CacheManager.GetClientById(request.ClientId);
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        var secretKey = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.KazPostToken).StringValue;
                        var url = CacheManager.GetPartnerSettingByKey(null, Constants.PartnerKeys.KazPostUrl).StringValue;
                        url = string.Format("{0}{1}{2}", url, " /api/v0/orders/payment/", request.ExternalTransactionId);
                        var requestHeaders = new Dictionary<string, string> { { "Authorization", "Token " + secretKey } };
                        var httpRequestInput = new HttpRequestInput
                        {
                            ContentType = Constants.HttpContentTypes.ApplicationJson,
                            RequestMethod = HttpMethod.Get,
                            Url = url
                        };
                        var checkResponse = JsonConvert.DeserializeObject<PaymentOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                        Program.DbLogger.Info("checkResponse=" + JsonConvert.SerializeObject(checkResponse));
                        if (checkResponse.Success)
                        {
                            if (checkResponse.Result.Status == (int)KazPostHelpers.Statuses.Paid ||
                                checkResponse.Result.Status == (int)KazPostHelpers.Statuses.Confirmed)
                            {
								clientBl.ApproveDepositFromPaymentSystem(request, false);
                            }
                        }
                     
                        return Ok(new StringContent(response, Encoding.UTF8));

                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null &&
                            (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                            ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                        {
                            return Ok(new StringContent(response, Encoding.UTF8));
                        }
                        Program.DbLogger.Error(ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName));

                        return Conflict(new StringContent(ex.Message, Encoding.UTF8));
                    }
                    catch (Exception ex)
                    {
                        Program.DbLogger.Error(ex);
                        return Conflict(new StringContent(ex.Message, Encoding.UTF8));
                    }
                }
            }
        }
    }
}
