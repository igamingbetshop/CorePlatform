using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Integration.Products.Models.TimelessTech;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.LuckyStreak;
using Microsoft.AspNet.SignalR.Client.Transports.ServerSentEvents;
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

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class LuckyStreakController : ApiController
	{
		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyStreak).Id;

		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/validate")]
		public HttpResponseMessage Validate(ValidateInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			try
			{
				var clientSession = ClientBll.GetClientProductSession(input.data.AuthorizationCode, Constants.DefaultLanguageId, checkExpiration: true);
				var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																 CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;

				var data = new ValidateOutput
				{
					userName = client.Id.ToString(),
					currency = client.CurrencyId,
					language = client.LanguageId,
					nickname = client.UserName,
					balance = balance,
					balanceTimestamp = DateTime.UtcNow,
					lastUpdateDate = clientSession.LastUpdateTime.Value
				};
				baseOutput.data = data;
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput.errors = new Error
					{
						code = ex.Detail.Id.ToString(),
						title = ex.Detail.Message
					};
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					title = ex.Message
				};
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/getBalance")]
		public HttpResponseMessage GetBalance(BalanceInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var baseOutput = new BaseOutput();
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			try
			{
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;

				var balanceOutput = new BalanceOutput
				{
					currency = client.CurrencyId,
					balance = balance,
					balanceTimestamp = DateTime.UtcNow
				};
				baseOutput.data = balanceOutput;				
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput.errors = new Error
					{
						code = ex.Detail.Id.ToString(),
						title = ex.Detail.Message
					};
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					title = ex.Message
				};
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}



		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/moveFunds")]
		public HttpResponseMessage MoveFunds(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			//WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var baseOutput = new BaseOutput();
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			try
			{
				var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
				var eventDetails = input.data.gameType == "ProviderGame" ?
					            JsonConvert.DeserializeObject<EventDetails>(JsonConvert.SerializeObject(input.data.eventDetails)) : null;
				var roundId = eventDetails == null ? input.data.eventId : eventDetails.roundId; // For lobby games keep eventId which used to get the bet
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var product = CacheManager.GetProductById(clientSession.ProductId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var documentId = string.Empty;
				switch (input.data.eventType)
				{
					case "Bet":
					case "Tip":
						var transactionId = input.data.transactionRequestId;
						documentId = DoBet(transactionId, input.data.amount, clientSession, client, product.Id, partnerProductSetting.Id, roundId, eventDetails == null);
						if(input.data.eventType == "Tip")
							DoWin(input.data.transactionRequestId, 0, clientSession, client, product, partnerProductSetting.Id, roundId, input.data.eventId, eventDetails == null);
						break;
					case "Win":
					case "Loss":
						documentId = DoWin(input.data.transactionRequestId, input.data.amount, clientSession, client, product, partnerProductSetting.Id, roundId, eventDetails == null ? input.data.eventId : eventDetails.refTransactionId, eventDetails == null);
						break;
					case "Refund":
						documentId = Rollback(input, client, product, roundId);
						break;
				}
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;
				var transactionOutput = new TransactionOutput
				{
					refTransactionId = documentId,
					currency = client.CurrencyId,
					balance = balance,
					balanceTimestamp = DateTime.UtcNow
				};
				baseOutput.data = transactionOutput;
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput.errors = new Error
					{
						code = ex.Detail.Id.ToString(),
						title = ex.Detail.Message
					};
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					title = ex.Message
				};
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}


		[HttpPost]

		[Route("{partnerId}/api/LuckyStreak/abortMoveFunds")]
		public HttpResponseMessage AbortMoveFunds(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			//WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var baseOutput = new BaseOutput();
			var response = string.Empty;
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			try
			{
				var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var product = CacheManager.GetProductById(clientSession.ProductId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var documentId = string.Empty;
				var eventDetails = input.data.eventType == "ProviderGame" ?
								JsonConvert.DeserializeObject<EventDetails>(JsonConvert.SerializeObject(input.data.eventDetails)) : null;
				var roundId = eventDetails == null ? input.data.roundId : eventDetails.roundId;
				documentId = Rollback(input, client, product, roundId);
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;
				var transactionOutput = new TransactionOutput
				{
					refTransactionId = documentId,
					currency = client.CurrencyId,
					balance = balance,
					balanceTimestamp = DateTime.UtcNow
				};
				baseOutput.data = transactionOutput;
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput.errors = new Error
					{
						code = ex.Detail.Id.ToString(),
						title = ex.Detail.Message
					};
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					title = ex.Message
				};
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}


		private static string DoBet(string transactionId, decimal amount, SessionIdentity session, BllClient client, int productId, int partnerProductSettingId, string roundId, bool isLobby)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var document = isLobby ? documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, roundId, ProviderId, client.Id)
						                   : documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
																			partnerProductSettingId, (int)OperationTypes.Bet);
					if (document == null)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = roundId,
							GameProviderId = ProviderId,
							ProductId = productId,
							TransactionId = transactionId,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = amount
						});
						document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
						BaseHelpers.BroadcastBetLimit(info);
						var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
																		 session.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
							}
							catch (FaultException<BllFnErrorType> ex)
							{
								WebApiApplication.DbLogger.Error(ex.Detail?.Id + " _ " + ex.Detail?.Message);
								documentBl.RollbackProductTransactions(operationsFromProduct);
								throw;
							}
							catch (Exception ex)
							{
								WebApiApplication.DbLogger.Error(ex.Message);
								documentBl.RollbackProductTransactions(operationsFromProduct);
								throw;
							}
						}
						else
						{
							BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
							BaseHelpers.BroadcastBalance(client.Id);
						}
					}

					return document.Id.ToString();
				}
			}
		}

		private static string DoWin(string transactionId, decimal amount, SessionIdentity session, BllClient client, BllProduct product, int partnerProductSettingId, string roundId, string betExternalId, bool isLobby)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var betDocument = isLobby ? documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, roundId, ProviderId, client.Id)
											  : documentBl.GetDocumentByExternalId(betExternalId, client.Id, ProviderId, partnerProductSettingId, (int)OperationTypes.Bet) ??
									throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					if (betDocument == null)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
					var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId,
																	partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = roundId,
							OperationTypeId = (int)OperationTypes.Win,
							ProductId = product.Id,
							TransactionId = transactionId,
							CreditTransactionId = betDocument.Id,
							State = state,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = amount
						});
						winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl).FirstOrDefault();
						var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
																	  betDocument.Id, operationsFromProduct, winDocument, WebApiApplication.DbLogger);
							}
							catch (Exception ex)
							{
								WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
							}
						}
						else
						{
							BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
							BaseHelpers.BroadcastWin(new ApiWin
							{
								GameName = product.NickName,
								ClientId = client.Id,
								ClientName = client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = amount,
								CurrencyId = client.CurrencyId,
								PartnerId = client.PartnerId,
								ProductId = product.Id,
								ProductName = product.NickName,
								ImageUrl = product.WebImageUrl
							});
						}
					}
					return winDocument.Id.ToString();
				}
			}
		}


		private string Rollback(TransactionInput input, BllClient client, BllProduct product, string roundId)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					GameProviderId = ProviderId,
					TransactionId = input.data.abortedTransactionRequestId,
					RoundId = roundId,
					ExternalProductId = product.ExternalId,
					ProductId = product.Id
				};
				var document = documentBl.RollbackProductTransactions(operationsFromProduct).FirstOrDefault();
				if (isExternalPlatformClient)
				{
					try
					{
						ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
							operationsFromProduct, document, WebApiApplication.DbLogger);
					}
					catch (Exception ex)
					{
						WebApiApplication.DbLogger.Error(ex.Message);
					}
				}
				else
					BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
				return document.Id.ToString();
			}
		}
	}
}