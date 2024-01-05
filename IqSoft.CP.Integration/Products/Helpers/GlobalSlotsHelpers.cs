using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Products.Models.GlobalSlots;
using log4net;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Helpers
{
    public static class GlobalSlotsHelpers
    {
        private static readonly BllGameProvider GameProvider = CacheManager.GetGameProviderByName(Constants.GameProviders.GlobalSlots);
        private static readonly int ProductId = CacheManager.GetProductByExternalId(GameProvider.Id, Constants.GameProviders.GlobalSlots).Id;

        public static void TransferToProvider(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var dbClient = clientBl.GetClientById(clientId);
                    var amount = Math.Round(BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientId).AvailableBalance, 2);
                    if (amount <= 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.LowBalance);                   

                    var transferInput = new TransferInput
                    {
                        key = dbClient.PasswordHash,
                        @event = "addmoney",
                        ghouse_id = dbClient.BetShopId.Value,
                        credit = amount,
                        termid = dbClient.UserName
                    };
                    CommonFunctions.GetSortedParamWithValuesAsString(transferInput, "&");

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Get,
                        Url = string.Format("{0}?{1}", GameProvider.GameLaunchUrl , CommonFunctions.GetSortedParamWithValuesAsString(transferInput, "&"))
                    };
                    var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    if (resp == "success=1")
                    {
                        var bllClient = CacheManager.GetClientById(clientId);
                        var operationsTransferToProduct = new ListOfOperationsFromApi
                        {
                            TransactionId = Guid.NewGuid().ToString(),
                            ProductId = ProductId,
                            CurrencyId = dbClient.CurrencyId,
                            GameProviderId = GameProvider.Id,
                            OperationTypeId = (int)OperationTypes.Bet
                        };
                        operationsTransferToProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = bllClient,
                            Amount = amount
                        });
                        var betDocument = clientBl.CreateCreditFromClient(operationsTransferToProduct, documentBl, out LimitInfo info);
                        var lostOperationsTransferToProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = dbClient.CurrencyId,
                            GameProviderId = GameProvider.Id,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = operationsTransferToProduct.TransactionId + "_lost",
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Lost,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        lostOperationsTransferToProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = bllClient,
                            Amount = 0
                        });

                        clientBl.CreateDebitsToClients(lostOperationsTransferToProduct, betDocument, documentBl);
                    }
                    else
                        throw new Exception(resp);
                }
            }
        }

        public static void TransferFromProvider(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var client = clientBl.GetClientById(clientId);
                    var blClient = CacheManager.GetClientById(clientId);
                    var transferInput = new TransferInput
                    {
                        key = client.PasswordHash,
                        @event = "moneyout",
                        ghouse_id = client.BetShopId.Value,
                        termid = client.UserName
                    };
                    CommonFunctions.GetSortedParamWithValuesAsString(transferInput, "&");

                    var httpRequestInput = new HttpRequestInput
                    {
                        ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                        RequestMethod = Constants.HttpRequestMethods.Get,
                        Url = string.Format("{0}?{1}", GameProvider.GameLaunchUrl, CommonFunctions.GetSortedParamWithValuesAsString(transferInput, "&"))
                    };

                    var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    var respValues = resp.Split('|');
                    if (respValues[0] == "1")
                    {
                        var amount = Convert.ToDecimal(respValues[1]);
                        if (amount <= 0)
                            return;
                        var operationsTransferToProduct = new ListOfOperationsFromApi
                        {
                            TransactionId = Guid.NewGuid().ToString(),
                            CurrencyId = client.CurrencyId,
                            GameProviderId = GameProvider.Id,
                            ProductId = ProductId,
                            OperationTypeId = (int)OperationTypes.Bet
                        };
                        operationsTransferToProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = blClient,
                            Amount = 0
                        });
                        var betDocument = clientBl.CreateCreditFromClient(operationsTransferToProduct, documentBl, out LimitInfo info);
                        var lostOperationsTransferToProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = GameProvider.Id,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = operationsTransferToProduct.TransactionId + "_won",
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        lostOperationsTransferToProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = blClient,
                            Amount = amount
                        });

                        clientBl.CreateDebitsToClients(lostOperationsTransferToProduct, betDocument, documentBl);
                    }
                    else
                        throw new Exception(resp);
                }
            }
        }
    }
}
