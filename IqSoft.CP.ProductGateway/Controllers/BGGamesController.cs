using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.BGGames;
using IqSoft.CP.ProductGateway.Models.MicroGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class BGGamesController : ApiController
	{
		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BGGames).Id;

		[HttpPost]
		[Route("{partnerId}/api/BGGames/ApiRequest")]
		public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage)
		{
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
			try
			{
				var client = CacheManager.GetClientById(Convert.ToInt32(input.UserID)) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BGGamesApiKey);
				var signatureKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BGGamesSignature);
				var withoutSign = inputString.Substring(0, inputString.IndexOf(",\"signature\""));
				var signature = CommonFunctions.ComputeMd5(Regex.Unescape(withoutSign) + '}' + signatureKey);
				if (signature != input.Signature)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				SessionIdentity clientSession = null;
				if (input.Action != BGGamesHelpers.Methods.RollbackBet &&
					input.Action != BGGamesHelpers.Methods.JackpotWin &&
					input.Action != BGGamesHelpers.Methods.Results)
				{
					var customer = JsonConvert.DeserializeObject<Extra>(JsonConvert.SerializeObject(input?.Extra)).Customer;
					clientSession = ClientBll.GetClientProductSession(customer, Constants.DefaultLanguageId, checkExpiration:
																	  input.Action != BGGamesHelpers.Methods.AddWins);
					if (clientSession.Id != client.Id)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
				}
				var product = new BllProduct();
				var partnerProductSetting = new BllPartnerProductSetting();
				if (input.Action == BGGamesHelpers.Methods.PlaceBet || input.Action == BGGamesHelpers.Methods.AddWins ||
					input.Action == BGGamesHelpers.Methods.BetWin || input.Action == BGGamesHelpers.Methods.RollbackBet)
				{
					product = CacheManager.GetProductByExternalId(ProviderId, input.GameID);
					partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				}
				else if (input.Action == BGGamesHelpers.Methods.Rollback || input.Action == BGGamesHelpers.Methods.Results ||
					input.Action == BGGamesHelpers.Methods.Betslip)
				{
					product = CacheManager.GetProductByExternalId(ProviderId, "pregame");
					partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				}

				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
														 BaseHelpers.GetClientProductBalance(client.Id, product?.Id ?? 0);

                baseOutput.Data = new Data
				{
					UserID = client.Id.ToString(),
					Username = client.UserName,
					OldBalance = Math.Round(balance,2).ToString(),
					Balance = Math.Round(balance, 2).ToString()
                };
				baseOutput.Data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(baseOutput.Data,
											new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);

				switch (input.Action)
				{
					case BGGamesHelpers.Methods.GetUser:
						baseOutput.Data.Id = client.Id.ToString();
						baseOutput.Data.UserID = null;
						baseOutput.Data.Currency = client.CurrencyId;
						break;
					case BGGamesHelpers.Methods.GetBalance:
						baseOutput.Data.Currency = client.CurrencyId;
						break;
					case BGGamesHelpers.Methods.PlaceBet:
						DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id);
						break;
					case BGGamesHelpers.Methods.AddWins:
						DoWin(input, client, clientSession, partnerProductSetting.Id, product);
						break;
					case BGGamesHelpers.Methods.BetWin:
						DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id);
						DoWin(input, client, clientSession, partnerProductSetting.Id, product);
						break;
					case BGGamesHelpers.Methods.RollbackBet:
						Rollback(input, client, product, (int)OperationTypes.Bet);
						break;
					case BGGamesHelpers.Methods.Betslip:
						DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id, true);
						baseOutput.Data.Currency = client.CurrencyId;
						break;
					case BGGamesHelpers.Methods.Results:
						DoWin(input, client, clientSession, partnerProductSetting.Id, product, true);
						baseOutput.Data.Currency = client.CurrencyId;
						break;
					case BGGamesHelpers.Methods.Rollback:
						Rollback(input, client, product, (int)OperationTypes.Bet);
						baseOutput.Data.Currency = client.CurrencyId;
						break;
				}
				var updatedBalance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																BaseHelpers.GetClientProductBalance(client.Id, product.Id);
				baseOutput.Data.Balance = Math.Round(updatedBalance, 2).ToString();
				baseOutput.Data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(baseOutput.Data,
											new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
			}
			catch (FaultException<BllFnErrorType> ex)
			{				
				if (ex.Detail.Id != Constants.Errors.TransactionAlreadyExists && ex.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
				{
					baseOutput = new BaseOutput { Error = ex.Detail.Id, Desc = ex.Detail.Message };
					httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
				}
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            catch (Exception ex)
			{
				baseOutput = new BaseOutput { Error = 100, Desc = ex.Message };
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
            }
            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		private void DoBet(BaseInput transaction, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, int? productId, bool isSport = false)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var transactionExternalId = isSport ? transaction.BetslipID : transaction.TransID;
					var amount = isSport ? transaction.Amount : transaction.BetAmount;
					var document = documentBl.GetDocumentByExternalId(transactionExternalId, Convert.ToInt32(transaction.UserID), ProviderId,
																			partnerProductSettingId, (int)OperationTypes.Bet);
					
					if (document == null)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = clientSession.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = transaction.RoundID,
							GameProviderId = ProviderId,
							ProductId = productId,
							TransactionId = transactionExternalId,
							Info = (transaction.Data == null ? null : JsonConvert.SerializeObject(transaction.Data)),
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = Convert.ToDecimal(amount)
						});
						document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
						BaseHelpers.BroadcastBetLimit(info);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
																		 clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
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
				}
			}
		}

		private void DoWin(BaseInput transaction, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product, bool isSport = false)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					Document betDocument;
					if (transaction.Action == BGGamesHelpers.Methods.JackpotWin)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = transaction.RoundID,
							ProductId = product.Id,
							TransactionId = $"{transaction.CId}_JackpotWin",
							OperationTypeId = (int)OperationTypes.Bet,
							State = (int)BetDocumentStates.Uncalculated,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = 0
						});
						betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
									client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
							}
							catch (Exception ex)
							{
								WebApiApplication.DbLogger.Error(ex.Message);
								documentBl.RollbackProductTransactions(operationsFromProduct);
								throw;
							}
						}
					}
					else
					{
						betDocument = isSport ? documentBl.GetDocumentByExternalId(transaction.BetslipID, Convert.ToInt32(transaction.UserID), ProviderId,
																			partnerProductSettingId, (int)OperationTypes.Bet)
									          : documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, transaction.RoundID, ProviderId, client.Id) ??
							                               throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					}
					var transactionId = isSport ? transaction.BetID : $"{transaction.TransID}_{transaction.GameID}";
					var winDocument = documentBl.GetDocumentByExternalId(transactionId, Convert.ToInt32(transaction.UserID), ProviderId,
																		 partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						int state;
						if (isSport)
						{
							state = transaction.Status == BGGamesHelpers.Statuses.Win ? (int)BetDocumentStates.Won :
																					  transaction.Status == BGGamesHelpers.Statuses.Cashout ?
																					  (int)BetDocumentStates.Cashouted : (int)BetDocumentStates.Lost;
						}
						else
							state = Convert.ToDecimal(transaction.WinAmount) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = clientSession?.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = transaction.RoundID,
							ProductId = betDocument.ProductId,
							TransactionId = transactionId,
							CreditTransactionId = betDocument.Id,
							State = state,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = isSport ? Convert.ToDecimal(transaction.Amount) : Convert.ToDecimal(transaction.WinAmount)
						});
						winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
								(betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument, WebApiApplication.DbLogger);
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
							Amount = Convert.ToDecimal(transaction.WinAmount),
							CurrencyId = client.CurrencyId,
							PartnerId = client.PartnerId,
							ProductId = product.Id,
							ProductName = product.NickName,
							ImageUrl = product.WebImageUrl
						});
					}
				}
			}
		}

		private void Rollback(BaseInput transaction, BllClient client, BllProduct product, int operationType, bool isSport = false)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					GameProviderId = ProviderId,
					TransactionId = transaction.RefTransID,
					ExternalProductId = product.ExternalId,
					ProductId = product.Id,
					OperationTypeId = operationType
				};
				var documents = isSport ? documentBl.RollbackProductTransactions(operationsFromProduct, externalTransactionId: transaction.BetslipID ) : 
					documentBl.RollbackProductTransactions(operationsFromProduct);
				if (isExternalPlatformClient)
				{
					try
					{
						ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
							operationsFromProduct, documents[0], WebApiApplication.DbLogger);
					}
					catch (Exception ex)
					{
						WebApiApplication.DbLogger.Error(ex.Message);
					}
				}
				else
					BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
			}
		}
	}
}