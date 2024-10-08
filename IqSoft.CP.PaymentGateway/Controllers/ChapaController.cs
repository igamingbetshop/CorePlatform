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
using System.Web.Http;
using System.Web.Http.Cors;
using System.Collections.Generic;
using System.Web;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class ChapaController : ApiController
	{
		public static class Events
		{
			public const string Payment = "charge.success";
			public const string Payout = "payout.success";
		}

		[HttpPost]
		[Route("api/Chapa/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var response = "OK";
			var userIds = new List<int>();
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
				WebApiApplication.DbLogger.Info(inputString);
				var inputSign = HttpContext.Current.Request.Headers.Get("x-chapa-signature");
				if (string.IsNullOrEmpty(inputSign))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				WebApiApplication.DbLogger.Info("x-chapa-signature: " + inputSign);
				var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.Event==Events.Payment ? input.TxRef : input.Reference)) ??
						throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
					var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
					var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																					   client.CurrencyId, paymentRequest.Type);
					var sign = CommonFunctions.ComputeHMACSha256(inputString, partnerPaymentSetting.Password.Split(',')[1]);
					if (sign.ToLower() != inputSign.ToLower())
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
					if (paymentRequest.Amount != input.Amount)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongOperationAmount);
					if (paymentRequest.CurrencyId != input.Currency)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongCurrencyId);

					var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters) ?? new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(input.PaymentMethod))
						if (parameters.ContainsKey("PaymentMethod"))
							parameters["PaymentMethod"] = input.PaymentMethod;
						else
							parameters.Add("PaymentMethod", input.PaymentMethod);

					if (parameters.ContainsKey("Charge"))
						parameters["Charge"] = input.Charge.ToString();
					else
						parameters.Add("Charge", input.Charge.ToString());
					paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
					paymentRequest.ExternalTransactionId = input.Event == Events.Payment ? input.Reference : input.ChapaReference;
					paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
					using (var clientBl = new ClientBll(paymentSystemBl))
					using (var notificationBl = new NotificationBll(paymentSystemBl))
					using (var documentBll = new DocumentBll(paymentSystemBl))
					{
						if (input.Status.ToLower() == "success")
						{
							if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
							{
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds);
                            }
							else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
							{
								var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
																			null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);
								clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
							}
						}
						else if (input.Status.ToLower() == "failed")
							if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
								clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.Status, notificationBl);
							else
								clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.Status,
																		   null, null, false, string.Empty, documentBll, notificationBl, out userIds);

                        foreach (var uId in userIds)
                        {
                            PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                        }
                        PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
						BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
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