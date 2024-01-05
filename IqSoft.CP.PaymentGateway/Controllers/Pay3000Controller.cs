using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Pay3000;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
	public class Pay3000Controller : ApiController
	{
		private static readonly BllPaymentSystem PaymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Pay3000);

		[HttpPost]
		[Route("api/Pay3000/ApiRequest")]
		public HttpResponseMessage ApiRequest(JObject obj)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(obj));
			ConsentInput consentInput = new ConsentInput();
			PaymentInput paymentInput = new PaymentInput();
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var inputString = JsonConvert.SerializeObject(obj);
							if (inputString.Contains("consentId"))
							{
								consentInput = JsonConvert.DeserializeObject<ConsentInput>(inputString);
								var info = clientBl.GetClientPaymentInfoesByWalletNumber(consentInput.ConsentId);
								if (info == null)
									throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
								if (consentInput.Status == "ACCEPTED")
								{
									info.State = (int)ClientPaymentInfoStates.Verified;
								}
								else if (consentInput.Status == "REVOKED" || consentInput.Status == "REJECTED")
								{
									clientBl.DeleteClientPaymentInfo(info.ClientId, info.Id);
								}
							}
							else
							{
								paymentInput = JsonConvert.DeserializeObject<PaymentInput>(inputString);
								var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(paymentInput.PaymentRequestId, PaymentSystem.Id);
								if (paymentRequest == null && paymentInput.Status != "PENDING")
									throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
								if (paymentInput.Status == "PENDING" && paymentInput.Type == "BET_TOPUP")
								{
									var paymentInfo = clientBl.GetClientPaymentAccountDetails(paymentInput.CustomerAliasId, PaymentSystem.Id, new List<int> { (int)ClientPaymentInfoTypes.Wallet })
											.FirstOrDefault();
									if (paymentInfo == null)
										throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongPaymentRequest);
									var client = CacheManager.GetClientById(paymentInfo.ClientId);
									var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
																PaymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
									paymentRequest = new PaymentRequest
									{
										Amount = paymentInput.Amount,
										ClientId = client.Id,
										CurrencyId = client.CurrencyId,
										PaymentSystemId = PaymentSystem.Id,
										PartnerPaymentSettingId = partnerPaymentSetting.Id,
										ExternalTransactionId = paymentInput.PaymentRequestId,
										Info = inputString
									};
									clientBl.CreateDepositFromPaymentSystem(paymentRequest, out LimitInfo info);
									PaymentHelpers.InvokeMessage("PaymentRequst", client.Id);
								}
								else if((paymentInput.Status == "REJECTED" && paymentInput.Type == "BET_TOPUP") || (paymentInput.Status == "CANCELED" && paymentInput.Type == "ECOM_DEBIT_CUSTOMER") )
								{
									clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, paymentInput.Status, notificationBl);
								}
								else if (paymentInput.Status == "CANCELED" && paymentInput.Type == "ECOM_CREDIT_CUSTOMER") 
								{
									using (var documentBll = new DocumentBll(paymentSystemBl))
									{
										clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Failed,
										                                          string.Empty, null, null, false, string.Empty, documentBll, notificationBl);
									}
								}
								else
								{
									var client = CacheManager.GetClientById(paymentRequest.ClientId);
									var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId,
																paymentRequest.PaymentSystemId, client.CurrencyId, paymentRequest.Type);

									if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
									{
										if (paymentInput.Status == "EXECUTED")
											clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false);
									}
									else if (paymentRequest.Type == (int)PaymentRequestTypes.Withdraw)
									{
										using (var documentBll = new DocumentBll(paymentSystemBl))
										{
											if (paymentInput.Status == "EXECUTED")
											{
												var resp = clientBl.ChangeWithdrawRequestState(paymentRequest.Id, PaymentRequestStates.Approved, string.Empty,
												 null, null, false, string.Empty, documentBll, notificationBl);
												clientBl.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBl);
											}
										}
									}
									PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId);
									BaseHelpers.BroadcastBalance(paymentRequest.ClientId);
								}

							}
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