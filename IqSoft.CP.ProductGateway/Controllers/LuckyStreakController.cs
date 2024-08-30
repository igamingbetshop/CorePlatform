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
using IqSoft.CP.ProductGateway.Models.LuckyStreak;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class LuckyStreakController : ApiController
	{
		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyStreak).Id;
		public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.LuckyStreak);

		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/validate")]
		public HttpResponseMessage Validate(ValidateInput input, int partnerId)
		{
			var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
			if (string.IsNullOrEmpty(ip))
				ip = HttpContext.Current.Request.UserHostAddress;
			WebApiApplication.DbLogger.Info("Ip: " + ip);
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var inputString = JsonConvert.SerializeObject(input);
			WebApiApplication.DbLogger.Info("input:  " + inputString);
			var auth = HttpContext.Current.Request.Headers.GetValues("Authorization")[0]; 
			WebApiApplication.DbLogger.Info("headersAuthorization:  " + JsonConvert.SerializeObject(auth));
			var client = new BllClient();
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
				var hash = GetAuthValue(auth, "hash=");
				var hashValue = GetSha256("hawk.1.payload", inputString);
				if (hash != hashValue)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var clientSession = ClientBll.GetClientProductSession(input.data.AuthorizationCode, Constants.DefaultLanguageId, checkExpiration: true);
				client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
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
						code = LuckyStreakHelpers.GetErrorCode(ex.Detail.Id),
						title = LuckyStreakHelpers.GetErrorMessage(ex.Detail.Id)
					};
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					code = LuckyStreakHelpers.GetErrorCode(Constants.Errors.GeneralException),
					title = LuckyStreakHelpers.GetErrorMessage(Constants.Errors.GeneralException)
				};
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {JsonConvert.SerializeObject(input)} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			var productGatewayUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.ProductGateway).StringValue;
			var hmacKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyStreakHmacKey);
			var header = GetHeader(auth, LuckyStreakHelpers.Method.Validate, productGatewayUrl.Replace("https://", string.Empty), hmacKey);
			httpResponseMessage.Content.Headers.Add("Server-Authorization", header);
			WebApiApplication.DbLogger.Info("response:  " + JsonConvert.SerializeObject(httpResponseMessage));
			return httpResponseMessage;
		}

		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/getBalance")]
		public HttpResponseMessage GetBalance(HttpRequestMessage httpRequestMessage, int partnerId)
		{
			var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
			if (string.IsNullOrEmpty(ip))
				ip = HttpContext.Current.Request.UserHostAddress;
			WebApiApplication.DbLogger.Info("Ip: " + ip);
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var auth = HttpContext.Current.Request.Headers.GetValues("Authorization")[0];
			WebApiApplication.DbLogger.Info("headersAuthorization:  " + JsonConvert.SerializeObject(auth));
			var client = new BllClient();
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
				var hash = GetAuthValue(auth, "hash=");
				var hashValue = GetSha256("hawk.1.payload", inputString);
				if (hash != hashValue)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var input = JsonConvert.DeserializeObject<BalanceInput>(inputString);
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId); 

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
						code = LuckyStreakHelpers.GetErrorCode(ex.Detail.Id),
						title = LuckyStreakHelpers.GetErrorMessage(ex.Detail.Id)
					};
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					code = LuckyStreakHelpers.GetErrorCode(Constants.Errors.GeneralException),
					title = LuckyStreakHelpers.GetErrorMessage(Constants.Errors.GeneralException)
				};
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			var productGatewayUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.ProductGateway).StringValue;
			var hmacKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyStreakHmacKey);
			var header = GetHeader(auth, LuckyStreakHelpers.Method.GetBalance, productGatewayUrl.Replace("https://", string.Empty), hmacKey);
			httpResponseMessage.Content.Headers.Add("Server-Authorization", header);
			WebApiApplication.DbLogger.Info("response:  " + JsonConvert.SerializeObject(httpResponseMessage));
			return httpResponseMessage;
		}


		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/moveFunds")]
		public HttpResponseMessage MoveFunds(HttpRequestMessage httpRequestMessage, int partnerId)
		{
			var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
			if (string.IsNullOrEmpty(ip))
				ip = HttpContext.Current.Request.UserHostAddress;
			WebApiApplication.DbLogger.Info("Ip: " + ip);
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var auth = HttpContext.Current.Request.Headers.GetValues("Authorization")[0];
			WebApiApplication.DbLogger.Info("headersAuthorization:  " + JsonConvert.SerializeObject(auth));
			var client = new BllClient();
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
				var hash = GetAuthValue(auth, "hash=");
				var hashValue = GetSha256("hawk.1.payload", inputString);
				if (hash != hashValue)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
				var eventDetails = input.data.gameType == "ProviderGame" ?
								JsonConvert.DeserializeObject<EventDetails>(JsonConvert.SerializeObject(input.data.eventDetails)) : null;
				var roundId = eventDetails == null ? input.data.eventId : eventDetails.roundId; // For lobby games keep eventId which used to get the bet
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var product = CacheManager.GetProductById(clientSession.ProductId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var documentId = string.Empty;
				switch (input.data.eventType)
				{
					case LuckyStreakHelpers.TransactionType.Bet:
					case LuckyStreakHelpers.TransactionType.Tip:
						var transactionId = input.data.transactionRequestId;
						documentId = DoBet(transactionId, input.data.amount, clientSession, client, product.Id, partnerProductSetting.Id, roundId, eventDetails == null);
						if (input.data.eventType == LuckyStreakHelpers.TransactionType.Tip)
							DoWin(input.data.transactionRequestId, 0, clientSession, client, product, partnerProductSetting.Id, roundId, input.data.eventId, eventDetails == null);
						break;
					case LuckyStreakHelpers.TransactionType.Win:
					case LuckyStreakHelpers.TransactionType.Loss:
						documentId = DoWin(input.data.transactionRequestId, input.data.amount, clientSession, client, product, partnerProductSetting.Id, roundId, eventDetails == null ? input.data.eventId : eventDetails.refTransactionId, eventDetails == null);
						break;
					case LuckyStreakHelpers.TransactionType.Refund:
						documentId = Rollback(input, client, product, roundId);
						break;
				}
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
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
						code = LuckyStreakHelpers.GetErrorCode(ex.Detail.Id),
						title = LuckyStreakHelpers.GetErrorMessage(ex.Detail.Id)
					};
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					code = LuckyStreakHelpers.GetErrorCode(Constants.Errors.GeneralException),
					title = LuckyStreakHelpers.GetErrorMessage(Constants.Errors.GeneralException)
				};
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			var productGatewayUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.ProductGateway).StringValue;
			var hmacKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyStreakHmacKey);
			var header = GetHeader(auth, LuckyStreakHelpers.Method.MoveFunds, productGatewayUrl.Replace("https://", string.Empty), hmacKey);
			httpResponseMessage.Content.Headers.Add("Server-Authorization", header);
			WebApiApplication.DbLogger.Info("response:  " + JsonConvert.SerializeObject(httpResponseMessage));
			return httpResponseMessage;
		}


		[HttpPost]
		[Route("{partnerId}/api/LuckyStreak/abortMoveFunds")]
		public HttpResponseMessage AbortMoveFunds(HttpRequestMessage httpRequestMessage, int partnerId)
		{
			var ip = HttpContext.Current.Request.Headers.Get("CF-Connecting-IP");
			if (string.IsNullOrEmpty(ip))
				ip = HttpContext.Current.Request.UserHostAddress;
			WebApiApplication.DbLogger.Info("Ip: " + ip);
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var baseOutput = new BaseOutput();
			var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
			var auth = HttpContext.Current.Request.Headers.GetValues("Authorization")[0];
			WebApiApplication.DbLogger.Info("headersAuthorization:  " + JsonConvert.SerializeObject(auth));
			var client = new BllClient();
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
				var hash = GetAuthValue(auth, "hash=");
				var hashValue = GetSha256("hawk.1.payload", inputString);
				if (hash != hashValue)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var input = JsonConvert.DeserializeObject<TransactionInput>(inputString);
				var clientSession = ClientBll.GetClientProductSession(input.data.additionalParams, Constants.DefaultLanguageId, checkExpiration: true);
				if (clientSession.Id.ToString() != input.data.username)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
				client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
				var product = CacheManager.GetProductById(clientSession.ProductId);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id) ??
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				var eventDetails = input.data.eventType == "ProviderGame" ?
								JsonConvert.DeserializeObject<EventDetails>(JsonConvert.SerializeObject(input.data.eventDetails)) : null;
				var roundId = eventDetails == null ? input.data.roundId : eventDetails.roundId;
				var documentId = Rollback(input, client, product, roundId);
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
				var balance = isExternalPlatformClient ? ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id) :
																BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
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
						code = LuckyStreakHelpers.GetErrorCode(ex.Detail.Id),
						title = LuckyStreakHelpers.GetErrorMessage(ex.Detail.Id)
					};
				}
				WebApiApplication.DbLogger.Error($"Code: {ex.Detail.Id} Message: {ex.Detail.Message} Input: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			catch (Exception ex)
			{
				baseOutput.errors = new Error
				{
					code = LuckyStreakHelpers.GetErrorCode(Constants.Errors.GeneralException),
					title = LuckyStreakHelpers.GetErrorMessage(Constants.Errors.GeneralException)
				};
				WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {inputString} Response: {JsonConvert.SerializeObject(baseOutput)}");
			}
			httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
			var productGatewayUrl = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.ProductGateway).StringValue;
			var hmacKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyStreakHmacKey);
			var header = GetHeader(auth, LuckyStreakHelpers.Method.AbortMoveFunds, productGatewayUrl.Replace("https://", string.Empty), hmacKey);
			httpResponseMessage.Content.Headers.Add("Server-Authorization", header);
			WebApiApplication.DbLogger.Info("response:  " + JsonConvert.SerializeObject(httpResponseMessage));
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

		private static string GetAuthValue(string auth, string val)
		{
			var hashKeyPosition = auth.IndexOf(val);
			var hashValueStartPosition = hashKeyPosition + $"{val}\"".Length;
			var hashValueEndPosition = auth.IndexOf("\"", hashValueStartPosition);
			var hashValue = auth.Substring(hashValueStartPosition, hashValueEndPosition - hashValueStartPosition);
			return hashValue;
		}

		private static string GetSha256(string hawk, string inputString)
		{
			var hashString = $"{hawk}\napplication/json\n{inputString}\n";
			byte[] bytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(hashString));
			return Convert.ToBase64String(bytes);
		}

		private static string GetHeader(string auth, string method, string productGatewayUrl, string hmacKey)
		{
			var nonce = GetAuthValue(auth, "nonce=");
			var ts = GetAuthValue(auth, "ts=");
			var ext = "X-Request-Header-To-Protect:secret";
			var norm = new StringBuilder(256)
				.AppendNewLine("hawk.1.response")
				.AppendNewLine(ts)
				.AppendNewLine(nonce)
				.AppendNewLine("POST")
				.AppendNewLine($"/1/api/LuckyStreak/{method}")
				.AppendNewLine(productGatewayUrl)
				.AppendNewLine("443")
				.AppendNewLine(null)
				.AppendNewLine(ext)
				.ToString();
			WebApiApplication.DbLogger.Info("norm:  " + norm);
			string mac = CalculateMac(norm, hmacKey);
			return String.Format("hawk mac=\"{0}\", ext=\"{1}\"", mac, ext);
		}

		private static string CalculateMac(string norm, string key)
		{
			using (var hashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA256"))
			{
				hashAlgorithm.Key = Encoding.UTF8.GetBytes(key);
				return Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(norm)));
			}
		}
	}
}