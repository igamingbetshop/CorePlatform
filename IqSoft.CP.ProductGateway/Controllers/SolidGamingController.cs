using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SolidGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class SolidGamingController : ApiController
    {
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming).Id;
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps(Constants.GameProviders.SolidGaming);
        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/tokens/{token}/authenticate")]
        public HttpResponseMessage Authenticate(int partnerId, string token, BaseInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };

            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                var brandCode = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingBrandCode);


                string authHeader = HttpContext.Current.Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                {
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                }

                var clientSession = ClientBll.GetClientProductSession(token, Constants.DefaultLanguageId);
                if (clientSession == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
                var client = CacheManager.GetClientById(clientSession.Id);
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var product = CacheManager.GetProductById(clientSession.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                if (product.GameProviderId != ProviderId)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongProductId);
                var response = new AuthenticationOutput
                {
                    ResponseCode = "OK",
                    BrandCode = brandCode,//get from provider settings
                    ClientId = client.Id.ToString(),
                    Country = "CN",
                    Currency = client.CurrencyId,
                    Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0.")),
                    PlayerUsername = client.FirstName + " " + client.LastName,
                    Language = "ENG"//clientSession.LanguageId
                };


                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = SolidGamingHelpers.GetStatusCode(response.ErrorCode);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-transaction")]
        public HttpResponseMessage CreateTransaction(int partnerId, BetInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };

            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = HttpContext.Current.Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                {
                    httpResponse.StatusCode = HttpStatusCode.Unauthorized;
                    WebApiApplication.DbLogger.Error(BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash).Data);
                }
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var clientBl = new ClientBll(documentBl))
                    {
                        var product = CacheManager.GetProductByExternalId(
                            CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming).Id,
                            input.GameCode);
                        if (product == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                        var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                        if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                        var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                        if (client == null)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                        if (client.CurrencyId != input.Currency)
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongCurrencyId);
                        if (input.bet != null)
                        {
                            var document = documentBl.GetDocumentByExternalId(input.bet.transactionId, client.Id,
                              ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                            if (document == null)
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    CurrencyId = input.Currency,
                                    RoundId = input.RoundId,
                                    GameProviderId = ProviderId,
                                    TransactionId = input.bet.transactionId,
                                    ExternalProductId = input.GameCode,
                                    ProductId = product.Id,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = Convert.ToDecimal(input.bet.amount / 100)
                                });
								clientBl.CreateCreditFromClient(operationsFromProduct, documentBl, out LimitInfo info);
                                BaseHelpers.BroadcastBetLimit(info);
                            }
                            else if (document.State == (int)BetDocumentStates.Deleted)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.GeneralException);
                        }
                        if (input.win != null)
                        {
                            DAL.Document betDocument = null;
                            if (input.bet != null)
                            {
                                betDocument = documentBl.GetDocumentByExternalId(input.bet.transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Bet);
                            }
                            else
                            {
                                var docs = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id, null);
                                if (!docs.Any())
                                {
                                    WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                                }
                                betDocument = docs.FirstOrDefault(x => x.State != (int)BetDocumentStates.Deleted);
                                if (betDocument == null)
                                    betDocument = docs.FirstOrDefault();
                            }


                            if (betDocument == null)
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                            }
                            if (betDocument.State == (int)BetDocumentStates.Deleted)
                                throw BaseBll.CreateException(string.Empty, Constants.Errors.GeneralException);

                            var winDocument = documentBl.GetDocumentByExternalId(input.win.transactionId, client.Id, ProviderId, partnerProductSetting.Id, (int)OperationTypes.Win);
                            if (winDocument == null)
                            {
                                var state = (Convert.ToInt32(input.win.amount) > 0 ? (int)BetDocumentStates.Won : (int)BetDocumentStates.Lost);
                                betDocument.State = state;
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.RoundId.ToString(),
                                    GameProviderId = ProviderId,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    TransactionId = input.win.transactionId,
                                    ExternalProductId = input.GameCode,
                                    ProductId = betDocument.ProductId,
                                    CreditTransactionId = betDocument.Id,
                                    State = state,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = Convert.ToDecimal(input.win.amount / 100)
                                });
								clientBl.CreateDebitsToClients(operationsFromProduct, betDocument, documentBl);
                            }
                            else if (winDocument.State == (int)BetDocumentStates.Deleted)
                                throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.GeneralException);
                        }
                        if (input.RoundEnded)
                        {
                            var betDocuments = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, input.RoundId, ProviderId, client.Id, (int)BetDocumentStates.Uncalculated);
                            foreach (var b in betDocuments)
                            {
                                var operationsFromProduct = new ListOfOperationsFromApi
                                {
                                    CurrencyId = client.CurrencyId,
                                    RoundId = input.RoundId.ToString(),
                                    GameProviderId = ProviderId,
                                    OperationTypeId = (int)OperationTypes.Win,
                                    TransactionId = Guid.NewGuid().ToString(),
                                    ExternalProductId = input.GameCode,
                                    ProductId = b.ProductId,
                                    CreditTransactionId = b.Id,
                                    State = (int)BetDocumentStates.Lost,
                                    OperationItems = new List<OperationItemFromProduct>()
                                };
                                operationsFromProduct.OperationItems.Add(new OperationItemFromProduct
                                {
                                    Client = client,
                                    Amount = 0
                                });
								clientBl.CreateDebitsToClients(operationsFromProduct, b, documentBl);
                            }
                        }
                        BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        var response = new BaseOutput
                        {
                            ResponseCode = "OK",
                            Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0."))
                        };
                        httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = SolidGamingHelpers.GetStatusCode(response.ErrorCode);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-transactions/{transactionId}")]
        public HttpResponseMessage Rollback(int partnerId, string transactionId, RollbackInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = HttpContext.Current.Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                {
                    httpResponse.StatusCode = HttpStatusCode.Unauthorized;
                    WebApiApplication.DbLogger.Error(BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash).Data);
                }
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var product = CacheManager.GetProductByExternalId(
                        CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming).Id,
                        input.GameCode);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    if (input.Action.ToUpper() == "CANCEL")
                    {
                        try
                        {
                            var operationsFromProduct = new ListOfOperationsFromApi
                            {
                                GameProviderId = ProviderId,
                                TransactionId = input.TransactionId,
                                ProductId = product.Id
                            };

                            documentBl.RollbackProductTransactions(operationsFromProduct);
                            BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                        }
                        catch (FaultException<BllFnErrorType> faultException)
                        {
                            if (faultException.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                                throw;
                        }
                    }
                    var response = new BaseOutput
                    {
                        ResponseCode = "OK",
                        Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0."))
                    };
                    httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = SolidGamingHelpers.GetStatusCode(response.ErrorCode);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-rounds/{roundId}")]
        public HttpResponseMessage RollbackRound(int partnerId, string roundId, RollbackInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = HttpContext.Current.Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                {
                    httpResponse.StatusCode = HttpStatusCode.Unauthorized;
                    WebApiApplication.DbLogger.Error(BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash).Data);
                }
                using (var documentBl = new DocumentBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var provider = CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming);
                    var product = CacheManager.GetProductByExternalId(provider.Id, input.GameCode);
                    if (product == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ProductNotFound);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                    var client = CacheManager.GetClientById(Convert.ToInt32(input.ClientId));
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    if (input.Action.ToUpper() == "CANCEL")
                    {
                        var documents = documentBl.GetDocumentsByRoundId((int)OperationTypes.Bet, roundId, provider.Id, client.Id, null);
                        if (!documents.Any())
                            throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.RoundNotFound);
                        if (documents.Any(x => x.State != (int)BetDocumentStates.Deleted))
                        {
                            try
                            {
                                foreach (var d in documents)
                                {
                                    var operationsFromProduct = new ListOfOperationsFromApi
                                    {
                                        GameProviderId = ProviderId,
                                        TransactionId = d.ExternalTransactionId,
                                        ProductId = product.Id
                                    };
                                    documentBl.RollbackProductTransactions(operationsFromProduct);                                    
                                }
                                BaseHelpers.RemoveClientBalanceFromeCache(client.Id);
                            }
                            catch (FaultException<BllFnErrorType> faultException)
                            {
                                if (faultException.Detail.Id != Constants.Errors.DocumentAlreadyRollbacked)
                                    throw;
                            }
                        }
                    }
                    var response = new BaseOutput
                    {
                        ResponseCode = "OK",
                        Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0."))
                    };
                    httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = SolidGamingHelpers.GetStatusCode(response.ErrorCode);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponse;
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/players/{playerId}/balance")]
        public HttpResponseMessage GetBalance(int partnerId, string playerId, BaseInput input)
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson)
            };
            try
            {
                BaseBll.CheckIp(WhitelistedIps);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = HttpContext.Current.Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                {
                    httpResponse.StatusCode = HttpStatusCode.Unauthorized;
                    WebApiApplication.DbLogger.Error(BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongHash).Data);
                }
                var product = CacheManager.GetProductByExternalId(
                    CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming).Id,
                    input.GameCode);
                if (product != null)
                {
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, product.Id);
                    if (partnerProductSetting == null || partnerProductSetting.Id == 0)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerProductSettingNotFound);
                }

                var client = CacheManager.GetClientById(Convert.ToInt32(playerId));
                if (client == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);

                var response = new BaseOutput
                {
                    ResponseCode = "OK",
                    Balance = Convert.ToDecimal((BaseHelpers.GetClientProductBalance(client.Id, product.Id) * 100).ToString("0."))
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson);
                httpResponse.StatusCode = SolidGamingHelpers.GetStatusCode(response.ErrorCode);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(response));
                httpResponse.Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8);
                httpResponse.StatusCode = HttpStatusCode.BadRequest;
            }
            return httpResponse;
        }
    }
}