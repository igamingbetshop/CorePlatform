using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    public class TotalProcessingController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.PaymentSystems.TotalProcessing);
        /*[HttpGet]
        [Route("api/TotalProcessing/ApiRequest/{paymentId}")]
        public HttpResponseMessage ApiRequest([FromUri]string id, int paymentId)
        {
			BaseBll.CheckIp(WhitelistedIps);
			HttpResponseMessage response;
			var content = string.Empty;


			using (var paymentSystemBl = new PaymentSystemBll(WebApiApplication.Identity, WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    using (var documentBl = new DocumentBll(paymentSystemBl))
                    {
                        try
                        {
                            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(id));

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
                            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(result));
                            if (result.Result.Code == "000.000.000")
                            {
                                clientBl.ApproveDepositFromPaymentSystem(request, false);
                                PaymentHelpers.CheckNthDepositBonus(client.Id, request.Amount, WebApiApplication.Identity.LanguageId, WebApiApplication.DbLogger);

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

                            WebApiApplication.DbLogger.Error(exp);
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent(exp.Message, Encoding.UTF8) };
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex);
                        }
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, Content = new StringContent("", Encoding.UTF8) };
                    }
                }
            }
        }*/

        [HttpGet]
		[Route("api/TotalProcessing/ApiRequest")]
		public HttpResponseMessage ApiRequest([FromUri]long paymentRequestId, [FromUri]string status)
		{			
			using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(paymentSystemBl))
				{
					using (var documentBl = new DocumentBll(paymentSystemBl))
					{
                        using (var notificationBl = new NotificationBll(paymentSystemBl))
                        {
                            try
                            {
                                BaseBll.CheckIp(WhitelistedIps);
                                var request = paymentSystemBl.GetPaymentRequestById(paymentRequestId);
                                if (request == null)
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);

                                var client = CacheManager.GetClientById(request.ClientId.Value);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
                                    request.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                                if (status == "000.000.000")
                                {
                                    clientBl.ApproveDepositFromPaymentSystem(request, false, out List<int> userIds);
                                    foreach (var uId in userIds)
                                    {
                                        PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                    }
                                    PaymentHelpers.RemoveClientBalanceFromCache(request.ClientId.Value);
                                    BaseHelpers.BroadcastBalance(request.ClientId.Value);
                                }
                                else
                                {
                                    clientBl.ChangeDepositRequestState(request.Id, PaymentRequestStates.Deleted, string.Empty, notificationBl);
                                }
                                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                            }
                            catch (FaultException<BllFnErrorType> ex)
                            {
                                if (ex.Detail != null &&
                                    (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
                                    ex.Detail.Id == Constants.Errors.RequestAlreadyPayed))
                                {
                                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                                }
                                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                                WebApiApplication.DbLogger.Error(exp);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                            }
                            return new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict };
                        }
					}
				}
			}
		}
	}
}
