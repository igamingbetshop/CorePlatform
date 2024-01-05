using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using System.Collections.Generic;
using IqSoft.CP.ProductGateway.Models.JackpotGaming;
using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Services;
using System.ServiceModel;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class JackpotGamingController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.JackpotGaming).Id;

        [HttpGet]
        [Route("{partnerId}/api/JackpotGaming/wallets")]
        public ActionResult Authenticate([FromQuery] BalanceInput input)
        {
            try
            {
                if (!Request.Headers.ContainsKey("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = Request.Headers["token"];
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Currency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                return Ok(new BalanceOutput()
                {
                    UserId = client.Id.ToString(),
                    Balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance,
                    Currency = client.CurrencyId,
                    WalletId = client.Id.ToString(),
                    Data = new Data
                    {
                        User = new User
                        {
                            Username = client.UserName,
                            Name = client.FirstName ?? client.UserName,
                            Lastname = client.LastName ?? client.UserName,
                            Email = client.Email,
                            Status = client.State == (int)ClientStates.Active
                        }
                    }
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail != null ? fex.Detail.Message : fex.Message);
                return NotFound(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/wallet/{walletId}/debit")]
        public ActionResult Debit(DebitInput input)
        {
            try
            {
                if (!Request.Headers.ContainsKey("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = Request.Headers["token"];
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.Details.Request.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Details.Request.AmountCurrency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.Details.Request.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                if (product.Id != clientSession.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.TransactionType.ToUpper() == "ROLLBACK")
                {
                    var docId = RollbackTransaction(input, product.Id, client, OperationTypes.Bet);
                    return Ok( JsonConvert.SerializeObject(new DebitOutput
                    {
                        TransactionId = docId,
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Debit = input.Details.Request.Amount,
                        Message = "debited"
                    }));
                }
                if (input.TransactionType.ToUpper() == "BET")
                {
                    using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                    using var clientBl = new ClientBll(documentBl);
                    var document = documentBl.GetDocumentByExternalId(input.Details.Request.TransactionId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        RoundId = input.Reference,
                        ExternalProductId = product.ExternalId,
                        GameProviderId = ProviderId,
                        ProductId = product.Id,
                        TransactionId = input.Details.Request.TransactionId,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = input.Details.Request.Amount,
                        DeviceTypeId = clientSession.DeviceType
                    });
                    document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastBalance(client.Id);

                    return Ok(new DebitOutput
                    {
                        TransactionId = document.Id.ToString(),
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Debit = input.Details.Request.Amount,
                        Message = "debited"
                    });
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail != null ? fex.Detail.Message : fex.Message);
                return NotFound(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return NotFound(ex.Message);
            }
            return BadRequest();
        }


        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/wallet/{walletId}/credit")]
        public ActionResult Credit(DebitInput input)
        {
            try
            {
                if (!Request.Headers.ContainsKey("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = Request.Headers["token"];
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId, checkExpiration: false);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.Details.Request.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (input.Details.Request.AmountCurrency != client.CurrencyId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                var product = CacheManager.GetProductByExternalId(ProviderId, input.Details.Request.GameId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                if (product.Id != clientSession.ProductId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                if (input.TransactionType.ToUpper() == "ROLLBACK")
                {
                    var docId = RollbackTransaction(input, product.Id, client, OperationTypes.Bet);
                    return Ok(JsonConvert.SerializeObject(new DebitOutput
                    {
                        TransactionId = docId,
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Debit = input.Details.Request.Amount,
                        Message = "debited"
                    }));
                }
                if (input.TransactionType.ToUpper() == "WIN")
                {
                    using var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger);
                    using var clientBl = new ClientBll(documentBl);
                    var betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.Reference, ProviderId,
                                                                     client.Id, (int)BetDocumentStates.Uncalculated);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winDocument = documentBl.GetDocumentByExternalId(input.Details.Request.TransactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.DocumentAlreadyWinned);

                    var state = input.Details.Request.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        CurrencyId = client.CurrencyId,
                        GameProviderId = ProviderId,
                        RoundId = input.Reference,
                        ProductId = betDocument.ProductId,
                        TransactionId = input.Details.Request.TransactionId,
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        OperationItems = new List<OperationItemFromProduct>()
                    };
                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                    {
                        Client = client,
                        Amount = input.Details.Request.Amount
                    });
                    winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];

                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    BaseHelpers.BroadcastWin(new ApiWin
                    {
                        GameName = product.NickName,
                        ClientId = client.Id,
                        ClientName = client.FirstName,
                        Amount = input.Details.Request.Amount,
                        CurrencyId = client.CurrencyId,
                        PartnerId = client.PartnerId,
                        ProductId = product.Id,
                        ProductName = product.NickName,
                        ImageUrl = product.WebImageUrl
                    });
                    return Ok(new DebitOutput()
                    {
                        TransactionId = winDocument.Id.ToString(),
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, product.Id),
                        Currency = client.CurrencyId,
                        Credit = Convert.ToDecimal(input.Amount),
                        Message = "credited"
                    });
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail != null ? fex.Detail.Message : fex.Message);
                return NotFound(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return NotFound(ex.Message);
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("{partnerId}/api/JackpotGaming/status")]
        public ActionResult CloseRound(CloseRoundInput input)
        {
            try
            {
                if (!Request.Headers.ContainsKey("token"))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var token = Request.Headers["token"];
                if (string.IsNullOrEmpty(token))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (input.UserId != client.Id.ToString())
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product.GameProviderId != ProviderId || product.ExternalId != input.GameId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);

                if (input.RoundClose)
                {
                    using var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger);
                    using var documentBl = new DocumentBll(clientBl);
                    var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId,
                                                                    client.Id, (int)BetDocumentStates.Uncalculated);
                    var listOfOperationsFromApi = new ListOfOperationsFromApi
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
                        var doc = clientBl.CreateDebitsToClients(listOfOperationsFromApi, betDoc, documentBl);
                    }
                }
                return Ok(new DebitOutput()
                {
                    TransactionId = "0",
                    Balance = CacheManager.GetClientCurrentBalance(client.Id).AvailableBalance,
                    Currency = client.CurrencyId,
                    Message = "status"
                });
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(fex.Detail?.Message);
                return NotFound(new ErrorOutput
                {
                    Message = fex.Detail != null ? fex.Detail.Message : fex.Message
                });
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return NotFound(new ErrorOutput { Message = ex.Message });
            }
        }
        private string RollbackTransaction(DebitInput input, int productId, BllClient client, OperationTypes operationType)
        {
            using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
            {
                var product = CacheManager.GetProductById(productId);
                var operationsFromProduct = new ListOfOperationsFromApi
                {
                    GameProviderId = ProviderId,
                    TransactionId = input.Details.Request.TransactionId,
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
                    var document = documentBl.GetDocumentByExternalId(input.Reference, client.PartnerId, ProviderId,
                                                                      partnerProductSetting.Id, (int)operationType);
                    return document.Id.ToString();
                }
            }
        }
    }
}