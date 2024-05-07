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
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.RiseUp;
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

namespace IqSoft.CP.ProductGateway.Controllers
{
	public class RiseUpController : ApiController
	{
		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.RiseUp).Id;

		[HttpPost]
		[Route("{partnerId}/api/RiseUp/GetBalance")]
		[Route("{partnerId}/api/RiseUp/SetPlay")]
		public HttpResponseMessage ApiRequest(TransactionInput input, int partnerId)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			try
			{
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.RiseUpOperatorId);
				if (input.OperatorId != operatorId)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
				var client = CacheManager.GetClientById(Convert.ToInt32(input.Clientid)) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				var clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, checkExpiration: (input.Type != RiseUpHelpers.Types.WIN));
				if (clientSession.Id.ToString() != input.Clientid)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
				var product = CacheManager.GetProductByExternalId(ProviderId, $"{input.GameId},{input.Provider.Replace(" ", "/")}");
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

				var documentId = string.Empty;
				switch (input.Type)
				{
					case RiseUpHelpers.Types.BET:
						documentId = DoBet(input, clientSession, client, product.Id, partnerProductSetting.Id);
						break;
					case RiseUpHelpers.Types.BETWIN:
						DoBet(input, clientSession, client, product.Id, partnerProductSetting.Id);
						documentId = DoWin(input, clientSession, client, partnerProductSetting.Id);
						break;
					case RiseUpHelpers.Types.WIN:
						documentId = DoWin(input, clientSession, client, partnerProductSetting.Id);
						break;
					case RiseUpHelpers.Types.REFUND:
						documentId = Rollback(input, client, product);
						break;
				}
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																BaseHelpers.GetClientProductBalance(client.Id, product?.Id ?? 0);
				baseOutput = new BaseOutput()
				{
					status = true,
					balance = Math.Round(balance, 2),
					referenceTID = documentId
				};
				WebApiApplication.DbLogger.Info("Response: " + JsonConvert.SerializeObject(baseOutput));
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput = new BaseOutput { error = ex.Detail.Message };
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput = new BaseOutput { error = ex.Message };
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		private static string DoBet(TransactionInput input, SessionIdentity session, BllClient client, int productId, int partnerProductSettingId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
																			partnerProductSettingId, (int)OperationTypes.Bet);
					if (document == null)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = input.RoundId,
							GameProviderId = ProviderId,
							ProductId = productId,
							TransactionId = input.TransactionId,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = input.BetAmount
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

		private static string DoWin(TransactionInput input, SessionIdentity session, BllClient client, int partnerProductSettingId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var product = CacheManager.GetProductByExternalId(ProviderId, $"{input.GameId},{input.Provider.Replace(" ", "/")}") ??
					   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
					var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId.ToString(), ProviderId, client.Id) ??
									throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					if (betDocument == null)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
					var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
																	partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						var state = (input.WinAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = input.RoundId,
							OperationTypeId = (int)OperationTypes.Win,
							ProductId = product.Id,
							TransactionId = input.TransactionId,
							CreditTransactionId = betDocument.Id,
							State = state,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = input.WinAmount
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
								WebApiApplication.DbLogger.Error(ex.Message);
								documentBl.RollbackProductTransactions(operationsFromProduct);
								throw;
							}
						}
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
						BaseHelpers.BroadcastWin(new ApiWin
						{
							GameName = product.NickName,
							ClientId = client.Id,
							ClientName = client.FirstName,
							Amount = input.WinAmount,
							CurrencyId = client.CurrencyId,
							PartnerId = client.PartnerId,
							ProductId = product.Id,
							ProductName = product.NickName,
							ImageUrl = product.WebImageUrl
						});
					}
					return winDocument.Id.ToString();
				}
			}
		}


		private string Rollback(TransactionInput input, BllClient client, BllProduct product)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					GameProviderId = ProviderId,
					TransactionId = input.TransactionId,
					RoundId = input.RoundId,
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