using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.PragmaticPlay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Integration.Platforms.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    [Consumes("application/x-www-form-urlencoded")]
    public class PragmaticPlayController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.PragmaticPlay).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "88.99.114.52",
            "88.99.114.53",
            "88.99.114.54",
            "195.69.223.146",
            "176.112.120.11",
            "185.8.155.162",
            "5.2.130.103",
            "84.1.113.218",
            "89.149.209.64",
            "89.149.209.27"
        };

        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT
        };

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/authenticate")]
        public ActionResult CheckSession([FromForm]BaseInput input)
        {
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

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var balance = 0m;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (isExternalPlatformClient)
                    {
                        balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                    }
                    else
                        balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                    return Ok(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/balance")]
        public ActionResult GetBalance([FromForm]BaseInput input)
        {
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
                    balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                return Ok(responseObject);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }
        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/bet")]
        public ActionResult DoBet([FromForm] BetInput input)
        {
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
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var product = CacheManager.GetProductByExternalId(ProviderId, input.gameId);
                        if (product == null)
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
                                amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
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
                            document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, document);
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
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                        return Ok(responseObject);

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
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/result")]
        public ActionResult DoWin([FromForm] BetInput input)
        {
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

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                                    (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument);
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
                                Amount = Convert.ToDecimal(amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance((int)ExternalPlatformTypes.IQSoft, client.Id);
                        }
                        else
                            balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                        return Ok(responseObject);
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
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/endRound")]
        public ActionResult FinalizeRound([FromForm] BetInput input)
        {
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
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                                        (betDoc == null ? (long?)null : betDoc.Id), listOfOperationsFromApi, doc[0]);
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(ex.Message);
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
                            balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                        if (NotSupportedCurrencies.Contains(client.CurrencyId))
                            balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                        var responseObject = new
                        {
                            cash = Math.Round(balance, 2),
                            bonus = 0,
                            error = 0,
                            description = "Success"
                        };
                        return Ok(responseObject);
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
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/bonusWin")]

        public ActionResult DoBonusWin([FromForm]BetInput input)
        {
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                        balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                    return Ok(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [Route("{partnerId}/api/pragmaticplay/jackpotWin")]
        public ActionResult DoJackpotWin([FromForm]BetInput input)
        {
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(ex.Message);
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
                                        (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument);
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
                                Amount = Convert.ToDecimal(input.amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;

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
                        return Ok(responseObject);
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
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }

        [Route("{partnerId}/api/pragmaticplay/promoWin")]
        public ActionResult DoPromoWin([FromForm]BetInput input)
        {
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(input.userId.Value);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
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
                            var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 0, operationsFromProduct, betDocument);
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(ex.Message);
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
                                        (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument);
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
                                Amount = Convert.ToDecimal(input.amount),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        var balance = 0m;
                        if (isExternalPlatformClient)
                        {
                            balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                        }
                        else
                            balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
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
                        return Ok(responseObject);
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
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }


        [HttpPost]
        [Route("{partnerId}/api/pragmaticplay/refund")]
        public ActionResult Refund([FromForm] BetInput input)
        {
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client, operationsFromProduct, documents[0]);
                        }
                        catch (Exception ex)
                        {
                            Program.DbLogger.Error(ex.Message);
                        }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);BaseHelpers.BroadcastBalance(client.Id);
                    var responseObject = new
                    {
                        transactionId = documents[0].Id,
                        error = 0,
                        description = "Success"
                    };
                    return Ok(responseObject);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(fex.Detail.Id),
                    description = fex.Detail.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    error = PragmaticPlayHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    description = ex.Message
                };
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
                return Ok(response);
            }
        }
    }
}