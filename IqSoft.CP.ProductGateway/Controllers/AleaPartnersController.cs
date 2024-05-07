using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using IqSoft.CP.ProductGateway.Models.AleaPartners;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using System.Text;
using System.IO;
using System.Web;
using IqSoft.CP.DAL;
using System.Net.Http.Headers;
using IqSoft.CP.ProductGateway.Models.Common;
using System.Linq;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class AleaPartnersController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.AleaPartners).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.AleaPartners);

        [HttpPost]
        [Route("{partnerId}/api/AleaPartners/getUserInfo")]
        [Route("{partnerId}/api/AleaPartners/getBalance")]
        public HttpResponseMessage Authenticate(BaseInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
                //BaseBll.CheckIp(WhitelistedIps);
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var userSession = userBl.CheckCashierSession(input.Token);
                    var user = CacheManager.GetUserById(userSession.Id);
                    var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                    baseOutput.Result = new AuthenticationOutput
                    {
                        Username = user.UserName.ToString(),
                        UserId = userSession.CashDeskId.ToString(),
                        UserUuid = user.Id.ToString(),
                        Balance = (int)betShop.CurrentLimit * 100,
                        CurrencyId = betShop.CurrencyId,
                        Language = userSession.LanguageId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Country = userSession.Country,
                        Email = user.Email,
                        LotCode1 = betShop.Id.ToString(),
                    };
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(baseOutput));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(fex.Detail?.Id ?? Constants.Errors.GeneralException);
                baseOutput.Message = fex.Detail?.Message ?? fex.Message;
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/AleaPartners/ticketPayin")]
        public HttpResponseMessage Credit(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var userSession = userBl.CheckCashierSession(input.Token);
                    var user = CacheManager.GetUserById(userSession.Id);
                    var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                    if (betShop.CurrencyId != input.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    if (user.Id.ToString() != input.UserId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId) ??
                       throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var betResult = DoBet(input.TransactionId, input.Amount/100, betShop.CurrencyId, user, product, userSession);
                    BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                    var ticketDetails = JsonConvert.DeserializeObject<List<TicketItem>>(input.TicketDetails);
                    var placeBetOutput = new PlaceBetOutput
                    {
                        CashierId = betResult.Documents[0].UserId.Value,
                        Bets = betResult.Documents.Select(x => new BetOutput
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
                            BetSelections = ticketDetails?.Select(y => new BllBetSelection
                            {
                                EventDate = Convert.ToDateTime(input.ReceivedDate),
                                EventInfo = y.Value,
                                RoundId = y.Event,
                                Coefficient = y.FutureCoefficient,
                                SelectionName = CacheManager.GetProductById(x.ProductId.Value).Name
                            }).ToList()
                        }).ToList()
                    };
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(placeBetOutput));
                    BaseHelpers.BroadcastBetShopBet(placeBetOutput);
                    baseOutput.Result = new TransactionResult
                    {
                        Balance = (int)betShop.CurrentLimit*100
                    };
                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(baseOutput));
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(fex.Detail?.Id ?? Constants.Errors.GeneralException);
                baseOutput.Message = fex.Detail?.Message ?? fex.Message;
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/AleaPartners/ticketListPayin")]
        public HttpResponseMessage BulkCredit(List<TransactionInput> input) // Check logs
        {
            var baseOutput = new BaseOutput();
            try
            {
                WebApiApplication.DbLogger.Info($"BulkCredit: {JsonConvert.SerializeObject(input)}");
           //     BaseBll.CheckIp(WhitelistedIps);
                var userSession = new SessionIdentity();
                var product = new BllProduct();
                var betShop = new BllBetShop();
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    foreach (var transaction in input)
                    {
                        userSession = userBl.CheckCashierSession(transaction.Token);
                        var user = CacheManager.GetUserById(userSession.Id);
                        betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                        if (betShop.CurrencyId != transaction.CurrencyId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        if (user.Id.ToString() != transaction.UserId)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        product = CacheManager.GetProductByExternalId(ProviderId, transaction.GameId) ??
                           throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                        var betResult = DoBet(transaction.TransactionId, transaction.Amount/100, betShop.CurrencyId, user, product, userSession);
                        BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                        var ticketDetails = JsonConvert.DeserializeObject<List<TicketItem>>(transaction.TicketDetails);
                        var placeBetOutput = new PlaceBetOutput
                        {
                            CashierId = betResult.Documents[0].UserId.Value,
                            Bets = betResult.Documents.Select(x => new BetOutput
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
                                BetSelections = ticketDetails?.Select(y => new BllBetSelection
                                {
                                    EventInfo = y.Value,
                                    RoundId = y.Event,
                                    Coefficient = y.FutureCoefficient,
                                    SelectionName = CacheManager.GetProductById(x.ProductId.Value).Name
                                }).ToList()
                            }).ToList()
                        };
                        WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(placeBetOutput));
                        BaseHelpers.BroadcastBetShopBet(placeBetOutput);
                    }
                    baseOutput.Result = new TransactionResult
                    {
                        Balance = (int)betShop.CurrentLimit*100
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(fex.Detail?.Id ?? Constants.Errors.GeneralException);
                baseOutput.Message = fex.Detail?.Message ?? fex.Message;
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/AleaPartners/ticketWon")]
        public HttpResponseMessage Debit(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
              //  BaseBll.CheckIp(WhitelistedIps);
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var userSession = userBl.CheckCashierSession(input.Token, false);
                    var user = CacheManager.GetUserById(userSession.Id);
                    var betShop = CacheManager.GetBetShopById(userSession.BetShopId.Value);
                    if (betShop.CurrencyId != input.CurrencyId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                    if (user.Id.ToString() != input.UserId)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId) ??
                       throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    DoWin(input.TransactionId, input.Amount/100, betShop.CurrencyId, user, userSession.CashDeskId, product);
                    BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                    baseOutput.Result = new TransactionResult
                    {
                        Balance = (int)betShop.CurrentLimit*100
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(fex.Detail?.Id ?? Constants.Errors.GeneralException);
                baseOutput.Message = fex.Detail?.Message ?? fex.Message;
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        [HttpPost]
        [Route("{partnerId}/api/AleaPartners/ticketRollback")]
        public HttpResponseMessage Rollback(TransactionInput input)
        {
            var baseOutput = new BaseOutput();
            try
            {
              //  BaseBll.CheckIp(WhitelistedIps);
                var user = CacheManager.GetUserById(Convert.ToInt32(input.UserId)) ??
                         throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.UserNotFound);  
                var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId) ??
                   throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                using (var userBl = new UserBll(documentBl))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = ProviderId,
                        TransactionId = input.TransactionId,
                        ProductId = product.Id,
                        OperationTypeId = input.RollbackType == "WIN" ? (int)OperationTypes.Win : (int)OperationTypes.Bet
                    };
                    var doc = documentBl.RollbackProductTransactions(operationsFromProduct)[0];
                    var cashDesk = CacheManager.GetCashDeskById(doc.CashDeskId.Value);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    BaseHelpers.RemoveBetshopFromeCache(betShop.Id);
                    baseOutput.Result = new TransactionResult
                    {
                        Balance = (int)betShop.CurrentLimit*100
                    };
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Code: {fex.Detail.Id} Message: {fex.Detail.Message} Input: {bodyStream.ReadToEnd()}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(fex.Detail?.Id ?? Constants.Errors.GeneralException);
                baseOutput.Message = fex.Detail?.Message ?? fex.Message;
            }
            catch (Exception ex)
            {
                var bodyStream = new StreamReader(HttpContext.Current.Request.InputStream);
                WebApiApplication.DbLogger.Error($"Error: {ex} InputString: {bodyStream.ReadToEnd()} Response: {ex}");
                baseOutput.Code = AleaPartnersHelpers.GetErrorCode(Constants.Errors.GeneralException);
                baseOutput.Message = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(baseOutput), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }

        private BetShopFinOperationsOutput DoBet(string transactionId, decimal amount, string currencyId, BllUser user, BllProduct product, SessionIdentity userSession)
        {
            using (var betShopBl = new BetShopBll(userSession, WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(betShopBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(user.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = betShopBl.GetDocumentByExternalId(transactionId, product.Id, ProviderId, (int)OperationTypes.Bet);
                    if (betDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        SessionId = userSession.SessionId,
                        CurrencyId = currencyId,
                        GameProviderId = ProviderId,
                        ExternalProductId = product.ExternalId,
                        ProductId = product.Id,
                        RoundId = transactionId,
                        TransactionId = transactionId,
                        TypeId = (int)CreditDocumentTypes.Single,
                        OperationItems = new List<OperationItemFromProduct>
                        {
                            new OperationItemFromProduct
                            {
                                CashierId = user.Id,
                                CashDeskId = userSession.CashDeskId,
                                Amount = amount
                            }
                        }
                    };
                    return betShopBl.CreateBetsFromBetShop(operationsFromProduct, documentBl);
                }
            }
        }

        private Document DoWin(string transactionId, decimal amount, string currencyId, BllUser user, int cashDeskId, BllProduct product)
        {
            using (var betShopBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(betShopBl))
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(user.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = betShopBl.GetDocumentByExternalId(transactionId, product.Id, ProviderId, (int)OperationTypes.Bet) ??
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winTransactionId = $"Win_{transactionId}";
                    var winDocument = betShopBl.GetDocumentByExternalId(winTransactionId, product.Id, ProviderId, (int)OperationTypes.Win);
                    if (winDocument != null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                    var state = amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost;
                    betDocument.State = state;
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        CurrencyId = currencyId,
                        RoundId = betDocument.RoundId,
                        GameProviderId = ProviderId,
                        OperationTypeId = (int)OperationTypes.Win,
                        ExternalProductId = product.ExternalId,
                        ProductId = betDocument.ProductId,
                        TransactionId = winTransactionId,
                        CreditTransactionId = betDocument.Id,
                        State = state,
                        Info = string.Empty,
                        OperationItems = new List<OperationItemFromProduct>
                        {
                            new OperationItemFromProduct
                            {
                                CashierId = user.Id,
                                CashDeskId = cashDeskId,
                                Amount = amount
                            }
                        }
                    };
                  return betShopBl.CreateWinsToBetShop(operationsFromProduct, documentBl).Documents[0];
                }
            }
        }
    }
}