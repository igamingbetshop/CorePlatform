using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.PaymentGateway.Helpers;
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
using IqSoft.CP.PaymentGateway.Models.IXOPay;
using System.Linq;
using IqSoft.CP.DAL;

namespace IqSoft.CP.PaymentGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "*")]
	public class IXOPayController : ApiController
	{
		[HttpPost]
		[Route("api/ixopay/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = string.Empty;
			var userIds = new List<int>();
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var paymentRequest = paymentSystemBl.GetPaymentRequestById(Convert.ToInt64(input.MerchantTransactionId));
							if (paymentRequest == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
							var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
														paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);
							var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId);
							paymentRequest.ExternalTransactionId = input.PurchaseId;
							paymentSystemBl.ChangePaymentRequestDetails(paymentRequest);
							if (input.TransactionType == "DEBIT")
							{
								if (input.Result.ToUpper() == "OK")
								{
									var info = paymentSystem.Name == Constants.PaymentSystems.IXOPayCC
								     ? clientBl.GetClientPaymentAccountDetails(client.Id, paymentRequest.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false, input.ReferenceUuid ?? input.Uuid).FirstOrDefault()
									 : clientBl.GetClientPaymentAccountDetails(client.Id, paymentRequest.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
										.FirstOrDefault();
									if (!string.IsNullOrWhiteSpace(input.ReferenceUuid) && info == null)
										throw BaseBll.CreateException(string.Empty, Constants.Errors.AccountNotFound);
									string cardNumber = string.Empty;
									if (!string.IsNullOrWhiteSpace(input?.ReturnData?.FirstSixDigits) && !string.IsNullOrWhiteSpace(input?.ReturnData?.LastFourDigits))
									{
										cardNumber = $"{input.ReturnData.FirstSixDigits}****{input.ReturnData.LastFourDigits}";
									}
									if (string.IsNullOrWhiteSpace(input.ReferenceUuid) && info == null &&
											  (paymentSystem.Name == Constants.PaymentSystems.IXOPayPayPal || paymentSystem.Name == Constants.PaymentSystems.IXOPayCC ||
											   paymentSystem.Name == Constants.PaymentSystems.IXOPayTrustly))
									{
										info = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
										{
											AccountNickName = !string.IsNullOrEmpty(cardNumber) ? cardNumber : paymentSystem.Name,
											Type = (int)ClientPaymentInfoTypes.Wallet,
											ClientId = paymentRequest.ClientId.Value,
											PartnerPaymentSystemId = partnerPaymentSetting.Id,
											WalletNumber = input.Uuid,
											CreationTime = paymentRequest.CreationTime,
											LastUpdateTime = paymentRequest.LastUpdateTime,
											State = (int)ClientPaymentInfoStates.Verified
										});
									}
									clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, out userIds, info: info ?? new ClientPaymentInfo());
                                }
								else if (input.Result.ToUpper() == "ERROR")
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
										string.Format("ErrorCode: {0}, ErrorMessage: {1}", input.Code, input.Message), notificationBl);
							}
							else if (input.TransactionType == "PAYOUT")
							{
								using (var documentBll = new DocumentBll(paymentSystemBl))
								{
									if (input.Result.ToUpper() == "OK")
									{
										var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
										 null, null, false, string.Empty, documentBll, notificationBl, out userIds);
										clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
									}
									else if (input.Result.ToUpper() == "ERROR")
									{
										var reason = string.Format("ErrorCode: {0}, ErrorMessage: {1} AdapterCode: {2}, AdapterMessage: {3}", input.AdapterCode, input.AdapterMessage);
										clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
											reason, null, null, false, string.Empty, documentBll, notificationBl, out userIds);
									}
								}
							}
							else if (input.TransactionType == "DEREGISTER")
							{
								var clientPaymentInfo = clientBl.GetClientPaymentAccountDetails(client.Id, paymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
												 .FirstOrDefault();
								if (clientPaymentInfo == null)
									throw BaseBll.CreateException(string.Empty, Constants.Errors.AccountNotFound);
								clientBl.DeleteClientPaymentInfo(clientPaymentInfo.ClientId, clientPaymentInfo.Id);
							}
                            foreach (var uId in userIds)
                            {
                                PaymentHelpers.InvokeMessage("NotificationsCount", uId);
                            }

                            PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
							BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
							response = "OK";
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