using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.PaymentGateway.Helpers;
using IqSoft.CP.PaymentGateway.Models.Paylado;
using Newtonsoft.Json;
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
	public class PayladoController : ApiController
	{
		private static readonly BllPaymentSystem PaymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.Paylado);

		[HttpPost]
		[Route("api/Paylado/ApiRequest")]
		public HttpResponseMessage ApiRequest([FromBody] PaymentInput input)
		{
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));

				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var paymentRequest = paymentSystemBl.GetPaymentRequestByExternalId(input.Token, PaymentSystem.Id);
							if (paymentRequest == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
							var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentRequest.PaymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
							if (partnerPaymentSetting == null)
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
							if (input.ResultStatus == "OK")
							{
								var info = clientBl.GetClientPaymentAccountDetails(client.Id, paymentRequest.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
											.FirstOrDefault();
								if (info == null && !string.IsNullOrWhiteSpace(input.PaymentOptionAlias))
								{
									info = PaymentHelpers.RegisterClientPaymentAccountDetails(new DAL.ClientPaymentInfo
									{
										AccountNickName = CacheManager.GetPaymentSystemById(paymentRequest.PaymentSystemId).Name,
										Type = (int)ClientPaymentInfoTypes.Wallet,
										ClientId = paymentRequest.ClientId.Value,
										PartnerPaymentSystemId = partnerPaymentSetting.Id,
										WalletNumber = input.PaymentOptionAlias,
										CreationTime = paymentRequest.CreationTime,
										LastUpdateTime = paymentRequest.LastUpdateTime,
										State = (int)ClientPaymentInfoStates.Verified
									});
								}
								var accountId = clientBl.GetfnAccounts(new FilterfnAccount
								{
									ObjectId = client.Id,
									ObjectTypeId = (int)ObjectTypes.Client
								}).FirstOrDefault(x => x.PaymentSystemId == paymentRequest.PaymentSystemId && x.BetShopId == null)?.Id;
								clientBl.ApproveDepositFromPaymentSystem(paymentRequest, false, info: info, accountId: accountId);
								PaymentHelpers.RemoveClientBalanceFromCache(paymentRequest.ClientId.Value);
								BaseHelpers.BroadcastBalance(paymentRequest.ClientId.Value);
							}
							else
							{
								clientBl.ChangeDepositRequestState(paymentRequest.Id, PaymentRequestStates.Failed, null, notificationBl);
							}
							response = "OK";
						}
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.ClientDocumentAlreadyExists ||
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
			return httpResponseMessage;
		}

		[HttpPost]
		[Route("api/Paylado/PremiumPartner")]
		public HttpResponseMessage PremiumPartner(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(inputString));
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK
			};
			try
			{
				//WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
				var input = JsonConvert.DeserializeObject<PremiumPartnerInput>(inputString);
				using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(paymentSystemBl))
					{
						using (var notificationBl = new NotificationBll(paymentSystemBl))
						{
							var client = CacheManager.GetClientById(Convert.ToInt32(input.external_customer_id));
							var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, PaymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
							if (partnerPaymentSetting == null)
								throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
							var info = new LimitInfo();
							if (input.status_text.ToLower() == "waitingforconfirmation")
							{
								var paymentRequest = new PaymentRequest
								{
									Amount = input.amount / 100,
									ClientId = client.Id,
									CurrencyId = client.CurrencyId,
									PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
									PartnerPaymentSettingId = partnerPaymentSetting.Id,
									ExternalTransactionId = input.id,
									Info = inputString
								};
								clientBl.CreateDepositFromPaymentSystem(paymentRequest, out info);
								PaymentHelpers.InvokeMessage("PaymentRequst", client.Id);
							}
							else if (input.status_text.ToLower() == "executed")
							{
								var request = paymentSystemBl.GetPaymentRequestByExternalId(input.id, PaymentSystem.Id);
								var paymentInfo = clientBl.GetClientPaymentAccountDetails(client.Id, request.PaymentSystemId, new List<int> { (int)ClientPaymentInfoTypes.Wallet }, false)
											.FirstOrDefault();
								var accountId = clientBl.GetfnAccounts(new FilterfnAccount
								{
									ObjectId = client.Id,
									ObjectTypeId = (int)ObjectTypes.Client
								}).FirstOrDefault(x => x.PaymentSystemId == request.PaymentSystemId)?.Id;
								clientBl.ApproveDepositFromPaymentSystem(request, false, input.status_text, paymentInfo, accountId: accountId);
								PaymentHelpers.RemoveClientBalanceFromCache(client.Id);
								BaseHelpers.BroadcastBalance(client.Id);
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