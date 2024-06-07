using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Elite;
using Newtonsoft.Json;
using Transaction = IqSoft.CP.ProductGateway.Models.Elite.Transaction;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EliteController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Elite).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Elite);

        [Route("{partnerId}/api/Elite/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
            var response = string.Empty;
            var output = new BaseOutput()
            {
                Result = new Result()
            };
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var client = new BllClient();
            decimal balance = 0m;
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                if (!Request.Headers.Contains("Authorization"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var signature = Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                client = CacheManager.GetClientById(Convert.ToInt32(input.TransactionData.Username));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var authorizationKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EliteAuthorizationKey);
                if (signature != authorizationKey)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var product = new BllProduct();
                var clientSession = new SessionIdentity();
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (input.Method != EliteHelpers.Methods.PromoWin)
                {
                    clientSession = ClientBll.GetClientProductSession(input.TransactionData.SessionToken, Constants.DefaultLanguageId, null, input.Method != EliteHelpers.Methods.Credit);
                    if (clientSession.Id != client.Id)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    product = CacheManager.GetProductById(clientSession.ProductId);
                }
                else
                {
                    product = CacheManager.GetProductByExternalId(ProviderId, "PromoWin");
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                }
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                switch (input.Method)
                {
                    case EliteHelpers.Methods.GetBalance:
                        break;
                    case EliteHelpers.Methods.Debit:
                        output.Result.ExternalTransactionId = DoBet(input.TransactionData, input.TransactionData.Amount, client, clientSession, partnerProductSetting.Id);
                        break;
                    case EliteHelpers.Methods.Credit:
                        var documentIds = DoWin(input.TransactionData, input.TransactionData.Amount, client, clientSession, partnerProductSetting.Id,
                                                product);
                        output.Result.ExternalTransactionId = documentIds.FirstOrDefault();
                        output.Result.ExternalPromoWinTransactionId = documentIds.LastOrDefault();
                        break;
                    case EliteHelpers.Methods.CloseGameRound:
                        CloseGameRound(input.TransactionData, client, clientSession, partnerProductSetting.Id, product);
                        break;
                    case EliteHelpers.Methods.CancelDebit:
                        var transId = $"{input.TransactionData.TransactionId}_{input.TransactionData.GameRoundId}";
                        output.Result.ExternalTransactionId = Rollback(transId, client, product, (int)OperationTypes.Bet);
                        break;
                    case EliteHelpers.Methods.CancelCredit:
                        transId = $"{input.TransactionData.TransactionId}_{input.TransactionData.GameRoundId}";
                        output.Result.ExternalTransactionId  = Rollback(transId, client, product, (int)OperationTypes.Win);
                        break;
                    case EliteHelpers.Methods.CreditDebit:
                        var debitDocumentId = string.Empty;
                        var creditDocumentId = string.Empty;
                        input.TransactionData.TransactionId = input.TransactionData.DebitTransactionId ?? input.TransactionData.CreditTransactionId;
                        debitDocumentId = DoBet(input.TransactionData, input.TransactionData.DebitAmount, client, clientSession, partnerProductSetting.Id);
                        input.TransactionData.TransactionId = input.TransactionData.CreditTransactionId ?? input.TransactionData.DebitTransactionId;
                        documentIds = DoWin(input.TransactionData, input.TransactionData.CreditAmount, client, clientSession,
                                            partnerProductSetting.Id, product);
                        output.Result.CashDebit = input.TransactionData.DebitAmount;
                        output.Result.CashCredit = input.TransactionData.CreditAmount;
                        output.Result.ExternalDebitTransactionId = debitDocumentId;
                        output.Result.ExternalCreditTransactionId = documentIds.FirstOrDefault();
                        break;
                    case EliteHelpers.Methods.CancelCreditDebit:
                        transId = $"{input.TransactionData.DebitTransactionId ?? input.TransactionData.CreditTransactionId}_{input.TransactionData.GameRoundId}";
                        debitDocumentId = Rollback(transId, client, product, (int)OperationTypes.Bet);
                        output.Result.CashDebit = input.TransactionData.DebitAmount;
                        output.Result.CashCredit = input.TransactionData.CreditAmount;
                        output.Result.ExternalDebitTransactionId = debitDocumentId;
                        output.Result.ExternalCreditTransactionId = debitDocumentId;
                        break;
                    case EliteHelpers.Methods.PromoWin:
                        output.Result.ExternalTransactionId = DoPromoWin(input.TransactionData.TransactionId, client, product, input.TransactionData.Amount, partnerProductSetting.Id);
                        break;
                }
                if (isExternalPlatformClient)
                    balance = Math.Round(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), client.Id), 2);
                else
                    balance = Math.Round(BaseHelpers.GetClientProductBalance(client.Id, product.Id), 2);
                output.Result.Balance = balance;
                output.Result.CashBalance = balance;
                output.Result.Currency = client.CurrencyId;
                output.Result.Cash = input.TransactionData.Amount;
                response = JsonConvert.SerializeObject(output);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var error = EliteHelpers.ErrorMapping(ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id, ex.Detail == null ? "GeneralException" : ex.Detail.Message);
                response = JsonConvert.SerializeObject(new ErrorOutput
                {
                    Error = new Error
                    {
                        Code = error.Code,
                        Message = error.Message,
                        CurrentBalance = new CurrentBalance
                        {
                            Balance = balance,
                            CashBalance = balance,
                            Currency = client?.CurrencyId,
                        }
                    }
                });
                WebApiApplication.DbLogger.Error(response);

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response = JsonConvert.SerializeObject(new ErrorOutput
                {
                    Error = new Error
                    {
                        Code = Constants.Errors.GeneralException,
                        Message = ex.Message,
                        CurrentBalance = new CurrentBalance
                        {
                            Balance = balance,
                            CashBalance = balance,
                            Currency = client?.CurrencyId,
                        }
                    }
                });
                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
            }
            httpResponseMessage.Content = new StringContent(response, Encoding.UTF8);
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private string DoBet(Transaction transaction, decimal amount, BllClient client, SessionIdentity clientSession, int partnerProductSettingId)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var document = documentBl.GetDocumentByExternalId($"{transaction.TransactionId}_{transaction.GameRoundId}", clientSession.Id, ProviderId,
                                                                            partnerProductSettingId, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = transaction.GameRoundId,
                            ExternalProductId = transaction.GameId,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            TransactionId = $"{transaction.TransactionId}_{transaction.GameRoundId}",
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
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                         clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
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

        private List<string> DoWin(Transaction transaction, decimal amount, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            var ids = new List<string>();
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    Document betDocument;
                    var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                    if (transaction.FreeSpinWin || transaction.JackpotWin)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = string.Format("{0}_{1}", transaction.TransactionId, transaction.FreeSpinWin ? "_FreeSpinBet" : "_JackpotWin"),
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
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue),
                                    client, clientSession.ParentId ?? 0, operationsFromProduct, betDocument, WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                    }
                    else
                    {
                        betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, transaction.GameRoundId, ProviderId, client.Id);
                        if (betDocument == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    }
                    var winDocument = documentBl.GetDocumentByExternalId($"{transaction.TransactionId}_{transaction.GameRoundId}", clientSession.Id, ProviderId,
                                                                         partnerProductSettingId, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            RoundId = transaction.GameRoundId,
                            ProductId = betDocument.ProductId,
                            TransactionId = $"{transaction.TransactionId}_{transaction.GameRoundId}",
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
                        ids.Add(winDocument.Id.ToString());
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, winDocument, WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                            }
                        }
                        if (transaction.PromoWinAmount > 0)
                        {
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                RoundId = transaction.GameRoundId,
                                ProductId = betDocument.ProductId,
                                TransactionId = $"{transaction.PromoWinReference}_{transaction.GameRoundId}",
                                CreditTransactionId = betDocument.Id,
                                State = (int)BetDocumentStates.Won,
                                OperationTypeId = (int)OperationTypes.Win,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = transaction.PromoWinAmount
                            });
                            var promowinDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            ids.Add(promowinDocument.Id.ToString());
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                    (betDocument == null ? (long?)null : betDocument.Id), operationsFromProduct, promowinDocument, WebApiApplication.DbLogger);
                                }
                                catch (Exception ex)
                                {
                                    WebApiApplication.DbLogger.Error("DebitException_" + ex.Message);
                                }
                            }
                        }
                        if (!isExternalPlatformClient)
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
                    else
                    {
                        ids.Add(winDocument.Id.ToString());
                    }
                    return ids;
                }
            }
        }

        private void CloseGameRound(Transaction input, BllClient client, SessionIdentity clientSession, int partnerProductSettingId, BllProduct product)
        {
            if (input.RoundCompleted)
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.GameRoundId, ProviderId,
                                                                            client.Id, (int)BetDocumentStates.Uncalculated);
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.GameRoundId,
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
                        foreach (var b in betDocuments)
                        {
                            b.State = (int)BetDocumentStates.Lost;
                            listOfOperationsFromApi.TransactionId = b.ExternalTransactionId;
                            listOfOperationsFromApi.CreditTransactionId = b.Id;
                            var doc = clientBl.CreateDebitsToClients(listOfOperationsFromApi, b, documentBl);
                            if (isExternalPlatformClient)
                            {
                                try
                                {
                                    ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                        (b == null ? (long?)null : b.Id), listOfOperationsFromApi, doc[0], WebApiApplication.DbLogger);
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
        }

        private string Rollback(string transactionId, BllClient client, BllProduct product, int operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = transactionId,
                    ExternalProductId = product.ExternalId,
                    ProductId = product.Id,
                    OperationTypeId = operationType
                };
                var documents = documentBl.RollbackProductTransactions(operationsFromProduct);
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
                else
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                return documents[0].Id.ToString();
            }
        }

        private string DoPromoWin(string transactionId, BllClient client, BllProduct product, decimal amount, int partnerProductSettingId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSettingId, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = transactionId + "_PromoBet",
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
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 0,
                                                                         operationsFromProduct, betDocument, WebApiApplication.DbLogger); //will not work without clientSession.ParentId 

                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;
                        operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            RoundId = transactionId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = transactionId,
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
    }
}