using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.ProductGateway.Models.TimelessTech;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Integration.Platforms.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class TimelessTechController : ApiController
	{
		[HttpPost]
		[Route("{partnerId}/api/tlt/{providerName}/authenticate")]
		[Route("{partnerId}/api/tlt/{providerName}/balance")]
		[Route("{partnerId}/api/tlt/{providerName}/changebalance")]
		[Route("{partnerId}/api/tlt/{providerName}/status")]
		[Route("{partnerId}/api/tlt/{providerName}/cancel")]
		[Route("{partnerId}/api/tlt/{providerName}/finishround")]
		public HttpResponseMessage ApiRequest([FromUri]int partnerId, [FromUri]string providerName, BaseInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));			
            var response = string.Empty;
			var secretKey = string.Empty;
			var request = new Request
			{
				Command = input.Command,
				Hash = input.Hash,
				RequestTimestamp = input.RequestTimestamp
			};
			object responseData = null;
			var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
			try
			{
                if (providerName != Constants.GameProviders.TimelessTech && providerName != Constants.GameProviders.BCWGames)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
                var providerId = CacheManager.GetGameProviderByName(providerName)?.Id;
                if (!providerId.HasValue)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProviderId);
                 secretKey = CacheManager.GetGameProviderValueByKey(partnerId, providerId.Value, Constants.PartnerKeys.TimelessTechSecretkey);
                var signature = CommonFunctions.ComputeSha1($"{input.Command}{input.RequestTimestamp}{secretKey}");
				if (signature != input.Hash)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				request.RequestData = input.DataInput;
                switch (input.Command)
				{
					case TimelessTechHelpers.Methods.Authenticate:
						var authenticate = JsonConvert.DeserializeObject<DataBaseInput>(JsonConvert.SerializeObject(input.DataInput));
						var clientSession = ClientBll.GetClientProductSession(authenticate.Token, Constants.DefaultLanguageId);
						var client = CacheManager.GetClientById(clientSession.Id);
						responseData = Authenticate(client);
						break;
					case TimelessTechHelpers.Methods.Balance:
						var balance = JsonConvert.DeserializeObject<DataBaseInput>(JsonConvert.SerializeObject(input.DataInput));
						clientSession = ClientBll.GetClientProductSession(balance.Token, Constants.DefaultLanguageId);
						if (clientSession.Id.ToString() != balance.UserId)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
						client = CacheManager.GetClientById(clientSession.Id);
						responseData = Balance(client);
						break;
					case TimelessTechHelpers.Methods.Changebalance:
						var transactiontData = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(input.DataInput));
						clientSession = ClientBll.GetClientProductSession(transactiontData.Token, Constants.DefaultLanguageId, 
																		  checkExpiration: transactiontData.TransactionType == TimelessTechHelpers.TransactionType.BET);
						if (clientSession.Id.ToString() != transactiontData.UserId)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
						client = CacheManager.GetClientById(clientSession.Id);
						responseData = ChangeBalance(transactiontData, clientSession, client, providerId.Value);
						break;
					case TimelessTechHelpers.Methods.FinishRound:
                        var finishRound = JsonConvert.DeserializeObject<FinishRoundInput>(JsonConvert.SerializeObject(input.DataInput));
                        client = CacheManager.GetClientById(Convert.ToInt32(finishRound.UserId)) ??
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
						responseData = FinishRound(finishRound, client, providerId.Value);
						break;

                    case TimelessTechHelpers.Methods.Cancel:
						var cancelData = JsonConvert.DeserializeObject<RoundInput>(JsonConvert.SerializeObject(input.DataInput));
						client = CacheManager.GetClientById(Convert.ToInt32(cancelData.UserId)) ??
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        responseData = Cancel(cancelData, client, providerId.Value);
						break;
					case TimelessTechHelpers.Methods.Status:
						var statusData = JsonConvert.DeserializeObject<StatusInput>(JsonConvert.SerializeObject(input.DataInput));
						client = CacheManager.GetClientById(Convert.ToInt32(statusData.UserId));
						if (client == null)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        responseData = Status(statusData, client.Id, providerId.Value);
						break;
					default:
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MessageNotFound);

                }
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "OK",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"OK{timestamp}{secretKey}"),
						ResponseData = responseData
					}
				});
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Detail.Id + " _ " + ex.Detail.Message);
				//if (ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
				//{

				//}
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "ERROR",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"ERROR{timestamp}{secretKey}"),
						ResponseData = new ErrorOutput
						{
							ErrorCode = TimelessTechHelpers.GetErrorCode(ex.Detail.Id),
							ErrorMessage = TimelessTechHelpers.GetErrorMessage(ex.Detail.Id)
						}
					}
				});
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex);
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "ERROR",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"ERROR{timestamp}{secretKey}"),
						ResponseData = new ErrorOutput
						{
							ErrorCode = TimelessTechHelpers.GetErrorCode(Constants.Errors.GeneralException),
							ErrorMessage = ex.Message
						}
					}
				});
			}
			var httpResponseMessage = new HttpResponseMessage
			{

				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(response, Encoding.UTF8)
			};
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		private static object Authenticate(BllClient client)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
			var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
													 CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;
			var regionPath = CacheManager.GetRegionPathById(client.RegionId);
			var isoCode = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.IsoCode;
			return new
			{
				user_id = client.Id.ToString(),
				user_name = client.UserName,
				user_country = isoCode ?? "AU",
				currency_code = client.CurrencyId,
				balance
			};
		}

		private static object Balance(BllClient client)
		{
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
			var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
													 CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;
			return new
			{
				balance,
				currency_code = client.CurrencyId
			};
		}

		private static object ChangeBalance(TransactionInput input, SessionIdentity session, BllClient client, int providerId)
		{
			var product = CacheManager.GetProductByExternalId (providerId, input.GameId.ToString()) ?? // for lobby
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
			var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id)  ??
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
			if (input.TransactionType == TimelessTechHelpers.TransactionType.BET)
				DoBet(input, session, client, partnerProductSetting.Id, product.Id, providerId);
			else if (input.TransactionType == TimelessTechHelpers.TransactionType.WIN)
				DoWin(input, session, client, partnerProductSetting.Id, product, providerId);
			else if (input.TransactionType == TimelessTechHelpers.TransactionType.REFUND)
			{
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var document = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId.ToString(), providerId, client.Id) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    Rolleback(document.ExternalTransactionId, input.GameId.ToString(), client, providerId);
				}
			}
			var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
			var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
													 CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance;
			return new
			{
				balance,
				currency_code = client.CurrencyId
			};
		}

		private static object Status(StatusInput input, int clientId, int providerId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var document = documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), providerId, clientId, (int)OperationTypes.Bet) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), providerId, clientId, (int)OperationTypes.BetRollback) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), providerId, clientId, (int)OperationTypes.Win) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), providerId, clientId, (int)OperationTypes.WinRollback) ??
							   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
				return new
				{
					user_id = clientId.ToString(),
					transaction_id = input.TransactionId,
					transaction_status = document.State == (int)BetDocumentStates.Deleted ? TimelessTechHelpers.TransactionStatus.CANCELED : TimelessTechHelpers.TransactionStatus.OK,
				};
			}
		}

		private static object Cancel(RoundInput input, BllClient client, int providerId)
		{
			var response = new
			{
				user_id = client.Id.ToString(),
				transaction_id = input.TransactionId,
				transaction_status = TimelessTechHelpers.TransactionStatus.CANCELED,
			};
			try
			{
				Rolleback(input.TransactionId.ToString(), input.GameId.ToString(), client, providerId);
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Detail.Id + " _ " + ex.Detail.Message);
				if (ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
					return response;
			}
			return response;
		}

		private static void Rolleback(string transactionId, string gameId, BllClient client, int providerId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var product = CacheManager.GetProductByExternalId(providerId, gameId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
				if (partnerProductSetting == null)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					GameProviderId = providerId,
					TransactionId = transactionId,
					ExternalProductId = product.ExternalId,
					ProductId = product.Id
				};
				var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
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


		private static object FinishRound(FinishRoundInput input, BllClient client, int providerId)
		{
			var product = CacheManager.GetProductByExternalId(providerId, input.GameId.ToString()) ??
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
			var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
			var transactioninput = new TransactionInput()
			{
				GameId = input.GameId,
				RoundId = input.RoundId,
				TransactionId = input.RoundId,
				Amount = 0
			};
			DoWin(transactioninput, new SessionIdentity(), client, partnerProductSetting.Id, product, providerId);
			return new
			{
				user_id = client.Id.ToString(),
				round_id = input.RoundId,
				game_id = input.GameId
			};
		}

		private static void DoBet(TransactionInput input, SessionIdentity session, BllClient client, int partnerProductSettingId, int productId, int providerId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var transactionId = input.TransactionId.ToString();
					if (input.Reason.ToLower().Contains("freespin"))
						transactionId = $"Bet_{Constants.FreeSpinPrefix}{input.TransactionId}";
                    var document = documentBl.GetDocumentByExternalId(transactionId, client.Id, providerId,
																			partnerProductSettingId, (int)OperationTypes.Bet);
					if (document == null)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = input.RoundId.ToString(),
							GameProviderId = providerId,
							ProductId = productId,
							TransactionId = transactionId,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = Convert.ToDecimal(input.Amount)
						});
						document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
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
							BaseHelpers.BroadcastBetLimit(info);
						}
					}
				}
			}
		}

		private static void DoWin(TransactionInput input, SessionIdentity session, BllClient client, int partnerProductSettingId, BllProduct product, int providerId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId.ToString(), providerId, client.Id) ??
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    var transactionId = input.TransactionId.ToString();
                    if (input.Reason.ToLower().Contains("freespin"))
                        transactionId = $"{Constants.FreeSpinPrefix}{input.TransactionId}";

                    var winDocument = documentBl.GetDocumentByExternalId(input.RoundId.ToString(), client.Id, providerId,
																		 partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						var state = Convert.ToDecimal(input.Amount) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = providerId,
							RoundId = input.RoundId.ToString(),
							ProductId = betDocument.ProductId,
							TransactionId = transactionId,
							CreditTransactionId = betDocument.Id,
							State = state,
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = Convert.ToDecimal(input.Amount)
						});
						winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
						var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
								(betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument, WebApiApplication.DbLogger);
							}
							catch (FaultException<BllFnErrorType> ex)
							{
								WebApiApplication.DbLogger.Error("DebitException_" + ex.Detail?.Id + " _ " + ex.Detail?.Message);
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
								Amount = Convert.ToDecimal(input.Amount),
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
		}
	}
}