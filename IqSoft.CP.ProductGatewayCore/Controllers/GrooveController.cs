using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.Groove;
using IqSoft.CP.BLL.Services;
using System;
using IqSoft.CP.Common.Enums;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Integration.Platforms.Helpers;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class GrooveController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.GrooveGaming).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "34.241.198.56",
            "34.241.79.104",
            "34.241.89.47",
            "194.6.233.147",
            "52.208.108.213",
            "34.249.184.31",
            "34.248.15.119",
            "3.248.231.5"
        };

        private static readonly List<string> NotSupportedCurrencies = new List<string>
        {
            Constants.Currencies.USDT
        };

        [HttpGet]
        [Route("{partnerId}/api/Groove/ApiRequest")]
        public ActionResult ApiRequest([FromQuery] BaseInput input)
        {
            var output = new BaseOutput
            {
                Code = GrooveHelpers.StatusCodes.Success,
                Status = "Success",
            };
            long transactionId = 0;
            int clientId = 0;
            var currency = string.Empty;
            bool isExternalPlatformClient = false;
            var externalPlatformType = new DAL.Models.Cache.PartnerKey();
            try
            {
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var checkSessionExpiration = true;
                if (input.request == GrooveHelpers.Actions.Win ||
                   input.request == GrooveHelpers.Actions.WinRollback ||
                   input.request == GrooveHelpers.Actions.Jackpot)
                    checkSessionExpiration = false;
                var token = input.gamesessionid.Split('_');
                if (token.Length != 2)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);

                var clientSession = ClientBll.GetClientProductSession(token[1], Constants.DefaultLanguageId,
                                                                      checkExpiration: checkSessionExpiration);
                var client = CacheManager.GetClientById(clientSession.Id);
                clientId = client.Id;
                currency = client.CurrencyId;
                if (input.accountid != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var casinoId = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.GrooveCasinoId);
                if (token[0] != casinoId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.NotAllowed);
                isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out externalPlatformType);
                switch (input.request.ToLower())
                {
                    case GrooveHelpers.Actions.GetBalance:
                    case GrooveHelpers.Actions.Authenticate:
                        output.ClientId = clientSession.Id;
                        output.City = clientSession.Country;
                        output.Country = clientSession.Country;
                        output.Currency = NotSupportedCurrencies.Contains(client.CurrencyId) ? Constants.Currencies.USADollar : client.CurrencyId;
                        output.Token = input.gamesessionid;
                        break;
                    case GrooveHelpers.Actions.Bet:
                        if (input.betamount < 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                        DoBet(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.TransactionId = transactionId.ToString();
                        output.BonusMoneyBet = 0;
                        output.RealMoneyBet = input.betamount;
                        break;
                    case GrooveHelpers.Actions.Win:
                        if (input.result < 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                        input.transactionid = String.Format("{0}_{1}", input.gamestatus, input.transactionid);
                        DoWin(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.WinTransactionId = transactionId.ToString();
                        output.BonusWin = 0;
                        output.RealMoneyWin = input.result;
                        break;
                    case GrooveHelpers.Actions.BetWin:
                        if (input.betamount < 0 || input.result < 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                        DoBet(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.TransactionId = transactionId.ToString();
                        input.transactionid = String.Format("{0}_{1}", input.gamestatus, input.transactionid);
                        DoWin(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.WinTransactionId = transactionId.ToString();
                        output.BonusMoneyBet = 0;
                        output.RealMoneyBet = input.betamount;
                        output.BonusWin = 0;
                        output.RealMoneyWin = input.result;
                        break;
                    case GrooveHelpers.Actions.BetRollback:
                    case GrooveHelpers.Actions.WinRollback:
                        Rollback(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.TransactionId = transactionId.ToString();
                        break;
                    case GrooveHelpers.Actions.Jackpot:
                        DoJackpotWin(input, clientSession, client, isExternalPlatformClient, externalPlatformType, out transactionId);
                        output.WinTransactionId = transactionId.ToString();
                        output.BonusMoneyBet = 0;
                        output.RealMoneyBet = input.betamount;
                        output.BonusWin = 0;
                        output.RealMoneyWin = input.result;
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                }
                var balance = 0M;
                if (isExternalPlatformClient)
                    balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id);
                else
                    balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                if (NotSupportedCurrencies.Contains(client.CurrencyId))
                    balance = BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, balance);
                output.ApiVersion = input.apiversion;
                output.RealBalance = Math.Round(balance, 2);
                output.Balance = Math.Round(balance, 2);
                output.BonusBalance = 0;
                output.GameMode = 0;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.LowBalance)
                {
                    CacheManager.SetFutureRollback(Constants.CacheItems.GrooveFailedBet, "1", input.roundid);
                }
                output.Code = GrooveHelpers.GetErrorCode(fex.Detail.Id);
                output.Status = GrooveHelpers.GetErrorMessage(output.Code);
                output.Message = GrooveHelpers.GetErrorMessage(output.Code);
                if (output.Code == GrooveHelpers.StatusCodes.Success)
                {
                    var balance = 0M;
                    if (isExternalPlatformClient)
                        balance = ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), clientId);
                    else
                        balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientId).AvailableBalance;
                    if (NotSupportedCurrencies.Contains(currency))
                        balance = BaseBll.ConvertCurrency(currency, Constants.Currencies.USADollar, balance);
                    output.ApiVersion = input.apiversion;
                    output.RealBalance = Math.Round(balance, 2); 
                    output.Balance = Math.Round(balance, 2); 
                    output.BonusBalance = 0;
                    output.GameMode = 0;
                    output.BonusMoneyBet = 0;
                    output.RealMoneyBet = input.betamount;
                    output.BonusWin = 0;
                    output.RealMoneyWin = input.result;
                    if (transactionId != 0)
                    {
                        output.TransactionId = transactionId.ToString();
                        output.WinTransactionId = transactionId.ToString();
                    }
                    else if (fex.Detail.IntegerInfo.HasValue)
                    {
                        output.TransactionId = fex.Detail.IntegerInfo.Value.ToString();
                        output.WinTransactionId = fex.Detail.IntegerInfo.Value.ToString();
                    }
                }
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                output.Code = GrooveHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Status = GrooveHelpers.GetErrorMessage(output.Code);
                output.Message = GrooveHelpers.GetErrorMessage(output.Code);
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            Program.DbLogger.Info("Output: " + JsonConvert.SerializeObject(output));
            return Ok(output);
        }

        private static void DoBet(BaseInput input, SessionIdentity clientSession, BllClient client, bool isExternalPlatformClient,
                                  DAL.Models.Cache.PartnerKey externalPlatformType, out long transactionId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.gameid);
                    if (product == null /*|| clientSession.ProductId != product.Id */)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.transactionid, client.Id,
                          ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument != null)
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(string.Format("{0}_{1}", input.gamestatus, input.transactionid), client.Id,
                          ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument != null)
                            transactionId = winDocument.Id;
                        else
                            transactionId = betDocument.Id;
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    }

                    var roundWin = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, input.roundid, ProviderId, client.Id);
                    if (roundWin != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        ExternalProductId = input.gameid,
                        ProductId = product.Id,
                        RoundId = input.roundid,
                        TransactionId = input.transactionid,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    var amount = input.betamount.Value;
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        amount = (int)BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                   // BaseHelpers.BroadcastBetLimit(info);

                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            var epBalance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                                     clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                            BaseHelpers.BroadcastBalance(client.Id, epBalance);
                        }
                        catch (Exception ex)
                        {
                            Program.DbLogger.Error(ex.Message);
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                            throw;
                        }
                    }
                    else
                    {
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    transactionId = betDocument.Id;
                }
            }
        }

        private static void DoWin(BaseInput input, SessionIdentity clientSession, BllClient client, bool isExternalPlatformClient,
                                  DAL.Models.Cache.PartnerKey externalPlatformType, out long transactionId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.gameid);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundid,
                                                                      ProviderId, client.Id);
                    if (betDocument == null)
                    {
                        if (input.frbid == null)
                        {
                            var failedBet = CacheManager.GetFutureRollback(Constants.CacheItems.GrooveFailedBet, input.roundid);
                            if (!string.IsNullOrEmpty(failedBet))
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongOperationAmount);
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        }
                        else
                        {
                            var betOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ExternalProductId = input.gameid,
                                ProductId = product.Id,
                                RoundId = input.roundid,
                                TransactionId = input.transactionid,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0,
                                DeviceTypeId = clientSession.DeviceType
                            });
                            betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl);
                            //BaseHelpers.BroadcastBetLimit(info);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    var epBalance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                                             clientSession.ParentId ?? 0,
                                                                                             betOperationsFromProduct, betDocument);
                                    BaseHelpers.BroadcastBalance(client.Id, epBalance);
                                }
                                catch (Exception ex)
                                {
                                    Program.DbLogger.Error(ex.Message);
                                    documentBl.RollbackProductTransactions(betOperationsFromProduct);
                                    throw;
                                }
                            }
                        }
                    }
                    var winDocument = documentBl.GetDocumentByExternalId(input.transactionid, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                    {
                        transactionId = winDocument.Id;
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WinAlreadyPayed);
                    }
                    else
                    {
                        var roundWins = documentBl.GetDocumentsByRoundId((int)OperationTypes.Win, input.roundid, ProviderId, client.Id, null);
                        if (roundWins.Any(x => x.ExternalTransactionId.Contains("completed_")))
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
                    }
                    var state = input.result > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = input.roundid,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalOperationId = null,
                        ExternalProductId = input.gameid,
                        ProductId = betDocument.ProductId,
                        TransactionId = input.frbid ?? input.transactionid,
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    var amount = input.result.Value;
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        amount = (int)BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);

                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = clientSession.DeviceType
                    });

                    var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                            (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, doc[0]);
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
                        Amount = amount,
                        CurrencyId = client.CurrencyId,
                        PartnerId = client.PartnerId,
                        ProductId = product.Id,
                        ProductName = product.NickName,
                        ImageUrl = product.WebImageUrl
                    });
                    transactionId = doc[0].Id;
                }
            }
        }

        private void Rollback(BaseInput input, SessionIdentity clientSession, BllClient client, bool isExternalPlatformClient,
                              DAL.Models.Cache.PartnerKey externalPlatformType, out long transactionId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.gameid);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    if (!string.IsNullOrEmpty(input.roundid))
                    {
                        var roundWin = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, input.roundid, ProviderId, clientSession.Id);
                        if (roundWin != null)
                        {
                            transactionId = roundWin.Id;
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentAlreadyWinned);
                        }
                    }
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.transactionid,
                        ExternalProductId = product.ExternalId,
                        ProductId = product.Id
                    };
                    List<Document> documents = new List<Document>();
                    try
                    {
                        documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        if (documents == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                            operationsFromProduct, documents[0]);
                            }
                            catch (Exception ex)
                            {
                                Program.DbLogger.Error(ex.Message);
                            }
                        }
                    }
                    catch (FaultException<BllFnErrorType> fex)
                    {
                        if (fex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
                        {
                            var rollbackDocument = documentBl.GetDocumentByExternalId(input.transactionid, clientSession.Id, ProviderId,
                                                                                  partnerProductSetting.Id, (int)OperationTypes.BetRollback);
                            if (rollbackDocument != null)
                                transactionId = rollbackDocument.Id;
                        }
                        throw;
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(clientSession.Id);
                    transactionId = documents[0].Id;
                }
            }
        }

        private static void DoJackpotWin(BaseInput input, SessionIdentity clientSession, BllClient client, bool isExternalPlatformClient,
                                         DAL.Models.Cache.PartnerKey externalPlatformType, out long transactionId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.gameid);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var winDocument = documentBl.GetDocumentByExternalId(input.transactionid, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                    {
                        transactionId = winDocument.Id;
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    }

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        RoundId = input.roundid,
                        ProductId = product.Id,
                        TransactionId = input.transactionid,
                        OperationTypeId = (int)OperationTypes.Bet,
                        State = (int)BetDocumentStates.Uncalculated,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = 0,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    var betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                    //BaseHelpers.BroadcastBetLimit(info);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            var epBalance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                                     clientSession.ParentId ?? 0, operationsFromProduct, betDocument);
                            BaseHelpers.BroadcastBalance(client.Id, epBalance);
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
                        ExternalProductId = input.gameid,
                        ProductId = betDocument.ProductId,
                        TransactionId = input.transactionid,
                        CreditTransactionId = betDocument.Id,
                        State = (int)BetDocumentStates.Won,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    var amount = input.amount.Value;
                    if (NotSupportedCurrencies.Contains(client.CurrencyId))
                        amount = (int)BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, amount);
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = amount,
                        DeviceTypeId = clientSession.DeviceType
                    });

                    winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    if (isExternalPlatformClient)
                    {
                        try
                        {
                            var epBalance = ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                                 (betDocument == null ? (long?)null : betDocument.Id), 
                                                                                  operationsFromProduct, winDocument);
                            BaseHelpers.BroadcastBalance(client.Id, epBalance);
                        }
                        catch (Exception ex)
                        {
                            Program.DbLogger.Error(ex.Message);
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
                            Amount = amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }
                    transactionId = winDocument.Id;
                }
            }
        }
    }
}