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
using IqSoft.CP.ProductGateway.Hubs;
using IqSoft.CP.ProductGateway.Models.BaseModels;
using IqSoft.CP.ProductGateway.Models.Common;
using IqSoft.CP.ProductGateway.Models.EveryMatrix;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class BaseHelpers
    {
        public static readonly dynamic _connectedClients =
        GlobalHost.ConnectionManager.GetHubContext<BaseHub>().Clients.Group("WebSiteWebApi");

        public static decimal GetClientProductBalance(int clientId, int productId)
        {
            bool allowBonus = false;

            if (productId > 0)
            {
                var cacheBonus = CacheManager.GetActiveWageringBonus(clientId);
                if (cacheBonus.Id > 0)
                {
                    var clientBonus = CacheManager.GetBonusById(cacheBonus.BonusId);
                    if (clientBonus.FreezeBonusBalance != true)
                    {
                        if (clientBonus.Type == (int)BonusTypes.CampaignWagerSport && (productId == Constants.SportsbookProductId || Constants.ExternalSportsbookProductIds.Contains(productId)))
                            allowBonus = true;
                        else if (clientBonus.Type != (int)BonusTypes.CampaignWagerSport && productId != Constants.SportsbookProductId && !Constants.ExternalSportsbookProductIds.Contains(productId))
                        {
                            var bonusProducts = CacheManager.GetBonusProducts(cacheBonus.BonusId);
                            var product = CacheManager.GetProductById(productId);
                            while (!allowBonus)
                            {
                                var pr = bonusProducts.FirstOrDefault(x => x.ProductId == product.Id);
                                if (pr != null)
                                {
                                    if (pr.Percent > 0)
                                        allowBonus = true;
                                    break;
                                }
                                else
                                {
                                    if (!product.ParentId.HasValue)
                                        break;
                                    product = CacheManager.GetProductById(product.ParentId.Value);
                                }
                            }
                        }
                    }
                }
            }
            if (!allowBonus)
                return Math.Floor(CacheManager.GetClientCurrentBalance(clientId).Balances.Where(x =>
                    x.TypeId != (int)AccountTypes.ClientCompBalance &&
                    x.TypeId != (int)AccountTypes.ClientCoinBalance &&
                    x.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(x => x.Balance) * 100) / 100;
            else
                return CacheManager.GetClientCurrentBalance(clientId).AvailableBalance;
        }

        public static void BroadcastBetShopBet(PlaceBetOutput placeBetOutput)
        {
            _connectedClients.BroadcastBet(placeBetOutput);
        }

        public static void BroadcastWin(ApiWin input)
        {
            var currency = CacheManager.GetCurrencyById(input.CurrencyId);
            input.CurrencyId = currency.Name;
            if (input.Amount > 0)
            {
                input.ApiBalance = CacheManager.GetClientCurrentBalance(input.ClientId).ToApiBalance();
                _connectedClients.BroadcastWin(input);
            }
        }

        public static void BroadcastBalance(int clientId, decimal? balance = null)
        {
            if (balance != null)
                _connectedClients.BroadcastBalance(new ApiWin
                {
                    ClientId = clientId,
                    ApiBalance = new ApiBalance
                    {
                        AvailableBalance = balance.Value,
                        Balances = new System.Collections.Generic.List<ApiAccount> { new ApiAccount { TypeId = (int)AccountTypes.ClientUsedBalance, Balance = balance.Value } }
                    }
                });
            else
            {
                var clientBalance = CacheManager.GetClientCurrentBalance(clientId);
                _connectedClients.BroadcastBalance(new ApiWin { ClientId = clientId, ApiBalance = clientBalance.ToApiBalance() });
            }
        }

        public static void BroadcastBetLimit(LimitInfo info)
        {
            if (info.DailyBetLimitPercent != null || info.WeeklyBetLimitPercent != null || info.MonthlyBetLimitPercent != null)
            {
                _connectedClients.BroadcastBetLimit(info);
            }
        }

        public static void RemoveFromeCache(string key)
        {
            InvokeMessage("RemoveKeyFromCache", key);
        }

        public static void RemoveSessionFromeCache(string token, int? productId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_0", Constants.CacheItems.ClientSessions, token));

            if (productId != null)
                InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSessions, token, productId));
        }

        public static void RemoveClientBalanceFromeCache(int clientId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, clientId));
        }

        public static void RemoveBetshopFromeCache(int betshopId)
        {
            InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.BetShops, betshopId));
        }

        private static void InvokeMessage(string messageName, params object[] obj)
        {
            Task.Run(() => WebApiApplication.JobHubProxy.Invoke(messageName, obj));
        }

        public static DAL.Document DoBet(TransactionInput transactionInput)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductByExternalId(transactionInput.ProviderId, transactionInput.ProductExternalId) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(transactionInput.Client.PartnerId, product.Id) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var document = documentBl.GetDocumentByExternalId(transactionInput.TransactionId, transactionInput.Client.Id, transactionInput.ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (transactionInput.ThrowDuplicateTransaction && document != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(transactionInput.Client, out PartnerKey externalPlatformType);

                    if (document == null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = transactionInput.SessionId,
                            CurrencyId = transactionInput.Client.CurrencyId,
                            RoundId = transactionInput.RoundId,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = product.GameProviderId.Value,
                            ProductId = product.Id,
                            TransactionId = transactionInput.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = transactionInput.Client,
                            Amount = transactionInput.Amount,
                            DeviceTypeId = transactionInput.SessionDeviceType
                        });
                        var doc = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
                                    transactionInput.Client, transactionInput.SessionParentId, listOfOperationsFromApi, doc, WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex);
                                documentBl.RollbackProductTransactions(listOfOperationsFromApi);
                                throw;
                            }
                        }
                        if (transactionInput.IsRoundClosed)
                            CloseRound(transactionInput, product, clientBl, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(transactionInput.Client.Id);
                        BaseHelpers.BroadcastBalance(transactionInput.Client.Id);
                        return doc;
                    }
                    return document;
                }
            }
        }

        public static DAL.Document DoWin(TransactionInput transactionInput)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductByExternalId(transactionInput.ProviderId, transactionInput.ProductExternalId) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(transactionInput.Client.PartnerId, product.Id) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                    DAL.Document betDocument = null;
                    if (transactionInput.IsFreeSpin)
                        betDocument =  DoFreeBet(transactionInput, product.Id, clientBl, documentBl);
                    else if (!string.IsNullOrEmpty(transactionInput.CreditTransactionId))
                        betDocument =   documentBl.GetDocumentByExternalId(transactionInput.CreditTransactionId, transactionInput.Client.Id,
                                                                           product.GameProviderId.Value, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    else
                        betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, transactionInput.RoundId,
                                                                      product.GameProviderId.Value, transactionInput.Client.Id);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(transactionInput.TransactionId, transactionInput.Client.Id,
                                                                         product.GameProviderId.Value, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (transactionInput.ThrowDuplicateTransaction && winDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(transactionInput.Client, out PartnerKey externalPlatformType);

                    if (winDocument == null)
                    {
                        var state = transactionInput.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = transactionInput.SessionId,
                            CurrencyId = transactionInput.Client.CurrencyId,
                            RoundId = transactionInput.RoundId,
                            GameProviderId = product.GameProviderId.Value,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = transactionInput.TransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = transactionInput.Client,
                            Amount = transactionInput.Amount
                        });

                        winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];

                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), transactionInput.Client,
                                                                      betDocument.Id, operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                            }
                        }
                        else
                        {
                            BaseHelpers.RemoveClientBalanceFromeCache(transactionInput.Client.Id);
                            BaseHelpers.BroadcastWin(new ApiWin
                            {
                                BetId = betDocument?.Id ?? 0,
                                GameName = product.NickName,
                                ClientId = transactionInput.Client.Id,
                                ClientName = transactionInput.Client.FirstName,
                                BetAmount = betDocument?.Amount,
                                Amount = Convert.ToDecimal(transactionInput.Amount),
                                CurrencyId = transactionInput.Client.CurrencyId,
                                PartnerId = transactionInput.Client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });
                        }
                        if (transactionInput.IsRoundClosed)
                            CloseRound(transactionInput, product, clientBl, documentBl);
                    }
                    return winDocument;
                }
            }
        }

        public static DAL.Document Rollback(BllClient client, string transactionId, string roundId, int providerId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, roundId,
                                                                providerId, client.Id, null);
                var d = betDocuments.FirstOrDefault(x => x.ExternalTransactionId == transactionId);
                if (d != null)
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = providerId,
                        TransactionId = transactionId,
                        ProductId = d.ProductId
                    };
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                    var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                    if (isExternalPlatformClient)
                        try
                        {
                            ExternalPlatformHelpers.RollbackTransaction(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                        operationsFromProduct, documents[0], WebApiApplication.DbLogger);
                        }
                        catch (Exception ex)
                        {
                            WebApiApplication.DbLogger.Error(ex.Message);
                        }
                    else
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    return documents[0];
                }
                return null;
            }
        }

        private static DAL.Document DoFreeBet(TransactionInput transactionInput, int productId, ClientBll clientBl, DocumentBll documentBl)
        {
            var listOfOperationsFromApi = new ListOfOperationsFromApi
            {
                SessionId = transactionInput.SessionId,
                CurrencyId = transactionInput.Client.CurrencyId,
                RoundId = transactionInput.RoundId,
                ExternalProductId = transactionInput.ProductExternalId,
                GameProviderId = transactionInput.ProviderId,
                ProductId = productId,
                TransactionId = transactionInput.TransactionId,
                OperationItems = new List<OperationItemFromProduct>()
            };
            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
            {
                Client = transactionInput.Client,
                Amount = 0,
                DeviceTypeId = transactionInput.SessionDeviceType
            });
            return clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out _);
        }

        private static void CloseRound(TransactionInput transactionInput, BllProduct product, ClientBll clientBl, DocumentBll documentBl)
        {
            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, transactionInput.RoundId, product.GameProviderId.Value,
                                                                transactionInput.Client.Id, (int)BetDocumentStates.Uncalculated);
            var operationsFromApi = new ListOfOperationsFromApi
            {
                SessionId = transactionInput.SessionId,
                CurrencyId = transactionInput.Client.CurrencyId,
                RoundId = transactionInput.RoundId,
                GameProviderId = product.GameProviderId.Value,
                ProductId = product.Id,
                OperationItems = new List<OperationItemFromProduct>()
            };
            operationsFromApi.OperationItems.Add(new OperationItemFromProduct
            {
                Client = transactionInput.Client,
                Amount = 0
            });
            var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(transactionInput.Client, out PartnerKey externalPlatformType);

            foreach (var betDoc in betDocuments)
            {
                betDoc.State = (int)BetDocumentStates.Lost;
                operationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                operationsFromApi.CreditTransactionId = betDoc.Id;
                var winDocuments = clientBl.CreateDebitsToClients(operationsFromApi, betDoc, documentBl);
                if (isExternalPlatformClient)
                {
                    try
                    {
                        ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), transactionInput.Client,
                         betDoc.Id, operationsFromApi, winDocuments[0], WebApiApplication.DbLogger);
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                    }
                }
            }
        }
    }
}