using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.CModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[ApiController]
	public class CModuleController : ControllerBase
	{
		private static readonly List<string> WhitelistedIps = new List<string>
		{
			"188.166.48.202",
			"142.93.140.148",
			"188.166.18.154"
		};
		private static readonly List<string> LowRateCurrencies = new List<string>
		{
			Constants.Currencies.IranianRial,
			Constants.Currencies.IranianTuman,
			Constants.Currencies.ChileanPeso,
			Constants.Currencies.KoreanWon
		};

		private static readonly List<string> NotSupportedCurrencies = new List<string>
		{
			Constants.Currencies.USDT
		};

		public static string GetSortedParamWithValuesAsString(object paymentRequest, string delimiter = "")
		{
			var sortedParams = new SortedDictionary<string, string>();
			var properties = paymentRequest.GetType().GetProperties();
			foreach (var field in properties)
			{
				var value = field.GetValue(paymentRequest, null);
				if (value != null && !field.Name.ToLower().Contains("meta") && !field.Name.ToLower().Contains("partner") &&
					!field.Name.ToLower().Contains("sign"))
					sortedParams.Add(field.Name, value.ToString());
			}
			var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + delimiter);

			return result.Remove(result.LastIndexOf(delimiter), delimiter.Length);
		}

		private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.CModule).Id;

		[HttpPost]
		[Route("{partnerId}/api/CModule/check.session")]
		public ActionResult CheckSession(int partnerId, BaseInput input)
		{
			var jsonResponse = string.Empty;
			try
			{
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModuleSecretKey);
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModulePartnerId);
				if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var signature = string.Format("{0}&{1}&{2}&{3}", GetSortedParamWithValuesAsString(input, "&"),
					CModuleHelpers.Actions.CheckSession, operatorId, secretKey);
				signature = CommonFunctions.ComputeMd5(signature);
				if (signature.ToLower() != input.sign.ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

				var clientSession = ClientBll.GetClientProductSession(input.session, Constants.DefaultLanguageId);
				var client = CacheManager.GetClientById(clientSession.Id);
				decimal balance;
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
				if (isExternalPlatformClient)
					balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
				else
					balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{
							GameId = clientSession.ProductId,
							GroupId = "default",//?????????
							PlayerId = client.Id,
							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
							Balance = (long)(balance * (LowRateCurrencies.Contains(client.CurrencyId) ? 1 : 100))
						}
					},
				new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore
				});
			}
			catch (FaultException<BllFnErrorType> fex)
			{
				var response = new ErrorOutput();
				if (fex.Detail != null)
				{
					response.Code = fex.Detail.Id;
					response.Message = fex.Detail.Message;
				}
				else
				{
					response.Code = Constants.Errors.GeneralException;
					response.Message = fex.Message;
				}
				jsonResponse = JsonConvert.SerializeObject(response);
				Program.DbLogger.Error("Output=" + jsonResponse);
			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
			}
			return Ok(jsonResponse);
		}

		[HttpPost]
		[Route("{partnerId}/api/CModule/check.balance")]
		public ActionResult GetBalance(int partnerId, BaseInput input)
		{
			var jsonResponse = string.Empty;
			try
			{
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModuleSecretKey);
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModulePartnerId);
				if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var signature = string.Format("{0}&{1}&{2}&{3}", GetSortedParamWithValuesAsString(input, "&"),
					CModuleHelpers.Actions.GetBalance, operatorId, secretKey);
				signature = CommonFunctions.ComputeMd5(signature);
				if (signature.ToLower() != input.sign.ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var clientSession = ClientBll.GetClientProductSession(input.session, Constants.DefaultLanguageId);
				var client = CacheManager.GetClientById(clientSession.Id);
				decimal balance;
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
				if (isExternalPlatformClient)
					balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
				else
					balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{
							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
							Balance = (long)(balance * (LowRateCurrencies.Contains(client.CurrencyId) ? 1 : 100))
						}
					},
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore
					});
			}
			catch (FaultException<BllFnErrorType> fex)
			{
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message;
				Program.DbLogger.Error(message);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = message });
			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
			}
			return Ok(jsonResponse);
		}

		[HttpPost]
		[Route("{partnerId}/api/CModule/withdraw.bet")]
		public ActionResult DoBet(int partnerId, BaseInput input)
		{
			var jsonResponse = string.Empty;
			try
			{
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModuleSecretKey);
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModulePartnerId);
				if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var signature = string.Format("{0}&{1}&{2}&{3}", GetSortedParamWithValuesAsString(input, "&"),
					CModuleHelpers.Actions.Bet, operatorId, secretKey);
				signature = CommonFunctions.ComputeMd5(signature);
				if (signature.ToLower() != input.sign.ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

				var clientSession = ClientBll.GetClientProductSession(input.session, Constants.DefaultLanguageId);
				var client = CacheManager.GetClientById(clientSession.Id);

				using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
				{
					using (var clientBl = new ClientBll(documentBl))
					{
						var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
						clientSession.ProductId);
						if (partnerProductSetting == null || partnerProductSetting.Id == 0)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
						var document = documentBl.GetDocumentByExternalId(input.trx_id, client.Id, ProviderId,
																		  partnerProductSetting.Id, (int)OperationTypes.Bet);
						var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
						if (document == null)
						{
							var amount = input.amount.Value;
							if (NotSupportedCurrencies.Contains(client.CurrencyId))
								amount = (int)BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = clientSession.SessionId,
								CurrencyId = client.CurrencyId,
								RoundId = input.meta.tag.round_id.ToString(),
								GameProviderId = ProviderId,
								ProductId = clientSession.ProductId,
								TransactionId = input.trx_id,
								OperationItems = new List<OperationItemFromProduct>()
							};
							operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
							{
								Client = client,
								Amount = amount / (LowRateCurrencies.Contains(client.CurrencyId) ? 1M : 100M),
								DeviceTypeId = clientSession.DeviceType
							});
							document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
							if (isExternalPlatformClient)
							{
								try
								{
									ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 
																		    clientSession.ParentId ?? 0, operationsFromProduct, document);
								}
								catch (Exception ex)
								{
									Program.DbLogger.Error(ex.Message);
									documentBl.RollbackProductTransactions(operationsFromProduct);
									throw;
								}
							}
							BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
							BaseHelpers.BroadcastBalance(client.Id);
						}
						decimal balance;
						if (isExternalPlatformClient)
							balance = ExternalPlatformHelpers.GetClientBalance((int)ExternalPlatformTypes.IQSoft, client.Id);
						else
							balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
						if (NotSupportedCurrencies.Contains(client.CurrencyId))
							balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
						jsonResponse = JsonConvert.SerializeObject(
							new
							{
								status = (int)HttpStatusCode.OK,
								response = new BaseOutput
								{
									Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
									Balance = (long)(balance * (LowRateCurrencies.Contains(client.CurrencyId) ? 1 : 100))
								}
							},
							new JsonSerializerSettings()
							{
								NullValueHandling = NullValueHandling.Ignore
							});
					}
				}
			}
			catch (FaultException<BllFnErrorType> fex)
			{
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message;
				Program.DbLogger.Error(message);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = message });
			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
			}
			return Ok(jsonResponse);
		}

		[HttpPost]
		[Route("{partnerId}/api/CModule/deposit.win")]
		public ActionResult DoWin(int partnerId, BaseInput input)
		{
			var jsonResponse = string.Empty;
			try
			{
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModuleSecretKey);
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModulePartnerId);
				if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var signature = string.Format("{0}&{1}&{2}&{3}", GetSortedParamWithValuesAsString(input, "&"),
					CModuleHelpers.Actions.Win, operatorId, secretKey);
				signature = CommonFunctions.ComputeMd5(signature);
				if (signature.ToLower() != input.sign.ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
				var clientSession = ClientBll.GetClientProductSession(input.session, Constants.DefaultLanguageId, checkExpiration: false);
				var client = CacheManager.GetClientById(clientSession.Id);
				var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
				using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
				{
					using (var clientBl = new ClientBll(documentBl))
					{
						var product = CacheManager.GetProductById(clientSession.ProductId);
						var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
							clientSession.ProductId);
						if (partnerProductSetting == null || partnerProductSetting.Id == 0)
							throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
						var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.meta.tag.round_id.ToString(),
								ProviderId, client.Id);
						if (betDocument == null) // freebet 
						{
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = clientSession.SessionId,
								CurrencyId = client.CurrencyId,
								GameProviderId = ProviderId,
								ProductId = product.Id,
								RoundId = input.meta.tag.round_id.ToString(),
								TransactionId = string.Format("FREEROUNDS_{0}", input.trx_id),
								OperationItems = new List<OperationItemFromProduct>()
							};
							operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
							{
								Client = client,
								Amount = 0,
								DeviceTypeId = clientSession.DeviceType
							});
							betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
						}
						var winDocument = documentBl.GetDocumentByExternalId(input.trx_id, client.Id, ProviderId,
							partnerProductSetting.Id, (int)OperationTypes.Win);
						if (winDocument == null)
						{
							var amount = input.amount.Value;
							if (NotSupportedCurrencies.Contains(client.CurrencyId))
								amount = (int)BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
							var state = input.amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
							betDocument.State = state;
							var operationsFromProduct = new ListOfOperationsFromApi
							{
								SessionId = clientSession.SessionId,
								CurrencyId = client.CurrencyId,
								RoundId = input.trx_id,
								GameProviderId = ProviderId,
								OperationTypeId = (int)OperationTypes.Win,
								TransactionId = input.trx_id,
								//ExternalProductId = input.Tag.GameId,
								ProductId = betDocument.ProductId,
								CreditTransactionId = betDocument.Id,
								State = state,
								OperationItems = new List<OperationItemFromProduct>()
							};
							operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
							{
								Client = client,
								Amount = Convert.ToDecimal(amount / (LowRateCurrencies.Contains(client.CurrencyId) ? 1M : 100M))
							});
							var winDocuments = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
							if (isExternalPlatformClient)
							{
								try
								{
									ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, betDocument.Id, operationsFromProduct, winDocuments[0]);
								}
								catch (Exception ex)
								{
									Program.DbLogger.Error(ex.Message);
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
								Amount = Convert.ToDecimal(input.amount / (LowRateCurrencies.Contains(client.CurrencyId) ? 1M : 100M)),
								CurrencyId = client.CurrencyId,
								PartnerId = client.PartnerId,
								ProductId = product.Id,
								ProductName = product.NickName,
								ImageUrl = product.WebImageUrl
							});
						}
					}
				}
				decimal balance;
				if (isExternalPlatformClient)
					balance = ExternalPlatformHelpers.GetClientBalance((int)ExternalPlatformTypes.IQSoft, client.Id);
				else
					balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{

							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
							Balance = (long)(balance * (LowRateCurrencies.Contains(client.CurrencyId) ? 1 : 100))
						}
					},
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore
					});
			}
			catch (FaultException<BllFnErrorType> fex)
			{
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message;
				Program.DbLogger.Error(message);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = message });
			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
			}
			return Ok(jsonResponse);
		}

		[HttpPost]
		[Route("{partnerId}/api/CModule/trx.cancel")]
		[Route("{partnerId}/api/CModule/trx.complete")]
		public ActionResult CancelTransaction(int partnerId, CancelInput input)
		{
			var jsonResponse = string.Empty;
			try
			{
				var ip = string.Empty;
				if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
					ip = header.ToString();
				BaseBll.CheckIp(WhitelistedIps, ip);
				var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModuleSecretKey);
				var operatorId = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.CModulePartnerId);
				if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(operatorId))
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
				var signature = string.Format("{0}&{1}&{2}&{3}", GetSortedParamWithValuesAsString(input.Body, "&"),
					input.Command, operatorId, secretKey);
				signature = CommonFunctions.ComputeMd5(signature);
				if (signature.ToLower() != input.Body.sign.ToLower())
					throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

				var clientSession = ClientBll.GetClientProductSession(input.Body.session, Constants.DefaultLanguageId, null, false);
				var client = CacheManager.GetClientById(clientSession.Id);

				using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
				{
					var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
					  clientSession.ProductId);
					if (partnerProductSetting == null || partnerProductSetting.Id == 0)
						throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
					var transactionType = input.Command == CModuleHelpers.Actions.Bet ? (int)OperationTypes.Bet : (int)OperationTypes.Win;
					var document = documentBl.GetDocumentByExternalId(input.Body.trx_id, client.Id, ProviderId,
						partnerProductSetting.Id, transactionType);
					if (document == null)
						throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
					var operationsFromProduct = new ListOfOperationsFromApi
					{
						SessionId = clientSession.SessionId,
						GameProviderId = ProviderId,
						TransactionId = input.Body.trx_id,
						ProductId = clientSession.ProductId
					};
					if (document.State != (int)BetDocumentStates.Deleted)
					{
						var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
						var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
						if (isExternalPlatformClient)
						{
							try
							{
								ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, operationsFromProduct, doc[0]);
							}
							catch (Exception ex)
							{
								Program.DbLogger.Error(ex.Message);
							}
						}
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
						BaseHelpers.BroadcastBalance(client.Id);
					}
				}
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{
							Command = CModuleHelpers.Actions.CancelResponse
						}
					},
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore
					});
			}
			catch (FaultException<BllFnErrorType> fex)
			{
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message;
				Program.DbLogger.Error(message);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = message });
			}
			catch (Exception ex)
			{
				Program.DbLogger.Error(ex);
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Message = ex.Message });
			}
			return Ok(jsonResponse);
		}
	}
}
