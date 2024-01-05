//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.ServiceModel;
//using System.Text;
//using System.Web.Http;
//using System.Xml.Serialization;
//using IqSoft.CP.BLL.Caching;
//using IqSoft.CP.BLL.Services;
//using IqSoft.CP.Common;
//using IqSoft.CP.Common.Enums;
//using IqSoft.CP.Common.Helpers;
//using IqSoft.CP.Common.Models;
//using IqSoft.CP.DAL.Models;
//using IqSoft.CP.DAL.Models.Cache;
//using IqSoft.CP.ProductGateway.Helpers;
//using IqSoft.CP.ProductGateway.Models.BetGames;
//using IqSoft.CP.ProductGateway.Models.Common;
//using Newtonsoft.Json;
//using BaseInput = IqSoft.CP.ProductGateway.Models.BetGames.BaseInput;
//using BaseOutput = IqSoft.CP.ProductGateway.Models.BetGames.BaseOutput;
//using PaymentOutput = IqSoft.CP.ProductGateway.Models.BetGames.PaymentOutput;

//namespace IqSoft.CP.ProductGateway.Controllers
//{
//    public class BetGamesBetShopGamesController : ApiController
//    {
//        [HttpPost]
//        [Route("{partnerId}/api/BetGamesBetShopGames/ApiRequest")]
//        public HttpResponseMessage ApiRequest(int partnerId, HttpRequestMessage request)
//        {
//            request.Content.Headers.ContentType.MediaType = Constants.HttpContentTypes.ApplicationXml;
//            var serializer = new XmlSerializer(typeof(BetInput), new XmlRootAttribute("root"));

//            BetInput input;
//            var byteArray = request.Content.ReadAsByteArrayAsync().Result;
//            var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
//            using (var reader = new StringReader(responseString))
//            {
//                input = (BetInput)serializer.Deserialize(reader);
//            }
//            try
//            {
//                BaseOutput response;
//                switch (input.Method)
//                {
//                    case BetGamesHelpers.Methods.Ping:
//                        response = ConnectApi(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.GetAccountDetails:
//                        response = GetAccountDetails(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.RefreshToken:
//                        response = RefreshToken(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.RequestNewToken:
//                        response = RequestNewToken(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.GetBalance:
//                        response = GetBalance(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.DoBet:
//                        response = DoBet(partnerId, input);
//                        break;
//                    case BetGamesHelpers.Methods.DoWin:
//                        response = DoWin(partnerId, input);
//                        break;
//                    default:
//                        throw new ArgumentNullException(Constants.Errors.NotAllowed.ToString());
//                }
//                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
//            }
//            catch (FaultException<BllFnErrorType> ex)
//            {
//                WebApiApplication.DbLogger.Error(ex);
//                var betGamesError = BetGamesHelpers.ErrorCodesMapping.FirstOrDefault(x => x.Key == ex.Detail.Id);
//                var betGamesErrorCode = betGamesError.Equals(default(KeyValuePair<int, int>))
//                    ? ex.Detail.Id
//                    : betGamesError.Value;
//                var epochTime = CheckTime(input.Time);
//                var strOutputParams = string.Format(
//                    "method{0}token{1}success{2}error_code{3}error_text{4}time{5}{6}", input.Method, input.Token,
//                    BetGamesHelpers.Statuses.Fail, betGamesErrorCode, string.Empty, epochTime,
//                    GetBetGamesSecretKey(partnerId));

//                var response = new BaseOutput
//                {
//                    Method = input.Method,
//                    Token = input.Token,
//                    Success = BetGamesHelpers.Statuses.Fail,
//                    ErrorCode = betGamesErrorCode,
//                    ErrorText = string.Empty,
//                    Time = epochTime,
//                    Signature = CommonFunctions.ComputeMd5(strOutputParams)
//                };
//                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
//            }
//            catch (Exception ex)
//            {
//                WebApiApplication.DbLogger.Error(ex);
//                var response = new BaseOutput
//                {
//                    Method = input.Method,
//                    Token = input.Token,
//                    Success = BetGamesHelpers.Statuses.Fail,
//                    ErrorCode = Constants.Errors.GeneralException,
//                    ErrorText = ex.Message,
//                    Time = input.Time,
//                    Signature = input.Signature
//                };
//                return CommonFunctions.ConvertObjectToXml(response, Constants.HttpContentTypes.ApplicationXml);
//            }
//        }

