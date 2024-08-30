using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Huch;
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
	public class HuchController : ApiController
	{
		[HttpPost]
		[Route("api/Huch/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = "OK";
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
					{
						var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.order_ref)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						paymentRequest.ExternalTransactionId = input.payment_id;
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							if (input.payment_status == "PAID_RECEIVED" || input.payment_status == "PAID")
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
							else if (input.payment_status == "EXPIRED" || input.payment_status == "CANCELLED" || input.payment_status == "FAILED" || 
								     input.payment_status == "AUTH_FAILED" || input.payment_status == "EXECUTE_FAILED")
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.payment_status, notificationBl);
							PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
							BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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