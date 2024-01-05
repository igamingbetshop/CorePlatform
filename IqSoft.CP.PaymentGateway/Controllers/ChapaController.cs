using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.PaymentGateway.Models.Chapa;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
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
	public class ChapaController : ApiController
	{
		[HttpPost]
		[Route("api/Chapa/ApiRequest")]
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
								var transactionInput = new Transaction();
								var transferInput = new Transfer();
								bool isDeposit = inputString.Contains("\"type\":\"API\"");
								if (isDeposit)
									transactionInput = JsonConvert.DeserializeObject<Transaction>(inputString);
								else
									transferInput = JsonConvert.DeserializeObject<Transfer>(inputString);
								var paymentRequest = isDeposit ? paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(transactionInput.TxRef)) :
																 paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(transferInput.Reference)) ??
										 throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
								paymentRequest.ExternalTransactionId = isDeposit ? transactionInput.Reference : transferInput.ChapaReference;
								if (transactionInput.Status == "success" || transferInput.Status == "success")
								{
									if (isDeposit)
										clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
									else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
									{
										var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
										  null, null, false, string.Empty, documentBll, notificationBl);
										clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
									}
									PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
									BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
									response = "OK";
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
}