//        private BaseOutput ConnectApi(int partnerId, BaseInput input)
//        {
//            var secretKey = GetBetGamesSecretKey(partnerId);
//            var strInputParams = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token,
//                input.Time, secretKey);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            var epochTime = CheckTime(input.Time);
//            var strOutputParams = string.Format(
//                "method{0}token{1}success{2}error_code{3}time{4}error_text{5}{6}", input.Method, input.Token,
//                BetGamesHelpers.Statuses.Success, 0, epochTime, string.Empty, secretKey);
//            var outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//            var response = new BaseOutput
//            {
//                Method = input.Method,
//                Token = input.Token,
//                Success = BetGamesHelpers.Statuses.Success,
//                ErrorCode = 0,
//                Time = epochTime,
//                Signature = outputSignature
//            };
//            return response;
//        }

//        private BaseOutput GetAccountDetails(int partnerId, BaseInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            string strInputParams = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token,
//                input.Time, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            int epochTime = CheckTime(input.Time);
//            var response = new AccountDetailsOutput
//            {
//                Method = input.Method,
//                Token = input.Token,
//                Success = BetGamesHelpers.Statuses.Success,
//                ErrorCode = 0,
//                ErrorText = string.Empty,
//                Time = epochTime
//            };
//            var identity = CheckUserSession(input.Token);

//            var cashDesk = CacheManager.GetCashDeskById(identity.CashDeskId);
//            var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
//            string strOutputParams =
//                string.Format(
//                    "method{0}token{1}success{2}error_code{3}error_text{4}time{5}user_id{6}username{7}currency{8}info{9}{10}",
//                    input.Method, input.Token, BetGamesHelpers.Statuses.Success, 0, string.Empty, epochTime,
//                    identity.Id, "bg_" + identity.Id, betShop.CurrencyId, "-", key);
//            string outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//            response.Parameters = new AccountDetailsOutputParams
//            {
//                UserId = identity.Id.ToString(),
//                UserName = "bg_" + identity.Id,
//                Currency = betShop.CurrencyId,
//                Info = "-"
//            };
//            response.Signature = outputSignature;
//            return response;
//        }

//        private BaseOutput RefreshToken(int partnerId, BetInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            var strInputParams = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token,
//                input.Time, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            int epochTime = CheckTime(input.Time);

//            CheckUserSession(input.Token);
//            var strOutputParams = string.Format(
//                "method{0}token{1}success{2}error_code{3}error_text{4}time{5}{6}", input.Method, input.Token,
//                BetGamesHelpers.Statuses.Success, 0, string.Empty, epochTime, key);
//            var outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//            var response = new BaseOutput
//            {
//                Method = input.Method,
//                Token = input.Token,
//                Success = BetGamesHelpers.Statuses.Success,
//                ErrorCode = 0,
//                ErrorText = string.Empty,
//                Time = epochTime,
//                Signature = outputSignature
//            };
//            return response;
//        }

//        private BaseOutput RequestNewToken(int partnerId, BetInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            string strInputParams = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token,
//                input.Time, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            var epochTime = CheckTime(input.Time);
//            var session = CheckUserSession(input.Token);
//            using (var userBl = new UserBll(session, WebApiApplication.DbLogger))
//            {
//                var outputSignature = "";
//                var newToken = "";
//                var newSession = userBl.RefreshUserSession(input.Token);
//                newToken = newSession.Token;
//                var strOutputParams =
//                    string.Format("method{0}token{1}success{2}error_code{3}error_text{4}time{5}new_token{6}{7}",
//                        input.Method, input.Token, BetGamesHelpers.Statuses.Success, 0, string.Empty, epochTime,
//                        newSession.Token, key);
//                outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//                var response = new NewTokenOutput
//                {
//                    Method = input.Method,
//                    Token = input.Token,
//                    Success = BetGamesHelpers.Statuses.Success,
//                    ErrorCode = 0,
//                    ErrorText = string.Empty,
//                    Time = epochTime,
//                    Parameters = new NewTokenOutputParams
//                    {
//                        NewToken = newToken
//                    },
//                    Signature = outputSignature
//                };
//                return response;
//            }
//        }

