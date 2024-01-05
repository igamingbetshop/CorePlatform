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
			var response = string.Empty;
			var baseOutput = new BaseOutput();
			var data = new Data();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var input = JsonConvert.DeserializeObject<BaseInput>(inputString);
			try
			{
				var client = CacheManager.GetClientById(Convert.ToInt32(input.UserID)) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				var apiKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BGGamesApiKey);
				var signatureKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BGGamesSignature);
				var withoutSign = inputString.Substring(0, inputString.IndexOf(",\"signature\""));
				var signature = CommonFunctions.ComputeMd5(withoutSign + '}' + signatureKey);
				if (signature != input.Signature)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var clientSession = new SessionIdentity();
				if (input.Extra != null && input.Extra is Extra)
				{
					var customer = JsonConvert.DeserializeObject<Extra>(JsonConvert.SerializeObject(input?.Extra)).Customer;
					clientSession = ClientBll.GetClientProductSession(customer, Constants.DefaultLanguageId);
					if (clientSession.Id != client.Id)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
				}
				var product = new BllProduct();
				var partnerProductSetting = new BllPartnerProductSetting();
				if (input.Action == BGGamesHelpers.Methods.PlaceBet || input.Action == BGGamesHelpers.Methods.AddWins ||
					input.Action == BGGamesHelpers.Methods.BetWin || input.Action == BGGamesHelpers.Methods.Rollback)
				{
					product = CacheManager.GetProductByExternalId(ProviderId, input.GameID);
					partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				}
				var balance = string.Empty;
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				if (isExternalPlatformClient)
					balance = Math.Round(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id), 2).ToString();
				else
					balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product?.Id ?? 0), 2).ToString();
				switch (input.Action)
				{
					case BGGamesHelpers.Methods.GetUser:
						data = new Data
						{
							Id = client.Id.ToString(),
							Username = client.UserName,
							Balance = balance
						};
						signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
						data.Signature = signature;
						break;
					case BGGamesHelpers.Methods.GetBalance:
						data = new Data
						{
							UserID = client.Id.ToString(),
							Username = client.UserName,
							Balance = balance,
						};
						signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
						data.Signature = signature;
						break;
					case BGGamesHelpers.Methods.PlaceBet:
						data = DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id, balance, signatureKey);
						break;
					case BGGamesHelpers.Methods.AddWins:
						data = DoWin(input, client, clientSession, partnerProductSetting.Id, product, balance, signatureKey);
						break;
					case BGGamesHelpers.Methods.BetWin:
					    DoBet(input, client, clientSession, partnerProductSetting.Id, product.Id, balance, signatureKey);
						data = DoWin(input, client, clientSession, partnerProductSetting.Id, product, balance, signatureKey);
						break;
					case BGGamesHelpers.Methods.Rollback:
						data = Rollback(input.RefTransID, client, product, (int)OperationTypes.Bet, data, balance, signatureKey);
						break;
				}
				baseOutput = new BaseOutput { Data = data };

				response = JsonConvert.SerializeObject(baseOutput);
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Detail.Id + " _ " + ex.Detail.Message);
				response = JsonConvert.SerializeObject(new BaseOutput { Error = ex.Detail.Id, Desc = ex.Detail.Message });
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
				response = JsonConvert.SerializeObject(new BaseOutput { Error = 100, Desc = ex.Message });
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}


		private Data DoBet(BaseInput transaction, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, int productId, string balance, string signatureKey)
		{
			var data = new Data
			{
				UserID = client.Id.ToString(),
				Username = client.UserName,
				OldBalance = balance,
				Balance = balance
			};
			data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
			try
			{
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(documentBl))
					{
						var document = documentBl.GetDocumentByExternalId($"{transaction.TransID}_{transaction.GameID}", Convert.ToInt32(transaction.UserID), ProviderId,
																				partnerProductSettingId, (int)OperationTypes.Bet);
						if (document == null)
						{
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = clientSession.SessionId,
								CurrencyId = client.CurrencyId,
								RoundId = transaction.RoundID,
								ExternalProductId = $"{transaction.TransID}_{transaction.GameID}",
								GameProviderId = ProviderId,
								ProductId = productId,
								TransactionId = transaction.TransID,
								OperationItems = new List<OperationItemFromProduct>()
							};
							operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
							{
								Client = client,
								Amount = Convert.ToDecimal(transaction.BetAmount)
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
				var updatedBalance = string.Empty;
				if (isExternalPlatformClient)
					updatedBalance = Math.Round(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id), 2).ToString();
				else
					updatedBalance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, productId), 2).ToString();
				data.Balance = updatedBalance;
				data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id == Constants.Errors.TransactionAlreadyExists)
					return data;
			}
			return data;
		}


		private Data DoWin(BaseInput transaction, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product, string balance, string signatureKey)
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
						betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, transaction.RoundID, ProviderId, client.Id) ??
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					}					
					var winDocument = documentBl.GetDocumentByExternalId($"{transaction.TransID}_{transaction.GameID}", Convert.ToInt32(transaction.UserID), ProviderId,
																		 partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						var state = Convert.ToDecimal(transaction.WinAmount) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = clientSession.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = transaction.RoundID,
							ProductId = betDocument.ProductId,
							TransactionId = $"{transaction.TransID}_{transaction.GameID}",
							CreditTransactionId = betDocument.Id,
							State = state,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = Convert.ToDecimal(transaction.WinAmount)
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
			var updatedBalance = string.Empty;
			if (isExternalPlatformClient)
				updatedBalance = Math.Round(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id), 2).ToString();
			else
				updatedBalance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2).ToString();
			var data = new Data
			{
				UserID = client.Id.ToString(),
				Username = client.UserName,
				OldBalance = balance,
				Balance = updatedBalance
			};
			data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
			return data;
		}

		private Data Rollback(string transactionId, BllClient client, BllProduct product, int operationType, Data data, string balance, string signatureKey)
		{
			data = new Data
			{
				UserID = client.Id.ToString(),
				Username = client.UserName,
				OldBalance = balance,
				Balance = balance
			};
			data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);
			try
			{
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var operationsFromProduct = new ListOfOperationsFromApi
					{
						GameProviderId = ProviderId,
						TransactionId = transactionId,
						ExternalProductId = product.ExternalId,
						ProductId = product.Id,
						OperationTypeId = operationType
					};
					var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
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
				var updatedBalance = string.Empty;
				if (isExternalPlatformClient)
					updatedBalance = Math.Round(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id), 2).ToString();
				else
					updatedBalance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2).ToString();
				data.Balance = updatedBalance;
				data.Signature = CommonFunctions.ComputeMd5(JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) + signatureKey);							
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
					return data;
			}
			return data;
		}
	}
}