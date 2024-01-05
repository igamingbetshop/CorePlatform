using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Stripe;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class StripeController : ApiController
	{
		[HttpPost]
		[Route("api/Stripe/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
				WebApiApplication.DbLogger.Info(inputString);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var sign = HttpContext.Current.Request.Headers.Get("Signature");
							var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
							PaymentRequest paymentRequest = null;
							if (input.Type == "checkout.session.completed")
							{
								paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Data.Object.Metadata.PaymentRequestId)) ??
												 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
								paymentRequest.ExternalTransactionId = input.Data.Object.PaymentIntent;
								paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
								response = "OK";
							}
							else if (input.Type == "payment_intent.succeeded")
							{
                               paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.Data.Object.Id, CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Stripe).Id) ??
									throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
								var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
								var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
															paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
								PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
								BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
								response = "OK";
							}							
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
				else
				{
					response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error(response);
			}
			catch (Exception ex)
			{
				response = ex.Message;
				WebApiApplication.DbLogger.Error(response);
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}

			httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}
	}
}