//        private BaseOutput GetBalance(int partnerId, BetInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            string strInputParams = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token,
//                input.Time, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            int epochTime = CheckTime(input.Time);
//            var session = CheckUserSession(input.Token);
//            using (var betShopBl = new BetShopBll(session, WebApiApplication.DbLogger))
//            {
//                var cashDesk = CacheManager.GetCashDeskById(session.CashDeskId);
//                if (cashDesk == null)
//                    throw BaseBll.CreateException(string.Empty, Constants.Errors.CashDeskNotFound);

//                var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
//                if (betShop == null)
//                    throw BaseBll.CreateException(string.Empty, Constants.Errors.BetShopNotFound);

//                var balance = Convert.ToInt32(betShop.CurrentLimit * 100);

//                var strOutputParams =
//                    string.Format("method{0}token{1}success{2}error_code{3}error_text{4}time{5}balance{6}{7}",
//                        input.Method, input.Token, BetGamesHelpers.Statuses.Success, 0, string.Empty, epochTime,
//                        balance, key);
//                string outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//                var response = new BalanceOutput
//                {
//                    Method = input.Method,
//                    Token = input.Token,
//                    Success = BetGamesHelpers.Statuses.Success,
//                    ErrorCode = 0,
//                    ErrorText = string.Empty,
//                    Time = epochTime,
//                    Parameters = new BalanceOutputParams
//                    {
//                        Balance = balance
//                    },
//                    Signature = outputSignature
//                };
//                return response;
//            }
//        }

//        private BaseOutput DoBet(int partnerId, BetInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            string strInputParams =
//                string.Format(
//                    "method{0}token{1}time{2}amount{3}currency{4}bet_id{5}transaction_id{6}retrying{7}bet{8}odd{9}bet_time{10}game{11}draw_code{12}draw_time{13}{14}",
//                    input.Method, input.Token, input.Time, input.Parameters.Amount,
//                    input.Parameters.Currency, input.Parameters.BetId, input.Parameters.TransactionId,
//                    input.Parameters.Retrying, input.Parameters.Bet, input.Parameters.Odd,
//                    input.Parameters.BetTime, input.Parameters.Game, input.Parameters.DrawCode,
//                    input.Parameters.DrawTime, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            var session = CheckUserSession(input.Token);
//            var cashDesk = CacheManager.GetCashDeskById(session.CashDeskId);
//            var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
//            int epochTime = CheckTime(input.Time);

//            var product =
//                CacheManager.GetProductByExternalId(
//                    CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id, input.Parameters.Game);

//            using (var betShopBl = new BetShopBll(session, WebApiApplication.DbLogger))
//            {
//                using (var documentBl = new DocumentBll(betShopBl))
//                {
//                    var operationsFromProduct = new ListOfOperationsFromApi
//                    {
//                        CurrencyId = betShop.CurrencyId,
//                        RoundId = input.Parameters.BetId.ToString(),
//                        GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id,
//                        ExternalOperationId = null,
//                        ProductId = product.Id,
//                        ExternalProductId = input.Parameters.Game,
//                        TransactionId = input.Parameters.TransactionId.ToString(),
//                        TypeId = (int)CreditDocumentTypes.Single,
//                        Info = input.Parameters.Odd.ToString(CultureInfo.InvariantCulture),
//                        OperationItems = new List<OperationItemFromProduct>
//                            {
//                                new OperationItemFromProduct
//                                {
//                                    CashierId = session.Id,
//                                    CashDeskId = session.CashDeskId,
//                                    Amount = (decimal) input.Parameters.Amount/100
//                                }
//                            }
//                    };

//                    var documents = betShopBl.CreateBetsFromBetShop(operationsFromProduct, documentBl);
//                    var balance = Convert.ToInt32(betShopBl.GetBetShopById(betShop.Id, false).CurrentLimit * 100);

