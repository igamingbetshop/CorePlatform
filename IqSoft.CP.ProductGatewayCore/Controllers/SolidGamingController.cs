using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.ProductGateway.Models.SolidGaming;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class SolidGamingController : ControllerBase
    {
        private static readonly List<string> WhitelistedIps = new List<string>
        {
            "185.86.45.221",
            "91.208.125.69",
            "91.208.125.73"
        };
        private static readonly int ProviderId = CacheManager.GetGameProviderByName(Constants.GameProviders.SolidGaming).Id;

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/tokens/{token}/authenticate")]
        public ActionResult Authenticate(int partnerId, string token)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                var brandCode = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingBrandCode);


                string authHeader = Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
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
                    Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0.")),
                    PlayerUsername = client.FirstName + " " + client.LastName,
                    Language = "ENG"//clientSession.LanguageId
                };

                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                if (faultException.Detail.Id == Constants.Errors.WrongApiCredentials)
                    return Unauthorized(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-transaction")]
        public ActionResult CreateTransaction(int partnerId, BetInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                                clientBl.CreateCreditFromClient(operationsFromProduct, documentBl);
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
                                    Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                                    throw BaseBll.CreateException(string.Empty, Constants.Errors.CanNotConnectCreditAndDebit);
                                }
                                betDocument = docs.FirstOrDefault(x => x.State != (int)BetDocumentStates.Deleted);
                                if (betDocument == null)
                                    betDocument = docs.FirstOrDefault();
                            }


                            if (betDocument == null)
                            {
                                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
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
                        BaseHelpers.BroadcastBalance(client.Id);
                        var response = new BaseOutput
                        {
                            ResponseCode = "OK",
                            Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0."))
                        };
                        return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
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
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                if (faultException.Detail.Id == Constants.Errors.WrongApiCredentials)
                    return Unauthorized(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-transactions/{transactionId}")]
        public ActionResult Rollback(int partnerId, string transactionId, RollbackInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                            BaseHelpers.BroadcastBalance(client.Id);
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
                        Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0."))
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                if (faultException.Detail.Id == Constants.Errors.WrongApiCredentials)
                    return Unauthorized(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/game-rounds/{roundId}")]
        public ActionResult RollbackRound(int partnerId, string roundId, RollbackInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
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
                                BaseHelpers.BroadcastBalance(client.Id);
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
                        Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0."))
                    };
                    return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                }
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                if (faultException.Detail.Id == Constants.Errors.WrongApiCredentials)
                    return Unauthorized(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                return Ok(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/SolidGaming/players/{playerId}/balance")]
        public ActionResult GetBalance(int partnerId, string playerId, BaseInput input)
        {
            try
            {
                var ip = string.Empty;
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out StringValues header))
                    ip = header.ToString();
                BaseBll.CheckIp(WhitelistedIps, ip);
                var providerUserName = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingUserName);
                var providerPwd = CacheManager.GetGameProviderValueByKey(partnerId, ProviderId, Constants.PartnerKeys.SolidGamingPwd);
                string authHeader = Request.Headers["Authorization"];
                if (authHeader == null ||
                    authHeader != "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(providerUserName + ":" + providerPwd)))
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongApiCredentials);

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
                    Balance = Convert.ToDecimal((BaseBll.GetObjectBalance((int)ObjectTypes.Client, client.Id).AvailableBalance * 100).ToString("0."))
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> faultException)
            {
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(faultException.Detail.Id),
                    ErrorMessage = faultException.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                if (faultException.Detail.Id == Constants.Errors.WrongApiCredentials)
                    return Unauthorized(new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, Constants.HttpContentTypes.ApplicationJson));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ErrorOutput
                {
                    ErrorCode = SolidGamingHelpers.GetErrorCode(Constants.Errors.GeneralException),
                    ErrorMessage = ex.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return BadRequest(response);
            }
        }
    }
}