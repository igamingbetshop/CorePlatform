using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Jmitsolutions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;


[EnableCors(origins: "*", headers: "*", methods: "*")]
public class JmitsolutionsController : ApiController
{
	[HttpPost]
	[Route("api/Jmitsolutions/ApiRequest")]
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
						using (var documentBll = new DocumentBll(paymentSystemBl))
						{
							var transaction = JsonConvert.DeserializeObject<PaymentInput>(inputString);
							var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(transaction.OrderNumber)) ??
									 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
							paymentRequest.ExternalTransactionId = transaction.Token;
							if (transaction.Status == "approved")
							{
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
								PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
								BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
								response = "OK";
							}
							else if(transaction.Status == "declined")
							{
                                clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, transaction.GatewayDetails?.DeclineReason, notificationBl);
							}
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