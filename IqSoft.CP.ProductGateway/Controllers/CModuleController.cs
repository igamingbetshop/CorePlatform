using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.CModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
	[EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class CModuleController : ApiController
    {
		public static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.CModule);
		
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
		private static Dictionary<string, string> CurrencyMap = new Dictionary<string, string>
		{
			{ Constants.Currencies.TunisianDinar, "TND2" }
		};
		public static string GetSortedParamWithValuesAsString(object paymentRequest, string delimiter = "")
        {
            var sortedParams = new SortedDictionary<string, string>();
            var properties = paymentRequest.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(paymentRequest, null);
                if(value!=null && !field.Name.ToLower().Contains("meta") && !field.Name.ToLower().Contains("partner") &&
                    !field.Name.ToLower().Contains("sign"))
                sortedParams.Add(field.Name,  value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + par.Value + delimiter);

            return result.Remove(result.LastIndexOf(delimiter), delimiter.Length);
        }
		
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.CModule).Id;
        [HttpPost]
        [Route("{partnerId}/api/CModule/check.session")]
        public HttpResponseMessage CheckSession(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
			var statusCode = HttpStatusCode.OK;
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
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
					balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				var product = CacheManager.GetProductById(clientSession.ProductId);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{
							GameId = product.ExternalId,
                            GroupId = "default",//?????????
							PlayerId = client.Id,
							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : 
								(CurrencyMap.ContainsKey(client.CurrencyId) ? CurrencyMap[client.CurrencyId] : client.CurrencyId),
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
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message + "_" + fex.Detail.Info;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");

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
				statusCode = HttpStatusCode.BadRequest;
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
            }
			catch (Exception ex)
			{
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = Constants.Errors.GeneralException, Message = ex.Message });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
				statusCode = HttpStatusCode.InternalServerError;
			}

			return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/CModule/check.balance")]
        public HttpResponseMessage GetBalance(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
			var statusCode = HttpStatusCode.OK;
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
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
					balance = BaseHelpers.GetClientProductBalance(client.Id, 0);
				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{
							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar :
								(CurrencyMap.ContainsKey(client.CurrencyId) ? CurrencyMap[client.CurrencyId] : client.CurrencyId),
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
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message + "_" + fex.Detail.Info;
				var code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()}");

                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = code, Message = message });
				statusCode = HttpStatusCode.BadRequest;
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
            }
			catch (Exception ex)
			{
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = Constants.Errors.GeneralException, Message = ex.Message });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");                
				statusCode = HttpStatusCode.InternalServerError;
			}

			return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/CModule/withdraw.bet")]
        public HttpResponseMessage DoBet(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
			var statusCode = HttpStatusCode.OK;
			try
            {
                BaseBll.CheckIp(WhitelistedIps);
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

                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId,
                        clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var document = documentBl.GetDocumentByExternalId(input.trx_id, client.Id,
                                ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
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
							document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
							if (isExternalPlatformClient)
							{
								try
								{
									var epBalance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, 
										operationsFromProduct, document, WebApiApplication.DbLogger);
									BaseHelpers.BroadcastBalance(client.Id, epBalance);
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
						decimal balance;
						if (isExternalPlatformClient)
							balance = ExternalPlatformHelpers.GetClientBalance((int)ExternalPlatformTypes.IQSoft, client.Id);
						else
							balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
						if (NotSupportedCurrencies.Contains(client.CurrencyId))
							balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
						jsonResponse = JsonConvert.SerializeObject(
                            new
                            {
                                status = (int)HttpStatusCode.OK,
                                response = new BaseOutput
                                {
                                    Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar :
										(CurrencyMap.ContainsKey(client.CurrencyId) ? CurrencyMap[client.CurrencyId] : client.CurrencyId),
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
                var message = fex.Detail == null ? fex.Message : fex.Detail.Message + "_" + fex.Detail.Info;
				var code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
				WebApiApplication.DbLogger.Error(message + "_" + JsonConvert.SerializeObject(input));
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = code, Message = message });
				statusCode = HttpStatusCode.BadRequest;
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
            }
            catch (Exception ex)
            {
				jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = Constants.Errors.GeneralException, Message = ex.Message });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");                
				statusCode = HttpStatusCode.InternalServerError;
			}
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/CModule/deposit.win")]
        public HttpResponseMessage DoWin(int partnerId, BaseInput input)
        {
            var jsonResponse = string.Empty;
			var statusCode = HttpStatusCode.OK;
			try
			{
				BaseBll.CheckIp(WhitelistedIps);
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
				using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
							betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
							BaseHelpers.BroadcastBetLimit(info);
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
								RoundId = input.meta.tag.round_id.ToString(),
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
									ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client, betDocument.Id,
										operationsFromProduct, winDocuments[0], WebApiApplication.DbLogger);
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
				}
				decimal balance;
				if (isExternalPlatformClient)
					balance = ExternalPlatformHelpers.GetClientBalance((int)ExternalPlatformTypes.IQSoft, client.Id);
				else
					balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
				if (NotSupportedCurrencies.Contains(client.CurrencyId))
					balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
				jsonResponse = JsonConvert.SerializeObject(
					new
					{
						status = (int)HttpStatusCode.OK,
						response = new BaseOutput
						{

							Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar :
								(CurrencyMap.ContainsKey(client.CurrencyId) ? CurrencyMap[client.CurrencyId] : client.CurrencyId),
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
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message + "_" + fex.Detail.Info;
				var code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = code, Message = message });
				statusCode = HttpStatusCode.BadRequest;
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
            }
			catch (Exception ex)
			{
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = Constants.Errors.GeneralException, Message = ex.Message });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
				statusCode = HttpStatusCode.InternalServerError;
			}
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/CModule/trx.cancel")]
        public HttpResponseMessage CancelTransaction(int partnerId, CancelInput input)
        {
            var jsonResponse = string.Empty;
			var statusCode = HttpStatusCode.OK;
			try
            {
                BaseBll.CheckIp(WhitelistedIps);
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
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
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
								ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), 
									client, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
							}
							catch (Exception ex)
							{
								WebApiApplication.DbLogger.Error(ex.Message);
							}
						}
						BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
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
				var message = fex.Detail == null ? fex.Message : fex.Detail.Message + "_" + fex.Detail.Info;
				var code = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message}  Input: {bodyStream.ReadToEnd()}");
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = code, Message = message });
				statusCode = HttpStatusCode.BadRequest;
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                {
                    WebApiApplication.DbLogger.Error("NotAllowd IP: " + HttpContext.Current.Request.Headers.Get("CF-Connecting-IP"));
                }
            }
            catch (Exception ex)
            {
                jsonResponse = JsonConvert.SerializeObject(new ErrorOutput { Code = Constants.Errors.GeneralException, Message = ex.Message });
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
				statusCode = HttpStatusCode.InternalServerError;
			}
            return new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8)
            };
        }

		[HttpPost]
		[Route("{partnerId}/api/CModule/trx.complete")]
		public HttpResponseMessage CompleteTransaction(int partnerId, CancelInput input)
		{
			WebApiApplication.DbLogger.Info("CompleteTransaction_" + JsonConvert.SerializeObject(input));
			return DoWin(partnerId, input.Body);
		}
	}
}
