using IqSoft.CP.ProductGateway.Models.EveryMatrix;
using IqSoft.CP.ProductGateway.Helpers;
using System.Collections.Generic;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.WebSiteModels;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class EveryMatrixController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.EveryMatrix).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "109.205.92.61",
            "109.205.92.81"
        };

        [HttpPost]
        [Route("{partnerId}/api/EveryMatrix/ApiRequest")]
        public ActionResult ApiRequest(BaseInput input)
        {
            var jsonResponse = string.Empty;
            var baseOutput = new BaseOutput
            {
                Request = input.Request,
                SessionId = input.SessionId,
                ReturnCode = (int)EveryMatrixHelpers.ReturnCodes.Success
            };
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                Program.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId, null, input.ValidateSession);
                var client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                var login = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EveryMatrixLogin);
                var pass = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EveryMatrixPassword);
                if (input.LoginName != login || input.Password != pass)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                switch (input.Request)
                {
                    case EveryMatrixHelpers.Methods.GetAccount:
                        using (var regionBl = new RegionBll(clientSession, Program.DbLogger))
                        {
                            var region = regionBl.GetRegionByCountryCode(clientSession.Country);
                            var accountObject = new AccountOutput(baseOutput)
                            {
                                ExternalUserId = client.Id.ToString(),
                                Country = region.IsoCode3,
                                Currency = client.CurrencyId,
                                SessionId = input.SessionId,
                                UserName = client.UserName,
                                FirstName = !string.IsNullOrEmpty(client.FirstName) ? client.FirstName : client.Id.ToString(),
                                LastName = !string.IsNullOrEmpty(client.LastName) ? client.LastName : client.UserName.ToString(),
                                Alias = client.Id.ToString()
                            };
                            jsonResponse = JsonConvert.SerializeObject(accountObject);
                        }
                        break;
                    case EveryMatrixHelpers.Methods.GetBalance:
                        var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                        var balanceObject = new BalanceOutput(baseOutput)
                        {
                            Balance = balance,
                            BonusMoney = 0m,
                            RealMoney = balance,
                            Currency = client.CurrencyId
                        };
                        jsonResponse = JsonConvert.SerializeObject(balanceObject);
                        break;
                    case EveryMatrixHelpers.Methods.WalletDebit:
                        var transactionId = string.Empty;
                        if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Bet.ToString())
                            transactionId = DoBet(input, clientSession, client);
                        else if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Rollback.ToString())
                            transactionId = RollbackTransaction(input, clientSession.ProductId, client, OperationTypes.Win);
                        else
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ObjectTypeNotFound);
                        balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                        var debitObject = new TransactionOutput(baseOutput)
                        {
                            AccountTransactionId = transactionId,
                            Currency = client.CurrencyId,
                            Balance = balance,
                            BonusMoneyAffected = 0m,
                            RealMoneyAffected = balance
                        };
                        jsonResponse = JsonConvert.SerializeObject(debitObject);
                        break;
                    case EveryMatrixHelpers.Methods.WalletCredit:
                        var winTransactionId = string.Empty;
                        if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Win.ToString())
                            winTransactionId =  DoWin(input, client, clientSession.ProductId);
                        else if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Rollback.ToString())
                            winTransactionId = RollbackTransaction(input, clientSession.ProductId, client, OperationTypes.Bet);
                        else
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ObjectTypeNotFound);
                        balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                        var creditObject = new TransactionOutput(baseOutput)
                        {
                            AccountTransactionId = winTransactionId,
                            Currency = client.CurrencyId,
                            Balance = balance,
                            BonusMoneyAffected = 0m,
                            RealMoneyAffected = balance
                        };
                        jsonResponse = JsonConvert.SerializeObject(creditObject);
                        break;
                    default:
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.ReturnCode = EveryMatrixHelpers.GetErrorCode(fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                baseOutput.ReturnCode = EveryMatrixHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
                Program.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            return Ok(jsonResponse);
        }

        private static string DoBet(BaseInput input, SessionIdentity clientSession, BllClient client)
        {
            using (var documentBl = new DocumentBll(clientSession, Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                            partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        var doc = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl);
                        if (input.RoundStatus.ToLower() == "close")
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId,
                                                                                client.Id, (int)BetDocumentStates.Uncalculated);
                            var operationsFromApi = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            foreach (var betDoc in betDocuments)
                            {
                                betDoc.State = (int)BetDocumentStates.Lost;
                                listOfOperationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                                listOfOperationsFromApi.CreditTransactionId = betDoc.Id;
                                clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        return doc.Id.ToString();
                    }
                    return document.Id.ToString();
                }
            }
        }

        private static string DoWin(BaseInput input, BllClient client, int productId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductById(productId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id);

                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                    var state = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            //SessionId = clientSession.SessionId,
                            State = state,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId,
                            CreditTransactionId = betDocument.Id,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount
                        });
                        var doc = clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = input.Amount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        return doc[0].Id.ToString();
                    }
                    return winDocument.Id.ToString();
                }
            }
        }

        private static string RollbackTransaction(BaseInput input, int productId, BllClient client, OperationTypes operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductById(productId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.RollbackTransactionId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id,
                    OperationTypeId = (int)operationType
                };
                try
                {
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);
                    return doc[0].Id.ToString();
                }
                catch (FaultException<BllFnErrorType> fex)
                {

                    if (fex.Detail.Id != (int)Constants.Errors.DocumentAlreadyRollbacked)
                        throw;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    var document = documentBl.GetDocumentByExternalId(input.RollbackTransactionId, client.PartnerId, ProviderId,
                                                               partnerProductSetting.Id, (int)operationType);
                    return document.Id.ToString();
                }
            }
        }
    }
}