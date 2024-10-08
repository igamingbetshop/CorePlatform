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
using IqSoft.CP.ProductGateway;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.ImperiumGames;
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

[EnableCors(origins: "*", headers: "*", methods: "POST")]
public class ImperiumGamesController : ApiController
{
	private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.ImperiumGames).Id;

	[HttpPost]
	[Route("{partnerId}/api/ImperiumGames/ApiRequest")]
	public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
	{
		var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
		var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
		WebApiApplication.DbLogger.Info(inputString);
		var responce = string.Empty;
		var baseOutput = new BaseOutput();
		try
		{
			var input = JsonConvert.DeserializeObject<WriteBetInput>(inputString);
			var clientSession = ClientBll.GetClientProductSession(input.login, Constants.DefaultLanguageId, checkExpiration: true);
			var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
			var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
			var documentId = string.Empty;
			if (input.cmd == "writeBet")
			{
				DoBet(input, clientSession, client, clientSession.ProductId, partnerProductSetting.Id);
				documentId = DoWin(input, clientSession, client, partnerProductSetting.Id);
			}
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out IqSoft.CP.DAL.Models.Cache.PartnerKey externalPlatformType);
			var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
															BaseHelpers.GetClientProductBalance(client.Id,  0);
			responce = JsonConvert.SerializeObject(new TransactionOutput
			{
				login = input.login,
				currency = client.CurrencyId,
				balance = balance.ToString(),
				operationId = documentId
			});
		}
		catch (FaultException<BllFnErrorType> ex)
		{
			if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
			{
				responce = JsonConvert.SerializeObject(new BaseOutput
				{
					status = "fail",
					error = ex.Detail.Message
				});
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
		}
		catch (Exception ex)
		{
			responce = JsonConvert.SerializeObject(new BaseOutput
			{
				status = "fail",
				error = ex.Message
			});
			httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
		}
		httpResponseMessage.Content = new StringContent(responce, Encoding.UTF8);
		httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
		return httpResponseMessage;
	}
	private static string DoBet(WriteBetInput input, SessionIdentity session, BllClient client, int productId, int partnerProductSettingId)
	{
		using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
		{
			using (var clientBl = new ClientBll(documentBl))
			{
				var document = documentBl.GetDocumentByExternalId(input.tradeId, client.Id, ProviderId,
																		partnerProductSettingId, (int)OperationTypes.Bet);
				if (document == null)
				{
					var operationsFromProduct = new ListOfOperationsFromApi
					{
						SessionId = session.SessionId,
						CurrencyId = client.CurrencyId,
						GameProviderId = ProviderId,
						ProductId = productId,
						TransactionId = input.tradeId,
						OperationItems = new List<OperationItemFromProduct>()
					};
					operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
					{
						Client = client,
						Amount = Convert.ToDecimal(input.bet)
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

	private static string DoWin(WriteBetInput input, SessionIdentity session, BllClient client, int partnerProductSettingId)
	{
		using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
		{
			using (var clientBl = new ClientBll(documentBl))
			{
				var product = CacheManager.GetProductById(session.ProductId);
				var betDocument = documentBl.GetDocumentByExternalId(input.tradeId, client.Id, ProviderId, partnerProductSettingId, (int)OperationTypes.Bet) ??
								throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
				if (betDocument == null)
					throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
				var winDocument = documentBl.GetDocumentByExternalId(input.tradeId, client.Id, ProviderId,
																partnerProductSettingId, (int)OperationTypes.Win);
				if (winDocument == null)
				{
					var state = (Convert.ToDecimal(input.win) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
					betDocument.State = state;
					var operationsFromProduct = new ListOfOperationsFromApi
					{
						SessionId = session.SessionId,
						CurrencyId = client.CurrencyId,
						GameProviderId = ProviderId,
						OperationTypeId = (int)OperationTypes.Win,
						ProductId = product.Id,
						TransactionId = input.tradeId,
						CreditTransactionId = betDocument.Id,
						State = state,
						OperationItems = new List<OperationItemFromProduct>()
					};
					operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
					{
						Client = client,
						Amount = Convert.ToDecimal(input.win)
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
                            BetId = betDocument?.Id ?? 0,
                            GameName = product.NickName,
							ClientId = client.Id,
							ClientName = client.FirstName,
							BetAmount = betDocument?.Amount,
							Amount = Convert.ToDecimal(input.win),
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
}