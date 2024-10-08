using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.QuikiPay;
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
	public class QuikiPayController : ApiController
	{
		[HttpPost]
		[Route("api/QuikiPay/ApiRequest")]
		public HttpResponseMessage ApiRequest(PaymentInput input)
		{
			var response = "OK";
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var partnerBl = new PartnerBll(new SessionIdentity(), WebApiApplication.DbLogger))
					{
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.order_id)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																						   client.CurrencyId, paymentRequest.Type);
						var signature = GetObjectPropertyValuesAsString(input, typeof(PaymentInput));

						var hash = CommonFunctions.ComputeSha256(signature + partnerPaymentSetting.Password);
						if(input.signature != hash)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
						var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(paymentRequest.Parameters) ?? new Dictionary<string, string>();
						if (!parameters.ContainsKey("PaymentMethod"))
							parameters.Add("PaymentMethod", input.payment_method);
						if (!parameters.ContainsKey("PayidCurrency"))
							parameters.Add("PayidCurrency", input.currency_symbol);
						if (!parameters.ContainsKey("PaymentRequestAmount"))
							parameters.Add("PaymentRequestAmount", paymentRequest.Amount.ToString());
						paymentRequest.Parameters = JsonConvert.SerializeObject(parameters);
						paymentRequest.ExternalTransactionId = input.tx_id;
						paymentRequest.Amount = Convert.ToDecimal(input.local_quantity);
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							if (input.status == "COMPLETED")
							{
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out List<int> userIds);
                                foreach (var uId in userIds)
                                {
                                    PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                                }
                            }
							else if (input.status == "REJECT")
								clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Deleted, input.status, notificationBl);

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

		[HttpPost]
		[Route("api/QuikiPay/PayoutRequest")]
		public HttpResponseMessage PayoutRequest(PayoutInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var response = "OK";
			var userIds = new List<int>();
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
						var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.withdrawal_id)) ??
							throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
						var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
						var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId,
																						   client.CurrencyId, paymentRequest.Type);
						var signature = GetObjectPropertyValuesAsString(input, typeof(PayoutInput));
						var hash = CommonFunctions.ComputeSha256(signature + partnerPaymentSetting.Password);
						if(input.signature != hash)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
						paymentRequest.ExternalTransactionId = input.id.ToString();
						paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
						using (var clientBl = new ClientBll(paymentSystemBl))
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						using (var documentBll = new DocumentBll(paymentSystemBl))
						{
							if (input.status == "accepted")
							{
								var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
																				null, null, false, paymentRequest.Parameters, documentBll, notificationBl, out userIds);
									clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
							}
							else if (input.status == "rejected")
								clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed, input.status,
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
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				response = ex.Detail == null ? ex.Message : ex.Detail.Id + " " + ex.Detail.NickName;
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
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

		public static string GetObjectPropertyValuesAsString(object obj, Type type)
		{
			var concatenatedString = new StringBuilder();
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				var value = property.GetValue(obj);
				if (value != null && !property.Name.ToLower().Contains("sign"))
				{
					concatenatedString.Append(value);
				}
			}

			return concatenatedString.ToString();
		}
	}
}