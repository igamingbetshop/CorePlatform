using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.PragmaticPlay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class PragmaticPlayController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.PragmaticPlay).Id;

        private static readonly List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.PragmaticPlay);

        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT
        };

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/authenticate")]
        public HttpResponseMessage CheckSession(BaseInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var hash = input.hash;
                input.hash = null;
                input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.userId.HasValue && input.userId.Value != client.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var balance = 0m;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                    }
                    else
                        balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);

                    var responseObject = new
                    {
                        userId = client.Id.ToString(),
                        currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                        cash = Math.Round(balance, 2),
                        bonus = 0,
                        usedPromo = 0,
                        token = clientSession.Token,
                        error = 0,
                        description = "Success"
                    };
                    BaseHelpers.RemoveSessionFromeCache(input.token, product.Id);
                    jsonResponse = JsonConvert.SerializeObject(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/balance")]
        public HttpResponseMessage GetBalance(BaseInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var hash = input.hash;
                input.hash = null;
                input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                if (input.userId.HasValue && input.userId.Value != client.Id)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                var balance = 0m;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                {
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                }
                else
                    balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                var responseObject = new
                {
                    userId = client.Id.ToString(),
                    currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                    cash = Math.Round(balance, 2),
                    bonus = 0,
                    error = 0,
                    description = "Success"
                };
                BaseHelpers.RemoveSessionFromeCache(input.token, product.Id);
                jsonResponse = JsonConvert.SerializeObject(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }
        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/bet")]
        public HttpResponseMessage DoBet(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, true);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                var hash = input.hash;
                input.hash = null;
                input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.gameId);
                        if(product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);                   

                        var document = documentBl.GetDocumentByExternalId(input.reference.ToString(), client.Id,
                        ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        if (document == null)
                        {
                            var amount = input.amount.Value;
                            if (NotSupportedCurrencies.Contains(client.CurrencyId))
                                amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId,  amount);
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                RoundId = input.roundId,
                                TransactionId = input.reference.ToString(),
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
                                        client, clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
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
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);

                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            transactionId = document.Id.ToString(),
                            currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            usedPromo = 0,
                            error = 0,
                            description = "Success"
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/result")]
        public HttpResponseMessage DoWin(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                var hash = input.hash;
                input.hash = null;
                input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.gameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                        if (betDocument.ProductId != product.Id)
                            throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongProductId);
                        var winDocument = documentBl.GetDocumentByExternalId(input.reference, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        if (winDocument == null)
                        {
                            var amount = input.amount.Value;
                            if (input.promoWinAmount.HasValue)
                                amount += input.promoWinAmount.Value;
                            if (NotSupportedCurrencies.Contains(client.CurrencyId))
                                amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
                            var state = (amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.roundId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = input.gameId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.reference,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
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
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    Amount = Convert.ToDecimal(amount),
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            transactionId = winDocument.Id.ToString(),
                            currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            error = 0,
                            description = "Success"
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/endRound")]
        public HttpResponseMessage FinalizeRound(BetInput input)
        {
            var jsonResponse = string.Empty;          
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                var client = CacheManager.GetClientById(clientSession.Id);
                var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                var hash = input.hash;
                input.hash = null;
                input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                if (hash.ToLower() != input.hash.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId,
                                                                        client.Id, (int)BetDocumentStates.Uncalculated);
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.roundId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);

                        foreach (var betDoc in betDocuments)
                        {
                            betDoc.State = (int)BetDocumentStates.Lost;
                            listOfOperationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                            listOfOperationsFromApi.CreditTransactionId = betDoc.Id;
                            var doc = clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                        (betDoc == null ? (long?)null : betDoc.Id), listOfOperationsFromApi, doc[0], WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(listOfOperationsFromApi);
                                    throw;
                                }
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            error = 0,
                            description = "Success"
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;

        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/bonusWin")]
       
        public HttpResponseMessage DoBonusWin(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.gameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var hash = input.hash;
                    input.hash = null;
                    input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                    if (hash.ToLower() != input.hash.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundId, ProviderId, client.Id);
                    var balance = 0m;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                    }
                    else
                        balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                    var responseObject = new
                    {
                        transactionId = betDocument != null ? betDocument.Id.ToString() : input.reference,
                        currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                        cash = Math.Round(balance, 2),
                        bonus = 0,
                        usedPromo = 0,
                        error = 0,
                        description = "Success"
                    };
                    jsonResponse = JsonConvert.SerializeObject(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        [Route("{partnerId}/api/pragmaticplay/jackpotWin")]
        public HttpResponseMessage DoJackpotWin(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                        var client = CacheManager.GetClientById(clientSession.Id);
                        var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.gameId);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var hash = input.hash;
                        input.hash = null;
                        input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                        if (hash.ToLower() != input.hash.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        var winDocument = documentBl.GetDocumentByExternalId(input.reference, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        var amount = input.amount.Value;
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
                        if (winDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.reference + "_jackpotBet",
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Uncalculated,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    var epBalance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), 
                                        client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                                    BaseHelpers.BroadcastBalance(client.Id, epBalance);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(operationsFromProduct);
                                    throw;
                                }
                            }
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = input.gameId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.reference,
                                CreditTransactionId = betDocument.Id,
                                State = (int)BetDocumentStates.Won,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
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
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    Amount = Convert.ToDecimal(input.amount),
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            transactionId = winDocument != null ? winDocument.Id.ToString() : input.reference,
                            currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            usedPromo = 0,
                            error = 0,
                            description = "Success"
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }

        [Route("{partnerId}/api/pragmaticplay/promoWin")]
        public HttpResponseMessage DoPromoWin(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(input.userId.Value);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {   
                        var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                        var product = CacheManager.GetProductByExternalId(ProviderId, "PromoWin");
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var hash = input.hash;
                        input.hash = null;
                        input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                        if (hash.ToLower() != input.hash.ToLower())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                        var winDocument = documentBl.GetDocumentByExternalId(input.reference, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        if (winDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.reference + "_PromoBet",
                                OperationTypeId = (int)OperationTypes.Bet,
                                State = (int)BetDocumentStates.Uncalculated,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                            BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                        0, operationsFromProduct, betDocument, WebApiApplication.DbLogger); //will not work without clientSession.ParentId 
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(operationsFromProduct);
                                    throw;
                                }
                            }
                            var amount = input.amount.Value;
                            if (NotSupportedCurrencies.Contains(client.CurrencyId))
                                amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
                            var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = state;
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.gameId,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.reference,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = amount
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
                            else
                            {
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastWin(new ApiWin
                                {
                                    GameName = product.NickName,
                                    ClientId = client.Id,
                                    ClientName = client.FirstName,
                                    Amount = Convert.ToDecimal(input.amount),
                                    CurrencyId = client.CurrencyId,
                                    PartnerId = client.PartnerId,
                                    ProductId = product.Id,
                                    ProductName = product.NickName,
                                    ImageUrl = product.WebImageUrl
                                });
                            }
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id);
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            transactionId = winDocument != null ? winDocument.Id.ToString() : input.reference,
                            currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId,
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            error = 0,
                            description = "Success"
                        };
                        jsonResponse = JsonConvert.SerializeObject(responseObject);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);

            return resp;
        }


        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/refund")]
        public HttpResponseMessage Refund(BetInput input)
        {
            var jsonResponse = string.Empty;
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientSession = ClientBll.GetClientProductSession(input.token.ToLower(), Constants.DefaultLanguageId, null, false);
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);

                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    if (product.GameProviderId != ProviderId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var hash = input.hash;
                    input.hash = null;
                    input.hash = CommonFunctions.ComputeMd5(CommonFunctions.GetSortedParamWithValuesAsString(input, "&") + key).ToLower();
                    if (hash.ToLower() != input.hash.ToLower())
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.reference.ToString(),
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };
                    var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                    if (documents == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
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
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    var responseObject = new
                    {
                        transactionId = documents[0].Id,
                        error = 0,
                        description = "Success"
                    };
                    jsonResponse = JsonConvert.SerializeObject(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                jsonResponse = JsonConvert.SerializeObject(response);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {jsonResponse}");
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }
    }
}