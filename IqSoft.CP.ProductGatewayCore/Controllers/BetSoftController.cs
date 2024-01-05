using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.BetSoft;
using IqSoft.CP.DAL.Models.Cache;
using Output = IqSoft.CP.ProductGateway.Models.BetSoft.Output;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    [Consumes("application/x-www-form-urlencoded")]
    public class BetSoftController : ControllerBase
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetSoft).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "87.233.234.7",
            "87.233.234.8",
            "141.0.173.104",
            "141.0.173.105"
        };

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/Authenticate")]
        public ActionResult Authenticate( [FromForm]AuthenticateInput input)
        {       
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    TOKEN = input.token,
                    HASH = input.hash,
                    CLIENTTYPE = input.clientType
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSoftPassKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}", input.token, passKey));
                if (hash != input.hash)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                response.Response = new ExtSystemResponse
                {
                    USERID = client.Id.ToString(),
                    CURRENCY = client.CurrencyId,
                    BALANCE = ((int)(balance * 100)).ToString()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/Balance")]
        public ActionResult GetBalance([FromForm] GetBalanceInput input)
        {
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    USERID = input.userId.ToString(),
                    TOKEN = input.token
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, clientSession.Id).AvailableBalance;
                response.Response = new ExtSystemResponse
                {
                    BALANCE = ((int)(balance * 100)).ToString()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/BetResult")]
        public ActionResult BetResult([FromForm]BetResultInput input)
        {
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    USERID = input.userId.ToString(),
                    TOKEN = input.token,
                    BET = input.bet,
                    WIN = input.win,
                    ROUNDID = input.roundId,
                    GAMEID = input.gameId,
                    ISROUNDFINISHED = input.isRoundFinished.ToString(),
                    HASH = input.hash,
                    GAMESESSIONID = input.gameSessionId,
                    NEGATIVEBET = input.negativeBet.ToString()
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                //// BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var hashSource = string.Format("{0}{1}{2}{3}{4}{5}", input.userId, input.bet, input.win,
                      input.isRoundFinished.ToString().ToLower(), input.roundId, input.gameId);
                var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSoftPassKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}", hashSource, passKey));
                if (hash != input.hash)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                if (string.IsNullOrWhiteSpace(input.bet) && string.IsNullOrWhiteSpace(input.win))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongParameters);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                Document document = null;
                if (!string.IsNullOrWhiteSpace(input.bet))
                    document = DoBet(input, clientSession, client, partnerProductSetting);
                if (!string.IsNullOrWhiteSpace(input.win))
                    document = DoWin(input, clientSession, client, partnerProductSetting, document != null ? document.Id : (long?)null);
                var balance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance;
                response.Response = new ExtSystemResponse
                {
                    EXTSYSTEMTRANSACTIONID = (document == null ? "0" : document.Id.ToString()),
                    BALANCE = ((int)(balance * 100)).ToString()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/RefundBet")]
        public ActionResult RefundBet([FromForm]RefundBetInput input)
        {
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    USERID = input.userId.ToString(),
                    TOKEN = input.token,
                    CASINOTRANSACTIONID = input.casinoTransactionId,
                    HASH = input.hash
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSoftPassKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}", input.userId, input.casinoTransactionId, passKey));
                if (hash != input.hash)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = clientSession.SessionId,
                        GameProviderId = ProviderId,
                        TransactionId = input.casinoTransactionId,
                        ExternalProductId = product.ExternalId,
                        ProductId = clientSession.ProductId
                    };
                    List<Document> documents = null;
                    try
                    {
                        documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                        if (documents == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.DocumentNotFound);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    catch (FaultException<BllFnErrorType> ex)
                    {
                        if (ex.Detail != null && ex.Detail.Id == Constants.Errors.DocumentAlreadyRollbacked)
                        {
                            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                            documents = new List<Document> { documentBl.GetDocumentByExternalId(input.casinoTransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet) };
                        }
                        else
                            throw;
                    }
                    response.Response = new ExtSystemResponse
                    {
                        EXTSYSTEMTRANSACTIONID = documents[0].Id.ToString()
                    };
                    return ReturnResponse(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/Account")]
        public ActionResult GetAccountInfo([FromForm] GetAccountInfoInput input)
        {
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    USERID = input.userId.ToString(),
                    HASH = input.hash
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                var client = CacheManager.GetClientById(input.userId);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSoftPassKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}", input.userId, passKey));
                if (hash != input.hash)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                response.Response = new ExtSystemResponse
                {
                    USERNAME = client.UserName,
                    FIRSTNAME = client.FirstName,
                    LASTNAME = client.LastName,
                    EMAIL = client.Email,
                    CURRENCY = client.CurrencyId
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        [HttpPost]
        [Route("{partnerId}/api/BetSoft/BonusWin")]
        public ActionResult BonusWin([FromForm] BonusWinInput input)
        {
            var response = new Output
            {
                Request = new ExtSystemRequest
                {
                    USERID = input.userId.ToString(),
                    BONUSID = input.bonusId.ToString(),
                    TRANSACTIONID = input.transactionId.ToString(),
                    HASH = input.hash
                }
            };
            try
            {
                Program.DbLogger.Info("Input:  " + JsonConvert.SerializeObject(input));
                // BaseBll.CheckIp(WhitelistedIps);
                var clientSession = ClientBll.GetClientProductSession(input.token, Constants.DefaultLanguageId);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client.Id != input.userId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                var passKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetSoftPassKey);
                var hash = CommonFunctions.ComputeMd5(string.Format("{0}{1}{2}{3}", input.userId, input.bonusId, input.amount, passKey));
                if (hash != input.hash)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongHash);
                var product = CacheManager.GetProductById(clientSession.ProductId);
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        var winDocument = documentBl.GetDocumentByExternalId(input.transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);

                        if (winDocument == null)
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                GameProviderId = ProviderId,
                                ProductId = clientSession.ProductId,
                                TransactionId = input.transactionId + "_bet",
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

                            var state = (input.amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;
                            operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.SessionId,
                                CurrencyId = client.CurrencyId,
                                RoundId = input.bonusId.ToString(),
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                ExternalOperationId = null,
                                ExternalProductId = product.ExternalId,
                                ProductId = betDocument.ProductId,
                                TransactionId = input.transactionId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                Info = string.Empty,
                                OperationItems = new List<OperationItemFromProduct>()
                            };
                            operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                            {
                                Client = client,
                                Amount = input.amount / 100m
                            });

                            winDocument = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl)[0];
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                            {
                                GameName = product.NickName,
                                ClientId = client.Id,
                                ClientName = client.FirstName,
                                Amount = Convert.ToDecimal(input.amount / 100m),
                                CurrencyId = client.CurrencyId,
                                PartnerId = client.PartnerId,
                                ProductId = product.Id,
                                ProductName = product.NickName,
                                ImageUrl = product.WebImageUrl
                            });                           
                        }
                    }
                    response.Response = new ExtSystemResponse
                    {
                        BALANCE = ((int)(BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100)).ToString()
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.GetResponseStatus(ex.Detail.Id)
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex.Message);
                response.Response = new ExtSystemResponse
                {
                    RESULT = BetSoftHelpers.ResponseResults.ErrorResponse,
                    CODE = BetSoftHelpers.Error.InternalError
                };
            }
            return ReturnResponse(response);
        }

        private static Document DoBet(BetResultInput input, SessionIdentity clientSession, BllClient client, BllPartnerProductSetting partnerProductSetting)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var betData = input.bet.Split('|');
                    var betAmount = Convert.ToDecimal(betData[0]) / 100;
                    var betTransactionId = betData[1];
                    var betDocument = documentBl.GetDocumentByExternalId(betTransactionId, clientSession.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = input.gameId,
                            ProductId = partnerProductSetting.ProductId,
                            TransactionId = betTransactionId,
                            RoundId = input.roundId,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = betAmount,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        betDocument = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastBalance(client.Id);
                    }
                    return betDocument;
                }
            }
        }

        private static Document DoWin(BetResultInput input, SessionIdentity clientSession, BllClient client, BllPartnerProductSetting partnerProductSetting, long? betDocumentId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var winData = input.win.Split('|');
                    var winTransactionId = winData[1];
                    Document betDocument = null;
                    
                    if (betDocumentId.HasValue)
                        betDocument = documentBl.GetDocumentById(betDocumentId.Value);
                    else
                        betDocument = documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, input.roundId, partnerProductSetting.ProviderId, client.Id, (int)BetDocumentStates.Uncalculated);
                    if (betDocument == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winDocument = documentBl.GetDocumentByExternalId(winTransactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var winAmount = (Convert.ToDecimal(winData[0]) + input.negativeBet) / 100;
                        int state = winAmount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = input.roundId,
                            OperationTypeId = (int)OperationTypes.Win,
                            GameProviderId = ProviderId,
                            ProductId = clientSession.ProductId,
                            ExternalProductId = input.gameId,
                            State = state,
                            TransactionId = winTransactionId,
                            CreditTransactionId = betDocument.Id,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        var operationItem = new OperationItemFromProduct
                        {
                            Amount = winAmount,
                            Client = client,
                        };
                        operationsFromProduct.OperationItems.Add(operationItem);
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        var product = CacheManager.GetProductById(clientSession.ProductId);
                        BaseHelpers.BroadcastWin(new Common.Models.WebSiteModels.ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = winAmount,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                    }                
                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                    return winDocument;
                }
            }
        }

        private ActionResult ReturnResponse(Output response)
        {
            response.TIME = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
            var xml = SerializeAndDeserialize.SerializeToXmlWithoutRoot<Output>(response);
            Program.DbLogger.Info("Output:  " + xml);
            return Ok(xml);
        }
    }
}