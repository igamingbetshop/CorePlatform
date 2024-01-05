using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    public class TotalProcessingController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "88.99.175.216"
        };

        /*[HttpGet]
        [Route("api/TotalProcessing/ApiRequest/{paymentId}")]
        public HttpResponseMessage ApiRequest([FromUri]string id, int paymentId)
        {
			BaseBll.CheckIp(WhitelistedIps);
			HttpResponseMessage response;
			var content = string.Empty;


			using (var paymentSystemBl = new PaymentSystemBll(Program.Identity, Program.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        try
                        {
                            Program.DbLogger.Info(JsonConvert.SerializeObject(id));

                            var request = paymentSystemBl.GetPaymentRequest(id, paymentId, (int)PaymentRequestTypes.Deposit);
                            if (request == null)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                            var client = CacheManager.GetClientById(request.ClientId);
                            if (client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                            var url = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.TotalProcessingUrl).StringValue;

                            var partner = CacheManager.GetPartnerById(client.PartnerId);
                            content =string.Format( content, partner.SiteUrl);

                            Dictionary<string, string> requestHeaders = new Dictionary<string, string> {
                                                                        { "Authorization", "Bearer " + partnerPaymentSetting.Password} };
                            var httpRequestInput = new HttpRequestInput
                            {
                                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                                RequestMethod = Constants.HttpRequestMethods.Post,
                                Url = string.Format("{0}/v1/checkouts", url),
                                RequestHeaders = requestHeaders,
                                PostData = string.Format("entityId={0}", partnerPaymentSetting.UserName)
                            };
                            var result = JsonConvert.DeserializeObject<CheckPaymentStateOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
                            Program.DbLogger.Info(JsonConvert.SerializeObject(result));
                            if (result.Result.Code == "000.000.000")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.CheckNthDepositBonus(client.Id, request.Amount, Program.Identity.LanguageId, Program.DbLogger);

                                response = Request.CreateResponse(HttpStatusCode.OK);
                                response.Content = new StringContent(content, Encoding.UTF8, "text/html");
                                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                                return response;
                            }
                            else
                            {
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("Error", Encoding.UTF8) };
                            }
                        }
                        catch (FaultException<BllFnErrorType> ex)
                        {
                            if (ex.Detail != null &&
                                (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK);
                                response.Content = new StringContent(content, Encoding.UTF8, "text/html");
                                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                                return response;
                            }
                            var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);

                            Program.DbLogger.Error(exp);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                        }
                        catch (Exception ex)
                        {
                            Program.DbLogger.Error(ex);
                        }
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("", Encoding.UTF8) };
                    }
                }
            }
        }*/

        [HttpGet]
        [Route("api/TotalProcessing/ApiRequest")]
        public ActionResult ApiRequest([FromQuery] long paymentRequestId, [FromQuery] string status)
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
                var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                if (request == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                if (status == "000.000.000")
                    clientBl.ApproveDepositFromPaymentSystem(request, false);
                else
                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
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