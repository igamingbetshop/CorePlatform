using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.BetGames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Xml.Serialization;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Helpers;
using System.Net;
using System.Net.Http.Headers;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels;
using System.Collections;
using System.Linq;
using IqSoft.CP.Common.Models.CacheModels;
using System.Web;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class BetGamesController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.BetGames).Id;

        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "??"
        };
        private static T ParseInputXML<T>(string xmlString) where T : new()
        {
            var result = new T();
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute("root"));
            using (var reader = new StringReader(xmlString))
            {
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }

        [HttpPost]
        [Route("{partnerId}/api/BetGames/ApiRequest")]
        public HttpResponseMessage ApiRequest(int partnerId, HttpRequestMessage request)
        {
            var partnerKey = string.Empty;
            var responseString = string.Empty;
            SessionIdentity clientSession = null;
            BllClient client = null;
            var response = new BaseOutput
            {
                Success = BetGamesHelpers.Statuses.Success,
                ErrorCode = 0,
                ErrorText = string.Empty,
                Parameters = new AccountDetailsOutputParams()
            };
            try
            {
                var byteArray = request.Content.ReadAsByteArrayAsync().Result;
                responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                WebApiApplication.DbLogger.Info("Input: " + responseString);
                var input = ParseInputXML<BaseInput>(responseString);
                response.Method = input.Method;
                response.Token = input.Token;         
                if (input.Method == BetGamesHelpers.Methods.Ping)
                    response = ConnectApi(partnerId, input);
                else
                {
                    if (input.Token != "-" && !string.IsNullOrEmpty(input.Token))
                    {
                        clientSession = ClientBll.GetClientProductSession(input.Token, Constants.DefaultLanguageId, null,
                                                                         !input.Method.Contains("payout"));
                        client = CacheManager.GetClientById(clientSession.Id);
                        partnerKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetGamesSecretKey);
                    }
                    var strInputParams = string.Empty;
                    int productId = 0;
                    switch (input.Method)
                    {
                        case BetGamesHelpers.Methods.GetAccountDetails:
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(input) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            response.Parameters = new AccountDetailsOutputParams
                            {
                                UserId = client.Id.ToString(),
                                UserName = "bg_" + client.Id,
                                Currency = client.CurrencyId,
                                Info = "-"
                            };
                            break;
                        case BetGamesHelpers.Methods.RefreshToken:
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(input) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            break;
                        case BetGamesHelpers.Methods.RequestNewToken:
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(input) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            response.Parameters = new AccountDetailsOutputParams
                            {
                                NewToken = input.Token
                            };
                            BaseHelpers.RemoveSessionFromeCache(input.Token, null);
                            break;
                        case BetGamesHelpers.Methods.GetBalance:
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(input) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            response.Parameters.Balance = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, 0) * 100).ToString();
                            break;
                        case BetGamesHelpers.Methods.DoBet:
                            var betInput = ParseInputXML<BetInput>(responseString);
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(betInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var alreadyProcessed = DoBet(betInput, clientSession, out productId);
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, productId) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !alreadyProcessed ? "0" : "1";
                            break;
                        case BetGamesHelpers.Methods.DoMultipleBets:
                            var betsInput = ParseInputXML<MultipleBetInput>(responseString);
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(betsInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var betsProcessed = false;
                            using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
                            {
                                if (documentBl.GetDocumentByRoundId((int)OperationTypes.Bet, betsInput.Parameters.SubscriptionId, ProviderId, client.Id) != null)
                                    betsProcessed = true;
                            }
                            if (!betsProcessed)
                            {
                                using (var transactionScope = CommonFunctions.CreateTransactionScope())
                                {
                                    foreach (var bet in betsInput.Parameters.BetItem)
                                        betsProcessed = DoBet(betsInput.Parameters.GameItem.Id.ToString(), bet.BetId,
                                                              betsInput.Parameters.SubscriptionId, bet.Amount.Value, clientSession, out productId);
                                    transactionScope.Complete();
                                }
                            }
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, 0) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !betsProcessed ? "0" : "1";
                            break;
                        case BetGamesHelpers.Methods.CombinationBet:
                            var combinationBetsInput = ParseInputXML<CombinationBetInput>(responseString);
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(combinationBetsInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var gameExtId = combinationBetsInput.Parameters.BetItem[0].GameItem.Id.ToString();
                            var combinationId = combinationBetsInput.Parameters.CombinationId;
                            var combBetsProcessed = DoBet(gameExtId, combinationId, combinationId,
                                                          combinationBetsInput.Parameters.Amount.Value, clientSession, out productId);
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, productId) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !combBetsProcessed ? "0" : "1";
                            break;
                        case BetGamesHelpers.Methods.CombinationWin:
                            var compinationWinsInput = ParseInputXML<CombinationBetInput>(responseString);
                            if (string.IsNullOrEmpty(partnerKey))
                            {
                                if (!int.TryParse(compinationWinsInput.Parameters.PlayerId, out int clientId))
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                                client = CacheManager.GetClientById(clientId);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                            }
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(compinationWinsInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var winsProcessed = DoWin(compinationWinsInput.Parameters, client, out productId);
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, productId) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !winsProcessed ? "0" : "1";
                            break;
                        case BetGamesHelpers.Methods.DoWin:
                            var winInput = ParseInputXML<BetInput>(responseString);
                            if (string.IsNullOrEmpty(partnerKey))
                            {
                                if (!winInput.Parameters.PlayerId.HasValue)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongClientId);
                                client = CacheManager.GetClientById(winInput.Parameters.PlayerId.Value);
                                if (client == null)
                                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                                partnerKey = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.BetGamesSecretKey);
                            }
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(winInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var winProcessed = DoWin(winInput, client, out productId);
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, productId) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !winProcessed ? "0" : "1";
                            break;
                        case BetGamesHelpers.Methods.DoPromoWin:
                            var promoWinInput = ParseInputXML<PromoWinInput>(responseString);
                            if (CommonFunctions.ComputeMd5(GetValueFromClassProperty(promoWinInput) + partnerKey).ToLower() != input.Signature ||
                                client == null)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                            var promoProcessed = DoPromoWin(promoWinInput, clientSession, out productId);
                            response.Parameters.BalanceAfter = Convert.ToInt32(BaseHelpers.GetClientProductBalance(client.Id, productId) * 100).ToString();
                            response.Parameters.AlreadyProcessed = !promoProcessed ? "0" : "1";
                            break;
                        default:
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.MethodNotFound);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                var errorDescription = fex.Detail != null ? fex.Detail.Message : fex.Message;
                WebApiApplication.DbLogger.Error(string.Format("ErrorMEssage: {0},  Input: {1}", errorDescription, responseString));
                response.Time = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
                response.Success = BetGamesHelpers.Statuses.Fail;
                response.ErrorCode = fex.Detail != null ? BetGamesHelpers.GetErrorCode(fex.Detail.Id) : BetGamesHelpers.ErrorCodes.InternalServerError;
                response.ErrorText = errorDescription;
                response.Parameters = null;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                response.Time = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
                response.Success = BetGamesHelpers.Statuses.Fail;
                response.ErrorCode = BetGamesHelpers.ErrorCodes.InternalServerError;
                response.ErrorText = ex.Message;
                response.Parameters = null;
            }
            var epochTime = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            response.Time = epochTime;
            response.Signature = CommonFunctions.ComputeMd5(GetValueFromClassProperty(response) + partnerKey);
            var output = CommonFunctions.ToXML(response);
            var resp = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(output, Encoding.UTF8)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml);
            return resp;
        }
        
        private BaseOutput ConnectApi(int partnerId, BaseInput input)
        {
            var secretKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.BetGamesSecretKey);
            var signString = string.Format("method{0}token{1}time{2}{3}", input.Method, input.Token, input.Time, secretKey);
            if (CommonFunctions.ComputeMd5(signString).ToLower() != input.Signature)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
            var epochTime = CheckTime(input.Time);
            var strOutputParams = string.Format(
                "method{0}token{1}success{2}error_code{3}time{4}error_text{5}{6}", input.Method, input.Token,
                BetGamesHelpers.Statuses.Success, 0, epochTime, string.Empty, secretKey);
            return new BaseOutput
            {
                Method = input.Method,
                Token = input.Token,
                Success = BetGamesHelpers.Statuses.Success,
                ErrorCode = 0,
                Time = epochTime,
                Signature = CommonFunctions.ComputeMd5(strOutputParams)
            };
        }

        private bool DoBet(BetInput input, SessionIdentity clientSession, out int productId)
        {
            using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.Parameters.Game);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var document = documentBl.GetDocumentByExternalId(input.Parameters.BetId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            RoundId = input.Parameters.TransactionId,
                            TransactionId = input.Parameters.BetId,
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (decimal)input.Parameters.Amount / 100,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, clientSession.ParentId ?? 0, 
                                    operationsFromProduct, document, WebApiApplication.DbLogger);
                                BaseHelpers.BroadcastBalance(client.Id, balance);
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
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        return false;
                    }
                    return true;
                }
            }
        }

        private bool DoBet(string gameExtId, string betId, string combinationId, int amount, SessionIdentity clientSession, out int productId)
        {
            using (var documentBl = new DocumentBll(clientSession, WebApiApplication.DbLogger))
            {
                using (var clientBl = new ClientBll(documentBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var product = CacheManager.GetProductByExternalId(ProviderId, gameExtId);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);

                    var document = documentBl.GetDocumentByExternalId(betId, client.Id, ProviderId,
                                                                      partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (document == null)
                    {
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            ExternalProductId = product.ExternalId,
                            ProductId = product.Id,
                            RoundId = combinationId,
                            TransactionId = betId,
                            OperationTypeId = (int)OperationTypes.Bet,
                            State = (int)BetDocumentStates.Uncalculated,
                            OperationItems = new List<OperationItemFromProduct>()
                        };

                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (decimal)amount / 100,
                            DeviceTypeId = clientSession.DeviceType
                        });
                        document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);

                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                var balance = ExternalPlatformHelpers.CreditFromClient(Convert.ToInt32(externalPlatformType.StringValue), client, 
                                    clientSession.ParentId ?? 0, operationsFromProduct, document, WebApiApplication.DbLogger);
                                BaseHelpers.BroadcastBalance(client.Id, balance);
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
                            BaseHelpers.BroadcastBetLimit(info);
                        }
                        return false;
                    }
                    return true;
                }
            }
        }

        private bool DoWin(BetInput input, BllClient client, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {                   
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.Parameters.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.Parameters.BetId, client.Id, ProviderId,
                                                                    partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    var externalTransactionId = string.Format("win_{0}", input.Parameters.BetId);
                    var winDocument = documentBl.GetDocumentByExternalId(externalTransactionId, client.Id, ProviderId,
                                                                    partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var state = (input.Parameters.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = betDocument.RoundId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = externalTransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (decimal)input.Parameters.Amount / 100
                        });
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                      betDocument.Id, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = (decimal)input.Parameters.Amount / 100,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        return false;
                    }

                    return true;
                }
            }
        }

        private bool DoWin(CombinationBetInputParams input, BllClient client, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {                    
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.BetItem[0].GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    productId = product.Id;
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.CombinationId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);

                    var winDocument = documentBl.GetDocumentByExternalId(input.BetItem[0].BetId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {
                        var state = (input.Amount > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                        betDocument.State = state;
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = betDocument.SessionId,
                            CurrencyId = client.CurrencyId,
                            RoundId = betDocument.RoundId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.BetItem[0].BetId,
                            CreditTransactionId = betDocument.Id,
                            State = state,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (decimal)input.Amount.Value / 100
                        });
                        var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out PartnerKey externalPlatformType);
                        if (isExternalPlatformClient)
                        {
                            try
                            {
                                ExternalPlatformHelpers.DebitToClient(Convert.ToInt32(externalPlatformType.StringValue), client,
                                                                      betDocument.Id, operationsFromProduct, doc[0], WebApiApplication.DbLogger);
                            }
                            catch (Exception ex)
                            {
                                WebApiApplication.DbLogger.Error(ex.Message);
                                documentBl.RollbackProductTransactions(operationsFromProduct);
                                throw;
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = (decimal)input.Amount.Value / 100,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        return false;
                    }
                    return true;
                }
            }
        }

        private bool DoPromoWin(PromoWinInput input, SessionIdentity clientSession, out int productId)
        {
            using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                using (var documentBl = new DocumentBll(clientBl))
                {
                    var client = CacheManager.GetClientById(clientSession.Id);
                    var key = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.PragmaticPlaySecureKey);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.Parameters.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                    productId = product.Id;

                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var betDocument = documentBl.GetDocumentByExternalId(input.Parameters.BetId, client.Id, ProviderId,
                                                     partnerProductSetting.Id, (int)OperationTypes.Bet);
                    if (betDocument == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                    var winDocument = documentBl.GetDocumentByExternalId(input.Parameters.PromoTransactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                    if (winDocument == null)
                    {                        
                        var operationsFromProduct = new ListOfOperationsFromApi
                        {
                            SessionId = clientSession.SessionId,
                            CurrencyId = client.CurrencyId,
                            GameProviderId = ProviderId,
                            OperationTypeId = (int)OperationTypes.Win,
                            ExternalOperationId = null,
                            ProductId = betDocument.ProductId,
                            TransactionId = input.Parameters.PromoTransactionId,
                            CreditTransactionId = betDocument.Id,
                            State = (int)BetDocumentStates.Won,
                            Info = string.Empty,
                            OperationItems = new List<OperationItemFromProduct>()
                        };
                        operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                        {
                            Client = client,
                            Amount = (decimal)input.Parameters.Amount / 100
                        });

                        clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        BaseHelpers.BroadcastWin(new ApiWin
                        {
                            GameName = product.NickName,
                            ClientId = client.Id,
                            ClientName = client.FirstName,
                            Amount = (decimal)input.Parameters.Amount / 100,
                            CurrencyId = client.CurrencyId,
                            PartnerId = client.PartnerId,
                            ProductId = product.Id,
                            ProductName = product.NickName,
                            ImageUrl = product.WebImageUrl
                        });
                        return false;
                    }
                    return true;
                }
            }
        }

        private int CheckTime(int time)
        {
            var epoch = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            if (epoch - time >= 60)
                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RequestExpired);
            return epoch;
        }

        private static string GetValueFromClassProperty(object instance)
        {
            var result = new StringBuilder();
            var properties = instance.GetType().GetProperties().OrderBy(x => x.DeclaringType.Name).ToList();
            foreach (var field in properties)
            {
                var value = field.GetValue(instance, null);
                if (value == null || field.Name.ToLower() == "signature")
                    continue;
                var elementName = field.GetCustomAttributes(true);
                var paramName = elementName != null && elementName.Any() && elementName[0] is XmlElementAttribute attribute
                                    ? attribute.ElementName : field.Name;
                if (field.PropertyType.IsValueType  || Type.GetTypeCode(field.PropertyType) == TypeCode.String)
                    result.Append(paramName + value.ToString());
                else if (field.PropertyType.IsGenericType)
                {
                    var resultBets = ((IEnumerable)value).Cast<object>().ToList();
                    if (resultBets != null && resultBets.Any())
                    {
                        result.Append(paramName);
                        foreach (var bet in resultBets)
                            result.Append(GetValueFromClassProperty(bet));
                    }
                }
                else
                {
                    if (paramName != "params")
                        result.Append(paramName);
                    result.Append(GetValueFromClassProperty(value));
                }
            }
            return result.ToString();
        }
    }
}