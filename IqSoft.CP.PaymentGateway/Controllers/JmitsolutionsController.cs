using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.JmitSolutions;
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
public class JmitSolutionsController : ApiController
{
	[HttpPost]
	[Route("api/Jmitsolutions/ApiRequest")]
	public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
	{
		var response = "SUCCESS";
		var httpResponseMessage = new HttpResponseMessage
		{
			StatusCode = HttpStatusCode.OK
		};
		try
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
			using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
			using (var clientBl = new ClientBll(paymentSystemBl))
			using (var notificationBl = new NotificationBll(paymentSystemBl))
			using (var documentBl = new DocumentBll(paymentSystemBl))
			{
				var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.OrderNumber)) ??
					throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
				if (((int)(paymentRequest.Amount*100)) != input.Amount)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestInValidAmount);
				if (input.Currency != paymentRequest.CurrencyId)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);
				if (input.Status.ToLower() == "approved")
				{
					if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
					{
						clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
						PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
						BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
					}
					else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
					{
						var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, input.Status,
																	  null, null, false, paymentRequest.Parameters, documentBl, notificationBl);
						clientBl.PayWithdrawFromPaymentSystem(resp, documentBl, notificationBl);
					}

				}
				else if (input.Status.ToLower() == "declined")
				{
					if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
						clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.GatewayDetails?.DeclineReason, notificationBl);
					else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
					{
						clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.GatewayDetails?.DeclineReason, null, null,
														   false, paymentRequest.Parameters, documentBl, notificationBl);
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