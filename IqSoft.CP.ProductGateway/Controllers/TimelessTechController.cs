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

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class TimelessTechController : ApiController
	{
		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.TimelessTech).Id;

		[HttpPost]
		[Route("{partnerId}/api/TLT/authenticate")]
		[Route("{partnerId}/api/TLT/balance")]
		[Route("{partnerId}/api/TLT/changebalance")]
		[Route("{partnerId}/api/TLT/status")]
		[Route("{partnerId}/api/TLT/cancel")]
		[Route("{partnerId}/api/TLT/finishround")]
		public HttpResponseMessage ApiRequest(BaseInput input, int partnerId)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var response = string.Empty;
			var sectetKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.TimelessTechSecretkey);
			var clientSession = new SessionIdentity();
			var client = new BllClient();
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
				var signature = CommonFunctions.ComputeSha1($"{input.Command}{input.RequestTimestamp}{sectetKey}");
				if (signature != input.Hash)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				switch (input.Command)
				{
					case TimelessTechHelpers.Methods.Authenticate:
						var authenticate = JsonConvert.DeserializeObject<AuthenticateInput>(JsonConvert.SerializeObject(input.DataInput));
						clientSession = ClientBll.GetClientProductSession(authenticate.Token, Constants.DefaultLanguageId, null, false);
						client = CacheManager.GetClientById(clientSession.Id);
						request.RequestData = authenticate;
						responseData = Authenticate(client);
						break;
					case TimelessTechHelpers.Methods.Balance:
						var balance = JsonConvert.DeserializeObject<BalanceInput>(JsonConvert.SerializeObject(input.DataInput));
						clientSession = ClientBll.GetClientProductSession(balance.Token, Constants.DefaultLanguageId, null, false);
						if (clientSession.Id.ToString() != balance.UserId)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
						client = CacheManager.GetClientById(clientSession.Id);
						request.RequestData = balance;
						responseData = Balance(client);
						break;
					case TimelessTechHelpers.Methods.Changebalance:
						var transactiontData = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(input.DataInput));
						clientSession = ClientBll.GetClientProductSession(transactiontData.Token, Constants.DefaultLanguageId, null, false);
						if (clientSession.Id.ToString() != transactiontData.UserId)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
						client = CacheManager.GetClientById(clientSession.Id);
						request.RequestData = transactiontData;
						responseData = Changebalance(transactiontData, clientSession, client);
						break;
					case TimelessTechHelpers.Methods.Cancel:
						var cancelData = JsonConvert.DeserializeObject<CancelInput>(JsonConvert.SerializeObject(input.DataInput));
						client = CacheManager.GetClientById(Convert.ToInt32(cancelData.UserId));
						request.RequestData = cancelData;
						responseData = Cancel(cancelData, client);
						break;
					case TimelessTechHelpers.Methods.Status:
						var statusData = JsonConvert.DeserializeObject<StatusInput>(JsonConvert.SerializeObject(input.DataInput));
						client = CacheManager.GetClientById(Convert.ToInt32(statusData.UserId));
						request.RequestData = statusData;
						responseData = Status(statusData, client);
						break;
				}
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "OK",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"OK{timestamp}{sectetKey}"),
						ResponseData = responseData
					}
				});
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Detail.Id + " _ " + ex.Detail.Message);
				if (ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
				{

				}
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "ERROR",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"ERROR{timestamp}{sectetKey}"),
						ResponseData = new ErrorOutput
						{
							ErrorCode = ex.Detail.Id.ToString(),
							ErrorMessage = ex.Detail.Message
						}
					}
				});
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Message);
				response = JsonConvert.SerializeObject(new BaseOutput()
				{
					Request = request,
					Response = new Response()
					{
						Status = "ERROR",
						ResponseTimestamp = timestamp,
						Hash = CommonFunctions.ComputeSha1($"ERROR{timestamp}{sectetKey}"),
						ResponseData = new ErrorOutput
						{
							ErrorCode = ex.Message,
							ErrorMessage = ex.Message
						}
					}
				});
				httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
			}
			httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			return httpResponseMessage;
		}

		private static object Authenticate(BllClient client)
		{
			var regionPath = CacheManager.GetRegionPathById(client.RegionId);
			var isoCode = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.IsoCode;
			return new
			{
				user_id = client.Id.ToString(),
				user_name = client.UserName,
				user_country = isoCode,
				currency_code = client.CurrencyId,
				balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance
			};
		}

		private static object Balance(BllClient client)
		{
			return new
			{
				balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance,
				currency_code = client.CurrencyId
			};
		}

		private static object Changebalance(TransactionInput input, SessionIdentity session, BllClient client)
		{
			var product = CacheManager.GetProductById(session.ProductId);
			if (product.ExternalId != input.GameId.ToString())
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
			var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
			if (partnerProductSetting == null)
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
			if (input.TransactionType == TimelessTechHelpers.TransactionType.BET)
				DoBet(input, session, client, partnerProductSetting.Id, product.Id);
			else if (input.TransactionType == TimelessTechHelpers.TransactionType.WIN)
				DoWin(input, session, client, partnerProductSetting.Id, product);
			else if (input.TransactionType == TimelessTechHelpers.TransactionType.REFUND)
			{
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var document = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId.ToString(), ProviderId, client.Id);
					Rolleback(document.ExternalTransactionId, input.GameId.ToString(), client);
				}
			}
			return new
			{
				balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance,
				currency_code = client.CurrencyId
			};
		}

		private static object Status(StatusInput input, BllClient client)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var document = documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), ProviderId, client.Id, (int)OperationTypes.Bet) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), ProviderId, client.Id, (int)OperationTypes.BetRollback) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), ProviderId, client.Id, (int)OperationTypes.Win) ??
							   documentBl.GetDocumentOnlyByExternalId(input.TransactionId.ToString(), ProviderId, client.Id, (int)OperationTypes.WinRollback) ??
							   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
				return new
				{
					user_id = client.Id.ToString(),
					transaction_id = input.TransactionId,
					transaction_status = document.State == (int)BetDocumentStates.Deleted ? TimelessTechHelpers.TransactionStatus.CANCELED : TimelessTechHelpers.TransactionStatus.OK,
				};
			}
		}

		private static object Cancel(CancelInput input, BllClient client)
		{
			var response = new
			{
				user_id = client.Id.ToString(),
				transaction_id = input.TransactionId,
				transaction_status = TimelessTechHelpers.TransactionStatus.CANCELED,
			};
			try
			{
				Rolleback(input.TransactionId.ToString(), input.GameId.ToString(), client);
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + ex.Detail.Id + " _ " + ex.Detail.Message);
				if (ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
					return response;
			}
			return response;
		}

		private static void Rolleback(string transactionId, string gameId, BllClient client)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				var product = CacheManager.GetProductByExternalId(ProviderId, gameId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
				if (partnerProductSetting == null)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					GameProviderId = ProviderId,
					TransactionId = transactionId,
					ExternalProductId = product.ExternalId,
					ProductId = product.Id
				};
				documentBl.RollbackProductTransactions(operationsFromProduct);
				BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
			}
		}

		private static void DoBet(TransactionInput input, SessionIdentity session, BllClient client, int partnerProductSettingId, int productId)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{

					var document = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), client.Id, ProviderId,
																			partnerProductSettingId, (int)OperationTypes.Bet);
					if (document == null)
					{
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.Id,
							CurrencyId = client.CurrencyId,
							RoundId = input.RoundId.ToString(),
							ExternalProductId = input.TransactionId.ToString(),
							GameProviderId = ProviderId,
							ProductId = productId,
							TransactionId = input.TransactionId.ToString(),
							OperationItems = new List<OperationItemFromProduct>()
						};
						operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
						{
							Client = client,
							Amount = Convert.ToDecimal(input.Amount)
						});
						document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
						BaseHelpers.BroadcastBalance(client.Id);
						BaseHelpers.BroadcastBetLimit(info);
					}
				}
			}
		}

		private static void DoWin(TransactionInput input, SessionIdentity session, BllClient client, int partnerProductSettingId, BllProduct product)
		{
			using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
			{
				using (var clientBl = new ClientBll(documentBl))
				{
					var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId.ToString(), ProviderId, client.Id) ??
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
					var winDocument = documentBl.GetDocumentByExternalId(input.RoundId.ToString(), client.Id, ProviderId,
																		 partnerProductSettingId, (int)OperationTypes.Win);
					if (winDocument == null)
					{
						var state = Convert.ToDecimal(input.Amount) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
						betDocument.State = state;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							GameProviderId = ProviderId,
							RoundId = input.RoundId.ToString(),
							ProductId = betDocument.ProductId,
							TransactionId = input.TransactionId.ToString(),
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
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
						BaseHelpers.BroadcastWin(new ApiWin
						{
							GameName = product.NickName,
							ClientId = client.Id,
							ClientName = client.FirstName,
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

		private static object Finishround(BaseInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));

			return null;
		}
	}
}