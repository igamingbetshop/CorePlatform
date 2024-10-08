using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.LuckyGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class LuckyGamingController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.LuckyGaming).Id;

        [HttpPost]
        [Route("{partnerId}/api/LuckyGaming/getBalances")]
        public HttpResponseMessage GetBalanse(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info("inputString " + inputString);
            var response = new List<BalanceOutput>();
            var sign = HttpContext.Current.Request.Headers.Get("AES-ENCODE");
            var aesKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingAESKey);
            var md5Key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingMD5Key);
            var aes = AESEncryptHelper.Encryption(inputString, aesKey);
            var hash = CommonFunctions.ComputeMd5(aes + md5Key);
            try
            {
                if (hash != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var inputs = JsonConvert.DeserializeObject<List<BalanceInput>>(inputString);
                foreach (var input in inputs)
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.AccountName.Replace("lucky", string.Empty)));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    response.Add(new BalanceOutput()
                    {
                        Balance = BaseHelpers.GetClientProductBalance(client.Id, 0) * 10000,
                        AccountName = input.AccountName,
                        AgentID = input.AgentID
                    });
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Add(new BalanceOutput() { ErrorCode = fex.Detail.Id + " _ " + fex.Detail.Message });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
                response.Add(new BalanceOutput() { ErrorCode = ex.Message });
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.TextXml)
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGaming/bets")]
        public HttpResponseMessage Debit(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            WebApiApplication.DbLogger.Info("inputString " + inputString);
            var response = new List<BetOutput>();
            var sign = HttpContext.Current.Request.Headers.Get("AES-ENCODE");
            var aesKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingAESKey);
            var md5Key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingMD5Key);
            var aes = AESEncryptHelper.Encryption(inputString, aesKey);
            var hash = CommonFunctions.ComputeMd5(aes + md5Key);
            try
            {
                if (hash != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                var inputs = JsonConvert.DeserializeObject<List<BetInput>>(inputString);
                foreach (var input in inputs)
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.AccountName.Replace("lucky", string.Empty)));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var clientSession = ClientBll.GetClientSessionByProductId(client.Id, product.Id);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                    var clientBalance = BaseHelpers.GetClientProductBalance(client.Id, clientSession.ProductId);
                    using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            var document = documentBl.GetDocumentByExternalId(input.ExternalTransactionId, client.Id, ProviderId,
                                                                                    partnerProductSetting.Id, (int)OperationTypes.Bet);
                            if (document == null)
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.Id,
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.ExternalTransactionId,
                                    ExternalProductId = product.ExternalId,
                                    GameProviderId = ProviderId,
                                    ProductId = product.Id,
                                    TransactionId = input.ExternalTransactionId,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = Convert.ToDecimal(input.Amount / 10000),
                                    DeviceTypeId = clientSession.DeviceType
                                });
                                document = clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                                BaseHelpers.BroadcastBetLimit(info);
                                document.State = (int)BetDocumentStates.Lost;
                                var recOperationsFromProduct = new ListOfOperationsFromApi
                                {
                                    SessionId = clientSession.Id,
                                    CurrencyId = client.CurrencyId,
                                    GameProviderId = ProviderId,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    ExternalProductId = product.ExternalId,
                                    ProductId = product.Id,
                                    TransactionId = input.ExternalTransactionId + "_win",
                                    CreditTransactionId = document.Id,
                                    State = (int)BetDocumentStates.Lost,
                                    Info = string.Empty,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                recOperationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                });
                                var doc = clientBl.CreateDebitsToClients(recOperationsFromProduct, document, documentBl);
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                BaseHelpers.BroadcastBalance(client.Id);
                            }
                            response.Add(new BetOutput()
                            {
                                AfterBalance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 10000,
                                AgentID = input.AgentID,
                                BeforeBalance = clientBalance * 10000,
                                Amount = input.Amount,
                                GameId = input.GameId,
                                ExternalTransactionId = input.ExternalTransactionId,
                                ResultCode = 1
                            });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Add(new BetOutput() { ErrorCode = fex.Detail.Message });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
                response.Add(new BetOutput() { ErrorCode = ex.Message });
            }
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGaming/paids")]
        public HttpResponseMessage Credit(HttpRequestMessage httpRequestMessage, int partnerId)
        {
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            var response = new List<BetOutput>();
            try
            {
                WebApiApplication.DbLogger.Info("inputString " + inputString);
                var inputs = JsonConvert.DeserializeObject<List<BetInput>>(inputString);
                var sign = HttpContext.Current.Request.Headers.Get("AES-ENCODE");
                var aesKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingAESKey);
                var md5Key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingMD5Key);
                var aes = AESEncryptHelper.Encryption(inputString, aesKey);
                var hash = CommonFunctions.ComputeMd5(aes + md5Key);
                if (hash != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                foreach (var input in inputs)
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.AccountName.Replace("lucky", string.Empty)));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var product = CacheManager.GetProductByExternalId(ProviderId, input.GameId);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var clientSession = ClientBll.GetClientSessionByProductId(client.Id, product.Id);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(client.PartnerId, clientSession.ProductId);
                    if (partnerProductSetting == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                    var clientBalance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 10000;
                    {
                        using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                        {
                            using (var clientBl = new ClientBll(documentBl))
                            {
                                var winDocument = documentBl.GetDocumentByExternalId(input.ExternalTransactionId, client.Id, ProviderId,
                                                                         partnerProductSetting.Id, (int)OperationTypes.Win);
                                if (winDocument == null && input.Amount / 10000 > 0)
                                {
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        GameProviderId = ProviderId,
                                        ProductId = product.Id,
                                        TransactionId = input.ExternalTransactionId + "_bet",
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
                                    betDocument.State = (int)BetDocumentStates.Won;
                                    operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        CurrencyId = client.CurrencyId,
                                        GameProviderId = ProviderId,
                                        OperationTypeId = (int)OperationTypes.Win,
                                        ExternalProductId = product.ExternalId,
                                        ProductId = betDocument.ProductId,
                                        TransactionId = input.ExternalTransactionId,
                                        CreditTransactionId = betDocument.Id,
                                        State = (int)BetDocumentStates.Won,
                                        OperationItems = new List<OperationItemFromProduct>()
                                    };
                                    operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                    {
                                        Client = client,
                                        Amount = input.Amount / 10000
                                    });
                                    var doc = clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                                    BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                                    BaseHelpers.BroadcastWin(new ApiWin
                                    {
                                        BetId = betDocument?.Id ?? 0,
                                        GameName = product.NickName,
                                        ClientId = client.Id,
                                        ClientName = client.FirstName,
                                        BetAmount = betDocument?.Amount,
                                        Amount = input.Amount,
                                        CurrencyId = client.CurrencyId,
                                        PartnerId = client.PartnerId,
                                        ProductId = product.Id,
                                        ProductName = product.NickName,
                                        ImageUrl = product.WebImageUrl
                                    });
                                }
                                response.Add( new BetOutput()
                                {
                                    AfterBalance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 10000,
                                    AgentID = input.AgentID,
                                    BeforeBalance = clientBalance,
                                    Amount = input.Amount,
                                    ExternalTransactionId = input.ExternalTransactionId,
                                    ResultCode = 1
                                });
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + fex.Detail.Id + " _ " + fex.Detail.Message);
                response.Add(new BetOutput() { ErrorCode = fex.Detail.Message });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(inputString + "_" + ex.Message);
                response.Add(new BetOutput() { ErrorCode = ex.Message });
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })),
            };
        }

        [HttpPost]
        [Route("{partnerId}/api/LuckyGaming/refunds")]
        public HttpResponseMessage Rollback(List<BetInput> inputs, int partnerId)
        {
            WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(inputs));
            var response = new List<BetOutput>();
            try
            {
                var sign = HttpContext.Current.Request.Headers.Get("AES-ENCODE");
                var aesKey = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingAESKey);
                var md5Key = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.LuckyGamingMD5Key);
                var aes = AESEncryptHelper.Encryption(JsonConvert.SerializeObject(inputs), aesKey);
                var hash = CommonFunctions.ComputeMd5(aes + md5Key);
                if (hash != sign)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash);
                foreach (var input in inputs)
                {
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.AccountName.Replace("lucky", string.Empty)));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var gameID = CacheManager.GetGameProviderValueByKey(client.PartnerId, ProviderId, Constants.PartnerKeys.LuckyGamingGameID);
                    var product = CacheManager.GetProductByExternalId(ProviderId, gameID);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);

                    var clientBalance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 10000;
                    var clientSession = ClientBll.GetClientSessionByProductId(client.Id, product.Id);
                    if (clientSession == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                    using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                    {
                        using (var clientBl = new ClientBll(documentBl))
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                SessionId = clientSession.Id,
                                GameProviderId = ProviderId,
                                TransactionId = input.ExternalTransactionId,
                                ExternalProductId = product.ExternalId,
                                ProductId = product.Id
                            };
                            var documents = new List<Document>();
                            try
                            {
                                documents = documentBl.RollbackProductTransactions(operationsFromProduct);
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            }
                            catch { }

                        }
                        response.Add(new BetOutput()
                        {
                            AfterBalance = BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 10000,
                            AgentID = input.AgentID,
                            BeforeBalance = clientBalance,
                            Amount = input.Amount,
                            ExternalTransactionId = input.ExternalTransactionId,
                            ResultCode = 1
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputs) + "_   ErrorMessage: " + fex.Detail.Message);
                response.Add(new BetOutput() { ErrorCode = fex.Detail == null ? fex.Message : fex.Detail.Message });
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(inputs) + "_   ErrorMessage: " + ex.Message);
                response.Add(new BetOutput() { ErrorCode = ex.Message });
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                })),
            };
        }
    }
}