//                    string strOutputParams =
//                        string.Format(
//                            "method{0}token{1}success{2}error_code{3}time{4}error_text{5}balance_after{6}already_processed{7}{8}",
//                            input.Method, input.Token, BetGamesHelpers.Statuses.Success, 0, epochTime, string.Empty,
//                            balance, 0, key);
//                    var outputSignature = CommonFunctions.ComputeMd5(strOutputParams);
//                    var response = new PaymentOutput
//                    {
//                        Method = input.Method,
//                        Token = input.Token,
//                        Success = BetGamesHelpers.Statuses.Success,
//                        ErrorCode = 0,
//                        Time = epochTime,
//                        Parameters = new PaymentOutputParams
//                        {
//                            BalanceAfter = balance,
//                            AlreadyProcessed = 0
//                        },
//                        Signature = outputSignature
//                    };
//                    SendTicketToBetShop(new BetShopBet
//                    {
//                        Token = session.Token,
//                        Bets = new List<BetOutput>
//                            {
//                                new BetOutput
//                                {
//                                    TransactionId = documents.Documents[0].Id.ToString(),
//                                    Barcode = documents.Documents[0].Barcode,
//                                    TicketNumber = documents.Documents[0].TicketNumber ?? 0,
//                                    GameId = documents.Documents[0].ProductId ?? 0,
//                                    GameName = product.NickName,
//                                    Amount = documents.Documents[0].Amount,
//                                    BetDate = documents.Documents[0].CreationTime,
//                                    Coefficient = input.Parameters.Odd,
//                                    BetType = (int) CreditDocumentTypes.Single,
//                                    BetSelections = new List<BllBetSelection>
//                                    {
//                                        new BllBetSelection
//                                        {
//                                            UnitName = product.NickName,
//                                            RoundName = product.NickName,
//                                            MarketName = input.Parameters.DrawCode,
//                                            SelectionName = input.Parameters.Bet,
//                                            Coefficient = input.Parameters.Odd,
//                                            EventDate = documents.Documents[0].CreationTime
//                                        }
//                                    }
//                                }
//                            },
//                        Balance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
//                                session.CashDeskId,
//                                betShop.CurrencyId).AvailableBalance,
//                        CurrentLimit = (decimal)balance / 100
//                    });
//                    return response;
//                }
//            }
//        }

//        private BaseOutput DoWin(int partnerId, BetInput input)
//        {
//            var key = GetBetGamesSecretKey(partnerId);
//            string strInputParams =
//                string.Format(
//                    "method{0}token{1}time{2}player_id{3}amount{4}currency{5}bet_id{6}transaction_id{7}retrying{8}{9}",
//                    input.Method, input.Token, input.Time, input.Parameters.PlayerId, input.Parameters.Amount,
//                    input.Parameters.Currency, input.Parameters.BetId, input.Parameters.TransactionId,
//                    input.Parameters.Retrying, key);
//            if (CommonFunctions.ComputeMd5(strInputParams).ToLower() != input.Signature)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
//            int epochTime = CheckTime(input.Time);
//            var gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id;
//            var response = new PaymentOutput { Parameters = new PaymentOutputParams() };

//            using (var betShopBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
//            {
//                using (var documentBl = new DocumentBll(betShopBl))
//                {
//                    var winDocument = betShopBl.GetDocumentByRoundId((int)OperationTypes.Win,
//                    input.Parameters.BetId.ToString(), gameProviderId, input.Parameters.PlayerId);
//                    int balance = 0;

//                    if (winDocument == null ||
//                        winDocument.ExternalTransactionId != input.Parameters.TransactionId.ToString())
//                    {
//                        var creditTransaction =
//                            betShopBl.GetDocumentByRoundId((int)OperationTypes.Bet,
//                                input.Parameters.BetId.ToString(), gameProviderId, input.Parameters.PlayerId);

//                        if (creditTransaction == null)
//                            throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

//                        var state = (input.Parameters.Amount > 0
//                            ? (int)DocumentStates.Won
//                            : (int)DocumentStates.Lost);

