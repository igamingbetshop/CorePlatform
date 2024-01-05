using IqSoft.CP.ProductGateway.Models.EveryMatrix;
using IqSoft.CP.ProductGateway.Helpers;
using System.Collections.Generic;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System;
using System.Net.Http;
using System.Web.Http;
using IqSoft.CP.Common.Models.CacheModels;
using System.ServiceModel;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Clients;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using IqSoft.CP.Common.Models.Document;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class EveryMatrixController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.EveryMatrix).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.EveryMatrix);

        [HttpPost]
        [Route("{partnerId}/api/EveryMatrix/ApiRequest")]
        public HttpResponseMessage ApiRequest(BaseInput input)
        {
            WebApiApplication.DbLogger.Info("EM_Input:" + JsonConvert.SerializeObject(input));
            var currentTime = DateTime.UtcNow;
            var jsonResponse = string.Empty;
            var baseOutput = new BaseOutput
            {
                Request = input.Request,
                SessionId = input.SessionId,
                ReturnCode = (int)EveryMatrixHelpers.ReturnCodes.Success
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                SessionIdentity clientSession = null;
                BllClient client = null;
                if (input.Request == EveryMatrixHelpers.Methods.GetBalance && !input.ValidateSession  &&
                    string.IsNullOrEmpty(input.SessionId) && int.TryParse(input.ExternalUserId, out int clientId))
                {
                    client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var balance = BaseHelpers.GetClientProductBalance(client.Id, 0);
                    var balanceObject = new BalanceOutput(baseOutput)
                    {
                        Balance = balance,
                        BonusMoney = 0m,
                        RealMoney = balance,
                        Currency = client.CurrencyId
                    };
                    jsonResponse = JsonConvert.SerializeObject(balanceObject);
                }
                else
                {
                    clientSession = ClientBll.GetClientProductSession(input.SessionId, Constants.DefaultLanguageId, null, input.ValidateSession);
                    client = CacheManager.GetClientById(Convert.ToInt32(clientSession.Id));
                    var login = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EveryMatrixLogin);
                    var pass = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.EveryMatrixPassword);
                    if (input.LoginName != login || input.Password != pass)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
                    switch (input.Request)
                    {
                        case EveryMatrixHelpers.Methods.GetAccount:
                            var region = CacheManager.GetRegionByCountryCode(clientSession.Country, clientSession.LanguageId);
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
                            break;
                        case EveryMatrixHelpers.Methods.GetBalance:
                            var balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
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
                            var debitObject = new TransactionOutput(baseOutput)
                            {
                                Currency = client.CurrencyId,
                                BonusMoneyAffected = 0m,
                                RealMoneyAffected = input.Amount
                            };
                            if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Bet.ToString())
                            {
                                var doc = DoBet(input, clientSession, client);
                                debitObject.AccountTransactionId = doc.Id.ToString();
                                if (!string.IsNullOrEmpty(doc.Info))
                                {
                                    var info = JsonConvert.DeserializeObject<DocumentInfo>(doc.Info);
                                    debitObject.BonusMoneyAffected = info.BonusAmount;
                                    debitObject.RealMoneyAffected -= debitObject.BonusMoneyAffected;
                                }
                            }
                            else if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Rollback.ToString())
                                debitObject.AccountTransactionId = RollbackTransaction(input, clientSession.ProductId, client, OperationTypes.Win);
                            else
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ObjectTypeNotFound);
                            debitObject.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);

                            jsonResponse = JsonConvert.SerializeObject(debitObject);
                            break;
                        case EveryMatrixHelpers.Methods.WalletCredit:
                            var creditObject = new TransactionOutput(baseOutput)
                            {
                                Currency = client.CurrencyId,
                                BonusMoneyAffected = 0m,
                                RealMoneyAffected = input.Amount
                            };
                            if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Win.ToString())
                            {
                                var doc = DoWin(input, client, clientSession.ProductId, out DAL.Document betDocument);
                                creditObject.AccountTransactionId = doc.Id.ToString();
                                if (!string.IsNullOrEmpty(betDocument.Info))
                                {
                                    var info = JsonConvert.DeserializeObject<DocumentInfo>(betDocument.Info);
                                    creditObject.BonusMoneyAffected = input.Amount;
                                    creditObject.RealMoneyAffected -= creditObject.BonusMoneyAffected;
                                }
                            }
                            else if (input.TransactionType.ToLower() == EveryMatrixHelpers.TransactionTypes.Rollback.ToString())
                                creditObject.AccountTransactionId = RollbackTransaction(input, clientSession.ProductId, client, OperationTypes.Bet);
                            else
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ObjectTypeNotFound);
                            creditObject.Balance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);

                            jsonResponse = JsonConvert.SerializeObject(creditObject);
                            break;
                        default:
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                baseOutput.ReturnCode = EveryMatrixHelpers.GetErrorCode(fex.Detail.Id);
                baseOutput.Message = fex.Detail.Message;
                jsonResponse = JsonConvert.SerializeObject(baseOutput);
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
            }
            catch (Exception ex)
            {
                baseOutput.ReturnCode = EveryMatrixHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
                jsonResponse = JsonConvert.SerializeObject(baseOutput);
                WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_" + ex);
            }
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return resp;
        }
        private DAL.Document DoBet(BaseInput input, SessionIdentity clientSession, BllClient client)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductById(clientSession.ProductId);
                    // var product = CacheManager.GetProductByExternalId(ProviderId, input.EMGameId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    var document = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var payload = string.IsNullOrEmpty(input.PayLoad) ? null : JsonConvert.DeserializeObject<BetPayload>(input.PayLoad);

                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.RoundId,
                            ExternalProductId = product.ExternalId,
                            GameProviderId = ProviderId,
                            ProductId = product.Id,
                            TransactionId = input.TransactionId,
                            TicketInfo = payload == null || payload.combination == null ? null : JsonConvert.SerializeObject(payload.ToTicketInfo(input.Amount)),
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = input.Amount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        var doc = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                        BaseHelpers.BroadcastBetLimit(info);
                        if (input.RoundStatus.ToLower() == "close" || input.RoundStatus.ToLower() == "closed")
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
                            operationsFromApi.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = 0
                            });
                            foreach (var betDoc in betDocuments)
                            {
                                betDoc.State = (int)BetDocumentStates.Lost;
                                operationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                                operationsFromApi.CreditTransactionId = betDoc.Id;
                                clientBl.CreateDebitsToClients(operationsFromApi, betDoc, documentBl);
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                        return doc;
                    }
                    return document;
                }
            }
        }

        private DAL.Document DoWin(BaseInput input, BllClient client, int productId, out DAL.Document betDocument)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var product = CacheManager.GetProductById(productId);

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);

                    lock (string.Intern(new StringBuilder().Append(ProviderId).Append(input.RoundId).Append(client.Id).Append((int)OperationTypes.Bet).ToString()))
                    {
                        if (!string.IsNullOrEmpty(input.AdditionalData.BonusId) && int.TryParse(input.AdditionalData.BonusId, out int bonusId))
                        {
                            var clientBonus = clientBl.GetClientBonusById(bonusId) ??
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.BonusNotFound);
                            input.TransactionId = $"FreeSpin_{input.TransactionId}";
                            var listOfOperationsFromApi = new ListOfOperationsFromApi
                            {
                                CurrencyId = client.CurrencyId,
                                RoundId = input.RoundId,
                                ExternalProductId = product.ExternalId,
                                GameProviderId = ProviderId,
                                ProductId = product.Id,
                                TransactionId = input.TransactionId,
                                OperationItems = new List<OperationItemFromProduct> { new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                }}
                            };
                            betDocument = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                            var st = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                            betDocument.State = st;
                            listOfOperationsFromApi.OperationTypeId = (int)OperationTypes.Win;
                            listOfOperationsFromApi.CreditTransactionId = betDocument.Id;
                            listOfOperationsFromApi.State = st;
                            listOfOperationsFromApi.OperationItems = new List<OperationItemFromProduct>{ new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.Amount
                            } };
                            clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDocument, documentBl);
                        }
                        else
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id, null);
                            betDocument = betDocuments.FirstOrDefault(x => x.State == (int)BetDocumentStates.Uncalculated);
                            if (betDocument == null)
                                betDocument = betDocuments.FirstOrDefault();
                            if (betDocument == null)
                            {
                                if (input.AdditionalData?.GameSlug == "sports-betting")
                                {
                                    var listOfOperationsFromApi = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        RoundId = input.RoundId,
                                        ExternalProductId = product.ExternalId,
                                        GameProviderId = ProviderId,
                                        ProductId = product.Id,
                                        TransactionId = $"FreeBet_{input.TransactionId}",
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    listOfOperationsFromApi.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = 0
                                    });
                                    betDocument = clientBl.CreateCreditFromClient(listOfOperationsFromApi, documentBl, out LimitInfo info);
                                }
                                else
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                            }
                        }
                        var state = input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        var winDocument = documentBl.GetDocumentByExternalId(input.TransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var payload = string.IsNullOrEmpty(input.PayLoad) ? null : JsonConvert.DeserializeObject<BetPayload>(input.PayLoad);
                            if (payload != null && payload.status == "CASHED_OUT")
                                state = (int)BetDocumentStates.Cashouted;
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
                            if (listOfOperationsFromApi.State == (int)BetDocumentStates.Cashouted && !string.IsNullOrEmpty(betDocument.Info))
                            {
                                try
                                {
                                    var info = JsonConvert.DeserializeObject<DocumentInfo>(betDocument.Info);
                                    if (info != null && info.BonusId > 0)
                                        documentBl.RevertClientBonusBet(info, betDocument.ClientId.Value, string.Empty, betDocument.Amount);
                                }
                                catch { }
                            }
                            if (input.RoundStatus.ToLower() == "close" || input.RoundStatus.ToLower() == "closed")
                            {
                                var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId,
                                                                                    client.Id, (int)BetDocumentStates.Uncalculated);
                                var operationsFromApi = new ListOfOperationsFromApi
                                {
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.RoundId,
                                    GameProviderId = ProviderId,
                                    ProductId = product.Id,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromApi.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                });
                                foreach (var betDoc in betDocuments)
                                {
                                    betDoc.State = (int)BetDocumentStates.Lost;
                                    operationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                                    operationsFromApi.CreditTransactionId = betDoc.Id;
                                    clientBl.CreateDebitsToClients(operationsFromApi, betDoc, documentBl);
                                }
                            }
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
                            return doc[0];
                        }

                        return winDocument;
                    }
                }
            }
        }

        private string RollbackTransaction(BaseInput input, int productId, BllClient client, OperationTypes operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var product = CacheManager.GetProductById(productId);
                var betDocuments = documentBl.GetDocumentsByRoundId((int)operationType, input.RoundId, ProviderId, client.Id, null);
                if (betDocuments.Count == 1 && string.IsNullOrEmpty(input.RollbackTransactionId))
                    input.RollbackTransactionId = betDocuments[0].ExternalTransactionId;

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