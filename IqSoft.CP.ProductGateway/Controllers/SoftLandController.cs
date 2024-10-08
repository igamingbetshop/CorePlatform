using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SoftLand;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
	public class SoftLandController : ApiController
	{
		private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftLand).Id;

		[HttpGet]
		[Route("{partnerId}/api/softland/balance")]
		public IHttpActionResult GetBalance([FromUri] BaseInput input)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
			var response = new BalanceOutput();
			try
			{
				var token = GetToken();
				var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, true);
				using (var clientBl = new ClientBll(clientSession, WebApiApplication.DbLogger))
				{
					var client = CacheManager.GetClientById(clientSession.Id);
					var currencyId = string.IsNullOrWhiteSpace(input.currency) ? client.CurrencyId : input.currency;
					var balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
					response.PlayerId = client.Id;
					response.Currency = input.currency;
					response.Balance = balance;
					WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
					return Ok(response);
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				response = ex.Detail == null
					? new BalanceOutput
					{
						ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException),
						ErrorMessage = ex.Message
					}
					: new BalanceOutput
					{
						ErrorCode = SoftLandHelpers.GetErrorCode(ex.Detail.Id),
						ErrorMessage = ex.Detail.Message
					};
				return Ok(response);
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(ex);
				response = new BalanceOutput
				{
					ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException),
					ErrorMessage = ex.Message
				};
				return Ok(response);
			}
		}


		[HttpPost]
		[Route("{partnerId}/api/softland/creditOperation")]
		public IHttpActionResult Credit(HttpRequestMessage httpRequestMessage)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(Request.Headers));
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = new[] { new OperationResponse() };
			try
			{
				var input = JsonConvert.DeserializeObject<OperationInput[]>(inputString).FirstOrDefault();
				var token = GetToken();
				var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, true);
				if (session.Id.ToString() != input.PlayerId)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				using (var clientBl = new ClientBll(session, WebApiApplication.DbLogger))
				{
					using (var documentBl = new DocumentBll(clientBl))
					{
						var client = CacheManager.GetClientById(session.Id);

						var operationsFromProduct = new ListOfOperationsFromApi
						{
							SessionId = session.SessionId,
							CurrencyId = client.CurrencyId,
							RoundId = input.RoundId,
							GameProviderId = providerId,
							ExternalProductId = input.GameId.ToString(),
							TransactionId = input.TransactionId.ToString(),
							OperationItems = new List<OperationItemFromProduct>
						    {
							    new OperationItemFromProduct
							    {
							    	Client = client,
							    	Amount = input.Amount
							    }
						    }
						};
						var document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
						BaseHelpers.BroadcastBetLimit(info);
						var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
						response[0].Balance = balance;
						response[0].PlatformTransactionId = document.Id.ToString();
						response[0].TransactionId = input.TransactionId;
						WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
						return Ok(response);
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail == null)
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
					response[0].ErrorMessage = ex.Message;
				}
				else
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(ex.Detail.Id);
					response[0].ErrorMessage = ex.Detail.Message;
				}
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + inputString);
				return Ok(response);
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(inputString, ex);
				response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
				response[0].ErrorMessage = ex.Message;
				return Ok(response);
			}
		}

		[HttpPost]
		[Route("{partnerId}/api/softland/debitOperation")]
		public IHttpActionResult Debit(HttpRequestMessage httpRequestMessage)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(Request.Headers));
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = new[] { new OperationResponse() };
			try
			{
				var input = JsonConvert.DeserializeObject<Models.SoftLand.OperationInput[]>(inputString).FirstOrDefault();
				var token = GetToken();
				var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, false);
				if (session.Id.ToString() != input.PlayerId)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				var client = CacheManager.GetClientById(session.Id);
				var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, session.ProductId);
				if (partnerProductSetting == null)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					using (var clientBl = new ClientBll(documentBl))
					{
						Document betDocument;
						if (input.OperationType == "tournamentWin")
						{
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = session.SessionId,
								CurrencyId = client.CurrencyId,
								RoundId = input.RoundId,
								GameProviderId = providerId,
								ExternalProductId = input.GameId.ToString(),
								TransactionId = input.TransactionId.ToString(),
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
						}
						else
						   betDocument = documentBl.GetDocumentByExternalId(input.RoundOperations.FirstOrDefault().TransactionId.ToString(), session.Id,
							      providerId, partnerProductSetting.Id, (int)OperationTypes.Bet);
						var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId.ToString(), session.Id, providerId,
																					partnerProductSetting.Id, (int)OperationTypes.Win);
						if (winDocument != null)
							throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

						var amount = Convert.ToDecimal(input.Amount);
						var product = CacheManager.GetProductById(session.ProductId);
						if (winDocument == null)
						{
							var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
							betDocument.State = state;
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = session.SessionId,
								CurrencyId = client.CurrencyId,
								GameProviderId = providerId,
								RoundId = input.RoundId,
								ProductId = betDocument.ProductId,
								TransactionId = input.TransactionId.ToString(),
								CreditTransactionId = betDocument.Id,
								State = state,
								OperationItems = new List<OperationItemFromProduct>()
							};
							operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
							{
								Client = client,
								Amount = amount
							});
							winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];

							BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
							BaseHelpers.BroadcastWin(new ApiWin
							{
                                BetId = betDocument?.Id ?? 0,
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
						var balance = BaseHelpers.GetClientProductBalance(client.Id, session.ProductId);
						response[0].Balance = balance;
						response[0].PlatformTransactionId = winDocument.Id.ToString();
						response[0].TransactionId = input.TransactionId;
						return Ok(response);
					}
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail == null)
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
					response[0].ErrorMessage = ex.Message;
				}
				else
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(ex.Detail.Id);
					response[0].ErrorMessage = ex.Detail.Message;
				}
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + inputString);
				return Ok(response);
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(inputString, ex);
				response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
				response[0].ErrorMessage = ex.Message;
				return Ok(response);
			}
		}

		[HttpPost]
		[Route("{partnerId}/api/softland/refundOperation")]
		public IHttpActionResult Refund(HttpRequestMessage httpRequestMessage)
		{
			WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(Request.Headers));
			var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
			WebApiApplication.DbLogger.Info(inputString);
			var response = new[] { new OperationResponse() };
			try
			{
				var input = JsonConvert.DeserializeObject<OperationInput[]>(inputString).FirstOrDefault();
				var token = GetToken();
				var session = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, null, false);
				if (session.Id.ToString() != input.PlayerId)
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
				var operationsFromProduct = new ListOfOperationsFromApi
				{
					SessionId = session.SessionId,
					GameProviderId = providerId,
					ExternalProductId = input.GameId.ToString(),
					TransactionId = input.RoundOperations.FirstOrDefault().TransactionId.ToString(),
					Info = inputString
				};
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
				{
					var document = documentBl.RollbackProductTransactions(operationsFromProduct).FirstOrDefault();
					BaseHelpers.RemoveClientBalanceFromeCache(session.Id);
					var balance = BaseHelpers.GetClientProductBalance(session.Id, session.ProductId);
					response[0].Balance = balance;
					response[0].PlatformTransactionId = document.Id.ToString();
					response[0].TransactionId = input.TransactionId;
					WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
					return Ok(response);
				}
			}
			catch (FaultException<BllFnErrorType> ex)
			{
				if (ex.Detail == null)
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
					response[0].ErrorMessage = ex.Message;
				}
				else
				{
					response[0].ErrorCode = SoftLandHelpers.GetErrorCode(ex.Detail.Id);
					response[0].ErrorMessage = ex.Detail.Message;
				}
				WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + inputString);
				return Ok(response);
			}
			catch (Exception ex)
			{
				WebApiApplication.DbLogger.Error(inputString, ex);
				response[0].ErrorCode = SoftLandHelpers.GetErrorCode(Constants.Errors.GeneralException);
				response[0].ErrorMessage = ex.Message;
				return Ok(response);
			}
		}

		private string GetToken()
		{
			if (!Request.Headers.Contains("SL-Token"))
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
			var token = Request.Headers.GetValues("SL-Token").FirstOrDefault();
			if (string.IsNullOrEmpty(token))
				throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
			return token;
		}
	}
}
