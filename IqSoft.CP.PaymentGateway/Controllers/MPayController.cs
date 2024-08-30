using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.MPAy;
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
	public class MPayController : ApiController
	{
		[HttpPost]
		[Route("api/MPay/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = JsonConvert.SerializeObject(new PaymentOutput());
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
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.data.trx)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
													paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
						var hash = CommonFunctions.ComputeSha1($"{partnerPaymentSetting.Password}{input.data.timestamp}{input.data.transaction_id}");
						if (input.data.checksum != hash)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
						var parameters = string.IsNullOrEmpty(paymentRequest.Parameters) ? new Dictionary<string, string>() :
								 JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters);
						if (!parameters.ContainsKey("Amount"))
							parameters.Add("Amount", paymentRequest.Amount.ToString());
						paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
						paymentRequest.ExternalTransactionId = input.data.transaction_id;
						paymentRequest.Amount = Convert.ToDecimal(input.data.amount);
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						using (var documentBll = new DocumentBll(paymentSystemBl))
						{
							if (input.data.status == "1")
								if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
								else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
								{
									var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
																				null, null, false, paymentRequest.Parameters, documentBll, notificationBl);
									clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
								}
								else if (input.data.status == "2")
								{
									if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
										clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.data.status, notificationBl);
									else
										clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.data.status,
																				   null, null, false, string.Empty, documentBll, notificationBl);
								}
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
					response = JsonConvert.SerializeObject(new PaymentOutput());
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