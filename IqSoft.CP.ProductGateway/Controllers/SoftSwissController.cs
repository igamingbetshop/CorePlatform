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
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SoftSwiss;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Action = IqSoft.CP.ProductGateway.Models.SoftSwiss.Action;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class SoftSwissController : ApiController
    {

        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SoftSwiss).Id;
        private static readonly int EvolutionId = CacheManager.GetGameProviderByName(Constants.GameProviders.Evolution).Id;
        private static readonly int PragmaticId = CacheManager.GetGameProviderByName(Constants.GameProviders.PragmaticPlay).Id;
        private static readonly int EzugiId = CacheManager.GetGameProviderByName(Constants.GameProviders.Ezugi).Id;
        private static readonly int SpinomenalId = CacheManager.GetGameProviderByName(Constants.GameProviders.Spinomenal).Id;

        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SoftSwiss);
        private readonly char[] digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public static List<string> UnsuppordedCurrenies = new List<string>
        {
            Constants.Currencies.IranianTuman,
            Constants.Currencies.IranianRial
        };
        private static Regex Rgx = new Regex(@"\d+$");

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/play")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            var output = new BaseOutput();
            var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
            var inputString = bodyStream.ReadToEnd();
            try
            {
                if (string.IsNullOrEmpty(input.GameId))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);

                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var checkExpiration = input.Actions != null ? input.Actions.Any(x => x.ActionName == "bet") : !input.Finished;

                var pSession = CacheManager.GetClientPlatformSession(input.ClientId, null);
                var clientSessions = ClientBll.GetProductSessionsByParentId(pSession.Id, input.ClientId, checkExpiration);
                var clientSession = clientSessions.FirstOrDefault(x => x.ProductId == product.Id);
                if(clientSession == null)
                {
                    var gameData = input.GameId.Split(':');
                    if (product == null)
                    {
                        product = CacheManager.GetProductByExternalId(ProviderId, $"{gameData[0]}:{gameData[1].TrimEnd(digits)}");
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                    }
                    clientSession = clientSessions.FirstOrDefault(x => x.ProductId == product.Id);
                    if (clientSession == null)
                    {
                        int productId = 0;
                        switch(gameData[0])
                        {
                            case "evolution":
                                productId = EvolutionId;
                                break;
                            case "pragmaticexternal":
                                productId = PragmaticId;
                                break;
                            case "ezugi":
                                productId = EzugiId;
                                break;
                            case "spinomenal":
                                productId = SpinomenalId;
                                break;
                            default:
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                        }
                        foreach (var cs in clientSessions)
                        {
                            var p = CacheManager.GetProductById(cs.ProductId);
                            if (p.GameProviderId == ProviderId && p.SubProviderId == productId)
                            {
                                clientSession = cs;
                                break;
                            }
                        }

                        if (clientSession == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);

                        product = CacheManager.GetProductById(clientSession.ProductId);
                    }
                }
                var client = CacheManager.GetClientById(input.ClientId);
                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = HttpContext.Current.Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.RoundId = input.RoundId;
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));

                if (input.Actions != null)
                {
                    var transactions = new List<Transaction>();
                    using (var scope = CommonFunctions.CreateTransactionScope())
                    {
                        foreach (var ac in input.Actions)
                        {
                            if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                                ac.Amount = BaseBll.ConvertCurrency(Constants.Currencies.USADollar, client.CurrencyId, ac.Amount);
                            if (ac.ActionName == "bet")
                                transactions.Add(DoBet(input, ac, client, clientSession));
                            else if (ac.ActionName == "win")
                                transactions.Add(DoWin(input, ac, client, clientSession));
                        }
                        scope.Complete();
                    }
                    output.Transactions = transactions;
                }
                if (input.Finished)
                {
                    FinalizeRound(client, input.RoundId, product.Id, clientSession.Id);
                }
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                BaseHelpers.BroadcastBalance(client.Id);
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }

            var resp = new HttpResponseMessage
            {
                StatusCode = output.Code.HasValue ? HttpStatusCode.PreconditionFailed : HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            if (resp.StatusCode != HttpStatusCode.OK)
                WebApiApplication.DbLogger.Error("Status: " + resp.StatusCode + ", Request: " + inputString +
                    ",  Response: " + JsonConvert.SerializeObject(output,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));
            return resp;
        }

        private Transaction DoBet(BaseInput actionInput, Action betAction, BllClient client, DAL.ClientSession sessionIdentity)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(sessionIdentity.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(betAction.ActionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = sessionIdentity.Id,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = actionInput.GameId,
                            ProductId = product.Id,
                            TransactionId = betAction.ActionId,
                            RoundId = actionInput.RoundId.Replace("-", string.Empty),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = betAction.Amount / 100,
                            DeviceTypeId = sessionIdentity.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.SoftSwissRollback, betAction.ActionId);
                        if (!string.IsNullOrEmpty(rollback))
                        {
                            var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                            {
                                GameProviderId = ProviderId,
                                TransactionId = betAction.ActionId,
                                ExternalProductId = product.ExternalId,
                                ProductId = product.Id
                            };
                            try
                            {
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                            }
                            catch {; }
                        }
                    }
                    return new Transaction
                    {
                        ActionId = betAction.ActionId,
                        TxId = betDocument.Id.ToString()
                    };
                }
            }
        }

        private Transaction DoWin(BaseInput actionInput, Action winAction, BllClient client, DAL.ClientSession sessionIdentity)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductById(sessionIdentity.ProductId);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, actionInput.RoundId.Replace("-", string.Empty), ProviderId, client.Id, (int)BetDocumentStates.Uncalculated);

                    if (betDocument == null) 
                    {
                        var betOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = sessionIdentity.Id,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = actionInput.GameId,
                            ProductId = product.Id,
                            TransactionId =string.Format("bet_{0}", winAction.ActionId),
                            RoundId = actionInput.RoundId.Replace("-", string.Empty),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        betOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = 0
                        });
                        betDocument = clientBl.CreateCreditFromClient(betOperationsFromProduct, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                    }

                    var winDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Win, actionInput.RoundId.Replace("-", string.Empty), ProviderId, client.Id);

                    if (winDocument != null)
                        BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                    var state = (winAction.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                    betDocument.State = state;

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = sessionIdentity.Id,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalProductId = actionInput.GameId,
                        ProductId = betDocument.ProductId,
                        TransactionId = winAction.ActionId,
                        RoundId = actionInput.RoundId.Replace("-", string.Empty),
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = winAction.Amount / 100
                    });
                    var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                    var transaction = new Transaction
                    {
                        ActionId = winAction.ActionId,
                        TxId = doc[0].Id.ToString()
                    };
                    var rollback = CacheManager.GetFutureRollback(Constants.CacheItems.SoftSwissRollback, winAction.ActionId);
                    if (!string.IsNullOrEmpty(rollback))
                    {
                        var rollbackOperationsFromProduct = new ListOfOperationsFromApi
                        {
                            GameProviderId = ProviderId,
                            TransactionId = winAction.ActionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id
                        };
                        try
                        {
                            documentBl.RollbackProductTransactions(operationsFromProduct);
                        }
                        catch {; }
                    }
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastWin(new ApiWin
                    {
                        BetId = betDocument?.Id ?? 0,
                        GameName = product.NickName,
                        ClientId = client.Id,
                        ClientName = client.FirstName,
                        BetAmount = betDocument?.Amount,
                        Amount = winAction.Amount / 100,
                        CurrencyId = client.CurrencyId,
                        PartnerId = client.PartnerId,
                        ProductId = product.Id,
                        ProductName = product.NickName,
                        ImageUrl = product.WebImageUrl
                    });
                    return transaction;
                }
            }
        }

        private void FinalizeRound(BllClient client, string roundId, int productId, long sessionId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    roundId =roundId.Replace("-", string.Empty);
                    var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, roundId, ProviderId,
                                                                    client.Id, (int)BetDocumentStates.Uncalculated);
                    var listOfOperationsFromApi = new ListOfOperationsFromApi
                    {
                        SessionId = sessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = roundId,
                        GameProviderId = ProviderId,
                        ProductId = productId,
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
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/rollback")]
        public HttpResponseMessage Rollback(BaseInput input)
        {
            var output = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                var client = CacheManager.GetClientById(input.ClientId);
                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = HttpContext.Current.Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                output.RoundId = input.RoundId;
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var transactions = new List<Transaction>();
                        foreach (var ac in input.Actions)
                        {
                            if (ac.ActionName == "rollback")
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    GameProviderId = ProviderId,
                                    TransactionId = ac.OriginalActionId,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = product.Id
                                };
                                List<DAL.Document> documents = null;
                                try
                                {
                                    documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                                }
                                catch
                                {
                                    CacheManager.SetFutureRollback(Constants.CacheItems.SoftSwissRollback, ac.ActionId, ac.OriginalActionId);
                                }

                                if (documents != null)
                                    transactions.Add(new Transaction { ActionId = ac.ActionId, TxId = documents[0].Id.ToString() });
                                else
                                    transactions.Add(new Transaction { ActionId = ac.ActionId });

                            }
                        }
                        output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100);
                        if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                            output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                        output.RoundId = input.RoundId;
                        output.Transactions = transactions;
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }

            var resp = new HttpResponseMessage
            {
                StatusCode = output.Code.HasValue ? HttpStatusCode.PreconditionFailed : HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            if (resp.StatusCode != HttpStatusCode.OK)
                WebApiApplication.DbLogger.Error("Status: " + resp.StatusCode + ", Response: " + JsonConvert.SerializeObject(output,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));
            return resp;
        }

        [HttpPost]
        [Route("{partnerId}/api/SoftSwiss/freespins")]
        public HttpResponseMessage FreeSpin(FreeSpinInput input)
        {
            var output = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("InputString: " + JsonConvert.SerializeObject(input));
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                var inputString = bodyStream.ReadToEnd();
                var bonusData = input.BonusData.Split('_');
                var client = CacheManager.GetClientById(Convert.ToInt32(bonusData[0]));
                var clientBonusId = Convert.ToInt32(bonusData[1]);

                var authToken = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.SoftSwissAuthToken);
                string authHeader = HttpContext.Current.Request.Headers["X-REQUEST-SIGN"];
                var hashString = CommonFunctions.ComputeHMACSha256(inputString, authToken);
                if (hashString.ToLower() != authHeader.ToLower())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var clientBonus = clientBl.GetClientBonusById(clientBonusId);
                    switch (input.Status)
                    {
                        case "expired":
                            try
                            {
                                clientBl.CancelClientFreespin(clientBonusId, false);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Info("Expired: " + JsonConvert.SerializeObject(input) +  "  Error: " + ex.Message);
                            }
                            break;
                        case "played":
                            if (input.TotalAmount > 0 && clientBonus != null && clientBonus.Status == (int)ClientBonusStatuses.Finished)
                            {
                                DoFreespinWin(clientBonus, client, input.TotalAmount / 100);
                            }
                            break;
                    }
                }
                output.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, 0) * 100);
                if (UnsuppordedCurrenies.Contains(client.CurrencyId))
                    output.Balance = Convert.ToInt32(BaseBll.ConvertCurrency(client.CurrencyId, Constants.Currencies.USADollar, output.Balance.Value));
                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                output.Code = SoftSwissHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                output.Message = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                output.Code = SoftSwissHelpers.GetErrorCode(Constants.Errors.GeneralException);
                output.Message = ex.Message;
            }

            var resp = new HttpResponseMessage
            {
                StatusCode = output.Code.HasValue ? HttpStatusCode.PreconditionFailed : HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(output,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            if (resp.StatusCode != HttpStatusCode.OK)
                WebApiApplication.DbLogger.Error("Status: " + resp.StatusCode + ", Response: " + JsonConvert.SerializeObject(output,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }));
            return resp;
        }

        private void DoFreespinWin(DAL.ClientBonu bonus, BllClient client, decimal winAmount)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var product = CacheManager.GetProductByExternalId(ProviderId, "Freespin");
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var transactionId = $"{Constants.FreeSpinPrefix}{bonus.Id}_{product.Id}";
                    var winDocument = documentBl.GetDocumentByExternalId(transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = $"Bet_{transactionId}",
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

                        var state = winAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
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
                            Amount = winAmount
                        });
                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            BetId = betDocument?.Id ?? 0,
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            BetAmount = betDocument?.Amount,
                            Amount = winAmount,
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
    }
}