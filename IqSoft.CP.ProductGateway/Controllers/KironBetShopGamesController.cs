using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.Common;
using IqSoft.CP.ProductGateway.Models.Kiron;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class KironBetShopGamesController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.Kiron).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.Kiron);

        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/activateSession")]
        public HttpResponseMessage CheckSession(BaseInput input)
        {
            var response = new AuthenticationOutput();
            try
            {
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    BaseBll.CheckIp(WhitelistedIps);


                    var session = userBl.GetUserSession(input.PlayerToken, false);
                    if (!session.CashDeskId.HasValue)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongToken);
                    var cashDesk = CacheManager.GetCashDeskById(session.CashDeskId.Value);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);

                    response.PlayerID = betShop.Id.ToString();
                    response.CurrencyCode = betShop.CurrencyId;
                    response.LanguageCode = session.LanguageId;
                }

            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/getBalance")]
        public HttpResponseMessage GetBalance(GetBalanceInput input)
        {
            var response = new BalanceOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var userSession = CheckUserSession(input.PlayerToken, true);
                var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                response.Amount = betShop.CurrentLimit;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }
        
        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/debit")]
        public HttpResponseMessage DoBet(DebitInput input)
        {
            var response = new TransactionOutput();
            try
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Info($" InputString: {bodyStream.ReadToEnd()}");

                BaseBll.CheckIp(WhitelistedIps);
                var userSession = CheckUserSession(input.PlayerToken, true);
                var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);

                using (var betShopBl = new BetShopBll(userSession, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(betShopBl))
                    {
                        BllProduct product;
                        if (input.GameIds != null && input.GameIds.Count != 0)
                        {
                            product = CacheManager.GetProductByExternalId(ProviderId, input.GameIds[0].ToString());
                            if (product == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        }
                        else
                            product = CacheManager.GetProductById(userSession.ProductId);

                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(betShop.PartnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                        var betDocument = betShopBl.GetDocumentByExternalId(input.BetManTransactionID, product.Id,
                              ProviderId, (int)OperationTypes.Bet);
                        if (betDocument != null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            CurrencyId = betShop.CurrencyId,
                            GameProviderId = ProviderId,
                            SessionId = userSession.SessionId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            TransactionId = input.BetManTransactionID,
                            RoundId = input.RoundID,
                            OperationItems = new List<OperationItemFromProduct>
                            {
                                new OperationItemFromProduct
                                {
                                    CashierId = userSession.Id,
                                    CashDeskId = userSession.CashDeskId,
                                    Amount = input.Amount
                                }
                            }
                        };
                        var document = betShopBl.CreateBetsFromBetShop(operationsFromProduct, documentBl);
                        response.TransactionID = document.Documents[0].Id.ToString();
                        BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                        var placeBetOutput = new PlaceBetOutput
                        {
                            CashierId = document.Documents[0].UserId.Value,
                            Bets = document.Documents.Select(x => new BetOutput
                            {
                                Id = x.Id,
                                BetAmount=  x.Amount,
                                Barcode = x.Barcode,
                                BetDate = x.CreationTime,
                                TicketNumber = x.TicketNumber??0,
                                GameId = x.ProductId.Value,
                                GameName = CacheManager.GetProductById(x.ProductId.Value).Name,
                                TypeId = x.TypeId ?? 0,
                                NumberOfBets = 1,
                                BetSelections = input.GameIds?.Select(y => new BllBetSelection
                                {
                                    EventDate = DateTime.UtcNow,
                                    RoundId = x.RoundId,
                                    SelectionId = y,
                                    SelectionName = CacheManager.GetProductByExternalId(ProviderId, y.ToString()).Description
                                }).ToList()
                            }).ToList()
                        };
                        BaseHelpers.BroadcastBetShopBet(placeBetOutput);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }
      
        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/credit")]
        public HttpResponseMessage DoWin(CreditInput input)
        {
            var response = new TransactionOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var userSession = CheckUserSession(input.PlayerToken, false);
                var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);

                using (var betShopBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(betShopBl))
                    {
                        var betDocument = documentBl.GetDocumentById(Convert.ToInt64(input.PreviousTransactionID));
                        if (betDocument == null || betDocument.OperationTypeId != (int)OperationTypes.Bet)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                        var winDocument = betShopBl.GetDocumentByExternalId(input.BetManTransactionID, betDocument.ProductId.Value, ProviderId, (int)OperationTypes.Win);
                        if (winDocument == null)
                        {
                            var product = CacheManager.GetProductById(betDocument.ProductId.Value);
                            var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                            betDocument.State = state;

                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = userSession.SessionId,
                                CurrencyId = betShop.CurrencyId,
                                RoundId = input.RoundID,
                                GameProviderId = ProviderId,
                                OperationTypeId = (int)OperationTypes.Win,
                                TransactionId = input.BetManTransactionID,
                                ExternalProductId = product.ExternalId,
                                ProductId = betDocument.ProductId,
                                CreditTransactionId = betDocument.Id,
                                State = state,
                                OperationItems = new List<OperationItemFromProduct>
                                {
                                    new OperationItemFromProduct
                                    {
                                        CashierId = userSession.Id,
                                        CashDeskId = userSession.CashDeskId,
                                        Amount = input.Amount
                                    }
                                }
                            };
                            var winDocuments = betShopBl.CreateWinsToBetShop(operationsFromProduct, documentBl);
                            response.TransactionID = winDocuments.Documents[0].Id.ToString();
                        }
                        else
                            response.TransactionID = winDocument.Id.ToString();
                    }
                }
                BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                WebApiApplication.DbLogger.Error(fex);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                WebApiApplication.DbLogger.Error(ex);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }
      
        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/finalizeRound")]
        public HttpResponseMessage FinalizeRound(FinalizeRoundInput input)
        {
            var response = new BaseOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var userSession = CheckUserSession(input.PlayerToken, true);
                var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                using (var betShopBl = new BetShopBll(userSession, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(betShopBl))
                    {
                        var betDocuments = betShopBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundID, ProviderId,
                                                                        userSession.Id, (int)BetDocumentStates.Uncalculated);
                        var listOfOperationsFromApi = new ListOfOperationsFromApi
                        {
                            State = (int)BetDocumentStates.Lost,
                            SessionId = userSession.SessionId,
                            CurrencyId = betShop.CurrencyId,
                            RoundId = input.RoundID,
                            GameProviderId = ProviderId,
                            OperationItems = new List<OperationItemFromProduct>
                                {
                                    new OperationItemFromProduct
                                    {
                                        CashierId = userSession.Id,
                                        CashDeskId = userSession.CashDeskId,
                                        Amount = 0
                                    }
                                }
                        };
                        foreach (var betDoc in betDocuments)
                        {
                            betDoc.State = (int)BetDocumentStates.Lost;
                            listOfOperationsFromApi.TransactionId = betDoc.ExternalTransactionId;
                            listOfOperationsFromApi.CreditTransactionId = betDoc.Id;
                            listOfOperationsFromApi.ProductId = betDoc.ProductId;
                            betShopBl.CreateWinsToBetShop(listOfOperationsFromApi, documentBl);
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/kironbetshopgames/rollback")]
        public HttpResponseMessage Rollback(DebitInput input)
        {
            var response = new TransactionOutput();
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Bet,
                        TransactionId = input.BetManTransactionID
                    };
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct, false);
                    response.TransactionID = doc[0].Id.ToString();
                    var cashDesk = CacheManager.GetCashDeskById(doc[0].CashDeskId.Value);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Code = KironHelpers.GetErrorCode(fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id);
                response.Status = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.Code = KironHelpers.GetErrorCode(Constants.Errors.GeneralException);
                response.Status = ex.Message;
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response)),
            };
        }
        private SessionIdentity CheckUserSession(string token, bool checkExpiration)
        {
            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var session = userBl.GetUserSession(token, checkExpiration);
                if (session.CashDeskId == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);

                var user = CacheManager.GetUserById(session.UserId.Value);
                var cashDesk = CacheManager.GetCashDeskById(session.CashDeskId.Value);

                var userIdentity = new SessionIdentity
                {
                    LanguageId = session.LanguageId,
                    LoginIp = session.Ip,
                    PartnerId = user.PartnerId,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId.Value,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = false,
                    CashDeskId = session.CashDeskId.Value,
                    BetShopId = cashDesk.BetShopId
                };
                return userIdentity;
            }
        }
    }
}