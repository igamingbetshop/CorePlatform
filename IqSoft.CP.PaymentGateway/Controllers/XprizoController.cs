using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Xprizo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class XprizoController : ApiController
	{
		[HttpPost]
		[Route("api/Xprizo/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var response = JsonConvert.SerializeObject(new { status = "success"});
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
					var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
					var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.transaction.reference)) ??
						throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
					var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																					   client.CurrencyId, (int)PaymentRequestTypes.Deposit);
					paymentRequest.ExternalTransactionId = input.transaction.id.ToString();
					paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
					using (var clientBl = new ClientBll(paymentSystemBl))
					using (var notificationBl = new NotificationBll(paymentSystemBl))
					using (var documentBll = new DocumentBll(paymentSystemBl))
					{
						if (input.status.ToLower() == "accepted")
						{
							clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }
                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
							BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
						}
						else if (input.status.ToLower() == "rejected" || input.status.ToLower() == "cancelled")
							clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, $"Status {input.status}, Description {input.description}", notificationBl);
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail?.Id != Constants.Errors.ClientDocumentAlreadyExists &&
					ex.Detail?.Id != Constants.Errors.RequestAlreadyPayed)
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