//                        creditTransaction.State = state;
//                        var cashDesk = CacheManager.GetCashDeskById(creditTransaction.CashDeskId.Value);
//                        var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
//                        if (winDocument == null)
//                        {
//                            var operationsFromProduct = new ListOfOperationsFromApi
//                            {
//                                CurrencyId = betShop.CurrencyId,
//                                RoundId = input.Parameters.BetId.ToString(),
//                                GameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id,
//                                ExternalOperationId = null,
//                                OperationTypeId = (int)OperationTypes.Win,
//                                ExternalProductId = input.Parameters.Game,
//                                ProductId = creditTransaction.ProductId,
//                                TransactionId = input.Parameters.TransactionId.ToString(),
//                                CreditTransactionId = creditTransaction.Id,
//                                State = state,
//                                Info = input.Parameters.Odd.ToString(CultureInfo.InvariantCulture),
//                                OperationItems = new List<OperationItemFromProduct>
//                                    {
//                                        new OperationItemFromProduct
//                                        {
//                                            CashierId = input.Parameters.PlayerId,
//                                            CashDeskId = cashDesk.Id,
//                                            Amount = (decimal) input.Parameters.Amount/100
//                                        }
//                                    }
//                            };
//                            betShopBl.CreateWinsToBetShop(operationsFromProduct, documentBl);
//                        }
//                        else
//                            response.Parameters.AlreadyProcessed = 1;

//                        balance = Convert.ToInt32(betShopBl.GetBetShopById(betShop.Id, false).CurrentLimit * 100);
//                    }
//                    else
//                    {
//                        response.Parameters.AlreadyProcessed = 1;
//                        var cashDesk = CacheManager.GetCashDeskById(winDocument.CashDeskId.Value);
//                        var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
//                        balance = Convert.ToInt32(betShopBl.GetBetShopById(betShop.Id, false).CurrentLimit * 100);
//                    }

//                    string strOutputParams =
//                        string.Format(
//                            "method{0}token{1}success{2}error_code{3}error_text{4}time{5}balance_after{6}already_processed{7}{8}",
//                            input.Method, input.Token, BetGamesHelpers.Statuses.Success, 0, string.Empty, epochTime,
//                            balance, response.Parameters.AlreadyProcessed, key);
//                    var outputSignature = CommonFunctions.ComputeMd5(strOutputParams);

//                    response.Method = input.Method;
//                    response.Token = input.Token;
//                    response.Success = BetGamesHelpers.Statuses.Success;
//                    response.ErrorCode = 0;
//                    response.ErrorText = string.Empty;
//                    response.Time = epochTime;
//                    response.Parameters.BalanceAfter = balance;
//                    response.Signature = outputSignature;
//                    return response;
//                }
//            }
//        }

//        private SessionIdentity CheckUserSession(string token, bool checkExpiration = true)
//        {
//            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
//            {
//                var session = userBl.GetUserSession(token, checkExpiration);
//                if (session.CashDeskId == null)
//                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.SessionNotFound);

//                var user = CacheManager.GetUserById(session.UserId);
//                var userIdentity = new SessionIdentity
//                {
//                    LanguageId = session.LanguageId,
//                    LoginIp = session.Ip,
//                    PartnerId = user.PartnerId,
//                    SessionId = session.Id,
//                    Token = session.Token,
//                    Id = session.UserId,
//                    CurrencyId = user.CurrencyId,
//                    IsAdminUser = false,
//                    CashDeskId = session.CashDeskId.Value
//                };
//                return userIdentity;
//            }
//        }

//        private int CheckTime(int time)
//        {
//            var epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
//            int epochTime = Convert.ToInt32(epoch);
//            if (epochTime - time >= 60)
//                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RequestExpired);
//            return epochTime;
//        }

//        private string GetBetGamesSecretKey(int partnerId)
//        {
//            var gameProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id;
//            var partnerKey = CacheManager.GetGameProviderValueByKey(partnerId, gameProviderId, Constants.PartnerKeys.BetGamesSecretKey);
//            if (partnerKey == null || partnerKey == string.Empty)
//                throw new ArgumentNullException(Constants.Errors.PartnerKeyNotFound.ToString());
//            return partnerKey;
//        }

//        public void SendTicketToBetShop(BetShopBet ticket)
//        {
//            try
//            {
//                var url = string.Format("{0}/{1}", WebApiApplication.BetShopConnectionUrl,
//                    "PrintExternalProductTicket");
//                var input = new HttpRequestInput
//                {
//                    Url = url,
//                    ContentType = Constants.HttpContentTypes.ApplicationJson,
//                    RequestMethod = Constants.HttpRequestMethods.Post,
//                    PostData = JsonConvert.SerializeObject(ticket)
//                };
//                CommonFunctions.SendHttpRequest(input);
//            }
//            catch (Exception e)
//            {
//                WebApiApplication.DbLogger.Error(e);
//            }
//        }
//    }
//}